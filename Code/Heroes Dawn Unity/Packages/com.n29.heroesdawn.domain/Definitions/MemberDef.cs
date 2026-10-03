using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class MemberDef
    {
        public MemberId Id { get; }
        public string NameKey { get; }
        public Family Family { get; }
        public Role Role { get; }
        public Rarity Rarity { get; }
        public StatBlock BaseStats { get; }
        public IReadOnlyList<SkillDef> Skills { get; }
        /// <summary>Zone à valider avant que la recrue entre dans la boutique ; nul = toujours disponible.</summary>
        public ZoneId? UnlockZone { get; }

        public MemberDef(MemberId id, string nameKey, Family family, Role role, Rarity rarity, StatBlock baseStats, IReadOnlyList<SkillDef> skills,
                         ZoneId? unlockZone = null)
        {
            Id = id; NameKey = nameKey; Family = family; Role = role; Rarity = rarity; BaseStats = baseStats; Skills = skills;
            UnlockZone = unlockZone;
        }
    }
}
