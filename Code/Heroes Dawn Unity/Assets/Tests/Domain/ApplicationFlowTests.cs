#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using N29.HeroesDawn.Application.Events;
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
        sealed class MemoryProfiles : IProfileGateway
        {
            public readonly Dictionary<string, ProfileData> Store = new Dictionary<string, ProfileData>();
            public int Saves;
            public Task<ProfileData?> LoadAsync(string id, CancellationToken ct)
                => Task.FromResult(Store.TryGetValue(id, out var p) ? p : (ProfileData?)null);
            public Task SaveAsync(string id, ProfileData data, CancellationToken ct) { Store[id] = data; Saves++; return Task.CompletedTask; }
            public void Delete(string id) => Store.Remove(id);
        }

        sealed class FakeAuth : IAuthGateway
        {
            public Task<Result<PlayerAccount>> SignInAsGuestAsync(CancellationToken ct)
                => Task.FromResult(Result<PlayerAccount>.Ok(new PlayerAccount("p1", "Invité-TEST")));
            public void SignOut() { }
        }

        sealed class Seeds : ISeedProvider { ulong _n = 11; public ulong NextSeed(string p) => _n += 7919; }
        sealed class Clock : IClock { public DateTime UtcNow { get; set; } = new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc); }
        sealed class Log : ILogger { public void Info(string m) { } public void Warn(string m) { } public void Error(string m) { } }
        sealed class Verifier : ICombatVerifier
        {
            public Task<bool> VerifyAsync(CombatSetup s, IReadOnlyList<CombatInput> i, ulong h, CancellationToken ct)
                => Task.FromResult(CombatReplay.Run(PrototypeContent.Build(), s, i).StateHash == h);
        }

        GameSession _session = null!;
        MemoryProfiles _profiles = null!;
        Clock _clock = null!;
        AuthService _auth = null!;
        HubService _hub = null!;
        ZoneService _zones = null!;
        CombatService _combat = null!;

        [SetUp]
        public void SetUp()
        {
            var content = PrototypeContent.Build();
            _session = new GameSession(content);
            _profiles = new MemoryProfiles();
            _clock = new Clock();
            var events = new GameEvents();
            var log = new Log();
            var seeds = new Seeds();
            var save = new SaveService(_session, _profiles, _clock, log, events);
            _auth = new AuthService(_session, new FakeAuth(), _profiles, _clock, save, events, log);
            _hub = new HubService(_session, save, events);
            _zones = new ZoneService(_session, seeds, save, events, log);
            _combat = new CombatService(_session, seeds, _zones, save, events, new Verifier(), log);
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
            Assert.IsTrue(_zones.StartZone(PrototypeContent.Zone11).IsSuccess);
            var run = _session.Run!;
            // Place l'équipe devant un combat facile pour tester le cycle combat.
            var forced = ZoneRun.ForTests(_session.Content, _session.RunZone!, 3UL, 1, true, "n02");
            _session.Run = forced;
            Assert.IsTrue(_zones.MoveTo(new CaseId("n03")).IsSuccess);
            Assert.AreEqual(ZoneRunPhase.InCombat, forced.Phase);

            var begin = _combat.Begin();
            Assert.IsTrue(begin.IsSuccess);
            var sim = begin.Value;
            _combat.SetAuto(true);
            sim.Step(20000);
            Assert.IsTrue(sim.IsOver);
            int credits = _session.Wallet.Credits;
            var report = _combat.Finish().Value;
            Assert.IsTrue(report.Fatigue.All(f => f.After > f.Before));
            if (report.Victory)
            {
                Assert.AreEqual(credits + _session.Content.Balance.CreditsEasy, _session.Wallet.Credits);
                Assert.IsTrue(forced.IsSpent(new CaseId("n03")));
            }
            else Assert.AreEqual(new CaseId("n02"), forced.Position);
            var ok = _combat.VerifyLastAsync(CancellationToken.None).GetAwaiter().GetResult();
            Assert.AreEqual(true, ok, "le rejeu doit redonner la même empreinte");
        }

        [Test]
        public void Zone_Abandon_ClearsRun()
        {
            _auth.SignInAsGuestAsync(CancellationToken.None).GetAwaiter().GetResult();
            _zones.StartZone(PrototypeContent.Zone11);
            _zones.Abandon();
            Assert.IsNull(_session.Run);
        }
    }
}
