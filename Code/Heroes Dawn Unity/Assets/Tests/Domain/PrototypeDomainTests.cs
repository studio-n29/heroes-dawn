using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    public sealed class PrototypeDomainTests
    {
        static ContentCatalog C => PrototypeContent.Build();

        [Test, Description("Vérifie que le contenu du prototype est cohérent : 30 agents, zone de 25 cases et 3 objectifs, liens valides, 3 compétences par recrue — parce qu'une donnée incohérente casserait le jeu à l'exécution")]
        public void Content_IsConsistent()
        {
            var c = C;
            Assert.AreEqual(30, c.Members.Count);
            var zone = c.Zone(PrototypeContent.Zone11);
            Assert.AreEqual(25, zone.Nodes.Count);
            Assert.AreEqual(3, zone.Objectives.Count);
            foreach (var l in zone.Links)
            {
                Assert.IsTrue(zone.Nodes.Any(n => n.Id == l.A), l.A.Value);
                Assert.IsTrue(zone.Nodes.Any(n => n.Id == l.B), l.B.Value);
            }
            foreach (var m in c.Members) Assert.AreEqual(3, m.Skills.Count, m.Id.Value);
        }

        [Test, Description("Vérifie que 6 PM non utilisés donnent un dé bonus et que la réserve plafonne à 2 dés — parce que c'est la règle du dé bonus (GDD §5)")]
        public void DiceBank_ConvertsSixPmIntoBonusDie_AndCapsAtTwo()
        {
            var bank = new DiceBank(new BalanceConfig());
            bank.AddUnusedPm(4);
            Assert.AreEqual(0, bank.BonusDice);
            bank.AddUnusedPm(3);
            Assert.AreEqual(1, bank.BonusDice);
            Assert.AreEqual(1, bank.StoredPm);
            bank.AddUnusedPm(20);
            Assert.AreEqual(2, bank.BonusDice);
            Assert.AreEqual(0, bank.StoredPm);
        }

        [Test, Description("Vérifie qu'un lancer puis un déplacement font avancer le pion et sont journalisés — parce que la sauvegarde et le rejeu de zone reposent sur ce journal")]
        public void ZoneRun_RollThenMove_RevealsFogAndLogsActions()
        {
            var c = C;
            var run = new ZoneRun(c, c.Zone(PrototypeContent.Zone11), 1234UL);
            Assert.AreEqual(ZoneRunPhase.AwaitingRoll, run.Phase);
            var ev = run.Roll();
            Assert.IsInstanceOf<DiceRolledEvent>(ev[0]);
            Assert.AreEqual(ZoneRunPhase.ChoosingDestination, run.Phase);
            Assert.IsNotEmpty(run.Destinations);
            var target = run.Destinations.Keys.First();
            run.MoveTo(target);
            Assert.AreNotEqual(new CaseId("n00"), run.Position);
            Assert.AreEqual(2, run.Log.Count);
        }

        [Test, Description("Vérifie que deux parties avec la même graine tirent les mêmes dés — parce que le plateau doit être déterministe (doc 05)")]
        public void ZoneRun_SameSeed_SameDice()
        {
            var c = C;
            var a = new ZoneRun(c, c.Zone(PrototypeContent.Zone11), 99UL);
            var b = new ZoneRun(c, c.Zone(PrototypeContent.Zone11), 99UL);
            Assert.AreEqual(((DiceRolledEvent)a.Roll()[0]).Value, ((DiceRolledEvent)b.Roll()[0]).Value);
        }

        [Test, Description("Vérifie que remplir un seul objectif ne valide pas la zone — parce qu'une zone n'est validée qu'avec ses 3 objectifs (GDD §5)")]
        public void ZoneRun_Validation_RequiresAllObjectives()
        {
            var c = C;
            var run = ZoneRun.ForTests(c, c.Zone(PrototypeContent.Zone11), 5UL, 1, true, "n22");
            var ev = run.MoveTo(new CaseId("n23"));
            Assert.AreEqual(ZoneRunPhase.InCombat, run.Phase);
            ev = run.ResolveCombat(true);
            Assert.IsTrue(ev.OfType<ObjectiveCompletedEvent>().Any());
            Assert.IsFalse(ev.OfType<ZoneValidatedEvent>().Any());
            Assert.AreNotEqual(ZoneRunPhase.Completed, run.Phase);
        }

        [Test, Description("Vérifie qu'une défaite ramène le pion sur la case précédente — parce que la défaite = recul d'une case (GDD §6)")]
        public void ZoneRun_CombatDefeat_StepsBack()
        {
            var c = C;
            var run = ZoneRun.ForTests(c, c.Zone(PrototypeContent.Zone11), 5UL, 1, true, "n22");
            run.MoveTo(new CaseId("n23"));
            var ev = run.ResolveCombat(false);
            Assert.AreEqual(new CaseId("n22"), run.Position);
            Assert.IsTrue(ev.OfType<SteppedBackEvent>().Any());
        }

        static CombatSetup Setup(ulong seed, bool auto) => new CombatSetup(seed, CombatRank.Normal,
            new[] { new AllyEntry(new MemberId("ancre"), 5, 0), new AllyEntry(new MemberId("givre"), 5, 0),
                    new AllyEntry(new MemberId("echo"), 5, 0), new AllyEntry(new MemberId("seve"), 5, 20) },
            new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("thug"), 5), new EnemyEntry(new EnemyId("street_pyro"), 5) },
            auto);

        [Test, Description("Vérifie qu'un combat auto se termine et donne exactement le même résultat avec la même graine — parce que le combat doit être rejouable à l'identique (doc 05)")]
        public void Combat_AutoBattle_TerminatesAndIsDeterministic()
        {
            var c = C;
            var a = new CombatSimulation(c, Setup(42UL, true));
            var b = new CombatSimulation(c, Setup(42UL, true));
            a.Step(20000);
            b.Step(20000);
            Assert.IsTrue(a.IsOver, "le combat auto doit se terminer");
            Assert.AreEqual(a.StateHash(), b.StateHash());
            Assert.AreEqual(a.Tick, b.Tick);
        }

        [Test, Description("Vérifie qu'un combat joué à la main se rejoue à l'identique depuis son journal d'entrées — parce que la vérification et les tests de rejeu reposent sur ce journal")]
        public void Combat_ManualInputs_ReplayToSameHash()
        {
            var c = C;
            var setup = Setup(7UL, false);
            var sim = new CombatSimulation(c, setup);
            int guard = 0;
            while (!sim.IsOver && guard++ < 20000)
            {
                foreach (var u in sim.Units)
                    if (u.Side == Side.Ally && u.IsReady)
                    {
                        int slot = sim.CanUse(u.Id, 2) ? 2 : sim.CanUse(u.Id, 1) ? 1 : 0;
                        sim.Submit(CombatCommand.UseSkill(u.Id, slot, -1));
                    }
                if (sim.Tick == 300) sim.Submit(CombatCommand.SetAuto(true));
                sim.Step(1);
            }
            Assert.IsTrue(sim.IsOver);
            var replay = CombatReplay.Run(c, setup, sim.InputLog);
            Assert.AreEqual(sim.StateHash(), replay.StateHash);
        }

        [Test, Description("Vérifie que la retraite termine le combat en défaite — parce que le bouton Retraite = défaite + recul (W11a)")]
        public void Combat_Retreat_EndsInDefeat()
        {
            var sim = new CombatSimulation(C, Setup(3UL, false));
            sim.Submit(CombatCommand.Retreat());
            sim.Step(1);
            Assert.IsTrue(sim.IsOver);
            Assert.AreEqual(false, sim.Victory);
        }
    }
}
