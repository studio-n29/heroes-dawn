#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Base des recettes de popups de case : gabarit commun et branchement de la base de vue.</summary>
    static class CasePopupKit
    {
        public static (RectTransform Root, RectTransform Content, TextMeshProUGUI Title) Build(string name, float width, float height, Color band,
                                                                                            out RectTransform window)
        {
            var root = PrefabContext.NewRoot(name);
            root.gameObject.AddComponent<CanvasGroup>();
            var (w, content, title) = PopupKit.Window(root, width, height, band);
            window = w;
            return (root, content, title);
        }

        /// <summary>Laurées dorées de part et d'autre du titre (kit d'extension Aube tactique).</summary>
        public static void Laurels(RectTransform window)
        {
            var band = window.Find("TitleBand") as RectTransform;
            var left = KitArt.GetExt("Decorations/laurel-left");
            var right = KitArt.GetExt("Decorations/laurel-right");
            if (band == null || left == null || right == null) return;
            var l = Ui.Img(band, "LaurelLeft", Color.white, left);
            l.rectTransform.At(0f, 0.5f, 70, 0, 84, 84);
            l.preserveAspect = true;
            var r = Ui.Img(band, "LaurelRight", Color.white, right);
            r.rectTransform.At(1f, 0.5f, -70, 0, 84, 84);
            r.preserveAspect = true;
        }

        public static void WireBase(PrefabContext ctx, PopupViewBase view, TMP_Text title, RectTransform window)
        {
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", view.GetComponent<CanvasGroup>());
        }
    }

    /// <summary>Bouton de choix d'événement (pleine largeur).</summary>
    public sealed class ChoiceButtonRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ChoiceButton.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var holder = PrefabContext.NewRoot("holder", 1000, 86);
            var b = Ui.Btn(holder, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Body, "ChoiceButton");
            b.Rect.SetParent(null, false);
            Object.DestroyImmediate(holder.gameObject);
            b.Rect.sizeDelta = new Vector2(1000, 86);
            b.Rect.Pref(-1, 86);
            return b.gameObject;
        }
    }

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
