using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Famille d'animation d'une compétence : feu, givre, foudre, acide, entrave, marque, soin, bouclier, hâte, temps, impact.</summary>
    public sealed class SkillFxTests
    {
        static SkillDef Skill(TargetRule target, params EffectDef[] fx) => new SkillDef(new SkillId("x"), SkillSlot.A1, 0, target, fx);
        static EffectDef Dmg => new EffectDef(EffectType.Damage, 100);
        static EffectDef St(StatusType s) => new EffectDef(EffectType.ApplyStatus, 0, s, 2, 100);

        [TestCase(StatusType.Burn, SkillFxKind.Fire), TestCase(StatusType.Freeze, SkillFxKind.Ice), TestCase(StatusType.Shock, SkillFxKind.Lightning),
         TestCase(StatusType.Corrosion, SkillFxKind.Acid), TestCase(StatusType.Bind, SkillFxKind.Chains), TestCase(StatusType.Mark, SkillFxKind.Mark),
         TestCase(StatusType.Vulnerable, SkillFxKind.Mark)]
        [Description("Vérifie qu'une compétence offensive prend la famille d'animation de son statut (Brûlure : feu, Gel : givre, Choc : foudre…) — parce que chaque famille a sa propre animation")]
        public void Offensive_FollowsItsStatus(StatusType status, SkillFxKind expected)
            => Assert.That(SkillFx.KindOf(Skill(TargetRule.SingleEnemy, Dmg, St(status))), Is.EqualTo(expected));

        [Test, Description("Vérifie qu'une attaque sans statut est un impact, qu'un statut élémentaire passe avant Marque et que Marque passe avant la gestion du temps — parce que l'ordre décide de l'animation quand une compétence cumule plusieurs effets")]
        public void Offensive_Precedence()
        {
            Assert.That(SkillFx.KindOf(Skill(TargetRule.SingleEnemy, Dmg)), Is.EqualTo(SkillFxKind.Impact));
            Assert.That(SkillFx.KindOf(Skill(TargetRule.AllEnemies, St(StatusType.Mark), St(StatusType.Burn))), Is.EqualTo(SkillFxKind.Fire));
            Assert.That(SkillFx.KindOf(Skill(TargetRule.AllEnemies, St(StatusType.Vulnerable), new EffectDef(EffectType.GaugeDelta, -100))), Is.EqualTo(SkillFxKind.Mark));
            Assert.That(SkillFx.KindOf(Skill(TargetRule.AllEnemies, new EffectDef(EffectType.GaugeDelta, -100))), Is.EqualTo(SkillFxKind.Time));
        }

        [Test, Description("Vérifie que les compétences sur un allié donnent Soin, Bouclier ou Hâte selon leur effet — parce qu'elles s'animent sur l'allié et non vers l'ennemi")]
        public void OnAllies_HealShieldOrHaste()
        {
            Assert.That(SkillFx.KindOf(Skill(TargetRule.SingleAlly, new EffectDef(EffectType.Heal, 30), new EffectDef(EffectType.Advance, 20))), Is.EqualTo(SkillFxKind.Heal));
            Assert.That(SkillFx.KindOf(Skill(TargetRule.Self, St(StatusType.Taunt), new EffectDef(EffectType.Shield, 25, StatusType.Shield, 2))), Is.EqualTo(SkillFxKind.Shield));
            Assert.That(SkillFx.KindOf(Skill(TargetRule.AllAllies, St(StatusType.Haste))), Is.EqualTo(SkillFxKind.Haste));
            Assert.That(SkillFx.KindOf(Skill(TargetRule.AllAllies, St(StatusType.Regen))), Is.EqualTo(SkillFxKind.Heal));
        }

        [Test, Description("Vérifie que chaque famille a sa couleur et que les compétences des 32 agents couvrent au moins huit familles d'animation — parce que l'animation doit varier d'un agent à l'autre")]
        public void EveryFamily_HasItsColor_AndTheRosterCoversMany()
        {
            var colors = System.Enum.GetValues(typeof(SkillFxKind)).Cast<SkillFxKind>().Select(SkillFx.ColorOf).ToList();
            Assert.That(colors.Distinct().Count(), Is.EqualTo(colors.Count));
            var kinds = PrototypeContent.Build().Members.SelectMany(m => m.Skills).Select(SkillFx.KindOf).Distinct().ToList();
            Assert.That(kinds.Count, Is.GreaterThanOrEqualTo(8), string.Join(", ", kinds));
        }
    }
}
