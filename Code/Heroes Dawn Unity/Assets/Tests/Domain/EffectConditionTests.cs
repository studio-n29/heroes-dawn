#nullable enable
using System;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Condition, ValueMod et EffectDef étendu (P1) : données seules, comportement de combat inchangé.</summary>
    public sealed class EffectConditionTests
    {
        static readonly Condition Burning = new Condition(ConditionKind.TargetHasStatus, StatusType.Burn);
        static readonly ValueMod Plus40 = new ValueMod(40, Burning);

        static SkillDef Skill(params EffectDef[] effects)
            => new SkillDef(new SkillId("t_skill"), SkillSlot.A3, 2, TargetRule.SingleEnemy, effects);

        [Test, Description("Vérifie que With copie la condition et les modificateurs et ne change que les champs demandés — parce que les reconstructions de valeur ne doivent jamais perdre les données de synergie")]
        public void EffectDef_With_KeepsConditionAndMods()
        {
            var e = new EffectDef(EffectType.ApplyStatus, 5, StatusType.Burn, 2, 60, Burning, new[] { Plus40 });
            var r = e.With(value: 9, duration: 3, chance: 70);
            Assert.That((r.Type, r.Value, r.Status, r.Duration, r.ChancePercent), Is.EqualTo((EffectType.ApplyStatus, 9, StatusType.Burn, 3, 70)));
            Assert.That(r.When, Is.SameAs(Burning));
            Assert.That(r.Mods, Is.EqualTo(new[] { Plus40 }));
            var same = e.With();
            Assert.That((same.Value, same.Duration, same.ChancePercent), Is.EqualTo((5, 2, 60)));
            Assert.That(same.When, Is.SameAs(Burning));
        }

        [Test, Description("Vérifie qu'un effet sans condition ni modificateur a When nul et Mods vide — parce que tout le contenu existant doit rester inchangé")]
        public void EffectDef_Default_HasNoConditionAndNoMods()
        {
            var e = new EffectDef(EffectType.Damage, 100);
            Assert.That(e.When, Is.Null);
            Assert.That(e.Mods, Is.Empty);
        }

        [Test, Description("Vérifie qu'un ValueMod à échelle exige un MaxCount d'au moins 1 — parce qu'un plafond nul rendrait le bonus muet sans prévenir")]
        public void ValueMod_Scaled_RequiresAPositiveMaxCount()
        {
            Assert.Throws<ArgumentException>(() => new ValueMod(10, null, ScaleKind.EnemiesAlive, 0));
            Assert.Throws<ArgumentException>(() => new ValueMod(10, null, ScaleKind.EnemiesAlive, -1));
            Assert.DoesNotThrow(() => new ValueMod(10, null, ScaleKind.EnemiesAlive, 3));
            Assert.DoesNotThrow(() => new ValueMod(10, Burning));
        }

        [Test, Description("Vérifie que MapEffects conserve la condition et les modificateurs quand la fonction passe par With — parce que c'est le chemin de toutes les reconstructions de compétence")]
        public void MapEffects_KeepsConditionAndMods()
        {
            var s = Skill(new EffectDef(EffectType.Damage, 100, when: Burning, mods: new[] { Plus40 }));
            var r = s.MapEffects(e => e.With(value: e.Value + 1));
            Assert.That(r.Effects[0].Value, Is.EqualTo(101));
            Assert.That(r.Effects[0].When, Is.SameAs(Burning));
            Assert.That(r.Effects[0].Mods, Is.EqualTo(new[] { Plus40 }));
        }

        [Test, Description("Vérifie que CardBonuses.Apply garde la condition et les modificateurs et n'augmente que la valeur de base (100 -> 120 pour +20 %, bonus du mod toujours 40) — parce que le bonus de carte ne doit pas gonfler les synergies")]
        public void CardBonusesApply_KeepsConditionAndMods_AndScalesTheBaseValueOnly()
        {
            var s = Skill(new EffectDef(EffectType.Damage, 100, when: Burning, mods: new[] { Plus40 }));
            var e = CardBonuses.Apply(s, 20).Effects[0];
            Assert.That(e.Value, Is.EqualTo(120));
            Assert.That(e.When, Is.SameAs(Burning));
            Assert.That(e.Mods.Count, Is.EqualTo(1));
            Assert.That(e.Mods[0].Bonus, Is.EqualTo(40));
        }

        [Test, Description("Vérifie que SkillAt conserve condition et modificateurs des effets de base mis à l'échelle et des effets de palier — parce que les niveaux ne doivent pas effacer les synergies")]
        public void SkillAt_KeepsConditionAndModsOfTierEffects()
        {
            var baseFx = new EffectDef(EffectType.Damage, 100, when: Burning, mods: new[] { Plus40 });
            var tierFx = new EffectDef(EffectType.Damage, 30, when: Burning, mods: new[] { Plus40 });
            var card = new SkillCardDef(new CardId("card_t"), "k", Family.Elemental, 1, Skill(baseFx), null, new[] { new CardTier(3, tierFx) });
            var fx = CardLevels.SkillAt(card, 10).Effects;
            Assert.That(fx.Count, Is.EqualTo(2));
            Assert.That(fx[0].Value, Is.GreaterThan(100));
            foreach (var e in fx)
            {
                Assert.That(e.When, Is.SameAs(Burning));
                Assert.That(e.Mods, Is.EqualTo(new[] { Plus40 }));
            }
            Assert.That(fx[1].Value, Is.EqualTo(30));
        }
    }
}
