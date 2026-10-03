#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Shop;

namespace N29.HeroesDawn.Application.Features.Shop
{
    /// <summary>Jour courant et tirage de la boutique, partagés par les trois cas d'usage.</summary>
    static class ShopDrawing
    {
        public static string Today(GameSession s, IClock clock) => ShopDay.Of(clock.LocalNow, s.Content.Balance.ShopResetHour);

        public static bool IsCurrent(GameSession s, IClock clock) => s.Shop.Day.Length > 0 && Today(s, clock) == s.Shop.Day;

        /// <summary>Remplace les offres par le tirage <paramref name="draw"/> du jour <paramref name="day"/>.</summary>
        public static void Redraw(GameSession s, string day, int draw)
        {
            var eligible = ShopDraw.Eligible(s.Content.Members, s.Owns, z => s.CompletedZones.Contains(z.Value));
            var seed = ShopSeed.For(s.Account?.PlayerId ?? "", day, draw);
            var ids = ShopDraw.Draw(new Pcg32(seed), eligible, ShopWeights.From(s.Content.Balance));
            s.Shop.Replace(day, draw, ids.Select(id => new ShopOffer(id)));
        }
    }
}
