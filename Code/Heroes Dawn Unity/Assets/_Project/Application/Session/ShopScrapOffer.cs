#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Un lot de scraps de la boutique du jour : famille, quantité, prix en Crédits et achat éventuel.</summary>
    public sealed class ShopScrapOffer
    {
        public ScrapKind Kind { get; }
        public int Count { get; }
        public int Price { get; }
        public bool Bought { get; internal set; }

        public ShopScrapOffer(ScrapKind kind, int count, int price) { Kind = kind; Count = count; Price = price; }
    }
}
