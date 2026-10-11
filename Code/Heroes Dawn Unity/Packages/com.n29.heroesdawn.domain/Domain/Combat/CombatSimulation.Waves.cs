#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>Vagues : quand tous les ennemis d'une vague sont vaincus, la suivante apparaît ; la victoire vient avec la dernière.</summary>
    public sealed partial class CombatSimulation
    {
        int _wave;

        /// <summary>Vague en cours (0 = première).</summary>
        public int WaveIndex => _wave;
        public int WaveCount => Setup.Waves.Count;

        void SpawnWave(int index)
        {
            foreach (var e in Setup.Waves[index]) _units.Add(UnitFactory.Enemy(_units.Count, _c.Enemy(e.Enemy), e, Setup.Rank, _b));
        }

        /// <summary>Fait apparaître la vague suivante ; renvoie faux s'il n'y en a plus.</summary>
        bool TryStartNextWave(List<CombatEvent> output)
        {
            if (_wave + 1 >= WaveCount) return false;
            _wave++;
            int first = _units.Count;
            SpawnWave(_wave);
            var ids = new List<int>();
            for (int i = first; i < _units.Count; i++)
            {
                _units[i].Point = _rng.Range(0, _b.TimelineStartPointMax + 1);
                ids.Add(i);
            }
            output.Add(new WaveStartedEvent(Tick, _wave, ids.ToArray()));
            return true;
        }
    }
}
