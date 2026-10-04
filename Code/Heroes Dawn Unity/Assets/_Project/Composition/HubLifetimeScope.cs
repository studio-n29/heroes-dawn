#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Scope de la scène Hub : instancie la coque (prefab du catalogue), le navigateur et la fabrique de pages ;
    /// la coque crée chaque page dans un scope enfant.
    /// </summary>
    public sealed class HubLifetimeScope : SceneScreenScope
    {
        protected override void ConfigureScreen(IContainerBuilder builder, RectTransform root)
        {
            builder.RegisterComponentInNewPrefab(Catalog.Screen("Hub").GetComponent<HubShellView>(), Lifetime.Scoped)
                   .UnderTransform(root).As<IHubShellView>().AsSelf();
            // Singleton dans ce scope : les scopes enfants des pages partagent le navigateur de la coque (Scoped en créerait un par page).
            builder.Register<HubNavigator>(Lifetime.Singleton);
            builder.Register<IHubPageFactory, HubPageFactory>(Lifetime.Singleton);
            builder.RegisterEntryPoint<HubShellPresenter>().AsSelf();
        }
    }
}
