#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Parcours applicatif complet sans Unity : connexion, QG, zone, combat, sauvegarde.</summary>
    public sealed class ApplicationFlowTests
    {
        TestApp _app = null!;
        GameSession _session = null!;
        TestApp.MemoryProfiles _profiles = null!;
        TestApp.Clock _clock = null!;
        AuthService _auth = null!;
        HubService _hub = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _session = _app.Session;
            _profiles = _app.Profiles;
            _clock = _app.Time;
            var wallet = _app.Publisher<WalletChanged>();
            var roster = _app.Publisher<RosterChanged>();
            _auth = new AuthService(_session, _app.Auth, _profiles, _clock, _app.Save, wallet, roster, _app.Logger);
            _hub = new HubService(_session, _app.Save, wallet, roster);
        }

        [Test]
        public void SignIn_NewProfile_StarterTeamAndSave()
        {
            var r = _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(r.IsSuccess);
            Assert.IsTrue(r.Value.NewProfile);
            Assert.AreEqual(4, _session.Squad.Count);
            Assert.AreEqual(MetaRules.StartCredits, _session.Wallet.Credits);
            Assert.IsTrue(_profiles.Store.ContainsKey("p1"));
        }

        [Test]
        public void SignIn_ExistingProfile_RecoversFatigueOverTime()
        {
            _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            foreach (var m in _session.Roster) m.Fatigue = 50;
            _profiles.Store["p1"] = _session.ToProfile(_clock.UtcNow);
            _clock.UtcNow = _clock.UtcNow.AddMinutes(20);
            var r = _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsFalse(r.Value.NewProfile);
            Assert.IsTrue(_session.Roster.All(m => m.Fatigue == 30));
        }

        [Test]
        public void Hub_RecruitTrainSquadRules()
        {
            _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(_hub.Recruit(new MemberId("etincelle")).IsSuccess, "Agent à 150 PR");
            Assert.AreEqual(MetaRules.StartPr - 150, _session.Wallet.Pr);
            Assert.IsTrue(_hub.Recruit(new MemberId("arc")).IsFailure, "Élite trop chère");
            Assert.AreEqual(4, _session.Squad.Count, "escouade pleine : la recrue reste en réserve");
            Assert.IsTrue(_hub.ToggleSquad(new MemberId("etincelle")).IsFailure);
            Assert.IsTrue(_hub.ToggleSquad(new MemberId("seve")).IsSuccess);
            Assert.IsTrue(_hub.ToggleSquad(new MemberId("etincelle")).IsSuccess);
            int before = _session.Find(new MemberId("ancre"))!.Level;
            Assert.IsTrue(_hub.Train(new MemberId("ancre")).IsSuccess);
            Assert.AreEqual(before + 1, _session.Find(new MemberId("ancre"))!.Level);
        }

        [Test]
        public void Zone_Combat_Flow_AppliesRewardsAndFatigue()
        {
            _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.IsTrue(_app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult().IsSuccess);
            var run = _session.Run!;
            // Place l'équipe devant un combat facile pour tester le cycle combat.
            var forced = ZoneRun.ForTests(_session.Content, _session.RunZone!, 3UL, 1, true, "n02");
            _session.Run = forced;
            Assert.IsTrue(_app.Zone.Move.ExecuteAsync(new CaseId("n03")).GetAwaiter().GetResult().IsSuccess);
            Assert.AreEqual(ZoneRunPhase.InCombat, forced.Phase);

            var begin = _app.Combat.Begin.ExecuteAsync().GetAwaiter().GetResult();
            Assert.IsTrue(begin.IsSuccess);
            var sim = begin.Value;
            _app.Combat.SetAuto.ExecuteAsync(true).GetAwaiter().GetResult();
            sim.Step(20000);
            Assert.IsTrue(sim.IsOver);
            int credits = _session.Wallet.Credits;
            var report = _app.Combat.Finish.ExecuteAsync().GetAwaiter().GetResult().Value;
            Assert.IsTrue(report.Fatigue.All(f => f.After > f.Before));
            if (report.Victory)
            {
                Assert.AreEqual(credits + _session.Content.Balance.CreditsEasy, _session.Wallet.Credits);
                Assert.IsTrue(forced.IsSpent(new CaseId("n03")));
            }
            else Assert.AreEqual(new CaseId("n02"), forced.Position);
            var ok = _app.Combat.Verify.ExecuteAsync().GetAwaiter().GetResult();
            Assert.AreEqual(true, ok, "le rejeu doit redonner la même empreinte");
        }

        [Test]
        public void Zone_Abandon_ClearsRun()
        {
            _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            _app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            _app.Zone.Leave.ExecuteAsync().GetAwaiter().GetResult();
            Assert.IsNull(_session.Run);
        }
    }
}
