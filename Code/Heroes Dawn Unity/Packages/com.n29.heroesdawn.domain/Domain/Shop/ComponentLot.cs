#nullable enable
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Lot de Composants proposé dans la boutique du jour : un type, une qualité, une quantité, un prix en Crédits.</summary>
    public readonly struct ComponentLot
    {
        public ComponentKind Kind { get; }
        public ComponentGrade Grade { get; }
        public int Count { get; }
        public int Price { get; }

        public ComponentLot(ComponentKind kind, ComponentGrade grade, int count, int price) { Kind = kind; Grade = grade; Count = count; Price = price; }
    }
}
