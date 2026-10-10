#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page Équipement d'un agent (liste à une colonne).</summary>
    public sealed class LoadoutPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Loadout.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<LoadoutPageView>(ctx, "LoadoutPage", 0, false, false).gameObject;
    }

    /// <summary>Page Grimoire : la collection de cartes, l'ouverture des boosters.</summary>
    public sealed class GrimoirePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Grimoire.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<GrimoirePageView>(ctx, "GrimoirePage", 0, false, false).gameObject;
    }
}
