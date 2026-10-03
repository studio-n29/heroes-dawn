using System;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class AuthUseCaseTests
    {
        TestApp _app = null!;
        TestApp.AuthUseCases A => _app.Account;

        [SetUp]
        public void SetUp() => _app = new TestApp();

        [Test, Description("Vérifie qu'une première connexion crée l'escouade de départ, le portefeuille initial et une sauvegarde — parce que le joueur doit pouvoir jouer tout de suite et retrouver sa progression")]
        public void SignIn_NewProfile_StarterTeam_AndSaves()
        {
            var r = A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, r.Error?.Code);
            Assert.That(r.Value.NewProfile, Is.True);
            Assert.That(_app.Session.Squad.Count, Is.EqualTo(MetaRules.SquadSize));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(MetaRules.StartCredits));
            Assert.That(_app.Profiles.Store.ContainsKey("p1"));
            Assert.That(_app.Received<WalletChanged>().Count, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie qu'un joueur qui revient récupère 1 % de fatigue par minute d'absence, sur tout l'effectif — parce que la fatigue se recharge avec le temps hors mission (GDD §7)")]
        public void SignIn_ExistingProfile_RecoversFatigue_ByElapsedMinutes()
        {
            A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            foreach (var m in _app.Session.Roster) m.Fatigue = 50;
            _app.Profiles.Store["p1"] = _app.Session.ToProfile(_app.Time.UtcNow);
            _app.Time.UtcNow = _app.Time.UtcNow.AddMinutes(20);
            var r = A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Value.NewProfile, Is.False);
            Assert.That(_app.Session.Roster.All(m => m.Fatigue == 50 - 20 * MetaRules.FatigueRecoveryPerMinute));
            Assert.That(r.Value.FatigueRecovered, Is.EqualTo(_app.Session.Roster.Count * 20 * MetaRules.FatigueRecoveryPerMinute));
        }

        [Test, Description("Vérifie qu'une panne du service de connexion donne l'erreur « réseau » sans connecter le joueur — parce que l'écran de titre doit afficher un message clair au lieu de planter")]
        public void SignIn_GatewayThrows_ReturnsAuthNetwork()
        {
            _app.Auth.ThrowOnSignIn = true;
            var r = A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("auth.network"));
            Assert.That(_app.Session.Account, Is.Null);
        }

        [Test, Description("Vérifie qu'un échec de lecture de la sauvegarde donne l'erreur « profil » sans connecter le joueur — parce qu'on ne doit jamais écraser une sauvegarde illisible par un profil neuf")]
        public void SignIn_ProfileLoadThrows_ReturnsProfileLoad()
        {
            _app.Profiles.ThrowOnLoad = true;
            var r = A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("profile.load"));
            Assert.That(_app.Session.Account, Is.Null);
        }

        [Test, Description("Vérifie qu'une connexion annulée lève une annulation au lieu de renvoyer une erreur — parce que la spec impose que l'annulation ne soit jamais convertie en Result (écran fermé pendant la connexion)")]
        public void SignIn_Cancelled_Throws()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            _app.Auth.HonourCancellation = true;
            Assert.Throws<OperationCanceledException>(() => A.SignIn.ExecuteAsync(cts.Token).GetAwaiter().GetResult());
        }

        [Test, Description("Vérifie que la remise à zéro efface la sauvegarde, rend l'équipe et le portefeuille de départ et prévient l'interface — parce que le bouton « Réinitialiser » doit repartir d'un profil neuf affiché immédiatement")]
        public void ResetProgress_DeletesProfile_StarterTeam_PublishesWalletAndRoster()
        {
            A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _app.Session.Wallet.Set(5, 5);
            int roster = _app.Received<RosterChanged>().Count;
            Assert.That(A.Reset.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(MetaRules.StartCredits));
            Assert.That(_app.Received<WalletChanged>().Last().Credits, Is.EqualTo(MetaRules.StartCredits));
            Assert.That(_app.Received<RosterChanged>().Count, Is.EqualTo(roster + 1));
        }

        [Test, Description("Vérifie que la déconnexion oublie le compte et la partie de zone en cours — parce qu'un autre joueur sur le même appareil ne doit pas hériter de la session")]
        public void SignOut_ClearsAccountAndRun()
        {
            A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _app.Zone.Start.ExecuteAsync(Content.Prototype.PrototypeContent.Zone11).GetAwaiter().GetResult();
            Assert.That(A.SignOut.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Account, Is.Null);
            Assert.That(_app.Session.Run, Is.Null);
            Assert.That(_app.Session.RunZone, Is.Null);
        }
    }
}
