#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ligne de membre des pièces (Salon, Infirmerie) : portrait, nom, niveau, fatigue et palier, bouton d'action de 96 px.</summary>
    public sealed class MemberRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/MemberRow.prefab";
        public int Order => 6;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("MemberRow", Theme.Surface, 4f, 6f);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(1100, 124);
            frame.Pref(-1, 124, 1);
            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.RoundedSmall);
            strip.rectTransform.anchorMin = new Vector2(0, 0);
            strip.rectTransform.anchorMax = new Vector2(0, 1);
            strip.rectTransform.pivot = new Vector2(0, 0.5f);
            strip.rectTransform.offsetMin = new Vector2(0, 0);
            strip.rectTransform.offsetMax = new Vector2(10, 0);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 0.5f, 22, 0, 96, 96);
            var name = Ui.Txt(fill.transform, "", 34, Theme.Navy, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 132, -10, 460, 42);
            name.Inked(1.5f);
            var detail = Ui.Txt(fill.transform, "", 22, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Regular, "Detail");
            detail.rectTransform.At(0f, 1f, 132, -50, 560, 28);
            var bar = Ui.Bar(fill.transform, "Fatigue", Theme.Success, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 0f, 132, 10, 420, 26);
            var band = Ui.Txt(fill.transform, "", 28, Theme.Success, TextAnchor.MiddleLeft, FontKind.Title, "Band");
            band.rectTransform.At(0f, 0f, 572, 8, 240, 40);
            var action = Ui.Btn(fill.transform, "", Theme.Info, Theme.TextDark, null!, 30, FontKind.Title, "Action");
            action.Rect.At(1f, 0.5f, -14, 0, 380, 96);
            var reason = Ui.Txt(fill.transform, "", 22, Theme.Danger, TextAnchor.MiddleRight, FontKind.Body, "Reason");
            reason.rectTransform.At(1f, 0.5f, -410, 0, 300, 60);
            var view = frame.gameObject.AddComponent<MemberRowView>();
            ctx.Wire(view, "strip", strip);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "detail", detail);
            ctx.Wire(view, "fatigue", bar);
            ctx.Wire(view, "band", band);
            ctx.Wire(view, "action", action);
            ctx.Wire(view, "reason", reason);
            return frame.gameObject;
        }
    }

    static class RoomKit
    {
        /// <summary>Bandeau « version simple » en haut de la pièce.</summary>
        public static TMPro.TextMeshProUGUI Banner(RectTransform rt)
        {
            var band = Ui.Img(rt, "Banner", Theme.Surface2, Sprites.RoundedSmall);
            band.type = Image.Type.Sliced;
            band.rectTransform.TopBand(70);
            var text = Ui.Txt(band.transform, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Text");
            text.rectTransform.Stretch(20, 0, 20, 0);
            return text;
        }

        /// <summary>Liste défilante de lignes de membre ; renvoie le contenu.</summary>
        public static RectTransform List(RectTransform rt, float bottom)
        {
            var area = Ui.Rect(rt, "ListArea").Stretch(0, bottom, 0, 86);
            var list = Ui.VScroll(area, out _);
            list.VLayout(12, 0, TextAnchor.UpperCenter);
            return list;
        }
    }

    /// <summary>Salon d'entraînement, version simple : bandeau, une ligne par membre avec Entraîner · N CR.</summary>
    public sealed class SalonPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Salon.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("SalonPage");
            var banner = RoomKit.Banner(rt);
            var list = RoomKit.List(rt, 0);
            var view = rt.gameObject.AddComponent<SalonPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "banner", banner);
            ctx.Wire(view, "list", list);
            ctx.Wire(view, "rowPrefab", ctx.Asset("Widgets/MemberRow.prefab").GetComponent<MemberRowView>());
            return rt.gameObject;
        }
    }

    /// <summary>Infirmerie, version simple : bandeau, fatigue de chaque membre, soin de toute l'équipe en bas.</summary>
    public sealed class InfirmaryPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Infirmary.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("InfirmaryPage");
            var banner = RoomKit.Banner(rt);
            var list = RoomKit.List(rt, 134);
            var reason = Ui.Txt(rt, "", 26, Theme.Danger, TextAnchor.MiddleLeft, FontKind.Body, "HealReason");
            reason.rectTransform.At(0f, 0f, 0, 0, 1000, 110);
            var heal = Ui.Btn(rt, "", Theme.Success, Theme.TextDark, null!, 36, FontKind.Title, "Heal");
            heal.Rect.At(1f, 0f, 0, 0, 640, 112);
            var view = rt.gameObject.AddComponent<InfirmaryPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "banner", banner);
            ctx.Wire(view, "list", list);
            ctx.Wire(view, "rowPrefab", ctx.Asset("Widgets/MemberRow.prefab").GetComponent<MemberRowView>());
            ctx.Wire(view, "heal", heal);
            ctx.Wire(view, "healReason", reason);
            return rt.gameObject;
        }
    }


    /// <summary>Ligne de réglage : libellé + aide à gauche, « − valeur + » / interrupteur / bouton / valeur à droite (92 px).</summary>
    public sealed class SettingRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/SettingRow.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("SettingRow", 1000, 116);
            root.Pref(-1, 116, 1);
            var rim = Ui.Img(root, "Rim", Theme.Ink, Sprites.Rounded);
            rim.type = Image.Type.Sliced;
            rim.rectTransform.Stretch();
            var fill = Ui.Img(rim.transform, "Fill", Theme.Surface2, Sprites.Rounded);
            fill.type = Image.Type.Sliced;
            fill.rectTransform.Stretch(4, 4, 4, 4);

            var label = Ui.Txt(fill.transform, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Label");
            label.rectTransform.anchorMin = new Vector2(0, 0.5f);
            label.rectTransform.anchorMax = new Vector2(1, 1);
            label.rectTransform.offsetMin = new Vector2(22, 0);
            label.rectTransform.offsetMax = new Vector2(-480, -4);
            label.AutoSize(30);
            label.Inked(1.5f);
            var hint = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Hint");
            hint.rectTransform.anchorMin = new Vector2(0, 0);
            hint.rectTransform.anchorMax = new Vector2(1, 0.5f);
            hint.rectTransform.offsetMin = new Vector2(22, 4);
            hint.rectTransform.offsetMax = new Vector2(-480, 0);

            var minus = Ui.Btn(fill.transform, "-", Theme.Surface3, Theme.Text, null!, 40, FontKind.Title, "Minus");
            minus.Rect.At(1f, 0.5f, -366, 0, 96, 96);
            var value = Ui.Txt(fill.transform, "", 28, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Value");
            value.rectTransform.At(1f, 0.5f, -116, 0, 240, 96);
                        value.AutoSize(28);
            var plus = Ui.Btn(fill.transform, "+", Theme.Surface3, Theme.Text, null!, 40, FontKind.Title, "Plus");
            plus.Rect.At(1f, 0.5f, -14, 0, 96, 96);
            var action = Ui.Btn(fill.transform, "", Theme.Primary, Theme.TextDark, null!, 26, FontKind.Title, "Action");
            action.Rect.At(1f, 0.5f, -14, 0, 440, 96);

            var view = root.gameObject.AddComponent<SettingRowView>();
            ctx.Wire(view, "label", label);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "value", value);
            ctx.Wire(view, "minus", minus);
            ctx.Wire(view, "plus", plus);
            ctx.Wire(view, "action", action);
            return root.gameObject;
        }
    }

    /// <summary>Paramètres (M80-M85) : 8 catégories à gauche, réglages de la catégorie à droite (défilants), « Réinitialiser » en tête.</summary>
    public sealed class SettingsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Settings.prefab";
        public int Order => 10;

        const float LeftW = 400;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("SettingsPage");
            var column = Ui.Rect(rt, "Categories");
            column.anchorMin = new Vector2(0, 1);
            column.anchorMax = new Vector2(1, 1);
            column.pivot = new Vector2(0.5f, 1);
            column.sizeDelta = new Vector2(0, 96);
            column.anchoredPosition = Vector2.zero;
            var h = column.HLayout(8, 0, TextAnchor.UpperLeft);
            h.childControlWidth = true;
            h.childForceExpandWidth = true;
            var categories = new List<Object>();
            for (int i = 0; i < 8; i++)
            {
                var b = Ui.Btn(column, "", Theme.Surface3, Theme.Text, null!, 26, FontKind.Title, "Category" + i);
                b.Rect.Pref(100, 96, 1);
                categories.Add(b);
            }

            var panel = Ui.Panel(rt, "Panel", Theme.WithAlpha(Theme.Frost, 0.92f), 5f, 10f, true);
            var frame = panel.Frame();
            frame.anchorMin = Vector2.zero;
            frame.anchorMax = Vector2.one;
            frame.offsetMin = Vector2.zero;
            frame.offsetMax = new Vector2(0, -108);
            var title = Ui.Txt(panel.transform, "", 42, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 24, -10, 700, 56);
            title.Inked();
            var reset = Ui.Btn(panel.transform, "", Theme.Surface3, Theme.Text, null!, 26, FontKind.Title, "ResetCategory");
            reset.Rect.At(1f, 1f, -20, -8, 330, 96);
            var listArea = Ui.Rect(panel.transform, "Rows");
            listArea.anchorMin = Vector2.zero;
            listArea.anchorMax = Vector2.one;
            listArea.offsetMin = new Vector2(16, 12);
            listArea.offsetMax = new Vector2(-16, -112);
            var content = Ui.VScroll(listArea, out _);
            var vl = content.VLayout(8, 0, TextAnchor.UpperCenter);
            vl.childControlHeight = true;
            vl.childForceExpandHeight = false;

            var view = rt.gameObject.AddComponent<SettingsPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "categories", categories);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "resetCategory", reset);
            ctx.Wire(view, "rows", content);
            ctx.Wire(view, "rowPrefab", ctx.Asset("Widgets/SettingRow.prefab").GetComponent<SettingRowView>());
            return rt.gameObject;
        }
    }
}
