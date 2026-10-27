#nullable enable
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Compteurs de statistiques du profil (combats gagnés et perdus, Crédits gagnés au total) dans <see cref="MetaSave.stats"/>.</summary>
    public static class Stats
    {
        public const string CombatsWon = "combatsWon", CombatsLost = "combatsLost", CreditsEarned = "creditsEarned";

        public static int Get(MetaSave m, string id) => m.GetCounter(m.stats, id);

        public static void Add(MetaSave m, string id, int n) => m.SetCounter(m.stats, id, m.GetCounter(m.stats, id) + n);
    }
}
