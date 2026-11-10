#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{

    /// <summary>Pop-up d'affinités : titre, rappel de la règle, liste défilante des agents avec le bonus de chaque duo, bouton Fermer.</summary>
    public sealed class AffinityPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/AffinityPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.AffinityPopupView>("AffinityPopup", "Popups/Affinity.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
