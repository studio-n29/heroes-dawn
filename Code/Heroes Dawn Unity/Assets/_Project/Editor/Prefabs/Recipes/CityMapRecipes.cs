#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Point précis de mission sur la carte : disque numéroté, anneau pulsé, coche, nom court dessous.</summary>
    public sealed class ZonePointRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ZonePoint.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ZonePoint", 100, 100);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            root.gameObject.AddComponent<CanvasGroup>();
            var ring = Ui.Img(root, "Ring", Theme.Primary, Sprites.Ring);
            ring.rectTransform.At(0.5f, 0.5f, 0, 0, 132, 132);
            ring.gameObject.AddComponent<Pulse>().Amplitude = 0.09f;
            ring.gameObject.SetActive(false);
            var frame = Ui.Img(root, "Frame", Theme.Ink, Sprites.Circle, true);
            frame.rectTransform.At(0.5f, 0.5f, 0, 0, 96, 96);
            var button = frame.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var disc = Ui.Img(frame.transform, "Disc", Theme.Info, Sprites.Circle);
            disc.rectTransform.Stretch(5, 5, 5, 5);
            var label = Ui.Txt(disc.transform, "", 34, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Label");
            label.rectTransform.Stretch();
            var check = Ui.Txt(root, "OK", 22, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Check");
            check.rectTransform.At(1f, 1f, 6, 6, 44, 30);
            check.Inked();
            check.gameObject.SetActive(false);
            var sub = Ui.Txt(root, "", 22, Theme.Text, TextAnchor.UpperCenter, FontKind.Body, "Sub");
            sub.rectTransform.At(0.5f, 0f, 0, -4, 240, 30);
            sub.raycastTarget = false;
            sub.Inked();

            var view = root.gameObject.AddComponent<ZonePointView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "disc", disc);
            ctx.Wire(view, "ring", ring);
            ctx.Wire(view, "label", label);
            ctx.Wire(view, "sub", sub);
            ctx.Wire(view, "check", check);
            return root.gameObject;
        }
    }

    /// <summary>Ligne de la liste des zones : nom à gauche, état à droite, cadre jaune quand elle est choisie.</summary>
    public sealed class ZoneRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ZoneRow.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("ZoneRow", 640, 66);
            root.Pref(640, 66);
            var rim = Ui.Img(root, "Rim", Theme.Ink, Sprites.Rounded, true);
            rim.type = Image.Type.Sliced;
            rim.rectTransform.Stretch();
            var button = rim.gameObject.AddComponent<Button>();
            button.targetGraphic = rim;
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var fill = Ui.Img(rim.transform, "Fill", Theme.Surface2, Sprites.Rounded);
            fill.type = Image.Type.Sliced;
            fill.rectTransform.Stretch(5, 5, 5, 5);
            var title = Ui.Txt(fill.transform, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.Stretch(18, 0, 200, 0);
            title.AutoSize(30);
            title.Inked(1.5f);
            var status = Ui.Txt(fill.transform, "", 24, Theme.Primary, TextAnchor.MiddleRight, FontKind.Body, "Status");
            status.rectTransform.Stretch(380, 0, 16, 0);
            var view = root.gameObject.AddComponent<ZoneRowView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "rim", rim);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "status", status);
            return root.gameObject;
        }
    }

    /// <summary>Page Carte de la ville (M60/M61) : carte à gauche, acte + liste + fiche de zone + DÉPLOYER à droite.</summary>
    public sealed class CityMapPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/CityMap.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<N29.HeroesDawn.Presentation.Hub.CityMapPageView>("CityMap", "CityMap/CityMap.uxml", 10);
    }
}
