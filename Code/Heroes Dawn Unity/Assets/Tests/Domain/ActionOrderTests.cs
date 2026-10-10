using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Manipulation de l'ordre d'action : reculs d'ennemis, propulsion d'alliés, vitesse de la frise (plan 2026-10-10, sous-projets J et K).</summary>
    public sealed class ActionOrderTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static CombatSimulation Sim(string[] allies, string[] enemies, ContentCatalog? catalog = null, ulong seed = 11)
            => new CombatSimulation(catalog ?? C, new CombatSetup(seed, CombatRank.Normal,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));

        static int Pct(CombatUnit u) => (int)(u.Gauge * 100 / u.GaugeMax);
        static void At(CombatUnit u, int percent) => u.Gauge = u.GaugeMax * percent / 100;
        static void Calm(CombatSimulation s) { foreach (var u in s.Units.Where(x => x.Side == Side.Enemy)) At(u, 0); }

        static System.Collections.Generic.IReadOnlyList<CombatEvent> Act(CombatSimulation s, CombatUnit ally, int slot, int target)
        {
            At(ally, 100);
            s.Submit(CombatCommand.UseSkill(ally.Id, slot, target));
            return s.Step(1);
        }

        [Test, Description("Vérifie qu'une attaque d'Impact (Anchor, A1) repousse l'ennemi touché de 15 % sur la frise — parce que certaines attaques retardent l'action adverse")]
        public void Knockback_PushesTheEnemyBackOnTheTimeline()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Calm(s);
            var brute = s.Unit(1);
            At(brute, 50);
            var events = Act(s, s.Unit(0), 0, brute.Id);
            Assert.That(events.OfType<PushedBackEvent>().Single().Percent, Is.EqualTo(15));
            Assert.That(Pct(brute), Is.EqualTo(35));
        }

        [Test, Description("Vérifie qu'un recul annule l'action annoncée par l'ennemi quand il le renvoie sous la zone de préparation — parce que repousser un ennemi doit pouvoir lui faire perdre son grand coup")]
        public void Knockback_CancelsAnAnnouncedAction()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Calm(s);
            var brute = s.Unit(1);
            At(brute, 90);
            brute.PlannedSlot = 0;
            brute.PlannedTarget = 0;
            var events = Act(s, s.Unit(0), 0, brute.Id);
            Assert.That(events.OfType<ActionInterruptedEvent>().Any(e => e.Unit == brute.Id), Is.True);
            Assert.That(brute.PlannedSlot, Is.EqualTo(-1));
        }

        [Test, Description("Vérifie qu'un ennemi déjà repoussé ne l'est pas une seconde fois avant sa prochaine action — parce qu'on ne doit pas pouvoir le bloquer indéfiniment")]
        public void Knockback_CannotBeChained_UntilTheTargetActs()
        {
            var s = Sim(new[] { "ancre", "rempart" }, new[] { "brute" });
            Calm(s);
            var brute = s.Unit(2);
            At(brute, 60);
            Act(s, s.Unit(0), 0, brute.Id);
            Assert.That(Pct(brute), Is.EqualTo(45));
            var again = Act(s, s.Unit(1), 0, brute.Id);
            Assert.That(again.OfType<PushedBackEvent>(), Is.Empty, "pas de second recul");
            Assert.That(Pct(brute), Is.EqualTo(45));
            brute.KnockbackGuard = false; // la cible a agi entre-temps
            s.Unit(0).Cooldowns[0] = 0;
            At(brute, 60);
            Assert.That(Act(s, s.Unit(0), 0, brute.Id).OfType<PushedBackEvent>().Count(), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un ennemi sous Ancrage n'est pas repoussé — parce que l'Ancrage protège de tout déplacement sur la frise")]
        public void Knockback_IgnoresAnchoredEnemies()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Calm(s);
            var brute = s.Unit(1);
            At(brute, 50);
            brute.Statuses.Add(new StatusInstance(StatusType.Anchor, 2, 0));
            var events = Act(s, s.Unit(0), 0, brute.Id);
            Assert.That(events.OfType<PushedBackEvent>(), Is.Empty);
            Assert.That(Pct(brute), Is.EqualTo(50));
        }

        [Test, Description("Vérifie qu'une compétence d'Élan fait avancer l'allié sans le rendre prêt tant qu'il n'atteint pas 100 % — parce que la propulsion n'est pas un tour gratuit")]
        public void Advance_MovesAnAllyTowardsTheActionPoint()
        {
            var s = Sim(new[] { "etincelle", "ancre" }, new[] { "brute" });
            Calm(s);
            var target = s.Unit(1);
            At(target, 20);
            var events = Act(s, s.Unit(0), 1, target.Id);
            var adv = events.OfType<AdvancedEvent>().Single();
            Assert.That(adv.Ready, Is.False);
            Assert.That(Pct(target), Is.EqualTo(80));
            Assert.That(target.IsReady, Is.False);
        }

        [Test, Description("Vérifie qu'un Élan qui atteint 100 % rend l'allié prêt aussitôt et fige la frise — parce que la propulsion court-circuite l'action ennemie")]
        public void Advance_ToTheActionPoint_ReadiesTheAllyAndFreezesTheTimeline()
        {
            var s = Sim(new[] { "etincelle", "ancre" }, new[] { "brute" });
            Calm(s);
            var ancre = s.Unit(1);
            var brute = s.Unit(2);
            At(ancre, 50);
            At(brute, 95);
            brute.PlannedSlot = 0;
            brute.PlannedTarget = 0;
            var events = Act(s, s.Unit(0), 1, ancre.Id);
            Assert.That(events.OfType<AdvancedEvent>().Single().Ready, Is.True);
            Assert.That(events.OfType<GaugeFullEvent>().Any(e => e.Unit == ancre.Id), Is.True);
            Assert.That(ancre.IsReady, Is.True);
            Assert.That(s.IsAwaitingAction, Is.True);
            var later = s.Step(60);
            Assert.That(later.OfType<SkillUsedEvent>().Any(e => e.Actor == brute.Id), Is.False, "l'ennemi n'agit pas avant l'allié propulsé");
            Assert.That(Pct(brute), Is.EqualTo(95), "la frise est figée");
        }

        [Test, Description("Vérifie qu'un Élan sur un allié déjà prêt ou sur le lanceur lui-même ne fait rien — parce qu'on ne propulse pas qui est déjà à l'action")]
        public void Advance_IgnoresReadyUnitsAndTheCaster()
        {
            var s = Sim(new[] { "etincelle", "ancre" }, new[] { "brute" });
            Calm(s);
            var ancre = s.Unit(1);
            At(ancre, 100);
            var events = Act(s, s.Unit(0), 1, ancre.Id);
            Assert.That(events.OfType<AdvancedEvent>(), Is.Empty);
        }

        [Test, Description("Vérifie que la vitesse de la frise réglable ralentit le remplissage des jauges (70 % par défaut) — parce que le joueur doit avoir plus de temps de réflexion")]
        public void TimelineSpeed_ScalesTheGaugeFill()
        {
            var normal = Sim(new[] { "ancre" }, new[] { "brute" }, PrototypeContent.Build(new BalanceConfig { TimelineSpeedPercent = 100 }));
            var slow = Sim(new[] { "ancre" }, new[] { "brute" });
            long full = normal.FillPerTick(normal.Unit(0));
            long reduced = slow.FillPerTick(slow.Unit(0));
            Assert.That(new BalanceConfig().TimelineSpeedPercent, Is.EqualTo(70));
            Assert.That(reduced, Is.EqualTo(full * 70 / 100));
        }

        [Test, Description("Vérifie que la même graine donne le même combat avec reculs et propulsions — parce que la simulation doit rester déterministe")]
        public void OrderEffects_AreDeterministic()
        {
            string Run()
            {
                var s = Sim(new[] { "etincelle", "ancre" }, new[] { "brute" });
                Calm(s);
                var log = new System.Text.StringBuilder();
                At(s.Unit(1), 40);
                foreach (var e in Act(s, s.Unit(0), 1, 1)) log.Append(e.GetType().Name).Append(e.Tick).Append(';');
                foreach (var e in Act(s, s.Unit(1), 0, 2)) log.Append(e.GetType().Name).Append(e.Tick).Append(';');
                return log.ToString() + s.Unit(2).Gauge;
            }
            Assert.That(Run(), Is.EqualTo(Run()));
        }
    }
}
