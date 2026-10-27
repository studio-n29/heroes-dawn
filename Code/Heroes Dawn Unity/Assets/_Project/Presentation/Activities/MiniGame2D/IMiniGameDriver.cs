#nullable enable
using N29.HeroesDawn.Domain.Activities.Games;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Pilote d'un mini-jeu 2D : construit les éléments dans la zone de jeu, les met à jour à chaque image, relaie les entrées vers la partie du domaine.
    /// La fenêtre commune ne connaît que cette interface.
    /// </summary>
    public interface IMiniGameDriver
    {
        /// <summary>Vrai si le jeu se pilote au joystick flottant (<see cref="MiniGameInput.Stick"/>).</summary>
        bool UsesStick { get; }
        IMiniGame Game { get; }
        MiniGameHud Hud { get; }
        void Build(RectTransform field);
        void Tick(float dt, MiniGameInput input);
        /// <summary>Détruit les éléments créés dans la zone ; peut être appelé plusieurs fois.</summary>
        void Dispose();
    }
}
