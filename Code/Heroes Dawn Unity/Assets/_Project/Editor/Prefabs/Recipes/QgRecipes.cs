#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page QG : document UI Toolkit (Screens/Qg/Qg.uxml).</summary>
    public sealed class QgPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Qg.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
            => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<QgPageView>("QgPage", "Qg/Qg.uxml", 10);
    }
}
