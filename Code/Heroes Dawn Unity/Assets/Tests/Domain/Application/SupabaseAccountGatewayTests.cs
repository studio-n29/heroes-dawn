using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Infrastructure.Supabase;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Client Supabase Auth (e-mail, session, jetons) avec un faux client HTTP : aucun appel réseau.</summary>
    public sealed class SupabaseAccountGatewayTests
    {
        const string Url = "https://projet.supabase.co";
        static readonly CancellationToken Ct = CancellationToken.None;

        FakeSupabaseHttp _http = null!;
        MemoryTokenStore _tokens = null!;
        FixedClock _clock = null!;
        SpyLogger _log = null!;
        SupabaseAccountGateway _gw = null!;

        [SetUp]
        public void SetUp()
        {
            _http = new FakeSupabaseHttp();
            _tokens = new MemoryTokenStore();
            _clock = new FixedClock();
            _log = new SpyLogger();
            _gw = new SupabaseAccountGateway(new SupabaseConfig(Url, "cle-anon-publique", "heroesdawn://auth"), _http, _tokens, _clock, _log);
        }

        long Now => new DateTimeOffset(_clock.UtcNow).ToUnixTimeSeconds();

        string SessionJson(string access = "ACCESS-1", string refresh = "REFRESH-1", long? expiresAt = null, string provider = "email", string name = "Moi", string email = "moi@exemple.fr")
            => "{\"access_token\":\"" + access + "\",\"refresh_token\":\"" + refresh + "\",\"expires_in\":3600,\"expires_at\":" + (expiresAt ?? Now + 3600)
             + ",\"user\":{\"id\":\"uid-1\",\"email\":\"" + email + "\",\"user_metadata\":{\"display_name\":\"" + name + "\"},\"app_metadata\":{\"provider\":\"" + provider + "\"}}}";

        static string Error(int code, string errorCode, string msg = "x") => "{\"code\":" + code + ",\"error_code\":\"" + errorCode + "\",\"msg\":\"" + msg + "\"}";

        [Test, Description("Vérifie que l'inscription envoie l'e-mail normalisé (minuscules, sans espaces) avec le pseudo en métadonnée à /auth/v1/signup, avec la clé publique, puis enregistre la session — parce que c'est le contrat avec Supabase")]
        public void SignUp_PostsToSignup_WithMetadataName_AndStoresTheSession()
        {
            _http.Enqueue(200, SessionJson());
            var r = _gw.SignUpAsync("  Moi@Exemple.FR ", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value.NeedsConfirmation, Is.False);
            Assert.That(r.Value.PlayerId, Is.EqualTo("uid-1"));
            var req = _http.Requests.Single();
            Assert.That((req.Method, req.Url), Is.EqualTo(("POST", Url + "/auth/v1/signup")));
            Assert.That(req.Body, Does.Contain("\"email\":\"moi@exemple.fr\"").And.Contain("\"display_name\":\"Moi\""));
            Assert.That(req.Headers["apikey"], Is.EqualTo("cle-anon-publique"));
            Assert.That(_tokens.Load()!.RefreshToken, Is.EqualTo("REFRESH-1"));
        }

        [Test, Description("Vérifie qu'une inscription sans session (confirmation d'e-mail requise) réussit avec NeedsConfirmation vrai et n'enregistre aucune session — parce que le joueur doit d'abord cliquer le lien reçu")]
        public void SignUp_WithoutSession_NeedsConfirmation_AndStoresNothing()
        {
            _http.Enqueue(200, "{\"id\":\"uid-2\",\"email\":\"moi@exemple.fr\",\"user_metadata\":{\"display_name\":\"Moi\"}}");
            var r = _gw.SignUpAsync("moi@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value.NeedsConfirmation, Is.True);
            Assert.That(_tokens.Load(), Is.Null);
        }

        [Test, Description("Vérifie que la connexion envoie le grant « password », ouvre la session (pseudo, e-mail, fournisseur) et enregistre les jetons — parce que c'est le chemin normal")]
        public void SignIn_PostsPasswordGrant_Succeeds_AndSavesTokens()
        {
            _http.Enqueue(200, SessionJson(provider: "email"));
            var r = _gw.SignInAsync("Moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That((r.Value.DisplayName, r.Value.Email, r.Value.Provider), Is.EqualTo(("Moi", "moi@exemple.fr", AuthProvider.Email)));
            Assert.That(_http.Requests.Single().Url, Is.EqualTo(Url + "/auth/v1/token?grant_type=password"));
            Assert.That(_tokens.Load()!.AccessToken, Is.EqualTo("ACCESS-1"));
        }

        [TestCase(400, "invalid_credentials", "errors.account_credentials")]
        [TestCase(400, "email_not_confirmed", "errors.account_unconfirmed")]
        [TestCase(429, "over_request_rate_limit", "errors.account_rate_limit")]
        [TestCase(422, "user_already_exists", "errors.account_exists")]
        [TestCase(422, "weak_password", "errors.account_weak_password")]
        [Test, Description("Vérifie que chaque code d'erreur de Supabase est traduit en clé de texte (identifiants, e-mail non confirmé, trop d'essais, compte existant, mot de passe faible) — parce que le joueur lit un message français, pas un code")]
        public void SignIn_MapsServerErrors(int status, string errorCode, string expectedKey)
        {
            _http.Enqueue(status, Error(status, errorCode));
            var r = _gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(r.Error!.LocKey, Is.EqualTo(expectedKey));
            Assert.That(_tokens.Load(), Is.Null);
        }

        [Test, Description("Vérifie qu'une erreur réseau ou une réponse qui n'est pas du JSON rend « serveur injoignable » sans exception — parce que le réseau peut tomber ou un proxy répondre une page HTML")]
        public void SignIn_NetworkError_AndGarbage_ReturnNetworkError()
        {
            _http.EnqueueNetworkError();
            Assert.That(_gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_network"));
            _http.Enqueue(200, "<html>pas du json</html>");
            Assert.That(_gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_network"));
            _http.Enqueue(200, "");
            Assert.That(_gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().IsFailure, Is.True);
            _http.Enqueue(200, "{}");
            Assert.That(_gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().IsFailure, Is.True, "une session sans jeton n'est pas une session");
        }

        [Test, Description("Vérifie qu'une adresse invalide ou un mot de passe de moins de 8 caractères est refusé sans aucune requête — parce que la validation se fait avant le réseau")]
        public void Validation_RejectsBadEmailOrShortPassword_WithoutAnyRequest()
        {
            Assert.That(_gw.SignUpAsync("pas-une-adresse", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_email"));
            Assert.That(_gw.SignUpAsync("@exemple.fr", "motdepasse1", "Moi", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_email"));
            Assert.That(_gw.SignUpAsync("moi@exemple.fr", "court", "Moi", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_password"));
            Assert.That(_gw.SignInAsync("", "motdepasse1", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_email"));
            Assert.That(_http.Requests, Is.Empty);
        }

        [Test, Description("Vérifie qu'une session valide se restaure sans réseau, qu'une session expirée se rafraîchit en remplaçant le jeton de rafraîchissement, et qu'un rafraîchissement refusé vide la mémoire et rend « pas de session » — parce que le jeton de rafraîchissement n'est utilisable qu'une fois")]
        public void RestoreSession_Valid_Expired_AndRefused()
        {
            Assert.That(_gw.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value, Is.Null, "rien en mémoire");
            _tokens.Save(new SupabaseSession { AccessToken = "A", RefreshToken = "R", ExpiresAtUnix = Now + 3600, UserId = "uid-1", Email = "moi@exemple.fr", DisplayName = "Moi" });
            Assert.That(_gw.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value!.PlayerId, Is.EqualTo("uid-1"));
            Assert.That(_http.Requests, Is.Empty, "session valide : aucun appel réseau");

            _clock.Advance(TimeSpan.FromHours(2));
            _http.Enqueue(200, SessionJson(access: "ACCESS-2", refresh: "REFRESH-2"));
            Assert.That(_gw.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value, Is.Not.Null);
            Assert.That(_http.Requests.Single().Url, Is.EqualTo(Url + "/auth/v1/token?grant_type=refresh_token"));
            Assert.That(_http.Requests.Single().Body, Does.Contain("\"refresh_token\":\"R\""));
            Assert.That(_tokens.Load()!.RefreshToken, Is.EqualTo("REFRESH-2"), "le nouveau jeton de rafraîchissement remplace l'ancien");

            _clock.Advance(TimeSpan.FromHours(2));
            _http.Enqueue(400, Error(400, "refresh_token_not_found"));
            Assert.That(_gw.RestoreSessionAsync(Ct).GetAwaiter().GetResult().Value, Is.Null);
            Assert.That(_tokens.Load(), Is.Null, "rafraîchissement refusé : la mémoire est vidée");
        }

        [Test, Description("Vérifie qu'une panne réseau pendant la restauration garde la session en mémoire (pour réessayer plus tard) et rend une erreur réseau — parce qu'être hors ligne ne doit pas déconnecter le joueur")]
        public void RestoreSession_Offline_KeepsTheStoredSession()
        {
            _tokens.Save(new SupabaseSession { AccessToken = "A", RefreshToken = "R", ExpiresAtUnix = Now - 10, UserId = "uid-1", Email = "moi@exemple.fr", DisplayName = "Moi" });
            _http.EnqueueNetworkError();
            var r = _gw.RestoreSessionAsync(Ct).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_network"));
            Assert.That(_tokens.Load(), Is.Not.Null);
        }

        [Test, Description("Vérifie que le jeton d'accès est rafraîchi quand il reste moins d'une minute et pas avant, et que deux demandes simultanées ne font qu'un seul rafraîchissement — parce que le jeton de rafraîchissement tourne à chaque usage")]
        public void AccessToken_RefreshesWhenLessThanAMinuteLeft_AndCoalescesConcurrentCalls()
        {
            _tokens.Save(new SupabaseSession { AccessToken = "A", RefreshToken = "R", ExpiresAtUnix = Now + 300, UserId = "u", Email = "e@x.fr", DisplayName = "n" });
            Assert.That(_gw.AccessTokenAsync(Ct).GetAwaiter().GetResult().Value, Is.EqualTo("A"));
            Assert.That(_http.Requests, Is.Empty);

            _clock.Advance(TimeSpan.FromSeconds(260));
            _http.Gate = new UniTaskCompletionSource();
            _http.Enqueue(200, SessionJson(access: "ACCESS-3", refresh: "REFRESH-3"));
            var first = _gw.AccessTokenAsync(Ct);
            var second = _gw.AccessTokenAsync(Ct);
            _http.Release();
            Assert.That(first.GetAwaiter().GetResult().Value, Is.EqualTo("ACCESS-3"));
            Assert.That(second.GetAwaiter().GetResult().Value, Is.EqualTo("ACCESS-3"));
            Assert.That(_http.Requests.Count, Is.EqualTo(1), "un seul rafraîchissement");
        }

        [Test, Description("Vérifie que sans session le jeton d'accès demandé rend « session expirée » sans requête — parce que le client cloud doit savoir qu'il faut se reconnecter")]
        public void AccessToken_WithoutSession_ReturnsSessionError()
        {
            var r = _gw.AccessTokenAsync(Ct).GetAwaiter().GetResult();
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_session"));
            Assert.That(_http.Requests, Is.Empty);
        }

        [Test, Description("Vérifie que la déconnexion appelle /auth/v1/logout avec le jeton et vide la mémoire, même si la requête échoue — parce que le joueur doit pouvoir se déconnecter hors ligne")]
        public void SignOut_PostsLogout_AndClearsTheStore_EvenIfTheRequestFails()
        {
            _tokens.Save(new SupabaseSession { AccessToken = "A", RefreshToken = "R", ExpiresAtUnix = Now + 3600, UserId = "u", Email = "e@x.fr", DisplayName = "n" });
            _http.EnqueueNetworkError();
            Assert.That(_gw.SignOutAsync(Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(_http.Requests.Single().Url, Is.EqualTo(Url + "/auth/v1/logout"));
            Assert.That(_http.Requests.Single().Headers["Authorization"], Is.EqualTo("Bearer A"));
            Assert.That(_tokens.Load(), Is.Null);
        }

        [Test, Description("Vérifie que la réinitialisation du mot de passe appelle /auth/v1/recover et réussit même quand le serveur dit que l'adresse est inconnue — parce qu'on ne révèle pas l'existence d'un compte")]
        public void ResetPassword_PostsRecover_AndNeverRevealsWhetherTheAccountExists()
        {
            _http.Enqueue(200);
            Assert.That(_gw.ResetPasswordAsync("Moi@Exemple.fr", Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(_http.Requests.Single().Url, Is.EqualTo(Url + "/auth/v1/recover"));
            Assert.That(_http.Requests.Single().Body, Does.Contain("moi@exemple.fr"));
            _http.Enqueue(404, Error(404, "user_not_found"));
            Assert.That(_gw.ResetPasswordAsync("inconnu@exemple.fr", Ct).GetAwaiter().GetResult().IsSuccess, Is.True);
            _http.EnqueueNetworkError();
            Assert.That(_gw.ResetPasswordAsync("moi@exemple.fr", Ct).GetAwaiter().GetResult().Error!.LocKey, Is.EqualTo("errors.account_network"));
        }

        [Test, Description("Vérifie qu'aucun jeton (accès ni rafraîchissement) n'apparaît dans les journaux, ni en cas de succès ni en cas d'échec — parce que les jetons donnent accès au compte")]
        public void Tokens_NeverAppearInLogs()
        {
            _http.Enqueue(200, SessionJson(access: "SECRET-ACCESS", refresh: "SECRET-REFRESH"));
            _gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult();
            _clock.Advance(TimeSpan.FromHours(2));
            _http.Enqueue(400, "{\"error\":\"invalid_grant\",\"detail\":\"SECRET-REFRESH\"}");
            _gw.RestoreSessionAsync(Ct).GetAwaiter().GetResult();
            Assert.That(_log.Lines.Any(l => l.Contains("SECRET")), Is.False);
            Assert.That(_log.Lines.Any(l => l.Contains("motdepasse1")), Is.False, "le mot de passe non plus");
        }

        [Test, Description("Vérifie que deux connexions simultanées avec les mêmes identifiants ne font qu'un seul envoi réseau — parce qu'un double appui ne doit pas doubler la requête")]
        public void ConcurrentSignIn_IsCoalesced()
        {
            _http.Gate = new UniTaskCompletionSource();
            _http.Enqueue(200, SessionJson());
            var a = _gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct);
            var b = _gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct);
            _http.Release();
            Assert.That(a.GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(b.GetAwaiter().GetResult().IsSuccess, Is.True);
            Assert.That(_http.Requests.Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un fournisseur Google ou Apple enregistré dans la session est restitué (app_metadata.provider) — parce que le profil affiche comment le compte a été ouvert")]
        public void Provider_IsReadFromTheSession()
        {
            _http.Enqueue(200, SessionJson(provider: "google"));
            Assert.That(_gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().Value.Provider, Is.EqualTo(AuthProvider.Google));
            _http.Enqueue(200, SessionJson(provider: "apple"));
            Assert.That(_gw.SignInAsync("moi@exemple.fr", "motdepasse1", Ct).GetAwaiter().GetResult().Value.Provider, Is.EqualTo(AuthProvider.Apple));
        }

        [Test, Description("Vérifie que la configuration refuse une URL sans https ou une clé vide, accepte une configuration valide, et ne lit que l'URL, la clé publique et la redirection — parce qu'une mauvaise configuration doit retomber sur le simulateur")]
        public void Config_Parse_ValidAndInvalid()
        {
            var ok = SupabaseConfig.Parse("{\"url\":\"https://x.supabase.co\",\"anonKey\":\"k\",\"redirectUrl\":\"heroesdawn://auth\"}");
            Assert.That(ok.IsValid, Is.True);
            Assert.That(SupabaseConfig.Parse("{\"url\":\"http://x.supabase.co\",\"anonKey\":\"k\",\"redirectUrl\":\"r\"}").IsValid, Is.False);
            Assert.That(SupabaseConfig.Parse("{\"url\":\"https://x.supabase.co\",\"anonKey\":\"\",\"redirectUrl\":\"r\"}").IsValid, Is.False);
            Assert.That(SupabaseConfig.Parse("pas du json").IsValid, Is.False);
            Assert.That(SupabaseConfig.Parse("").IsValid, Is.False);
        }
    }
}
