using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class EncounterDef
    {
        public CombatRank Rank { get; }
        /// <summary>Vagues successives d'ennemis (au moins une), ex. vague 1 : 3 ennemis, vague 2 : 2 ennemis.</summary>
        public IReadOnlyList<IReadOnlyList<EnemyId>> Waves { get; }
        /// <summary>Ennemis de la première vague.</summary>
        public IReadOnlyList<EnemyId> Enemies => Waves[0];

        public EncounterDef(CombatRank rank, IReadOnlyList<EnemyId> enemies) : this(rank, new[] { enemies }) { }

        public EncounterDef(CombatRank rank, IReadOnlyList<IReadOnlyList<EnemyId>> waves) { Rank = rank; Waves = waves; }
    }
}
