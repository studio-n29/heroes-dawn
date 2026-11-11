#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page à venir (Pass, Boutique, Opérations, Labo, Archives) : nom de la fonction, bandeau BIENTÔT, ce qui est prévu.</summary>
    public sealed class WipPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/WipPage.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<WipPageView>("WipPage", "Wip/Wip.uxml", 10);
    }
}
