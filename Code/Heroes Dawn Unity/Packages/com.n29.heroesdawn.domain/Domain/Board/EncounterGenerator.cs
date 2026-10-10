#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>Compose la rencontre d'une case Combat qui a un <see cref="CombatSpec"/> : une manche par vague, des ennemis tirés dans la table.</summary>
    public static class EncounterGenerator
    {
        public static EncounterDef Generate(CombatSpec spec, CombatRank rank, IRandom rng)
        {
            var waves = new List<IReadOnlyList<EnemyId>>();
            for (int w = 0; w < spec.Rounds; w++)
            {
                int count = rng.Range(spec.EnemiesMin, spec.EnemiesMax + 1);
                var wave = new List<EnemyId>();
                for (int i = 0; i < count; i++) wave.Add(spec.EnemyTable[rng.Range(0, spec.EnemyTable.Count)]);
                waves.Add(wave);
            }
            return new EncounterDef(rank, waves);
        }
    }
}
