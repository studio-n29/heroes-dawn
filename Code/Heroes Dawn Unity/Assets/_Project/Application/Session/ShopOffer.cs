#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Un emplacement de la boutique : héros proposé et achat éventuel.</summary>
    public sealed class ShopOffer
    {
        public MemberId Id { get; }
        public bool Recruited { get; internal set; }

        public ShopOffer(MemberId id, bool recruited = false) { Id = id; Recruited = recruited; }
    }
}
