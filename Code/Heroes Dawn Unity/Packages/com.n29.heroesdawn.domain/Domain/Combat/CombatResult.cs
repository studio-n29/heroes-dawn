using System.Collections.Generic;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed class CombatResult
    {
        public bool Victory { get; }
        public int Ticks { get; }
        public IReadOnlyList<(int UnitId, int Hp)> AllyHp { get; }
        public ulong StateHash { get; }

        public CombatResult(bool victory, int ticks, IReadOnlyList<(int UnitId, int Hp)> allyHp, ulong stateHash)
        {
            Victory = victory; Ticks = ticks; AllyHp = allyHp; StateHash = stateHash;
        }
    }
}
