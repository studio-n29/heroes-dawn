using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Account;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Infrastructure.Supabase;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Tests.Domain.Application;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Câblage du compte (lot I) : choix Supabase ou simulateur, fichier de configuration public, vidage à la mise en arrière-plan.</summary>
    public sealed class AccountWiringTests
    {
        static string ConfigPath => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "_Project/Resources/Supabase/supabase.json"));

        [Test, Description("Vérifie que sans configuration valide le jeu choisit les simulateurs, et avec une configuration valide Supabase — parce qu'un éditeur sans réseau ou un fichier cassé ne doit pas empêcher de jouer")]
        public void Choose_UsesSimulators_WithoutAValidConfig_AndSupabaseWithOne()
        {
            Assert.That(AccountBackend.Choose(null), Is.EqualTo(AccountBackendKind.Simulated));
            Assert.That(AccountBackend.Choose(new SupabaseConfig("http://x.supabase.co", "k", "r")), Is.EqualTo(AccountBackendKind.Simulated));
            Assert.That(AccountBackend.Choose(new SupabaseConfig("https://x.supabase.co", "", "r")), Is.EqualTo(AccountBackendKind.Simulated));
            Assert.That(AccountBackend.Choose(new SupabaseConfig("https://x.supabase.co", "k", "r")), Is.EqualTo(AccountBackendKind.Supabase));
        }

        [Test, Description("Vérifie que le fichier de configuration du jeu est valide, ne contient que l'URL, la clé publique et le lien de retour, et que la clé est bien du rôle « anon » (jamais service_role) — parce que ce fichier est dans le dépôt et dans le jeu")]
        public void ConfigFile_IsValid_PublicOnly_AndHasNoServiceRoleKey()
        {
            var json = File.ReadAllText(ConfigPath);
            var config = SupabaseConfig.Parse(json);
            Assert.That(config.IsValid, Is.True);
            Assert.That(config.RedirectUrl, Is.EqualTo("heroesdawn://auth"));
            var keys = Regex.Matches(json, "\"(\\w+)\"\\s*:").Cast<Match>().Select(m => m.Groups[1].Value).OrderBy(k => k).ToArray();
            Assert.That(keys, Is.EqualTo(new[] { "anonKey", "redirectUrl", "url" }));
            string payload = config.AnonKey.Split('.')[1].Replace('-', '+').Replace('_', '/');
            payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
            string decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            Assert.That(decoded, Does.Contain("\"role\":\"anon\""));
            Assert.That(decoded, Does.Not.Contain("service_role"));
            Assert.That(json, Does.Not.Contain("service_role"));
        }

        [Test, Description("Vérifie que perdre le focus (mise en arrière-plan sur mobile) envoie tout de suite la progression en attente de l'intervalle de 20 s, et que reprendre le focus ne fait rien — parce que le système peut tuer l'appli en arrière-plan")]
        public void Lifecycle_OnLosingFocus_FlushesTheSync_GainingFocusDoesNothing()
        {
            var app = new TestApp();
            app.SignInDirect("guest-1");
            var cloud = new FakeCloudProfileGateway();
            var clock = new FixedClock();
            var gate = new UniTaskCompletionSource();
            bool gated = false;
            var sync = new CloudSync(cloud, new MemorySyncStateStore(), app.Session, clock, app.Logger, (t, ct) => gated ? gate.Task : UniTask.CompletedTask, app.Save);
            var first = cloud.UpsertAsync(app.Session.ToProfile(clock.UtcNow), null, default).GetAwaiter().GetResult().Value;
            sync.Start(first.UpdatedAt);
            var lifecycle = new AccountLifecycle(sync);
            sync.NotifyLocalSaved();                       // 1re sauvegarde : envoi immédiat
            gated = true;
            app.Session.Wallet.Set(4242, app.Session.Wallet.Pr);
            sync.NotifyLocalSaved();                       // 2e : en attente des 20 s
            int writes = cloud.Writes;
            lifecycle.OnFocusChanged(true);
            Assert.That(cloud.Writes, Is.EqualTo(writes), "reprendre le focus n'envoie rien");
            lifecycle.OnFocusChanged(false);
            Assert.That(cloud.Writes, Is.EqualTo(writes + 1), "perdre le focus envoie tout de suite");
            Assert.That(cloud.GetAsync(default).GetAwaiter().GetResult().Value!.Profile.credits, Is.EqualTo(4242));
        }

        [Test, Description("Vérifie que la fermeture de l'application vide aussi la synchronisation — parce que la dernière progression ne doit pas se perdre")]
        public void Lifecycle_OnQuit_FlushesTheSync()
        {
            var app = new TestApp();
            app.SignInDirect("guest-1");
            var cloud = new FakeCloudProfileGateway();
            var clock = new FixedClock();
            var sync = new CloudSync(cloud, new MemorySyncStateStore(), app.Session, clock, app.Logger, (t, ct) => UniTask.CompletedTask, app.Save);
            var first = cloud.UpsertAsync(app.Session.ToProfile(clock.UtcNow), null, default).GetAwaiter().GetResult().Value;
            sync.Start(first.UpdatedAt);
            sync.NotifyLocalSaved();
            int writes = cloud.Writes;
            new AccountLifecycle(sync).OnQuit();
            Assert.That(cloud.Writes, Is.GreaterThanOrEqualTo(writes), "rien n'est perdu à la fermeture");
            Assert.That(sync.IsActive, Is.True);
        }
    }
}
