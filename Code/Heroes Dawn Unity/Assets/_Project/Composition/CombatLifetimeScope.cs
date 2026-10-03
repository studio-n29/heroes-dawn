#nullable enable
using N29.HeroesDawn.Presentation.Combat;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>Scope de la scène Combat (enfant du plateau) : instancie la vue du combat (prefab du catalogue) et son présentateur.</summary>
    public sealed class CombatLifetimeScope : SceneScreenScope
    {
        protected override void ConfigureScreen(IContainerBuilder builder, RectTransform root)
        {
            builder.RegisterComponentInNewPrefab(Catalog.Screen("Combat").GetComponent<CombatView>(), Lifetime.Scoped)
                   .UnderTransform(root).As<ICombatView>();
            builder.RegisterEntryPoint<CombatPresenter>().AsSelf();
        }
    }
}
