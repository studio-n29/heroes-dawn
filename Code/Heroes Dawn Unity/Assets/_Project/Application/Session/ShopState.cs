#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Boutique éphémère de la session (modifiée seulement par les cas d'usage de la boutique).</summary>
    public sealed class ShopState : IShopReadModel
    {
        readonly List<ShopOffer> _offers = new List<ShopOffer>();
        readonly List<ShopScrapOffer> _scraps = new List<ShopScrapOffer>();

        public string Day { get; private set; } = "";
        public int Draw { get; private set; }
        /// <summary>Dernier jour de boutique affiché sur l'onglet Recruter (vide = jamais vu).</summary>
        public string SeenDay { get; private set; } = "";
        public IReadOnlyList<ShopOffer> Offers => _offers;
        public IReadOnlyList<ShopScrapOffer> Scraps => _scraps;

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
            _scraps.Clear();
            SeenDay = "";
        }

        internal void ReplaceScraps(System.Collections.Generic.IEnumerable<ShopScrapOffer> scraps)
        {
            _scraps.Clear();
            _scraps.AddRange(scraps);
        }

        internal void SetSeenDay(string day) => SeenDay = day ?? "";
    }
}
