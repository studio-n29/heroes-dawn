#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Boutique éphémère de la session (modifiée seulement par les cas d'usage de la boutique).</summary>
    public sealed class ShopState : IShopReadModel
    {
        readonly List<ShopOffer> _offers = new List<ShopOffer>();
        readonly List<ShopComponentOffer> _components = new List<ShopComponentOffer>();

        public string Day { get; private set; } = "";
        public int Draw { get; private set; }
        /// <summary>Dernier jour de boutique affiché sur l'onglet Recruter (vide = jamais vu).</summary>
        public string SeenDay { get; private set; } = "";
        public IReadOnlyList<ShopOffer> Offers => _offers;
        public IReadOnlyList<ShopComponentOffer> Components => _components;

        internal void Replace(string day, int draw, IEnumerable<ShopOffer> offers)
        {
            Day = day;
            Draw = draw;
            _offers.Clear();
            _offers.AddRange(offers);
        }

        internal void Clear()
        {
            Replace("", 0, System.Array.Empty<ShopOffer>());
            _components.Clear();
            SeenDay = "";
        }

        internal void ReplaceComponents(System.Collections.Generic.IEnumerable<ShopComponentOffer> offers)
        {
            _components.Clear();
            _components.AddRange(offers);
        }

        internal void SetSeenDay(string day) => SeenDay = day ?? "";
    }
}
