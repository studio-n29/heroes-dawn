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

        [Test]
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

        [Test]
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

        [Test]
        public void SignIn_GatewayThrows_ReturnsAuthNetwork()
        {
            _app.Auth.ThrowOnSignIn = true;
            var r = A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("auth.network"));
            Assert.That(_app.Session.Account, Is.Null);
        }

        [Test]
        public void SignIn_ProfileLoadThrows_ReturnsProfileLoad()
        {
            _app.Profiles.ThrowOnLoad = true;
            var r = A.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("profile.load"));
            Assert.That(_app.Session.Account, Is.Null);
        }

        [Test]
        public void SignIn_Cancelled_Throws()
        {
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            _app.Auth.HonourCancellation = true;
            Assert.Throws<OperationCanceledException>(() => A.SignIn.ExecuteAsync(cts.Token).GetAwaiter().GetResult());
        }

        [Test]
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

        [Test]
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
