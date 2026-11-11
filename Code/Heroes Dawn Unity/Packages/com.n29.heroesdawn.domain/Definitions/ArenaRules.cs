#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    public enum DuelMode { Offline, Online }

    /// <summary>Issue d'un duel du point de vue du joueur : Quit = il a quitté, Cancelled = annulé (désynchronisation, déconnexion avant le début).</summary>
    public enum DuelOutcome { Win, Loss, Quit, Cancelled }

    /// <summary>Règles de l'arène des tournois : points, plafond quotidien, délais et paliers (bloc D de la 1.0.0).</summary>
    public static class ArenaRules
    {
        public const int WinOffline = 10, WinOnline = 20, QuitOnlinePenalty = 10, DailyDuelCap = 10;
        public const int QueueSeconds = 15, PassSeconds = 30, SilenceSeconds = 45, ClaimDays = 7;
        /// <summary>Tous les combien de ticks chaque appareil envoie l'empreinte de l'état.</summary>
        public const int HashEveryTicks = 20;

        public static readonly IReadOnlyList<int> TierPoints = new[] { 50, 150, 300, 500, 800 };

        /// <summary>Variation du score : jamais de perte sauf en quittant un duel en ligne, et jamais de score négatif.</summary>
        public static int PointsDelta(DuelMode mode, DuelOutcome outcome, int currentPoints)
        {
            switch (outcome)
            {
                case DuelOutcome.Win: return mode == DuelMode.Online ? WinOnline : WinOffline;
                case DuelOutcome.Quit when mode == DuelMode.Online: return -Math.Min(QuitOnlinePenalty, Math.Max(0, currentPoints));
                default: return 0;
            }
        }

        /// <summary>Le duel compte dans le plafond quotidien : faux pour un duel annulé et pour un départ hors ligne.</summary>
        public static bool CountsAsDuel(DuelMode mode, DuelOutcome outcome)
            => outcome != DuelOutcome.Cancelled && !(outcome == DuelOutcome.Quit && mode == DuelMode.Offline);

        /// <summary>Nombre de paliers atteints (0 à 5).</summary>
        public static int TierReached(int points)
        {
            int tier = 0;
            for (int i = 0; i < TierPoints.Count; i++) if (points >= TierPoints[i]) tier = i + 1;
            return tier;
        }

        public static bool CanStartDuel(int duelsToday) => duelsToday < DailyDuelCap;
    }
}
