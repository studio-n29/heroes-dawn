#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page Paramètres (UI Toolkit) : catégories à gauche, réglages à droite.</summary>
    public sealed class SettingsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Settings.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<SettingsPageView>("SettingsPage", "Settings/Settings.uxml", 10);
    }
}
