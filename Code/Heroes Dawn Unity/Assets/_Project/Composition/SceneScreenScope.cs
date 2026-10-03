#nullable enable
using UnityEngine;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Scope d'une scène d'écran (Hub, Board, Combat), enfant d'un scope chargé avant lui.
    /// La scène est générée par Heroes Dawn ▸ Setup ▸ Générer les scènes ; <see cref="ScreenRoot"/> est la zone sûre de son canvas.
    /// </summary>
    public abstract class SceneScreenScope : LifetimeScope
    {
        [SerializeField] RectTransform? screenRoot;

        public RectTransform? ScreenRoot => screenRoot;
    }
}
