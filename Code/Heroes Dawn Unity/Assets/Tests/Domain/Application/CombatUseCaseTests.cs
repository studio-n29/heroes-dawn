using System;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class CombatUseCaseTests
    {
        TestApp _app = null!;
        TestApp.CombatUseCases C => _app.Combat;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        /// <summary>Pion en n02, 1 PM : n03 est un combat Facile.</summary>
        CombatSimulation BeginEasyCombat()
        {
            _app.Place("n02", pm: 1);
            Assert.That(_app.Zone.Move.ExecuteAsync(new CaseId("n03")).GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Run!.Phase, Is.EqualTo(ZoneRunPhase.InCombat));
            var r = C.Begin.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.IsSuccess, r.Error?.Code);
            return r.Value;
        }

        static void RunToEnd(CombatSimulation sim)
        {
            for (int i = 0; i < 20000 && !sim.IsOver; i++) sim.Step(1);
            Assert.That(sim.IsOver);
        }

        [Test, Description("Vérifie qu'on ne peut pas lancer de combat hors d'une case Combat — parce que le combat doit toujours venir du plateau (erreur combat.none)")]
        public void Begin_WithoutPendingCombat_Fails()
        {
            var r = C.Begin.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("combat.none"));
        }

        [Test, Description("Vérifie que les ennemis ont le niveau moyen de l'escouade plus l'écart du rang (Facile −3) — parce que la difficulté s'aligne sur l'équipe (GDD §5, mise à niveau des ennemis)")]
        public void Begin_EnemyLevel_IsSquadAverage_PlusRankOffset()
        {
            BeginEasyCombat();
            var b = _app.Session.Content.Balance;
            int avg = (int)_app.Session.Squad.Average(m => m.Level);
            int expected = Math.Max(1, avg + b.RankEasyLevelOffset);
            Assert.That(C.Active.Setup!.Enemies.All(e => e.Level == expected));
            Assert.That(C.Active.Fighters.Count, Is.EqualTo(_app.Session.Squad.Count));
        }

        [Test, Description("Vérifie qu'une victoire Facile rapporte 200 Crédits, ajoute 12 % de fatigue et libère la case — parce que les récompenses et la fatigue par combat sont fixées par le GDD §6")]
        public void Finish_Victory_CreditsByRank_FatiguePerWin_AdvancesBoard()
        {
            var b = _app.Session.Content.Balance;
            var sim = BeginEasyCombat();
            int credits = _app.Session.Wallet.Credits;
            Assert.That(C.ForceWin.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            RunToEnd(sim);
            var report = C.Finish.ExecuteAsync().GetAwaiter().GetResult().Value;
            Assert.That(report.Victory, Is.True);
            Assert.That(report.Credits, Is.EqualTo(b.CreditsEasy));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + b.CreditsEasy));
            Assert.That(report.Fatigue.All(f => f.After - f.Before == b.FatiguePerWin));
            Assert.That(_app.Session.Run!.IsSpent(new CaseId("n03")));
        }

        [Test, Description("Vérifie qu'une défaite ne rapporte rien, ajoute 12 + 15 % de fatigue et fait reculer le pion — parce que la défaite = recul d'une case + fatigue accrue (GDD §6)")]
        public void Finish_Defeat_FatiguePerWinPlusLossExtra_PawnBack()
        {
            var b = _app.Session.Content.Balance;
            var sim = BeginEasyCombat();
            sim.Submit(CombatCommand.Retreat());
            RunToEnd(sim);
            var report = C.Finish.ExecuteAsync().GetAwaiter().GetResult().Value;
            Assert.That(report.Victory, Is.False);
            Assert.That(report.Credits, Is.EqualTo(0));
            Assert.That(report.Fatigue.All(f => f.After - f.Before == b.FatiguePerWin + b.FatiguePerLossExtra));
            Assert.That(_app.Session.Run!.Position, Is.EqualTo(new CaseId("n02")));
            Assert.That(report.ReturnedToHq, Is.False);
        }

        [Test, Description("Vérifie qu'une défaite qui épuise toute l'escouade ferme la zone et renvoie au QG — parce que l'équipe entière à 100 % de fatigue rentre au QG (GDD §6)")]
        public void Finish_DefeatWithSquadExhausted_LeavesZone_ReportToHq()
        {
            var b = _app.Session.Content.Balance;
            foreach (var m in _app.Session.Roster) m.Fatigue = 100 - b.FatiguePerWin - b.FatiguePerLossExtra;
            var sim = BeginEasyCombat();
            sim.Submit(CombatCommand.Retreat());
            RunToEnd(sim);
            var report = C.Finish.ExecuteAsync().GetAwaiter().GetResult().Value;
            Assert.That(report.ReturnedToHq, Is.True);
            Assert.That(_app.Session.Run, Is.Null);
        }

        [Test, Description("Vérifie que la fin de combat publie CombatFinished avec le rapport renvoyé et prévient l'effectif — parce que l'interface et les futurs écrans de résultats se mettent à jour par message")]
        public void Finish_PublishesCombatFinished_WithSameReport()
        {
            var sim = BeginEasyCombat();
            C.ForceWin.ExecuteAsync().GetAwaiter().GetResult();
            RunToEnd(sim);
            var report = C.Finish.ExecuteAsync().GetAwaiter().GetResult().Value;
            Assert.That(_app.Received<CombatFinished>().Single().Report, Is.SameAs(report));
            Assert.That(_app.Received<RosterChanged>().Count, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie qu'un combat gagné par le bouton debug n'est pas vérifié (null) — parce qu'il n'aurait aucun sens de signaler un faux « rejeu divergent » (Review Focus 4)")] // Review Focus 4
        public void Verify_AfterForceWin_ReturnsNull()
        {
            var sim = BeginEasyCombat();
            C.ForceWin.ExecuteAsync().GetAwaiter().GetResult();
            RunToEnd(sim);
            C.Finish.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(C.Verify.ExecuteAsync().GetAwaiter().GetResult(), Is.Null);
        }

        [Test, Description("Vérifie qu'un combat normal rejoué depuis sa graine et son journal donne la même empreinte — parce que la simulation est déterministe et vérifiable (doc 05)")]
        public void Verify_NormalAutoCombat_ReturnsTrue()
        {
            var sim = BeginEasyCombat();
            C.SetAuto.ExecuteAsync(true).GetAwaiter().GetResult();
            RunToEnd(sim);
            C.Finish.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(C.Verify.ExecuteAsync().GetAwaiter().GetResult(), Is.True);
        }

        [Test, Description("Vérifie que le mode auto est mémorisé dans le profil et appliqué au combat en cours — parce que le choix auto/manuel doit persister d'un combat à l'autre")]
        public void SetAuto_PersistsInSession_AndSubmitsToSimulation()
        {
            var sim = BeginEasyCombat();
            Assert.That(C.SetAuto.ExecuteAsync(true).GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.AutoCombat, Is.True);
            sim.Step(1);
            Assert.That(sim.Auto, Is.True);
            Assert.That(_app.Profiles.Saves, Is.GreaterThan(0));
        }
    }
}
