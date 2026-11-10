#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Poids d'apparition par rang d'agent (relatifs, pas forcément sur 100).</summary>
    public readonly struct ShopWeights
    {
        readonly int[]? _weights;

        /// <summary>Un poids par rang, du rang 1 au rang 5.</summary>
        public ShopWeights(int rank1, int rank2, int rank3, int rank4, int rank5)
        {
            _weights = new[] { rank1, rank2, rank3, rank4, rank5 };
        }

        public int For(int rank) => _weights == null ? AgentRanks.ShopWeight(rank) : _weights[AgentRanks.Clamp(rank) - 1];

        /// <summary>Poids par défaut du jeu (<see cref="AgentRanks.ShopWeight"/>).</summary>
        public static ShopWeights Default => new ShopWeights(
            AgentRanks.ShopWeight(1), AgentRanks.ShopWeight(2), AgentRanks.ShopWeight(3), AgentRanks.ShopWeight(4), AgentRanks.ShopWeight(5));

        public static ShopWeights From(BalanceConfig b) => Default;
    }
}
