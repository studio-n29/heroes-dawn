#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    public sealed class SettingRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/SettingRow.prefab";
        public int Order => 0;
        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("SettingRow", Theme.Surface2, 1.5f, 2, false);
            var root = fill.Frame(); root.sizeDelta = new Vector2(1184, 144); root.Pref(-1, 144, 1);
            var label = Ui.Txt(fill.transform, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Label");
            label.rectTransform.Stretch(24, 18, 536, 14);
            var hint = Ui.Txt(fill.transform, "", 23, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Hint");
            hint.rectTransform.Stretch(24, 12, 536, 76);
            hint.overflowMode = TMPro.TextOverflowModes.Ellipsis;
            var value = Ui.Txt(fill.transform, "", 28, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Value");
            value.rectTransform.At(1, .5f, -16, 0, 484, 96);
            value.AutoSize(28);
            var minus = Ui.Btn(fill.transform, "−", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "Minus");
            minus.Rect.At(1, .5f, -408, 0, 96, 96);
            var plus = Ui.Btn(fill.transform, "+", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "Plus");
            plus.Rect.At(1, .5f, -16, 0, 96, 96);
            var action = Ui.Btn(fill.transform, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Title, "Action");
            action.Rect.At(1, .5f, -16, 0, 484, 96);
            var view = root.gameObject.AddComponent<SettingRowView>();
            ctx.Wire(view, "label", label); ctx.Wire(view, "hint", hint); ctx.Wire(view, "value", value);
            ctx.Wire(view, "minus", minus); ctx.Wire(view, "plus", plus); ctx.Wire(view, "action", action);
            return root.gameObject;
        }
    }

    public sealed class SettingsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Settings.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx)
        {
            var root = ScreenKit.Root("SettingsPage");
            var railArea = Ui.Rect(root, "Categories");
            railArea.anchorMin = Vector2.zero; railArea.anchorMax = Vector2.up; railArea.pivot = new Vector2(0, .5f);
            railArea.offsetMin = Vector2.zero; railArea.offsetMax = new Vector2(280, 0);
            var rail = Ui.VScroll(railArea, out _); rail.VLayout(8, 0, TextAnchor.UpperLeft);
            var categories = new List<Object>();
            for (int i = 0; i < 8; i++)
            {
                var button = Ui.Btn(rail, "", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Category" + i);
                button.Rect.Pref(280, 96); categories.Add(button);
            }
            var portrait = Ui.Rect(root, "AgentArt");
            portrait.anchorMin = Vector2.zero; portrait.anchorMax = Vector2.up;
            portrait.offsetMin = new Vector2(294, 0); portrait.offsetMax = new Vector2(536, 0);
            portrait.gameObject.AddComponent<RectMask2D>();
            var source = AgentArt.For("giulia_rossi");
            var art = Ui.Img(portrait, "Portrait", Color.white, source);
            var fitter = art.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            if (source != null) fitter.aspectRatio = source.rect.width / source.rect.height;
            var body = Ui.Panel(root, "SettingsBody", Theme.Surface, 1.5f, 2, false);
            body.Frame().Stretch(550, 0, 0, 0);
            var title = Ui.Txt(body.transform, "", 42, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.anchorMin = Vector2.up; title.rectTransform.anchorMax = Vector2.one;
            title.rectTransform.offsetMin = new Vector2(22, -108); title.rectTransform.offsetMax = new Vector2(-356, -12);
            title.AutoSize(42);
            var reset = Ui.Btn(body.transform, "", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "ResetCategory");
            reset.Rect.At(1, 1, -16, -12, 320, 96);
            var area = Ui.Rect(body.transform, "Rows").Stretch(16, 16, 16, 124);
            var rows = Ui.VScroll(area, out _); rows.VLayout(12, 0, TextAnchor.UpperLeft);
            var view = root.gameObject.AddComponent<SettingsPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "categories", categories); ctx.Wire(view, "title", title);
            ctx.Wire(view, "resetCategory", reset); ctx.Wire(view, "rows", rows);
            ctx.Wire(view, "rowPrefab", ctx.Asset("Widgets/SettingRow.prefab").GetComponent<SettingRowView>());
            return root.gameObject;
        }
    }
}
