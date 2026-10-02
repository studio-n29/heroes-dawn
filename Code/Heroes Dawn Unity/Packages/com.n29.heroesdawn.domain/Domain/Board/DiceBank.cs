using System;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>GDD §5 — PM non utilisés → jauge ; 6 PM = 1 dé bonus ; réserve max 2, surplus perdu.</summary>
    public sealed class DiceBank
    {
        readonly BalanceConfig _b;
        public int StoredPm { get; private set; }
        public int BonusDice { get; private set; }

        public DiceBank(BalanceConfig b) : this(b, 0, 0) { }

        public DiceBank(BalanceConfig b, int storedPm, int bonusDice)
        {
            _b = b; StoredPm = storedPm; BonusDice = bonusDice;
        }

        public void AddUnusedPm(int pm)
        {
            if (pm < 0) throw new ArgumentOutOfRangeException(nameof(pm));
            if (BonusDice >= _b.MaxBonusDice) { StoredPm = 0; return; }
            StoredPm += pm;
            while (StoredPm >= _b.PmPerBonusDie && BonusDice < _b.MaxBonusDice)
            {
                StoredPm -= _b.PmPerBonusDie;
                BonusDice++;
            }
            if (BonusDice >= _b.MaxBonusDice) StoredPm = 0;
        }

        public bool TryConsumeBonusDie()
        {
            if (BonusDice == 0) return false;
            BonusDice--;
            return true;
        }
    }
}
