#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Saisie du tag d'un ami : titre, champ de texte sur une ligne, Ajouter / Annuler.</summary>
    public sealed class FriendAddPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/FriendAddPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.FriendAddPopupView>("FriendAddPopup", "Popups/FriendAdd.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
