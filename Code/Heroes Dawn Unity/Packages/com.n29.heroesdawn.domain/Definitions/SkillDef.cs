#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class SkillDef
    {
        public SkillId Id { get; }
        public SkillSlot Slot { get; }
        /// <summary>Recharge historique en tours (ne bloque plus la compétence : seule l énergie la limite ; sert à dériver le coût par défaut).</summary>
        public int Cooldown { get; }
        /// <summary>Énergie dépensée à chaque usage. Sans coût explicite, dérivé de la recharge historique (10 par tour de recharge).</summary>
        public int Cost { get; }
        public TargetRule Target { get; }
        /// <summary>Le premier effet est l'effet de base ; les suivants sont les sous-effets (altérations, faiblesses…).</summary>
        public IReadOnlyList<EffectDef> Effects { get; }

        public SkillDef(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<EffectDef> effects, int cost = -1)
        {
            Id = id; Slot = slot; Cooldown = cooldown; Target = target; Effects = effects;
            Cost = cost >= 0 ? cost : cooldown * 10;
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
