using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Active Timeline : pause globale au point d'action, temps actif, catégories, vagues, compétences d'avatar (spec 2026-10-07).</summary>
    public sealed class ActiveTimelineTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static CombatSimulation Sim(string[] allies, string[] enemies, CombatRank rank = CombatRank.Normal, ulong seed = 11)
            => new CombatSimulation(C, new CombatSetup(seed, rank,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));

        static void At(CombatUnit u, int percent) => u.Point = (int)System.Math.Round(percent * u.ActionPoint / 100.0);
        static void Calm(CombatSimulation s) { foreach (var u in s.Units.Where(x => x.Side == Side.Enemy)) At(u, 0); }

        [Test, Description("Vérifie que lorsqu'un allié arrive au point d'action, plus aucune unité n'avance ni ne perd de recharge, et que le temps actif s'arrête — parce que le monde attend que l'action soit jouée")]
        public void WhenAllyIsReady_EverythingIsFrozen()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            Calm(s);
            var givre = s.Unit(0); var ancre = s.Unit(1); var brute = s.Unit(2);
            At(ancre, 50); At(brute, 50);
            At(givre, 100);
            int ancreBefore = ancre.Point, bruteBefore = brute.Point;
            int active = s.ActiveTick;
            s.Step(60);
            Assert.That(s.IsAwaitingAction, Is.True);
            Assert.That(ancre.Point, Is.EqualTo(ancreBefore));
            Assert.That(brute.Point, Is.EqualTo(bruteBefore));
            Assert.That(s.ActiveTick, Is.EqualTo(active));
        }

        [Test, Description("Vérifie que la frise repart dès que l'action du joueur est jouée — parce que la pause ne dure que le temps de la décision")]
        public void AfterTheAction_TimeFlowsAgain()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            Calm(s);
            var givre = s.Unit(0); var ancre = s.Unit(1);
            At(ancre, 10);
            At(givre, 100);
            s.Submit(CombatCommand.UseSkill(givre.Id, 0, 2));
            s.Step(1);
            Assert.That(s.IsAwaitingAction, Is.False);
            int before = ancre.Point;
            s.Step(10);
            Assert.That(ancre.Point, Is.GreaterThan(before));
            Assert.That(s.ActiveTick, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie qu'en mode auto la frise ne se fige jamais — parce que l'IA agit tout de suite")]
        public void InAutoMode_NoPause()
        {
            var s = new CombatSimulation(C, new CombatSetup(5UL, CombatRank.Normal,
                new[] { new AllyEntry(new MemberId("givre"), 5, 0) }, new[] { new EnemyEntry(new EnemyId("brute"), 5) }, true));
            s.Step(200);
            Assert.That(s.IsAwaitingAction, Is.False);
            Assert.That(s.ActiveTick, Is.EqualTo(s.Tick));
        }

        [Test, Description("Vérifie qu'un combat en deux vagues fait apparaître la seconde quand la première est vaincue, et ne se termine qu'à la fin de la dernière — parce que les combats peuvent être découpés en vagues")]
        public void Waves_SpawnNextWave_VictoryOnlyAfterLast()
        {
            var setup = new CombatSetup(3UL, CombatRank.Normal,
                new[] { new AllyEntry(new MemberId("ancre"), 5, 0) },
                new IReadOnlyList<EnemyEntry>[]
                {
                    new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5) },
                    new[] { new EnemyEntry(new EnemyId("brute"), 5) },
                }, false);
            var s = new CombatSimulation(C, setup);
            Assert.That(s.WaveCount, Is.EqualTo(2));
            Assert.That(s.Units.Count(u => u.Side == Side.Enemy), Is.EqualTo(2));
            foreach (var e in s.Units.Where(u => u.Side == Side.Enemy).ToList()) s.ApplyDamageForTests(e, 1_000_000);
            var ev = s.Step(1);
            Assert.That(ev.OfType<WaveStartedEvent>().Single().Wave, Is.EqualTo(1));
            Assert.That(s.IsOver, Is.False);
            Assert.That(s.Units.Count(u => u.Side == Side.Enemy && u.IsAlive), Is.EqualTo(1));
            foreach (var e in s.Units.Where(u => u.Side == Side.Enemy && u.IsAlive).ToList()) s.ApplyDamageForTests(e, 1_000_000);
            s.Step(1);
            Assert.That(s.Victory, Is.EqualTo(true));
        }
    }
}
