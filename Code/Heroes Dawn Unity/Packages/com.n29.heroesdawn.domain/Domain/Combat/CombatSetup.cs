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
        /// <summary>Ennemis de la première vague.</summary>
        public IReadOnlyList<EnemyEntry> Enemies { get; }
        /// <summary>Vagues successives d'ennemis (au moins une) ; la vague 1 est <see cref="Enemies"/>.</summary>
        public IReadOnlyList<IReadOnlyList<EnemyEntry>> Waves { get; }
        public bool Auto { get; }

        public CombatSetup(ulong seed, CombatRank rank, IReadOnlyList<AllyEntry> allies, IReadOnlyList<EnemyEntry> enemies, bool auto)
            : this(seed, rank, allies, new[] { enemies }, auto) { }

        public CombatSetup(ulong seed, CombatRank rank, IReadOnlyList<AllyEntry> allies, IReadOnlyList<IReadOnlyList<EnemyEntry>> waves, bool auto)
        {
            Seed = seed; Rank = rank; Allies = allies; Waves = waves; Enemies = waves[0]; Auto = auto;
        }
    }
}
