#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Première connexion (popup plein écran) en deux temps : cinématique d introduction, choix du personnage joueur.
    /// Chaque étape se termine quand le joueur agit (toucher, « Passer », « Valider »).
    /// </summary>
    public interface IIntroPopupView : IPopupView
    {
        /// <summary>Joue les écrans l'un après l'autre ; se termine à la fin ou quand le joueur passe la cinématique.</summary>
        UniTask PlayCinematicAsync(IReadOnlyList<IntroSlideData> slides, IntroCinematicTexts texts, CancellationToken ct);

        /// <summary>Propose les campagnes ; seules celles qui sont disponibles se choisissent. Renvoie l'identifiant choisi.</summary>
        UniTask<string> ChooseCampaignAsync(ChooseCampaignData data, CancellationToken ct);
    }
}
