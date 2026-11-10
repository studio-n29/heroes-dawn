#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Formulaire de retour de playtest : quatre questions notées de 1 à 5, un champ de commentaire, Envoyer / Plus tard.</summary>
    public sealed class PlaytestFormPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/PlaytestFormPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.PlaytestFormPopupView>("PlaytestFormPopup", "Popups/PlaytestForm.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
