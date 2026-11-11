#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ouverture d'un booster : fenêtre UI Toolkit plein écran (paquet, cartes retournées, résumé).</summary>
    public sealed class BoosterOpeningPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/BoosterOpeningPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.BoosterOpeningPopupView>("BoosterOpeningPopup", "Popups/BoosterOpening.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
