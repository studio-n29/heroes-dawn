using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class EncounterDef
    {
        public CombatRank Rank { get; }
        public IReadOnlyList<EnemyId> Enemies { get; }
        public EncounterDef(CombatRank rank, IReadOnlyList<EnemyId> enemies) { Rank = rank; Enemies = enemies; }
    }
}
