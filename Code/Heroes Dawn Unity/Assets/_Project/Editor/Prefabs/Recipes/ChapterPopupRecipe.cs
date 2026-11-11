#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ouverture d'un chapitre : image large (facultative), intitulé, texte d'ouverture, bouton Continuer.</summary>
    public sealed class ChapterPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ChapterPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.ChapterPopupView>("ChapterPopup", "Popups/Chapter.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
