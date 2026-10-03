#nullable enable
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Scope d'une scène d'écran (Hub, Board, Combat), enfant d'un scope chargé avant lui.
    /// Il instancie le prefab d'écran du catalogue sous <see cref="ScreenRoot"/> (zone sûre du canvas) et enregistre son présentateur.
    /// La scène est générée par Heroes Dawn ▸ Setup ▸ Générer les scènes.
    /// </summary>
    public abstract class SceneScreenScope : LifetimeScope
    {
        [SerializeField] RectTransform? screenRoot;
        [SerializeField] ScreenCatalog? catalog;

        public RectTransform? ScreenRoot => screenRoot;

        protected ScreenCatalog Catalog => catalog != null ? catalog
            : throw new System.InvalidOperationException("ScreenCatalog manquant sur " + GetType().Name + " : relancer Heroes Dawn ▸ Setup ▸ Générer les scènes.");

        /// <summary>Instancie la vue sous <paramref name="root"/> et enregistre vue et présentateur.</summary>
        protected abstract void ConfigureScreen(IContainerBuilder builder, RectTransform root);

        protected override void Configure(IContainerBuilder builder)
        {
            var root = screenRoot != null ? screenRoot : (RectTransform)transform;
            ConfigureScreen(builder, root);
            // Les boutons de l'écran trouvent l'audio parmi leurs parents.
            builder.RegisterBuildCallback(c => UiSoundRelay.Attach(root.gameObject, c.Resolve<IAudioService>()));
        }
    }
}
