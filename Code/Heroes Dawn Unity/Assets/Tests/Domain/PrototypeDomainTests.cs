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

        [Test]
        public void Content_IsConsistent()
        {
            var c = C;
            Assert.AreEqual(8, c.Members.Count);
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

        [Test]
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

        [Test]
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

        [Test]
        public void ZoneRun_SameSeed_SameDice()
        {
            var c = C;
            var a = new ZoneRun(c, c.Zone(PrototypeContent.Zone11), 99UL);
            var b = new ZoneRun(c, c.Zone(PrototypeContent.Zone11), 99UL);
            Assert.AreEqual(((DiceRolledEvent)a.Roll()[0]).Value, ((DiceRolledEvent)b.Roll()[0]).Value);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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
