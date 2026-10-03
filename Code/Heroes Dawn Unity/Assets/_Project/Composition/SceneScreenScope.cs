#nullable enable
using N29.HeroesDawn.Presentation;
using N29.HeroesDawn.Presentation.Mvp;
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
        [SerializeField] ScreenCatalog? catalog;

        public RectTransform? ScreenRoot => screenRoot;
        public UiScreen? Screen { get; private set; }

        protected ScreenCatalog Catalog => catalog != null ? catalog
            : throw new System.InvalidOperationException("ScreenCatalog manquant sur " + GetType().Name + " : relancer Heroes Dawn ▸ Setup ▸ Générer les scènes.");

        /// <summary>
        /// Écran en prefab : instancie la vue sous <paramref name="root"/> et enregistre vue et présentateur.
        /// Renvoie false tant que l'écran de la scène est encore construit par code (<see cref="CreateScreen"/>).
        /// </summary>
        protected virtual bool ConfigureScreen(IContainerBuilder builder, RectTransform root) => false;

        /// <summary>Écran encore construit par code (avant sa conversion en prefab).</summary>
        protected virtual UiScreen? CreateScreen(RectTransform root, UiContext ctx) => null;

        protected override void Configure(IContainerBuilder builder)
        {
            var screenParent = screenRoot != null ? screenRoot : (RectTransform)transform;
            if (ConfigureScreen(builder, screenParent)) return;
            builder.RegisterBuildCallback(c =>
            {
                var root = screenRoot != null ? screenRoot : (RectTransform)transform;
                Screen = CreateScreen(root, c.Resolve<UiRoot>().Context);
                Screen?.Show();
            });
        }
    }
}
