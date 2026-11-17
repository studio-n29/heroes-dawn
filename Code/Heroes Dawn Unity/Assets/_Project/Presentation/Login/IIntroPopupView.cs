#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Première connexion (popup plein écran) : cinématique de lancement, puis la séquence de Rayner (accueil en Full Art, fiche d'identification,
    /// réplique de conclusion, choix de la ville de départ). Chaque étape se termine quand le joueur agit (toucher, « Passer », validation).
    /// </summary>
    public interface IIntroPopupView : IPopupView
    {
        /// <summary>
        /// Joue la cinématique de lancement (vidéos et voix off) ; se termine à la fin ou quand le joueur la passe. <paramref name="mainTheme"/> règle le niveau
        /// (0 à 1) du thème principal du jeu : coupé pendant la cinématique, rouvert pour sa dernière séquence.
        /// </summary>
        UniTask PlayCinematicAsync(IntroCinematicTexts texts, Action<float>? mainTheme, CancellationToken ct, CinematicKind kind = CinematicKind.Launch, Action<string>? voiceCue = null);

        /// <summary>Rayner en Full Art : joue ses répliques, un toucher passe à la suivante ; se termine après la dernière.</summary>
        UniTask ShowRaynerAsync(RaynerSceneData scene, CancellationToken ct);

        /// <summary>La fiche d'identification (nom, prénom, âge, avatar, pouvoir) ; se termine quand elle est complète et validée.</summary>
        UniTask<AgentForm> FillFormAsync(AgentFormData data, CancellationToken ct);

        /// <summary>Choix de la ville de départ ; rend l'identifiant du parcours retenu (jamais un parcours indisponible).</summary>
        UniTask<string> ChooseStartAsync(StartChoiceData data, CancellationToken ct);
    }
}
