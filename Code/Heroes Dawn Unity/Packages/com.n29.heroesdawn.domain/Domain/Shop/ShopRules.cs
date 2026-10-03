#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    public static class ShopRules
    {
        /// <summary>Coût du prochain renouvellement : le premier du jour est gratuit, puis Cost1..Cost4 (le dernier sert de plafond).</summary>
        public static int RerollCost(int rerollsToday, BalanceConfig b)
        {
            if (rerollsToday <= 0) return 0;
            switch (rerollsToday)
            {
                case 1: return b.ShopRerollCost1;
                case 2: return b.ShopRerollCost2;
                case 3: return b.ShopRerollCost3;
                default: return b.ShopRerollCost4;
            }
        }
    }
}
