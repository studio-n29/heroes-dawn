using System;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>02_Combat — Dégâts = ATQ × % × (1 − DEF/(DEF+K)) × critique × vulnérabilité × (1 ± 5 %).</summary>
    public static class DamageFormula
    {
        public static int Compute(int atk, int pct, int def, int defK, bool crit, bool vulnerable, int varianceRoll, BalanceConfig b)
        {
            long raw = (long)atk * pct / 100;
            long reductionPermille = (long)def * 1000 / (def + defK);
            long dmg = raw * (1000 - reductionPermille) / 1000;
            if (crit) dmg = dmg * b.CritMultiplierPercent / 100;
            if (vulnerable) dmg = dmg * (100 + b.VulnerableBonusPercent) / 100;
            dmg = dmg * (100 + varianceRoll) / 100;
            return (int)Math.Max(1, dmg);
        }
    }
}
