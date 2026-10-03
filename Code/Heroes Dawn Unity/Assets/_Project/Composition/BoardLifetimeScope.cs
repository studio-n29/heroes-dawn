#nullable enable
using N29.HeroesDawn.Presentation.Board;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>Scope de la scène Board : instancie la vue du plateau (prefab du catalogue), son présentateur et celui des cases.</summary>
    public sealed class BoardLifetimeScope : SceneScreenScope
    {
        protected override bool ConfigureScreen(IContainerBuilder builder, RectTransform root)
        {
            builder.RegisterComponentInNewPrefab(Catalog.Screen("Board").GetComponent<BoardView>(), Lifetime.Scoped)
                   .UnderTransform(root).As<IBoardView>();
            builder.Register<CasePopupPresenter>(Lifetime.Scoped);
            builder.RegisterEntryPoint<BoardPresenter>().AsSelf();
            return true;
        }
    }
}
