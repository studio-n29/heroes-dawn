using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Coût des compétences en énergie et régénération à chaque pas de la frise (notes d'amélioration du combat, 2026-10-22).</summary>
    public sealed class EnergyTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        static CombatSimulation Sim(string[] allies, string[] enemies, bool auto = false, ulong seed = 11)
            => new CombatSimulation(C, new CombatSetup(seed, CombatRank.Normal,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), auto));

        static void MakeReady(CombatUnit u) => u.Point = u.ActionPoint;
        static void Calm(CombatSimulation s) { foreach (var u in s.Units.Where(x => x.Side == Side.Enemy)) u.Point = 0; }

        [Test, Description("Vérifie que le coût d'une compétence vaut sa valeur explicite, sinon 10 par tour de recharge historique — parce que les compétences déjà écrites gardent un coût cohérent")]
        public void Cost_DefaultsFromTheLegacyCooldown()
        {
            SkillDef Make(int cd, int cost = -1) => new SkillDef(new SkillId("x"), SkillSlot.A2, cd, TargetRule.SingleEnemy, new[] { new EffectDef(EffectType.Damage, 100) }, cost);
            Assert.That(Make(0).Cost, Is.EqualTo(0), "compétence de base gratuite");
            Assert.That(Make(3).Cost, Is.EqualTo(30));
            Assert.That(Make(5).Cost, Is.EqualTo(50));
            Assert.That(Make(3, 15).Cost, Is.EqualTo(15), "coût explicite");
            Assert.That(Make(3, 0).Cost, Is.EqualTo(0), "un coût explicite de zéro reste gratuit");
        }

        [Test, Description("Vérifie que chaque unité commence le combat avec l'énergie de départ réglée dans l'équilibrage — parce que la 2e compétence doit être possible dès le premier tour mais pas la 3e")]
        public void Units_StartWithTheConfiguredEnergy()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            foreach (var u in s.Units) { Assert.That(u.Energy, Is.EqualTo(C.Balance.EnergyStart)); Assert.That(u.MaxEnergy, Is.EqualTo(C.Balance.EnergyMax)); }
        }

        [Test, Description("Vérifie que l'énergie gagne le montant réglé à chaque pas de la frise et ne dépasse jamais le maximum — parce que la régénération par tick est ce qui limite les compétences fortes")]
        public void Energy_RegeneratesEachTick_UpToTheMaximum()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Calm(s);
            var ally = s.Unit(0);
            ally.Point = 0;
            int before = ally.Energy;
            s.Step(1);
            Assert.That(ally.Energy, Is.EqualTo(before + C.Balance.EnergyRegenPerTick));
            ally.Energy = ally.MaxEnergy - 1;
            ally.Point = 0;
            s.Step(1);
            Assert.That(ally.Energy, Is.EqualTo(ally.MaxEnergy), "plafonnée");
        }

        [Test, Description("Vérifie qu'une compétence est refusée sans assez d'énergie, autorisée avec, et que l'usage retire son coût — parce que le coût remplace la recharge")]
        public void Skill_NeedsEnoughEnergy_AndSpendsIt()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Calm(s);
            var ally = s.Unit(0);
            MakeReady(ally);
            int cost = ally.Skills[2].Cost;
            Assert.That(cost, Is.GreaterThan(0));
            ally.Energy = cost - 1;
            Assert.That(s.CanUse(ally.Id, 2), Is.False, "énergie insuffisante");
            Assert.That(s.CanUse(ally.Id, 0), Is.True, "la compétence de base est gratuite");
            ally.Energy = cost + 7;
            Assert.That(s.CanUse(ally.Id, 2), Is.True);
            s.Submit(CombatCommand.UseSkill(ally.Id, 2, -1));
            s.Step(1);
            Assert.That(ally.Energy, Is.EqualTo(7 + C.Balance.EnergyRegenPerTick).Or.EqualTo(7), "coût retiré (+ régénération du pas éventuel)");
        }

        [Test, Description("Vérifie que l'énergie ne se régénère pas pendant la pause d'action (un allié prêt attend l'ordre) — parce que prendre son temps ne doit rien rapporter")]
        public void Energy_DoesNotRegenerateWhileWaitingForAnOrder()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Calm(s);
            var ally = s.Unit(0);
            MakeReady(ally);
            int before = ally.Energy;
            s.Step(5);
            Assert.That(ally.Energy, Is.EqualTo(before));
        }

        [Test, Description("Vérifie que l'IA n'utilise jamais une compétence sans en avoir l'énergie — parce que les ennemis suivent les mêmes règles que les alliés")]
        public void Ai_NeverCastsWithoutEnergy()
        {
            var s = Sim(new[] { "ancre", "givre" }, new[] { "brute", "thug" }, auto: true, seed: 5);
            int guard = 0;
            while (!s.IsOver && guard++ < 4000)
            {
                var energyBefore = s.Units.ToDictionary(u => u.Id, u => u.Energy);
                var events = s.Step(1);
                foreach (var e in events.OfType<SkillUsedEvent>())
                {
                    var u = s.Unit(e.Actor);
                    int cost = u.Skills.First(k => k.Id == e.Skill).Cost;
                    Assert.That(energyBefore[u.Id] + C.Balance.EnergyRegenPerTick, Is.GreaterThanOrEqualTo(cost), u.DefId + " " + e.Skill.Value);
                }
            }
            Assert.That(s.IsOver, Is.True, "le combat se termine (pas de blocage faute d'énergie)");
        }
    }
}
