#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Fenêtre commune des mini-jeux 2D : fenêtre UI Toolkit (bandeau score / temps / vies, zone tactile, joystick flottant, Abandonner).</summary>
    public sealed class MiniGame2DPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/MiniGame2DPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<MiniGame2DPopupView>("MiniGame2DPopup", "Popups/MiniGame2D.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
