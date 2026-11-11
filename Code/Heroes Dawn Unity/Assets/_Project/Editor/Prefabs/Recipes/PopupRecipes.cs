#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Case Repos (reprise de CasePopups.Rest).</summary>
    public sealed class RestPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/RestPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Popups.RestPopupView>("RestPopup", "Popups/Rest.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Case Événement (reprise de CasePopups.Narrative).</summary>
    public sealed class NarrativePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/NarrativePopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Popups.NarrativePopupView>("NarrativePopup", "Popups/Narrative.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Case Mini-jeu : fenêtre UI Toolkit avec le jeu de réflexe (QTE).</summary>
    public sealed class MiniGamePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/MiniGamePopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Popups.MiniGamePopupView>("MiniGamePopup", "Popups/MiniGame.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Activité hors réflexe (potion, survie, pêche, arène, dialogue) : fenêtre de test du framework d'activités.</summary>
    public sealed class ActivityPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ActivityPopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Popups.ActivityPopupView>("ActivityPopup", "Popups/Activity.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }

    /// <summary>Zone sécurisée (reprise de BoardScreen.ShowZoneComplete).</summary>
    public sealed class ZoneCompletePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/ZoneCompletePopup.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Popups.ZoneCompletePopupView>("ZoneCompletePopup", "Popups/ZoneComplete.uxml", N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.ModalOrder);
    }
}
