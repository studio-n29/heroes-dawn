#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using UnityEngine;
using ILogger = N29.HeroesDawn.Application.Ports.ILogger;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>
    /// Compte joueur par Supabase Auth (API REST). Les jetons restent ici (mémorisés par <see cref="ISupabaseTokenStore"/>), jamais dans le profil ni les journaux.
    /// Le jeton de rafraîchissement tourne à chaque usage : les rafraîchissements simultanés sont fusionnés en un seul.
    /// </summary>
    public sealed class SupabaseAccountGateway : IAccountGateway
    {
        /// <summary>Marge avant l'expiration à partir de laquelle le jeton d'accès est rafraîchi.</summary>
        const int RefreshMarginSeconds = 60;

        [Serializable] sealed class MetaDto { public string display_name = ""; public string full_name = ""; public string name = ""; }
        [Serializable] sealed class AppMetaDto { public string provider = ""; }
        [Serializable] sealed class UserDto { public string id = ""; public string email = ""; public MetaDto? user_metadata; public AppMetaDto? app_metadata; }
        [Serializable] sealed class SessionDto { public string access_token = ""; public string refresh_token = ""; public long expires_in; public long expires_at; public UserDto? user; }
        [Serializable] sealed class SignUpData { public string display_name = ""; }
        [Serializable] sealed class SignUpBody { public string email = ""; public string password = ""; public SignUpData data = new SignUpData(); }
        [Serializable] sealed class PasswordBody { public string email = ""; public string password = ""; }
        [Serializable] sealed class RefreshBody { public string refresh_token = ""; }
        [Serializable] sealed class RecoverBody { public string email = ""; }
        [Serializable] sealed class PkceBody { public string auth_code = ""; public string code_verifier = ""; }

        readonly SupabaseConfig _config;
        readonly ISupabaseHttp _http;
        readonly ISupabaseTokenStore _tokens;
        readonly IClock _clock;
        readonly ILogger? _log;
        // Appels simultanés fusionnés : plusieurs attentes possibles sur la même source.
        UniTaskCompletionSource<Result<PlayerAccount>>? _pendingSignIn;
        string _pendingSignInKey = "";
        UniTaskCompletionSource<Result<string>>? _pendingRefresh;
        readonly IBrowserOpener? _browser;
        readonly IAuthRedirectInbox? _inbox;
        readonly TimeSpan _oauthTimeout;
        bool _oauthPending;

        public SupabaseAccountGateway(SupabaseConfig config, ISupabaseHttp http, ISupabaseTokenStore tokens, IClock clock, ILogger? log = null,
                                      IBrowserOpener? browser = null, IAuthRedirectInbox? inbox = null, TimeSpan? oauthTimeout = null)
        {
            _config = config; _http = http; _tokens = tokens; _clock = clock; _log = log;
            _browser = browser; _inbox = inbox; _oauthTimeout = oauthTimeout ?? TimeSpan.FromMinutes(5);
        }

        long NowUnix => new DateTimeOffset(_clock.UtcNow).ToUnixTimeSeconds();

        // ------------------------------------------------------------------ Inscription et connexion

        public async UniTask<Result<PlayerAccount>> SignUpAsync(string email, string password, string displayName, CancellationToken ct)
        {
            var invalid = Validate(email, password);
            if (invalid != null) return Result<PlayerAccount>.Fail(invalid);
            string mail = Normalize(email);
            var body = JsonUtility.ToJson(new SignUpBody { email = mail, password = password, data = new SignUpData { display_name = displayName ?? "" } });
            var r = await _http.SendAsync("POST", _config.Url + "/auth/v1/signup", Headers(), body, ct);
            if (!r.IsSuccess) return Result<PlayerAccount>.Fail(SupabaseErrors.Map(r));
            var session = ReadSession(r.Body);
            if (session != null)
            {
                _tokens.Save(session);
                _log?.Info("Compte : inscription réussie");
                return Result<PlayerAccount>.Ok(session.ToAccount());
            }
            // Pas de session : l'adresse doit d'abord être confirmée (le serveur répond alors avec l'utilisateur seul).
            var user = ReadUser(r.Body);
            if (user == null || string.IsNullOrEmpty(user.id)) return Result<PlayerAccount>.Fail(SupabaseErrors.Network());
            _log?.Info("Compte : inscription réussie, confirmation requise");
            return Result<PlayerAccount>.Ok(new PlayerAccount(user.id, NameOf(user, mail), mail, AuthProvider.Email, needsConfirmation: true));
        }

        public async UniTask<Result<PlayerAccount>> SignInAsync(string email, string password, CancellationToken ct)
        {
            var invalid = Validate(email, password);
            if (invalid != null) return Result<PlayerAccount>.Fail(invalid);
            string key = Normalize(email) + "\n" + password;
            if (_pendingSignIn != null && _pendingSignInKey == key) return await _pendingSignIn.Task;
            var source = new UniTaskCompletionSource<Result<PlayerAccount>>();
            _pendingSignIn = source;
            _pendingSignInKey = key;
            try
            {
                var result = await SignInCoreAsync(Normalize(email), password, ct);
                source.TrySetResult(result);
                return result;
            }
            catch (Exception e) { source.TrySetException(e); throw; }
            finally { _pendingSignIn = null; _pendingSignInKey = ""; }
        }

        async UniTask<Result<PlayerAccount>> SignInCoreAsync(string email, string password, CancellationToken ct)
        {
            var body = JsonUtility.ToJson(new PasswordBody { email = email, password = password });
            var r = await _http.SendAsync("POST", _config.Url + "/auth/v1/token?grant_type=password", Headers(), body, ct);
            if (!r.IsSuccess) return Result<PlayerAccount>.Fail(SupabaseErrors.Map(r));
            var session = ReadSession(r.Body);
            if (session == null) return Result<PlayerAccount>.Fail(SupabaseErrors.Network());
            _tokens.Save(session);
            _log?.Info("Compte : connexion réussie");
            return Result<PlayerAccount>.Ok(session.ToAccount());
        }

        /// <summary>
        /// Google ou Apple : ouvre le navigateur sur l'adresse d'autorisation (PKCE), attend le retour par lien profond (au plus <c>oauthTimeout</c>),
        /// puis échange le code contre une session. Un seul flux à la fois ; l'annulation par le joueur est propagée.
        /// </summary>
        public async UniTask<Result<PlayerAccount>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct)
        {
            var failed = new DomainError("account.oauth", "errors.account_oauth");
            if (provider == AuthProvider.Email || _browser == null || _inbox == null || _oauthPending) return Result<PlayerAccount>.Fail(failed);
            _oauthPending = true;
            try
            {
                var (verifier, challenge) = Pkce.Create();
                using var wait = CancellationTokenSource.CreateLinkedTokenSource(ct);
                wait.CancelAfter(_oauthTimeout);
                _browser.Open(OAuthFlow.AuthorizeUrl(_config, provider, challenge));
                string? redirect = await _inbox.WaitForRedirectAsync(_config.RedirectUrl, wait.Token);
                ct.ThrowIfCancellationRequested();   // annulé par le joueur : pas une erreur
                string? code = redirect == null ? null : OAuthFlow.CodeFrom(redirect, _config.RedirectUrl);
                if (code == null)
                {
                    _log?.Info("Compte : connexion par fournisseur annulée ou échouée");
                    return Result<PlayerAccount>.Fail(failed);
                }
                var body = JsonUtility.ToJson(new PkceBody { auth_code = code, code_verifier = verifier });
                var r = await _http.SendAsync("POST", _config.Url + "/auth/v1/token?grant_type=pkce", Headers(), body, ct);
                if (r.NetworkError || r.Status >= 500) return Result<PlayerAccount>.Fail(SupabaseErrors.Network());
                if (!r.IsSuccess) return Result<PlayerAccount>.Fail(failed);
                var session = ReadSession(r.Body);
                if (session == null) return Result<PlayerAccount>.Fail(failed);
                _tokens.Save(session);
                _log?.Info("Compte : connexion par fournisseur réussie");
                return Result<PlayerAccount>.Ok(session.ToAccount());
            }
            finally { _oauthPending = false; }
        }

        // ------------------------------------------------------------------ Session

        public async UniTask<Result<PlayerAccount?>> RestoreSessionAsync(CancellationToken ct)
        {
            var stored = _tokens.Load();
            if (stored == null) return Result<PlayerAccount?>.Ok(null);
            if (stored.ExpiresAtUnix - NowUnix > RefreshMarginSeconds) return Result<PlayerAccount?>.Ok(stored.ToAccount());
            var refreshed = await AccessTokenAsync(ct);
            if (refreshed.IsSuccess) return Result<PlayerAccount?>.Ok(_tokens.Load()?.ToAccount());
            if (refreshed.Error!.Code == "account.session") return Result<PlayerAccount?>.Ok(null);   // rafraîchissement refusé : session perdue
            return Result<PlayerAccount?>.Fail(refreshed.Error);                                       // réseau : on garde la session pour plus tard
        }

        /// <summary>Le jeton d'accès valide (rafraîchi s'il expire dans moins d'une minute), pour les appels connectés.</summary>
        public async UniTask<Result<string>> AccessTokenAsync(CancellationToken ct)
        {
            var stored = _tokens.Load();
            if (stored == null) return Result<string>.Fail(SupabaseErrors.Session());
            if (stored.ExpiresAtUnix - NowUnix > RefreshMarginSeconds) return Result<string>.Ok(stored.AccessToken);
            if (_pendingRefresh != null) return await _pendingRefresh.Task;
            var source = new UniTaskCompletionSource<Result<string>>();
            _pendingRefresh = source;
            try
            {
                var result = await RefreshAsync(stored, ct);
                source.TrySetResult(result);
                return result;
            }
            catch (Exception e) { source.TrySetException(e); throw; }
            finally { _pendingRefresh = null; }
        }

        async UniTask<Result<string>> RefreshAsync(SupabaseSession stored, CancellationToken ct)
        {
            var body = JsonUtility.ToJson(new RefreshBody { refresh_token = stored.RefreshToken });
            var r = await _http.SendAsync("POST", _config.Url + "/auth/v1/token?grant_type=refresh_token", Headers(), body, ct);
            if (r.NetworkError || r.Status >= 500) return Result<string>.Fail(SupabaseErrors.Network());
            if (!r.IsSuccess)
            {
                _tokens.Clear();
                _log?.Info("Compte : session expirée");
                return Result<string>.Fail(SupabaseErrors.Session());
            }
            var session = ReadSession(r.Body);
            if (session == null) return Result<string>.Fail(SupabaseErrors.Network());
            _tokens.Save(session);
            return Result<string>.Ok(session.AccessToken);
        }

        public async UniTask<Result> SignOutAsync(CancellationToken ct)
        {
            var stored = _tokens.Load();
            _tokens.Clear();   // la mémoire est vidée quoi qu'il arrive : on peut se déconnecter hors ligne
            if (stored != null)
            {
                try { await _http.SendAsync("POST", _config.Url + "/auth/v1/logout", Headers(stored.AccessToken), null, ct); }
                catch (OperationCanceledException) { throw; }
                catch (Exception) { /* meilleur effort */ }
            }
            _log?.Info("Compte : déconnexion");
            return Result.Ok();
        }

        public async UniTask<Result> ResetPasswordAsync(string email, CancellationToken ct)
        {
            string mail = Normalize(email);
            if (!LooksLikeEmail(mail)) return Result.Fail(new DomainError("account.email", "errors.account_email"));
            var r = await _http.SendAsync("POST", _config.Url + "/auth/v1/recover", Headers(), JsonUtility.ToJson(new RecoverBody { email = mail }), ct);
            if (r.NetworkError) return Result.Fail(SupabaseErrors.Network());
            if (r.Status == 429) return Result.Fail(SupabaseErrors.Map(r));
            return Result.Ok();   // adresse connue ou non : même réponse
        }

        // ------------------------------------------------------------------ Aides

        Dictionary<string, string> Headers(string? bearer = null)
        {
            var h = new Dictionary<string, string> { ["apikey"] = _config.AnonKey, ["Content-Type"] = "application/json" };
            if (bearer != null) h["Authorization"] = "Bearer " + bearer;
            return h;
        }

        static string Normalize(string email) => (email ?? "").Trim().ToLowerInvariant();

        static bool LooksLikeEmail(string mail)
        {
            int at = mail.IndexOf('@');
            return at > 0 && at < mail.Length - 1 && mail.IndexOf('@', at + 1) < 0;
        }

        static DomainError? Validate(string email, string password)
        {
            if (!LooksLikeEmail(Normalize(email))) return new DomainError("account.email", "errors.account_email");
            if ((password ?? "").Length < 8) return new DomainError("account.password", "errors.account_password");
            return null;
        }

        static string NameOf(UserDto user, string fallbackEmail)
        {
            var m = user.user_metadata;
            if (m != null)
            {
                if (!string.IsNullOrWhiteSpace(m.display_name)) return m.display_name;
                if (!string.IsNullOrWhiteSpace(m.full_name)) return m.full_name;
                if (!string.IsNullOrWhiteSpace(m.name)) return m.name;
            }
            string mail = string.IsNullOrEmpty(user.email) ? fallbackEmail : user.email;
            int at = mail.IndexOf('@');
            return at > 0 ? mail.Substring(0, at) : "Joueur";
        }

        static AuthProvider ProviderOf(UserDto user)
        {
            switch ((user.app_metadata?.provider ?? "").ToLowerInvariant())
            {
                case "google": return AuthProvider.Google;
                case "apple": return AuthProvider.Apple;
                default: return AuthProvider.Email;
            }
        }

        static UserDto? ReadUser(string json)
        {
            try { return JsonUtility.FromJson<UserDto>(json); }
            catch (Exception) { return null; }
        }

        /// <summary>La session d'une réponse de connexion, ou <c>null</c> si le corps n'en contient pas une complète.</summary>
        SupabaseSession? ReadSession(string json)
        {
            SessionDto? dto;
            try { dto = JsonUtility.FromJson<SessionDto>(json); }
            catch (Exception) { return null; }
            if (dto == null || string.IsNullOrEmpty(dto.access_token) || string.IsNullOrEmpty(dto.refresh_token) || dto.user == null || string.IsNullOrEmpty(dto.user.id)) return null;
            return new SupabaseSession
            {
                AccessToken = dto.access_token,
                RefreshToken = dto.refresh_token,
                ExpiresAtUnix = dto.expires_at > 0 ? dto.expires_at : NowUnix + dto.expires_in,
                UserId = dto.user.id,
                Email = dto.user.email ?? "",
                DisplayName = NameOf(dto.user, dto.user.email ?? ""),
                Provider = ProviderOf(dto.user),
            };
        }
    }
}
