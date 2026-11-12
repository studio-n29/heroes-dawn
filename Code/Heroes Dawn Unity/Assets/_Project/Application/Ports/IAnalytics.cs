#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>
    /// Mesure d'usage (UGS Analytics). Les présentateurs n'y touchent jamais : seule la passerelle d'événements (<c>AnalyticsBridge</c>) émet,
    /// à partir des messages existants. Rien n'est émis tant que le joueur n'a pas consenti (<c>ConsentGatedAnalytics</c>).
    /// Schéma des événements : <c>04_Tech_Unity/10_Analytics.md</c>.
    /// </summary>
    public interface IAnalytics
    {
        /// <summary>Enregistre un événement nommé avec ses paramètres.</summary>
        void Track(string name, IReadOnlyDictionary<string, object>? parameters = null);

        /// <summary>Le consentement du joueur a changé : démarre ou arrête la collecte.</summary>
        void SetConsent(bool granted);
    }

    /// <summary>Noms des événements du jeu (snake_case, stables : ils servent de clés aux tableaux de bord).</summary>
    public static class AnalyticsEvents
    {
        public const string StateEntered = "state_entered";
        public const string ZoneValidated = "zone_validated";
        public const string CombatFinished = "combat_finished";
        public const string ConsentChanged = "consent_changed";
    }
}
