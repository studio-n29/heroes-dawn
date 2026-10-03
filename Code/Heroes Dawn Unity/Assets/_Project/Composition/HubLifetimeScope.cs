#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>Scope de la scène Hub : instancie la vue du QG (prefab du catalogue) et son présentateur.</summary>
    public sealed class HubLifetimeScope : SceneScreenScope
    {
        protected override bool ConfigureScreen(IContainerBuilder builder, RectTransform root)
        {
            builder.RegisterComponentInNewPrefab(Catalog.Screen("Hub").GetComponent<HubView>(), Lifetime.Scoped)
                   .UnderTransform(root).As<IHubView>();
            builder.RegisterEntryPoint<HubPresenter>().AsSelf();
            return true;
        }
    }
}
