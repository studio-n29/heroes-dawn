using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public static class UnitFactory
    {
        /// <summary>Base × (1 + croissance)^(niveau−1), en entiers.</summary>
        public static int ScaleStat(int baseValue, int level, BalanceConfig b)
        {
            long v = baseValue;
            for (int i = 1; i < level; i++) v = v * (1000 + b.StatGrowthPermillePerLevel) / 1000;
            return (int)v;
        }

        /// <summary>03_Metahumains — malus linéaire au-delà du seuil, jusqu'à −30 % à 100 % de fatigue.</summary>
        public static int FatigueMultiplierPermille(int fatigue, BalanceConfig b)
        {
            if (fatigue <= b.FatigueMalusThreshold) return 1000;
            int over = fatigue - b.FatigueMalusThreshold;
            int span = 100 - b.FatigueMalusThreshold;
            return 1000 - b.FatigueMalusMaxPercent * 10 * over / span;
        }

        public static CombatUnit Ally(int id, MemberDef d, AllyEntry e, BalanceConfig b)
        {
            int f = FatigueMultiplierPermille(e.FatiguePercent, b);
            int S(int v) => ScaleStat(v, e.Level, b) * f / 1000;
            return new CombatUnit(id, Side.Ally, d.Id.Value, d.Family, d.Role, false,
                                  S(d.BaseStats.Hp), S(d.BaseStats.Atk), S(d.BaseStats.Def), d.BaseStats.Spd,
                                  (long)b.AtbGaugePoints * 1000, d.Skills);
        }

        public static CombatUnit Enemy(int id, EnemyDef d, EnemyEntry e, CombatRank rank, BalanceConfig b)
        {
            int pct = rank == CombatRank.Easy ? b.RankEasyStatPercent
                    : (rank == CombatRank.Elite || rank == CombatRank.Boss) ? b.RankEliteStatPercent : 100;
            int S(int v) => ScaleStat(v, e.Level, b) * pct / 100;
            return new CombatUnit(id, Side.Enemy, d.Id.Value, d.Family, d.Role, d.IsBoss,
                                  S(d.BaseStats.Hp), S(d.BaseStats.Atk), S(d.BaseStats.Def), d.BaseStats.Spd,
                                  (long)b.AtbGaugePoints * 1000, d.Skills);
        }
    }
}
