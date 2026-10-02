using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class AllyEntry
    {
        public MemberId Member { get; }
        public int Level { get; }
        public int FatiguePercent { get; }
        public AllyEntry(MemberId member, int level, int fatiguePercent) { Member = member; Level = level; FatiguePercent = fatiguePercent; }
    }

    public sealed class EnemyEntry
    {
        public EnemyId Enemy { get; }
        public int Level { get; }
        public EnemyEntry(EnemyId enemy, int level) { Enemy = enemy; Level = level; }
    }

    public sealed class CombatSetup
    {
        public ulong Seed { get; }
        public CombatRank Rank { get; }
        public IReadOnlyList<AllyEntry> Allies { get; }
        public IReadOnlyList<EnemyEntry> Enemies { get; }
        public bool Auto { get; }

        public CombatSetup(ulong seed, CombatRank rank, IReadOnlyList<AllyEntry> allies, IReadOnlyList<EnemyEntry> enemies, bool auto)
        {
            Seed = seed; Rank = rank; Allies = allies; Enemies = enemies; Auto = auto;
        }
    }
}
