#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Lot de scraps proposé dans la boutique du jour : une famille, une quantité, un prix en Crédits.</summary>
    public readonly struct ScrapLot
    {
        public ScrapKind Kind { get; }
        public int Count { get; }
        public int Price { get; }

        public ScrapLot(ScrapKind kind, int count, int price) { Kind = kind; Count = count; Price = price; }
    }
}
