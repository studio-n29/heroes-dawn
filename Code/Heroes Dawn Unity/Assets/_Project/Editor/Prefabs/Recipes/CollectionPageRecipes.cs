#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page Équipement d'un agent : ses objets et cartes équipés, puis la réserve, en grille avec fiche détaillée.</summary>
    public sealed class LoadoutPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Loadout.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<LoadoutPageView>("LoadoutPage", "CardGrid/CardGrid.uxml", 10);
    }

    /// <summary>Registre des agents : les 30 agents et leur voie d obtention.</summary>
    public sealed class AgentsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Agents.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<AgentsPageView>("AgentsPage", "CardGrid/CardGrid.uxml", 10);
    }

    /// <summary>Page Grimoire : la collection de cartes en grille, l'ouverture des boosters, et la recherche par nom dans l'en-tête.</summary>
    public sealed class GrimoirePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Grimoire.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            return N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<GrimoirePageView>("GrimoirePage", "CardGrid/CardGrid.uxml", 10);
        }
    }
}

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page Inventaire : les objets possédés en grille avec fiche détaillée.</summary>
    public sealed class InventoryPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Inventory.prefab";
        public int Order => 10;
        public UnityEngine.GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.InventoryPageView>("InventoryPage", "CardGrid/CardGrid.uxml", 10);
    }
}

