#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>La potion magique : fenêtre UI Toolkit (livre d'indices, ingrédients, quantité, étapes, « Préparer »).</summary>
    public sealed class PotionPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/PotionPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<PotionPopupView>("PotionPopup", "Popups/Potion.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Le dialogue : réplique en haut, réponses au choix, réaction et progression.</summary>
    public sealed class DialoguePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/DialoguePopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<DialoguePopupView>("DialoguePopup", "Popups/Dialogue.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Activités 3D (survie, pêche, arène) : image de la scène, HUD, joystick, deux boutons, introduction et choix de l'agent (UI Toolkit).</summary>
    public sealed class StagePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/StagePopup.prefab";
        public int Order => 11;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<StageGameView>("StagePopup", "Popups/StageGame.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
