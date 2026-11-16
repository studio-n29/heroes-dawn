#nullable enable
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Capacités d'une compétence (P0) : structure seule, la vue Effects reste celle d'avant.</summary>
    public sealed class AbilityTests
    {
        static readonly EffectDef A = new EffectDef(EffectType.Damage, 100);
        static readonly EffectDef B = new EffectDef(EffectType.Heal, 20);
        static readonly EffectDef C = new EffectDef(EffectType.Shield, 30);

        static SkillDef Skill(params EffectDef[] effects)
            => new SkillDef(new SkillId("t_skill"), SkillSlot.A3, 2, TargetRule.SingleEnemy, effects);

        [Test, Description("Vérifie qu'une compétence construite depuis des effets les enveloppe dans une seule capacité OnCast — parce que le constructeur historique doit rester valable pour tous les appels existants")]
        public void FromEffects_WrapsInOneOnCastAbility()
        {
            var s = Skill(A, B);
            Assert.That(s.Abilities.Count, Is.EqualTo(1));
            Assert.That(s.Abilities[0].Trigger, Is.EqualTo(AbilityTrigger.OnCast));
            Assert.That(s.Effects, Is.EqualTo(new[] { A, B }));
            Assert.That(s.Abilities[0].Effects, Is.EqualTo(new[] { A, B }));
        }

        [Test, Description("Vérifie que Effects est la concaténation ordonnée des effets des capacités OnCast — parce que le moteur lit encore Effects")]
        public void Effects_IsTheOrderedConcatenationOfOnCastAbilities()
        {
            var s = SkillDef.FromAbilities(new SkillId("t"), SkillSlot.A3, 1, TargetRule.SingleEnemy, new[]
            {
                new Ability(AbilityTrigger.OnCast, new[] { A, B }),
                new Ability(AbilityTrigger.OnCast, new[] { C }),
            });
            Assert.That(s.Effects, Is.EqualTo(new[] { A, B, C }));
            Assert.That(s.EffectsOf(AbilityTrigger.OnCast), Is.EqualTo(new[] { A, B, C }));
        }

        [Test, Description("Vérifie que MapEffects conserve le nombre et l'ordre des capacités ainsi que l'identité de la compétence — parce que seule la valeur des effets doit changer")]
        public void MapEffects_KeepsStructureOrderAndIdentity()
        {
            var s = SkillDef.FromAbilities(new SkillId("t"), SkillSlot.A2, 3, TargetRule.AllEnemies, new[]
            {
                new Ability(AbilityTrigger.OnCast, new[] { A, B }),
                new Ability(AbilityTrigger.OnCast, new[] { C }),
            }, 7);
            var m = s.MapEffects(e => new EffectDef(e.Type, e.Value + 1));
            Assert.That(m.Abilities.Count, Is.EqualTo(2));
            Assert.That(m.Abilities[0].Effects.Select(e => e.Value), Is.EqualTo(new[] { 101, 21 }));
            Assert.That(m.Abilities[1].Effects.Select(e => e.Value), Is.EqualTo(new[] { 31 }));
            Assert.That(m.Id, Is.EqualTo(s.Id));
            Assert.That(m.Slot, Is.EqualTo(s.Slot));
            Assert.That(m.Cooldown, Is.EqualTo(s.Cooldown));
            Assert.That(m.Target, Is.EqualTo(s.Target));
            Assert.That(m.Cost, Is.EqualTo(7));
        }

        [Test, Description("Vérifie que WithAbilities ajoute les capacités après les existantes sans modifier l'original — parce que les définitions sont immuables")]
        public void WithAbilities_AppendsAfterExistingOnes()
        {
            var s = Skill(A);
            var extra = new Ability(AbilityTrigger.OnCast, new[] { B });
            var w = s.WithAbilities(new[] { extra });
            Assert.That(w.Abilities.Count, Is.EqualTo(2));
            Assert.That(w.Abilities[1], Is.SameAs(extra));
            Assert.That(w.Effects, Is.EqualTo(new[] { A, B }));
            Assert.That(s.Abilities.Count, Is.EqualTo(1));
            Assert.That(w.Cost, Is.EqualTo(s.Cost));
        }

        [Test, Description("Vérifie qu'une compétence sans effet n'a pas d'effet de base, est de catégorie Dégâts et porte une capacité — parce que le comportement historique est conservé")]
        public void EmptySkill_HasNullBaseEffect_AndDamageCategory()
        {
            var s = Skill();
            Assert.That(s.BaseEffect, Is.Null);
            Assert.That(s.Category, Is.EqualTo(SkillCategory.Damage));
            Assert.That(s.Abilities.Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que le coût dérivé de la recharge et l'effet de base sont inchangés — parce que rien ne doit changer pour le moteur")]
        public void CostAndBaseEffect_AreUnchanged()
        {
            var s = Skill(A, B);
            Assert.That(s.Cost, Is.EqualTo(20));
            Assert.That(s.BaseEffect, Is.SameAs(A));
        }
    }
}
