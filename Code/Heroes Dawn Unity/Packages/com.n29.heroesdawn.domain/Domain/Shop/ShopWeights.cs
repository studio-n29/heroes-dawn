#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Poids d'apparition par rareté (relatifs, pas forcément sur 100).</summary>
    public readonly struct ShopWeights
    {
        readonly int _agent, _elite, _legend;

        public ShopWeights(int agent, int elite, int legend)
        {
            _agent = agent; _elite = elite; _legend = legend;
        }

        public int For(Rarity r) => r == Rarity.Legend ? _legend : r == Rarity.Elite ? _elite : _agent;

        public static ShopWeights From(BalanceConfig b) => new ShopWeights(b.ShopWeightAgent, b.ShopWeightElite, b.ShopWeightLegend);
    }
}
