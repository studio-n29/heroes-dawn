#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class SkillDef
    {
        public SkillId Id { get; }
        public SkillSlot Slot { get; }
        /// <summary>Nombre de tours du personnage pendant lesquels la compétence reste indisponible après usage.</summary>
        public int Cooldown { get; }
        public TargetRule Target { get; }
        /// <summary>Le premier effet est l'effet de base ; les suivants sont les sous-effets (altérations, faiblesses…).</summary>
        public IReadOnlyList<EffectDef> Effects { get; }

        public SkillDef(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<EffectDef> effects)
        {
            Id = id; Slot = slot; Cooldown = cooldown; Target = target; Effects = effects;
        }

        public EffectDef? BaseEffect => Effects.Count > 0 ? Effects[0] : null;

        /// <summary>Effets secondaires après l'effet de base.</summary>
        public IEnumerable<EffectDef> SubEffects
        {
            get { for (int i = 1; i < Effects.Count; i++) yield return Effects[i]; }
        }

        public SkillCategory Category => SkillCategories.Of(Target, BaseEffect);
    }

    /// <summary>Catégorie déduite de l'effet de base : pas de donnée supplémentaire à maintenir.</summary>
    public static class SkillCategories
    {
        public static SkillCategory Of(TargetRule target, EffectDef? baseEffect)
        {
            if (baseEffect == null) return SkillCategory.Damage;
            switch (baseEffect.Type)
            {
                case EffectType.Heal: return SkillCategory.Heal;
                case EffectType.Shield: return SkillCategory.Protection;
                case EffectType.GaugeDelta: return SkillCategory.TimeManagement;
                case EffectType.ApplyStatus:
                    if (target == TargetRule.SingleEnemy || target == TargetRule.AllEnemies) return SkillCategory.Damage;
                    return IsDefensive(baseEffect.Status) ? SkillCategory.Protection : SkillCategory.Boost;
                default: return SkillCategory.Damage;
            }
        }

        static bool IsDefensive(StatusType s) => s == StatusType.Taunt || s == StatusType.Evade || s == StatusType.Anchor || s == StatusType.Shield;
    }
}
