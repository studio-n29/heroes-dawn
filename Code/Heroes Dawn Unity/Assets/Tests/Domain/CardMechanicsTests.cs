#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Mécaniques des paliers de cartes (lot C1) : vol de vie, exécution, purge, rebond, jouées par une carte de test en vrai combat.</summary>
    public sealed class CardMechanicsTests
    {
        /// <summary>
        /// Combat où « verre » (unité 0) lance aussitôt une carte de test Matière (coût 0) faite de ces effets sur <paramref name="targetUnit"/> ;
        /// les ennemis suivent (unités 1, 2…). <paramref name="before"/> prépare les unités avant le lancer.
        /// </summary>
        static CombatSimulation Cast(TargetRule target, EffectDef[] effects, string[] enemies, out List<CombatEvent> events,
                                     Action<CombatSimulation>? before = null, int targetUnit = 1, ulong seed = 11)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var card = new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, ItemRarity.Common,
                                        new SkillDef(new SkillId("card_test"), SkillSlot.A3, 0, target, effects, 0));
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var ally = new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { new CardId("card_test") });
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, new[] { ally },
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));
            foreach (var u in sim.Units) u.Point = 0;
            before?.Invoke(sim);
            var verre = sim.Unit(0);
            verre.Point = verre.ActionPoint;
            verre.Energy = verre.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(verre.Id, 2, targetUnit));
            events = sim.Step(1).ToList();
            return sim;
        }

        static EffectDef Fx(EffectType t, int v, StatusType s = StatusType.None, int d = 0) => new EffectDef(t, v, s, d);

        static IEnumerable<HealEvent> HealsOn(IEnumerable<CombatEvent> e, int unit) => e.OfType<HealEvent>().Where(h => h.Target == unit);

        // ------------------------------------------------------------------ Vol de vie

        [Test, Description("Vérifie que le vol de vie rend au lanceur le pourcentage des dégâts qui ont traversé le bouclier de la cible (pas de la partie absorbée) — parce qu'on ne vole que la vie réellement retirée")]
        public void Lifesteal_HealsThePartOfDamageThatGotThrough()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 200), Fx(EffectType.Lifesteal, 50) }, new[] { "brute" }, out var events, s =>
            {
                s.Unit(0).Hp = s.Unit(0).MaxHp / 4;
                s.Unit(1).Statuses.Add(new StatusInstance(StatusType.Shield, 2, 20));
            });
            var dmg = events.OfType<DamageEvent>().Single(d => d.Target == 1);
            Assert.That(events.OfType<ShieldAbsorbedEvent>().Any(), Is.True, "le bouclier a absorbé une partie");
            Assert.That(HealsOn(events, 0).Single().Amount, Is.EqualTo(dmg.Amount * 50 / 100));
        }

        [Test, Description("Vérifie que le vol de vie ne soigne pas au-delà des PV max, et ne fait rien sur une compétence sans dégâts — parce qu'un vol de vie sans coup ne doit rien rendre")]
        public void Lifesteal_IsCappedByMaxHp_AndNothingWithoutDamage()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Lifesteal, 50) }, new[] { "brute" }, out var full);
            Assert.That(HealsOn(full, 0).Sum(h => h.Amount), Is.EqualTo(0), "déjà à PV max");
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2), Fx(EffectType.Lifesteal, 50) }, new[] { "brute" }, out var none,
                 s => s.Unit(0).Hp = s.Unit(0).MaxHp / 4);
            Assert.That(HealsOn(none, 0), Is.Empty);
        }

        [Test, Description("Vérifie qu'une compétence de zone avec vol de vie soigne le lanceur une fois par ennemi touché — parce que le vol de vie compte cible par cible")]
        public void Lifesteal_OnAoe_HealsOncePerTarget()
        {
            Cast(TargetRule.AllEnemies, new[] { Fx(EffectType.Damage, 80), Fx(EffectType.Lifesteal, 20) }, new[] { "brute", "brute" }, out var events,
                 s => s.Unit(0).Hp = s.Unit(0).MaxHp / 4);
            Assert.That(HealsOn(events, 0).Count(), Is.EqualTo(2));
        }

        // ------------------------------------------------------------------ Exécution

        static int ExecuteDamage(int hpPercent, bool executeFirst, out List<CombatEvent> events)
        {
            var fx = executeFirst
                ? new[] { Fx(EffectType.Execute, 50), Fx(EffectType.Damage, 100) }
                : new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Execute, 50) };
            Cast(TargetRule.SingleEnemy, fx, new[] { "brute" }, out events, s => s.Unit(1).Hp = s.Unit(1).MaxHp * hpPercent / 100);
            return events.OfType<DamageEvent>().Single(d => d.Target == 1).Amount;
        }

        [Test, Description("Vérifie que l'exécution augmente les dégâts de 50 % sous 30 % des PV de la cible (et pas au-dessus), où qu'elle soit placée dans la liste — parce que le bonus vise les ennemis presque vaincus")]
        public void Execute_AppliesBelowThreshold_WhereverItIsInTheList()
        {
            int normal = ExecuteDamage(31, false, out var above);
            Assert.That(above.OfType<ExecutedEvent>(), Is.Empty);
            foreach (bool first in new[] { true, false })
            {
                int boosted = ExecuteDamage(29, first, out var below);
                Assert.That(boosted, Is.EqualTo(normal * 150 / 100), first ? "exécution avant les dégâts" : "exécution après les dégâts");
                Assert.That(below.OfType<ExecutedEvent>().Single().Target, Is.EqualTo(1));
            }
        }
    }
}
