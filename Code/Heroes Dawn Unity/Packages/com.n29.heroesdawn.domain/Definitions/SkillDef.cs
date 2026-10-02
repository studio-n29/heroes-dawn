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
        public IReadOnlyList<EffectDef> Effects { get; }

        public SkillDef(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<EffectDef> effects)
        {
            Id = id; Slot = slot; Cooldown = cooldown; Target = target; Effects = effects;
        }
    }
}
