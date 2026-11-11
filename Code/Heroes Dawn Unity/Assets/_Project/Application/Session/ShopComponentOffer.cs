#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Un lot de Composants de la boutique du jour : type, qualité, quantité, prix en Crédits et achat éventuel.</summary>
    public sealed class ShopComponentOffer
    {
        public ComponentKind Kind { get; }
        public ComponentGrade Grade { get; }
        public int Count { get; }
        public int Price { get; }
        public bool Bought { get; internal set; }

        public ShopComponentOffer(ComponentKind kind, ComponentGrade grade, int count, int price) { Kind = kind; Grade = grade; Count = count; Price = price; }
    }
}
