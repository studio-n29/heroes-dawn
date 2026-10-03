#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Boutique éphémère de la session (modifiée seulement par les cas d'usage de la boutique).</summary>
    public sealed class ShopState : IShopReadModel
    {
        readonly List<ShopOffer> _offers = new List<ShopOffer>();

        public string Day { get; private set; } = "";
        public int Draw { get; private set; }
        public IReadOnlyList<ShopOffer> Offers => _offers;

        internal void Replace(string day, int draw, IEnumerable<ShopOffer> offers)
        {
            Day = day;
            Draw = draw;
            _offers.Clear();
            _offers.AddRange(offers);
        }

        internal void Clear() => Replace("", 0, System.Array.Empty<ShopOffer>());
    }
}
