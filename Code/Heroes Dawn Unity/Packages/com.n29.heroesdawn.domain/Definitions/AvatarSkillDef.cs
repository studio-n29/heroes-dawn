using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Compétence propre au joueur (son avatar) : lançable à tout moment, même pendant la pause d'action.
    /// La recharge est en ticks de temps actif (elle ne baisse pas pendant l'attente d'une action).
    /// </summary>
    public sealed class AvatarSkillDef
    {
        public SkillId Id { get; }
        public int CooldownTicks { get; }
        public TargetRule Target { get; }
        /// <summary>Premier effet = effet de base ; les suivants = sous-effets.</summary>
        public IReadOnlyList<EffectDef> Effects { get; }
        public SkillCategory Category => SkillCategories.Of(Target, Effects.Count > 0 ? Effects[0] : null);

        public AvatarSkillDef(SkillId id, int cooldownTicks, TargetRule target, IReadOnlyList<EffectDef> effects)
        {
            Id = id; CooldownTicks = cooldownTicks; Target = target; Effects = effects;
        }
    }
}
