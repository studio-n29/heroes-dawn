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

        const float MapW = 1230, RightW = 640;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("CityMapPage");

            // ---- Carte (gauche)
            var mapPanel = Ui.Panel(rt, "Map", Theme.WithAlpha(Theme.SurfaceDark, 0.88f), 5f, 8f, false);
            var mapFrame = mapPanel.Frame();
            mapFrame.anchorMin = new Vector2(0, 0);
            mapFrame.anchorMax = new Vector2(0, 1);
            mapFrame.pivot = new Vector2(0, 0.5f);
            mapFrame.anchoredPosition = Vector2.zero;
            mapFrame.sizeDelta = new Vector2(MapW, 0);
            var mapArea = Ui.Rect(mapPanel.transform, "Area").Stretch(18, 18, 18, 18);

            // ---- Colonne de droite
            var column = Ui.Rect(rt, "Column");
            column.anchorMin = new Vector2(1, 0);
            column.anchorMax = new Vector2(1, 1);
            column.pivot = new Vector2(1, 0.5f);
            column.anchoredPosition = Vector2.zero;
            column.sizeDelta = new Vector2(RightW, 0);

            var act = Ui.Panel(column, "Act", Theme.Primary, 4f, 6f, false);
            act.Frame().At(0f, 1f, 0, 0, RightW, 52);
            var actTitle = Ui.Txt(act.transform, "", 34, Theme.TextDark, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            actTitle.rectTransform.Stretch(18, 0, 18, 0);
            actTitle.AutoSize(34);

            // Les zones se choisissent sur la carte (points de 96 px) : pas de liste dupliquée.
            var rows = new List<Object>();

            var detail = Ui.Panel(column, "Detail", Theme.Surface, 5f, 8f, true);
            var detailFrame = detail.Frame();
            detailFrame.anchorMin = new Vector2(0, 0);
            detailFrame.anchorMax = new Vector2(1, 1);
            detailFrame.offsetMin = new Vector2(0, 108);
            detailFrame.offsetMax = new Vector2(0, -62);
            var title = Ui.Txt(detail.transform, "", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 18, -8, RightW - 36, 44);
            title.AutoSize(34);
            title.Inked();
            var description = Ui.Txt(detail.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Description");
            description.rectTransform.At(0f, 1f, 18, -52, RightW - 36, 72);
            var chips = Ui.Txt(detail.transform, "", 22, Theme.Info, TextAnchor.MiddleLeft, FontKind.Body, "Chips");
            chips.rectTransform.At(0f, 1f, 18, -126, RightW - 36, 30);
            var objectives = Ui.Rect(detail.transform, "Objectives").At(0f, 1f, 18, -158, RightW - 36, 100);
            objectives.VLayout(2, 0, TextAnchor.UpperLeft);
            var rewards = Ui.Txt(detail.transform, "", 24, Theme.Success, TextAnchor.MiddleLeft, FontKind.Body, "Rewards");
            rewards.rectTransform.At(0f, 0f, 18, 10, RightW - 36, 34);

            var deploy = Ui.Btn(column, "", Theme.Danger, Color.white, null!, 54, FontKind.Title, "Deploy");
            deploy.Rect.At(0.5f, 0f, 0, 0, RightW, 96);
            deploy.Label.Inked();
            RecipeKit.Mark(deploy, TutorialTarget.MapDeploy);

            var tagTemplate = Ui.Txt(mapArea, "", 28, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Title, "TagTemplate");
            tagTemplate.raycastTarget = false;
            tagTemplate.Inked();
            tagTemplate.gameObject.SetActive(false);
            var view = rt.gameObject.AddComponent<CityMapPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "mapArea", mapArea);
            ctx.Wire(view, "tagTemplate", tagTemplate);
            ctx.Wire(view, "pointPrefab", ctx.Asset("Widgets/ZonePoint.prefab").GetComponent<ZonePointView>());
            ctx.Wire(view, "lockedMaterial", RecipeKit.Mat("UiHatch"));
            ctx.Wire(view, "actTitle", actTitle);
            ctx.WireArray(view, "rows", rows);
            ctx.Wire(view, "detailTitle", title);
            ctx.Wire(view, "description", description);
            ctx.Wire(view, "chips", chips);
            ctx.Wire(view, "objectives", objectives);
            ctx.Wire(view, "objectiveLinePrefab", ctx.Asset("Widgets/TextLine.prefab").GetComponent<TextLineView>());
            ctx.Wire(view, "rewards", rewards);
            ctx.Wire(view, "deploy", deploy);
            return rt.gameObject;
        }
    }
}
