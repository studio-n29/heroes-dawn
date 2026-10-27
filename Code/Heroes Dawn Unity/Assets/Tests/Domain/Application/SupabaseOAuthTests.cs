using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Infrastructure.Supabase;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Connexion Google et Apple par le navigateur (OAuth avec PKCE) : adresse construite, retour analysé, échange du code.</summary>
    public sealed class SupabaseOAuthTests
    {
        const string Url = "https://projet.supabase.co";
        const string Redirect = "heroesdawn://auth";

        sealed class FakeBrowser : IBrowserOpener
        {
            public readonly List<string> Opened = new List<string>();
            public void Open(string url) => Opened.Add(url);
        }

        sealed class FakeInbox : IAuthRedirectInbox
        {
            /// <summary>Adresse de retour à rendre ; nulle : le joueur ne revient jamais (attend l'annulation).</summary>
            public string? Redirect;
            public string? ExpectedPrefix;
            public async UniTask<string?> WaitForRedirectAsync(string expectedPrefix, CancellationToken ct)
            {
                ExpectedPrefix = expectedPrefix;
                if (Redirect != null) return Redirect;
                // Attente terminée à l'annulation, sur le fil qui annule (pas de boucle Unity : les tests bloquent le fil principal).
                var source = new UniTaskCompletionSource<string?>();
                using var registration = ct.Register(() => source.TrySetResult(null));
                return await source.Task;
            }
        }

        FakeSupabaseHttp _http = null!;
        MemoryTokenStore _tokens = null!;
        FakeBrowser _browser = null!;
        FakeInbox _inbox = null!;
        SupabaseAccountGateway _gw = null!;

        [SetUp]
        public void SetUp()
        {
            _http = new FakeSupabaseHttp();
            _tokens = new MemoryTokenStore();
            _browser = new FakeBrowser();
            _inbox = new FakeInbox();
            _gw = Gateway(TimeSpan.FromMinutes(5));
        }

        SupabaseAccountGateway Gateway(TimeSpan timeout)
            => new SupabaseAccountGateway(new SupabaseConfig(Url, "cle-anon", Redirect), _http, _tokens, new FixedClock(), null, _browser, _inbox, timeout);

        static string SessionJson(string provider)
            => "{\"access_token\":\"ACCESS\",\"refresh_token\":\"REFRESH\",\"expires_in\":3600,\"expires_at\":4102444800,\"user\":{\"id\":\"uid-9\",\"email\":\"moi@exemple.fr\","
             + "\"user_metadata\":{\"full_name\":\"Moi Complet\"},\"app_metadata\":{\"provider\":\"" + provider + "\"}}}";

        static string QueryValue(string url, string key)
        {
            var q = url.Substring(url.IndexOf('?') + 1).Split('&').Select(p => p.Split('=')).FirstOrDefault(p => p[0] == key);
            return q == null ? "" : Uri.UnescapeDataString(q[1]);
        }

        [Test, Description("Vérifie que le vecteur de la RFC 7636 donne le bon défi PKCE (SHA-256 puis base64url sans remplissage), que deux créations donnent des vérificateurs différents de 43 caractères base64url — parce que PKCE protège l'échange du code")]
        public void Pkce_Challenge_IsTheS256OfTheVerifier()
        {
            Assert.That(Pkce.ChallengeOf("dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"), Is.EqualTo("E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM"));
            var a = Pkce.Create(); var b = Pkce.Create();
            Assert.That(a.Verifier, Is.Not.EqualTo(b.Verifier));
            Assert.That(a.Verifier.Length, Is.InRange(43, 128));
            Assert.That(a.Verifier, Does.Match("^[A-Za-z0-9_-]+$"));
            Assert.That(a.Challenge, Is.EqualTo(Pkce.ChallengeOf(a.Verifier)));
        }

        [TestCase(AuthProvider.Google, "google")]
        [TestCase(AuthProvider.Apple, "apple")]
        [Test, Description("Vérifie que l'adresse ouverte dans le navigateur porte le fournisseur, le lien de retour encodé et le défi PKCE (méthode S256), et que le jeu attend un retour sur ce lien — parce que c'est ce que Supabase exige")]
        public void Authorize_Url_HasProviderRedirectAndChallenge(AuthProvider provider, string name)
        {
            _inbox.Redirect = Redirect + "?code=abc";
            _http.Enqueue(200, SessionJson(name));
            _gw.SignInWithProviderAsync(provider, CancellationToken.None).GetAwaiter().GetResult();
            var url = _browser.Opened.Single();
            Assert.That(url, Does.StartWith(Url + "/auth/v1/authorize?"));
            Assert.That(QueryValue(url, "provider"), Is.EqualTo(name));
            Assert.That(QueryValue(url, "redirect_to"), Is.EqualTo(Redirect));
            Assert.That(QueryValue(url, "code_challenge_method"), Is.EqualTo("S256"));
            Assert.That(QueryValue(url, "code_challenge").Length, Is.EqualTo(43));
            Assert.That(_inbox.ExpectedPrefix, Is.EqualTo(Redirect));
        }

        [Test, Description("Vérifie qu'un retour avec un code est échangé (grant « pkce » avec le vérificateur dont le défi a été envoyé), que la session est enregistrée et que le fournisseur est restitué — parce que c'est le chemin normal de Google et d'Apple")]
        public void Redirect_WithCode_ExchangesAndStoresTheSession()
        {
            _inbox.Redirect = Redirect + "?code=le-code%2B1";
            _http.Enqueue(200, SessionJson("google"));
            var r = _gw.SignInWithProviderAsync(AuthProvider.Google, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, Is.True);
            Assert.That((r.Value.Provider, r.Value.DisplayName), Is.EqualTo((AuthProvider.Google, "Moi Complet")));
            var exchange = _http.Requests.Single();
            Assert.That(exchange.Url, Is.EqualTo(Url + "/auth/v1/token?grant_type=pkce"));
            Assert.That(exchange.Body, Does.Contain("\"auth_code\":\"le-code+1\""));
            string verifier = System.Text.RegularExpressions.Regex.Match(exchange.Body, "\"code_verifier\":\"([^\"]+)\"").Groups[1].Value;
            Assert.That(Pkce.ChallengeOf(verifier), Is.EqualTo(QueryValue(_browser.Opened.Single(), "code_challenge")), "le vérificateur envoyé correspond au défi annoncé");
            Assert.That(_tokens.Load()!.AccessToken, Is.EqualTo("ACCESS"));
        }

        [TestCase("?error=access_denied&error_description=User+cancelled")]
        [TestCase("#error=access_denied&error_description=User+cancelled")]
        [TestCase("?foo=bar")]
        [TestCase("")]
        [Test, Description("Vérifie qu'un retour avec une erreur (en paramètre ou en fragment), sans code, ou sans rien, rend « connexion échouée ou annulée » sans aucun échange ni session — parce que le joueur peut refuser ou fermer le navigateur")]
        public void Redirect_WithErrorOrWithoutCode_IsRefused_AndNothingIsStored(string suffix)
        {
            _inbox.Redirect = Redirect + suffix;
            var r = _gw.SignInWithProviderAsync(AuthProvider.Apple, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_oauth"));
            Assert.That(_http.Requests, Is.Empty);
            Assert.That(_tokens.Load(), Is.Null);
        }

        [Test, Description("Vérifie qu'un retour sur un autre lien que celui attendu est refusé — parce qu'une autre application ne doit pas pouvoir injecter un code")]
        public void Redirect_OnAnotherLink_IsRefused()
        {
            _inbox.Redirect = "autreapp://auth?code=abc";
            var r = _gw.SignInWithProviderAsync(AuthProvider.Google, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_oauth"));
            Assert.That(_http.Requests, Is.Empty);
        }

        [Test, Description("Vérifie que si le joueur ne revient jamais, la connexion rend « échouée ou annulée » après le délai et ne laisse aucune session — parce qu'un navigateur abandonné ne doit pas bloquer le jeu")]
        public void Redirect_Timeout_ReturnsOauthError()
        {
            var gw = Gateway(TimeSpan.FromMilliseconds(100));
            var r = gw.SignInWithProviderAsync(AuthProvider.Google, CancellationToken.None).AsTask().GetAwaiter().GetResult();
            Assert.That(r.Error!.LocKey, Is.EqualTo("errors.account_oauth"));
            Assert.That(_tokens.Load(), Is.Null);
        }

        [Test, Description("Vérifie que l'annulation par le joueur (jeton) est propagée comme une annulation, pas comme une erreur — parce que fermer la fenêtre Compte ne doit pas afficher d'erreur")]
        public void Redirect_CancelledByThePlayer_PropagatesCancellation()
        {
            using var cts = new CancellationTokenSource();
            var task = _gw.SignInWithProviderAsync(AuthProvider.Google, cts.Token).AsTask();
            cts.Cancel();
            Assert.That(() => task.GetAwaiter().GetResult(), Throws.InstanceOf<OperationCanceledException>());
        }

        [Test, Description("Vérifie qu'un échange de code refusé par le serveur ou injoignable rend « connexion échouée » sans session — parce que le code ne sert qu'une fois")]
        public void Exchange_Failure_ReturnsOauthOrNetworkError()
        {
            _inbox.Redirect = Redirect + "?code=abc";
            _http.Enqueue(400, "{\"error_code\":\"bad_code_verifier\",\"msg\":\"x\"}");
            var refused = _gw.SignInWithProviderAsync(AuthProvider.Google, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(refused.Error!.LocKey, Is.EqualTo("errors.account_oauth"));
            _http.EnqueueNetworkError();
            var offline = _gw.SignInWithProviderAsync(AuthProvider.Google, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(offline.Error!.LocKey, Is.EqualTo("errors.account_network"));
            Assert.That(_tokens.Load(), Is.Null);
        }

        [Test, Description("Vérifie qu'une seconde connexion par fournisseur lancée pendant que la première attend le retour est refusée sans ouvrir un second navigateur — parce que deux flux simultanés mélangeraient les retours")]
        public void SecondProviderSignIn_WhilePending_IsRejected()
        {
            using var cts = new CancellationTokenSource();
            var first = _gw.SignInWithProviderAsync(AuthProvider.Google, cts.Token).AsTask();
            var second = _gw.SignInWithProviderAsync(AuthProvider.Apple, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(second.IsFailure, Is.True);
            Assert.That(_browser.Opened.Count, Is.EqualTo(1));
            cts.Cancel();
            Assert.That(() => first.GetAwaiter().GetResult(), Throws.InstanceOf<OperationCanceledException>());
        }

        [Test, Description("Vérifie que demander le fournisseur « e-mail » par cette méthode est refusé sans ouvrir le navigateur — parce que l'e-mail passe par le mot de passe")]
        public void EmailProvider_IsRefused()
        {
            var r = _gw.SignInWithProviderAsync(AuthProvider.Email, CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsFailure, Is.True);
            Assert.That(_browser.Opened, Is.Empty);
        }
    }
}
