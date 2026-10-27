#nullable enable
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Infrastructure.Supabase
{
    /// <summary>PKCE (RFC 7636) : un vérificateur secret gardé par le jeu et son défi (SHA-256) envoyé dans l'adresse de connexion.</summary>
    public static class Pkce
    {
        /// <summary>Un vérificateur aléatoire (32 octets, 43 caractères base64url) et son défi.</summary>
        public static (string Verifier, string Challenge) Create(byte[]? entropy = null)
        {
            if (entropy == null)
            {
                entropy = new byte[32];
                using var rng = RandomNumberGenerator.Create();
                rng.GetBytes(entropy);
            }
            string verifier = Base64Url(entropy);
            return (verifier, ChallengeOf(verifier));
        }

        public static string ChallengeOf(string verifier)
        {
            using var sha = SHA256.Create();
            return Base64Url(sha.ComputeHash(Encoding.ASCII.GetBytes(verifier)));
        }

        static string Base64Url(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>Ouvre une adresse dans le navigateur du téléphone.</summary>
    public interface IBrowserOpener
    {
        void Open(string url);
    }

    /// <summary>Attend le retour dans l'app (lien profond) après la connexion dans le navigateur.</summary>
    public interface IAuthRedirectInbox
    {
        /// <summary>L'adresse reçue (qui commence par <paramref name="expectedPrefix"/>), ou <c>null</c> si l'attente est annulée.</summary>
        UniTask<string?> WaitForRedirectAsync(string expectedPrefix, CancellationToken ct);
    }

    public sealed class UnityBrowserOpener : IBrowserOpener
    {
        public void Open(string url) => UnityEngine.Application.OpenURL(url);
    }

    /// <summary>Écoute les liens profonds d'Unity (<c>deepLinkActivated</c>) et le lien qui a lancé l'app.</summary>
    public sealed class UnityRedirectInbox : IAuthRedirectInbox
    {
        public async UniTask<string?> WaitForRedirectAsync(string expectedPrefix, CancellationToken ct)
        {
            var source = new UniTaskCompletionSource<string?>();
            void OnLink(string url) { if (url.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)) source.TrySetResult(url); }
            UnityEngine.Application.deepLinkActivated += OnLink;
            using var registration = ct.Register(() => source.TrySetResult(null));
            try
            {
                string launch = UnityEngine.Application.absoluteURL;
                if (!string.IsNullOrEmpty(launch)) OnLink(launch);   // l'app a pu être relancée par le lien lui-même
                return await source.Task;
            }
            finally { UnityEngine.Application.deepLinkActivated -= OnLink; }
        }
    }

    /// <summary>Construction de l'adresse de connexion et lecture du retour.</summary>
    public static class OAuthFlow
    {
        public static string ProviderName(AuthProvider provider) => provider == AuthProvider.Google ? "google" : "apple";

        public static string AuthorizeUrl(SupabaseConfig config, AuthProvider provider, string challenge)
            => config.Url + "/auth/v1/authorize?provider=" + ProviderName(provider)
             + "&redirect_to=" + Uri.EscapeDataString(config.RedirectUrl)
             + "&code_challenge=" + challenge + "&code_challenge_method=S256";

        /// <summary>
        /// Le code du retour, ou <c>null</c> : adresse d'un autre lien que celui attendu, erreur renvoyée par le fournisseur (en paramètre ou en fragment),
        /// ou pas de code. Le code n'est utilisable qu'avec le vérificateur PKCE gardé par le jeu.
        /// </summary>
        public static string? CodeFrom(string url, string expectedPrefix)
        {
            if (!url.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)) return null;
            var values = new Dictionary<string, string>();
            foreach (var part in url.Substring(expectedPrefix.Length).TrimStart('?', '#').Split(new[] { '&', '#', '?' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int eq = part.IndexOf('=');
                if (eq <= 0) continue;
                values[part.Substring(0, eq)] = Uri.UnescapeDataString(part.Substring(eq + 1).Replace('+', ' '));
            }
            if (values.ContainsKey("error") || values.ContainsKey("error_code")) return null;
            return values.TryGetValue("code", out var code) && code.Length > 0 ? code : null;
        }
    }
}
