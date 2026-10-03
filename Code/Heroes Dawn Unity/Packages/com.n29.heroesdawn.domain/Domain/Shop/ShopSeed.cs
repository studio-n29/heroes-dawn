#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Shop
{
    /// <summary>Graine du tirage, recalculable par un serveur : joueur + jour de boutique + numéro de tirage du jour.</summary>
    public static class ShopSeed
    {
        public static ulong For(string playerId, string day, int draw)
        {
            var h = new Fnv1a64();
            // Longueur avant chaque texte : « ab » + « c » ≠ « a » + « bc ».
            h.Add(playerId.Length);
            h.Add(playerId);
            h.Add(day.Length);
            h.Add(day);
            h.Add(draw);
            return h.Value;
        }
    }
}
