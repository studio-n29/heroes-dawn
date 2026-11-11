#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Fenêtre des mini-jeux 2D de héros : bandeau (score, temps, vies), zone de jeu tactile, flèches de couloir, Abandonner (UI Toolkit).</summary>
    public sealed class HeroGamePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/HeroGamePopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<HeroGamePopupView>("HeroGamePopup", "Popups/HeroGame.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
