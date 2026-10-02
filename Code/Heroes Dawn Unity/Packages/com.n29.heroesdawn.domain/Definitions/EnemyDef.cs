using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class EnemyDef
    {
        public EnemyId Id { get; }
        public string NameKey { get; }
        public Role Role { get; }
        public Family Family { get; }
        public StatBlock BaseStats { get; }
        public IReadOnlyList<SkillDef> Skills { get; }
        public bool IsBoss { get; }

        public EnemyDef(EnemyId id, string nameKey, Role role, Family family, StatBlock baseStats, IReadOnlyList<SkillDef> skills, bool isBoss = false)
        {
            Id = id; NameKey = nameKey; Role = role; Family = family; BaseStats = baseStats; Skills = skills; IsBoss = isBoss;
        }
    }
}
