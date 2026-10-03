#nullable enable
using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Screens;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Scope d'une scène d'écran (Hub, Board, Combat), enfant d'un scope chargé avant lui.
    /// À sa construction, il crée l'écran de la scène sous <see cref="ScreenRoot"/> (zone sûre du canvas) et l'affiche.
    /// La scène est générée par Heroes Dawn ▸ Setup ▸ Générer les scènes.
    /// </summary>
    public abstract class SceneScreenScope : LifetimeScope
    {
        [SerializeField] RectTransform? screenRoot;

        public RectTransform? ScreenRoot => screenRoot;
        public UiScreen? Screen { get; private set; }

        protected abstract UiScreen CreateScreen(RectTransform root, UiContext ctx);

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterBuildCallback(c =>
            {
                var root = screenRoot != null ? screenRoot : (RectTransform)transform;
                Screen = CreateScreen(root, c.Resolve<UiRoot>().Context);
                Screen.Show();
            });
        }
    }
}
