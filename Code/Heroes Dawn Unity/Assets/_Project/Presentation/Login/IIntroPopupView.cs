#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Première connexion (popup plein écran) en deux temps : cinématique de lancement, choix du personnage joueur.
    /// Chaque étape se termine quand le joueur agit (toucher, « Passer », « Commencer »).
    /// </summary>
    public interface IIntroPopupView : IPopupView
    {
        /// <summary>
        /// Joue les plans l'un après l'autre ; se termine à la fin ou quand le joueur passe la cinématique. <paramref name="mainTheme"/> règle le niveau
        /// (0 à 1) du thème principal du jeu : coupé pendant la cinématique, rouvert pour sa dernière séquence.
        /// </summary>
        UniTask PlayCinematicAsync(IReadOnlyList<CinematicShot> shots, IntroCinematicTexts texts, Action<float>? mainTheme, CancellationToken ct);

        /// <summary>Propose les campagnes ; seules celles qui sont disponibles se choisissent. Renvoie l'identifiant choisi.</summary>
        UniTask<string> ChooseCampaignAsync(ChooseCampaignData data, CancellationToken ct);
    }
}
