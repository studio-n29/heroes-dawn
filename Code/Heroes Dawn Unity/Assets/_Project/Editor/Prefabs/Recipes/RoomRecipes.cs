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
            var name = Ui.Txt(fill.transform, "", 34, Color.white, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 132, -10, 460, 42);
            name.Inked(1.5f);
            var detail = Ui.Txt(fill.transform, "", 22, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Regular, "Detail");
            detail.rectTransform.At(0f, 1f, 132, -54, 560, 30);
            var bar = Ui.Bar(fill.transform, "Fatigue", Theme.Success, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 0f, 132, 14, 420, 28);
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

    /// <summary>Paramètres (M80-M85) : 6 catégories à gauche (BIENTÔT sauf Compte), le compte à droite.</summary>
    public sealed class SettingsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Settings.prefab";
        public int Order => 10;

        const float LeftW = 440;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("SettingsPage");
            var column = Ui.Rect(rt, "Categories").At(0f, 1f, 0, 0, LeftW, 6 * 96 + 5 * 16);
            column.VLayout(16, 0, TextAnchor.UpperLeft);
            var categories = new List<Object>();
            var wips = new List<Object>();
            for (int i = 0; i < 6; i++)
            {
                var b = Ui.Btn(column, "", Theme.Surface3, Theme.Text, null!, 32, FontKind.Title, "Category" + i);
                b.Rect.Pref(LeftW, 96);
                var wip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", b.transform, "Wip");
                ((RectTransform)wip.transform).Stretch();
                wip.transform.Find("Band").localScale = new Vector3(0.8f, 0.8f, 1f);
                ((RectTransform)wip.transform.Find("Band")).anchoredPosition = new Vector2(110, 0);
                wip.gameObject.SetActive(false);
                categories.Add(b);
                wips.Add(wip);
            }

            var panel = Ui.Panel(rt, "Account", Theme.Surface, 5f, 10f, true);
            var frame = panel.Frame();
            frame.anchorMin = Vector2.zero;
            frame.anchorMax = Vector2.one;
            frame.offsetMin = new Vector2(LeftW + 30, 0);
            frame.offsetMax = Vector2.zero;
            var title = Ui.LocTxt(panel.transform, "settings.account", 48, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 30, -20, 800, 60);
            title.Inked();
            var player = Ui.Txt(panel.transform, "", 28, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Player");
            player.rectTransform.At(0f, 1f, 30, -90, 1100, 40);
            var logout = Ui.LocBtn(panel.transform, "settings.logout", Theme.Surface3, Theme.Text, null!, 32, FontKind.Title, "Logout");
            logout.Rect.At(0f, 1f, 30, -170, 560, 96);
            var reset = Ui.LocBtn(panel.transform, "settings.reset", Theme.Danger, Color.white, null!, 32, FontKind.Title, "Reset");
            reset.Rect.At(0f, 1f, 30, -300, 560, 96);
            Ui.LocTxt(panel.transform, "settings.reset.hint", 22, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "ResetHint")
              .rectTransform.At(0f, 1f, 30, -406, 1000, 60);

            var view = rt.gameObject.AddComponent<SettingsPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "categories", categories);
            ctx.WireArray(view, "categoryWips", wips);
            ctx.Wire(view, "playerLine", player);
            ctx.Wire(view, "logout", logout);
            ctx.Wire(view, "reset", reset);
            return rt.gameObject;
        }
    }
}
