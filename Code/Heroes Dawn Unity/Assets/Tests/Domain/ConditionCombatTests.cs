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
    /// <summary>Évaluation des conditions (<see cref="EffectDef.When"/>) et des modificateurs de valeur (<see cref="EffectDef.Mods"/>) en vrai combat.</summary>
    public sealed class ConditionCombatTests
    {
        /// <summary>Même montage que <c>CardMechanicsTests.Cast</c> : « verre » (unité 0) lance aussitôt une carte de test (coût 0) ; les ennemis suivent (unités 1, 2…).</summary>
        static CombatSimulation Cast(TargetRule target, EffectDef[] effects, string[] enemies, out List<CombatEvent> events,
                                     Action<CombatSimulation>? before = null, int targetUnit = 1, ulong seed = 11)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var card = new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, 1,
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

        static EffectDef Fx(EffectType t, int v, StatusType s = StatusType.None, int d = 0, int chance = 100, Condition? when = null, params ValueMod[] mods)
            => new EffectDef(t, v, s, d, chance, when, mods);

        static void Give(CombatUnit u, params StatusType[] statuses) { foreach (var st in statuses) u.Statuses.Add(new StatusInstance(st, 3, st == StatusType.Shield ? 50 : 0)); }

        static readonly string[] One = { "brute" };

        /// <summary>Vrai si un effet de dégâts sous cette condition a touché la cible (unité 1).</summary>
        static bool Applies(Condition when, string[] enemies, Action<CombatSimulation>? before = null)
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100, when: when) }, enemies, out var events, before);
            return events.OfType<DamageEvent>().Any(d => d.Target == 1);
        }

        static void Check(Condition when, string[] enemies, Action<CombatSimulation>? makeTrue, Action<CombatSimulation>? makeFalse)
        {
            Assert.That(Applies(when, enemies, makeTrue), Is.True, "condition vraie : l'effet s'applique");
            Assert.That(Applies(when, enemies, makeFalse), Is.False, "condition fausse : l'effet est sauté");
        }

        // ------------------------------------------------------------------ Une condition par nature

        [Test, Description("Vérifie que TargetHasStatus n'applique l'effet que si la cible porte l'altération — parce que la condition lit les statuts de la cible")]
        public void Condition_TargetHasStatus() => Check(new Condition(ConditionKind.TargetHasStatus, StatusType.Burn), One, s => Give(s.Unit(1), StatusType.Burn), null);

        [Test, Description("Vérifie que TargetLacksStatus n'applique l'effet que si la cible n'a pas l'altération — parce que c'est l'inverse de TargetHasStatus")]
        public void Condition_TargetLacksStatus() => Check(new Condition(ConditionKind.TargetLacksStatus, StatusType.Burn), One, null, s => Give(s.Unit(1), StatusType.Burn));

        [Test, Description("Vérifie que TargetHpBelow 50 n'applique l'effet que sous 50 % des PV de la cible — parce que le seuil est en pourcentage des PV max")]
        public void Condition_TargetHpBelow() => Check(new Condition(ConditionKind.TargetHpBelow, value: 50), One, s => s.Unit(1).Hp = s.Unit(1).MaxHp * 40 / 100, null);

        [Test, Description("Vérifie que TargetHpAbove 50 n'applique l'effet qu'au-dessus de 50 % des PV de la cible — parce que c'est l'inverse de TargetHpBelow")]
        public void Condition_TargetHpAbove() => Check(new Condition(ConditionKind.TargetHpAbove, value: 50), One, null, s => s.Unit(1).Hp = s.Unit(1).MaxHp * 40 / 100);

        [Test, Description("Vérifie que CasterHasStatus n'applique l'effet que si le lanceur porte l'altération — parce que la condition lit les statuts du lanceur")]
        public void Condition_CasterHasStatus() => Check(new Condition(ConditionKind.CasterHasStatus, StatusType.Haste), One, s => Give(s.Unit(0), StatusType.Haste), null);

        [Test, Description("Vérifie que CasterHpBelow 50 n'applique l'effet que si le lanceur est sous 50 % de ses PV — parce que le seuil porte sur les PV du lanceur")]
        public void Condition_CasterHpBelow() => Check(new Condition(ConditionKind.CasterHpBelow, value: 50), One, s => s.Unit(0).Hp = s.Unit(0).MaxHp * 40 / 100, null);

        [Test, Description("Vérifie que AnyAllyHasStatus n'applique l'effet que si un allié vivant du lanceur porte l'altération — parce que la condition regarde le camp du lanceur")]
        public void Condition_AnyAllyHasStatus()
        {
            Check(new Condition(ConditionKind.AnyAllyHasStatus, StatusType.Regen), One, s => Give(s.Unit(0), StatusType.Regen), null);
            Assert.That(Applies(new Condition(ConditionKind.AnyAllyHasStatus, StatusType.Regen), One, s => Give(s.Unit(1), StatusType.Regen)), Is.False, "un ennemi n'est pas un allié");
        }

        [Test, Description("Vérifie que TargetStatusCountAtLeast 2 n'applique l'effet que si la cible porte au moins deux altérations — parce que la condition compte les statuts de la cible")]
        public void Condition_TargetStatusCountAtLeast()
        {
            var when = new Condition(ConditionKind.TargetStatusCountAtLeast, value: 2);
            Check(when, One, s => Give(s.Unit(1), StatusType.Burn, StatusType.Freeze), s => Give(s.Unit(1), StatusType.Burn));
        }

        [Test, Description("Vérifie que TargetPlannedAction n'applique l'effet que si la cible a une compétence annoncée sur la frise — parce que la condition lit son plan")]
        public void Condition_TargetPlannedAction() => Check(new Condition(ConditionKind.TargetPlannedAction), One, s => s.Unit(1).PlannedSlot = 0, null);

        [Test, Description("Vérifie que EnemiesAliveAtLeast 3 n'applique l'effet que s'il reste au moins trois ennemis vivants — parce que le compte est relatif au camp du lanceur")]
        public void Condition_EnemiesAliveAtLeast()
        {
            var when = new Condition(ConditionKind.EnemiesAliveAtLeast, value: 3);
            Assert.That(Applies(when, new[] { "brute", "brute", "brute" }), Is.True);
            Assert.That(Applies(when, new[] { "brute", "brute" }), Is.False);
        }

        [Test, Description("Vérifie que EnemiesAliveAtMost 2 n'applique l'effet que s'il reste au plus deux ennemis vivants — parce que c'est l'inverse de EnemiesAliveAtLeast")]
        public void Condition_EnemiesAliveAtMost()
        {
            var when = new Condition(ConditionKind.EnemiesAliveAtMost, value: 2);
            Assert.That(Applies(when, new[] { "brute", "brute" }), Is.True);
            Assert.That(Applies(when, new[] { "brute", "brute", "brute" }), Is.False);
        }

        // ------------------------------------------------------------------ Tirage de chance

        [Test, Description("Vérifie qu'un effet à condition fausse est sauté sans consommer son tirage de chance : le tirage suivant est identique avec et sans lui — parce que les combats seedés ne doivent pas dériver")]
        public void FalseCondition_SkipsTheEffect_WithoutDrawingItsChance()
        {
            var plain = Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100) }, One, out _);
            var withFalse = Cast(TargetRule.SingleEnemy,
                                 new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Damage, 50, chance: 50, when: new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)) },
                                 One, out var events);
            Assert.That(events.OfType<DamageEvent>().Count(d => d.Target == 1), Is.EqualTo(1), "l'effet conditionnel faux n'a rien fait");
            Assert.That(withFalse.RngProbeForTests(), Is.EqualTo(plain.RngProbeForTests()));
        }

        // ------------------------------------------------------------------ Modificateurs

        /// <summary>Valeur que prend l effet une fois la carte montée (bonus de position et de famille inclus) : les modificateurs, eux, ne sont pas mis à l échelle.</summary>
        static int Effective(EffectType type, int value)
        {
            var sim = Cast(TargetRule.SingleEnemy, new[] { Fx(type, value) }, One, out _);
            return sim.Unit(0).Skills[2].EffectsOf(AbilityTrigger.OnCast)[0].Value;
        }

        /// <summary>Effet « nu » (sans modificateur) dont la valeur effective en combat vaut exactement <paramref name="effective"/> : la référence à comparer à un effet à modificateurs.</summary>
        static EffectDef Plain(EffectType type, int effective)
        {
            for (int raw = Math.Abs(effective); raw >= 0; raw--)
                if (Math.Abs(Effective(type, Math.Sign(effective) * raw)) == Math.Abs(effective)) return Fx(type, Math.Sign(effective) * raw);
            throw new InvalidOperationException("aucune valeur brute ne donne " + effective);
        }

        static int DamageTo1(EffectDef[] fx, out List<CombatEvent> events, Action<CombatSimulation>? before = null)
        {
            Cast(TargetRule.SingleEnemy, fx, One, out events, before);
            return events.OfType<DamageEvent>().Single(d => d.Target == 1).Amount;
        }

        [Test, Description("Vérifie qu'un modificateur fixe dont la condition tient ajoute son bonus à la puissance (100 + 40 = 140), et pas sinon — parce que la valeur finale est base + bonus")]
        public void ModBonus_FlatWhenConditionHolds()
        {
            var mod = new ValueMod(40, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn));
            int boosted = DamageTo1(new[] { Fx(EffectType.Damage, 100, mods: mod) }, out _, s => Give(s.Unit(1), StatusType.Burn));
            int reference = DamageTo1(new[] { Plain(EffectType.Damage, Effective(EffectType.Damage, 100) + 40) }, out _, s => Give(s.Unit(1), StatusType.Burn));
            int notHeld = DamageTo1(new[] { Fx(EffectType.Damage, 100, mods: mod) }, out _);
            int baseOnly = DamageTo1(new[] { Fx(EffectType.Damage, 100) }, out _);
            Assert.That(boosted, Is.EqualTo(reference));
            Assert.That(boosted, Is.GreaterThan(baseOnly));
            Assert.That(notHeld, Is.EqualTo(baseOnly));
        }

        [Test, Description("Vérifie qu'un modificateur à échelle est plafonné par MaxCount (+15 par statut, max 3, 5 statuts : bonus 45) — parce que le bonus vaut Bonus × min(compte, MaxCount)")]
        public void ModBonus_Scaled_IsCappedByMaxCount()
        {
            var mod = new ValueMod(15, per: ScaleKind.TargetStatusCount, maxCount: 3);
            Action<CombatSimulation> five = s => Give(s.Unit(1), StatusType.Burn, StatusType.Freeze, StatusType.Corrosion, StatusType.Bind, StatusType.Haste);
            int scaled = DamageTo1(new[] { Fx(EffectType.Damage, 100, mods: mod) }, out _, five);
            Assert.That(scaled, Is.EqualTo(DamageTo1(new[] { Plain(EffectType.Damage, Effective(EffectType.Damage, 100) + 45) }, out _, five)));
            Action<CombatSimulation> two = s => Give(s.Unit(1), StatusType.Burn, StatusType.Freeze);
            Assert.That(DamageTo1(new[] { Fx(EffectType.Damage, 100, mods: mod) }, out _, two), Is.EqualTo(DamageTo1(new[] { Plain(EffectType.Damage, Effective(EffectType.Damage, 100) + 30) }, out _, two)), "sous le plafond : 2 × 15");
        }

        [Test, Description("Vérifie que le bonus par dixième de PV manquants de la cible utilise (PV max − PV) × 10 / PV max en entier — parce que le compte est un entier de 0 à 10")]
        public void ModBonus_PerMissingHpTenths()
        {
            int tenths = 0;
            Action<CombatSimulation> hurt = s => { var u = s.Unit(1); u.Hp = u.MaxHp * 63 / 100; tenths = (u.MaxHp - u.Hp) * 10 / u.MaxHp; };
            var mod = new ValueMod(10, per: ScaleKind.TargetMissingHpTenths, maxCount: 10);
            int scaled = DamageTo1(new[] { Fx(EffectType.Damage, 100, mods: mod) }, out _, hurt);
            Assert.That(tenths, Is.EqualTo(3), "63 % de PV : 3 dixièmes manquants");
            Assert.That(scaled, Is.EqualTo(DamageTo1(new[] { Plain(EffectType.Damage, Effective(EffectType.Damage, 100) + 30) }, out _, hurt)));
        }

        [Test, Description("Vérifie qu'une jauge négative garde son signe quand un bonus s'ajoute (−200 avec +100 donne −300, pas −100) — parce que le bonus s'applique à la valeur absolue")]
        public void NegativeGaugeBonus_KeepsItsSign()
        {
            Action<CombatSimulation> setup = s => { var u = s.Unit(1); u.Point = u.ActionPoint * 3 / 4; };
            int PointAfter(EffectDef fx) { var sim = Cast(TargetRule.SingleEnemy, new[] { fx }, One, out _, setup); return sim.Unit(1).Point; }
            int modded = PointAfter(Fx(EffectType.GaugeDelta, -200, mods: new ValueMod(100)));
            Assert.That(modded, Is.EqualTo(PointAfter(Plain(EffectType.GaugeDelta, Effective(EffectType.GaugeDelta, -200) - 100))));
            Assert.That(modded, Is.LessThan(PointAfter(Fx(EffectType.GaugeDelta, -200))), "recule plus qu'avec -200");
        }

        [Test, Description("Vérifie qu'un effet supplémentaire sous condition (Taunt 2 tours) ne s'applique que si la condition tient — parce que les effets conditionnels s'ajoutent à la compétence")]
        public void ExtraConditionalEffect_AppliesOnlyWhenTrue()
        {
            var fx = new[] { Fx(EffectType.Damage, 100), Fx(EffectType.ApplyStatus, 0, StatusType.Taunt, 2, when: new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)) };
            Assert.That(Cast(TargetRule.SingleEnemy, fx, One, out _, s => Give(s.Unit(1), StatusType.Burn)).Unit(1).Has(StatusType.Taunt), Is.True);
            Assert.That(Cast(TargetRule.SingleEnemy, fx, One, out _).Unit(1).Has(StatusType.Taunt), Is.False);
        }

        [Test, Description("Vérifie que le rebond lit la valeur de base du premier effet de dégâts, sans ses modificateurs — parce que les modificateurs ne s'appliquent pas au rebond")]
        public void Bounce_UsesTheBaseValueWithoutMods()
        {
            int BounceDamage(EffectDef damage)
            {
                Cast(TargetRule.SingleEnemy, new[] { damage, Fx(EffectType.Bounce, 50) }, new[] { "brute", "brute", "brute" }, out var events);
                var bounce = events.OfType<BounceEvent>().Single();
                return events.Skip(events.IndexOf(bounce)).OfType<DamageEvent>().First(d => d.Target == bounce.To).Amount;
            }
            Assert.That(BounceDamage(Fx(EffectType.Damage, 100, mods: new ValueMod(100))), Is.EqualTo(BounceDamage(Fx(EffectType.Damage, 100))));
        }
    }
}
