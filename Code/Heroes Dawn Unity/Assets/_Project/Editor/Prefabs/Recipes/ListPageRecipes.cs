#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ligne de liste : pastille, titre, sous-titre, barre, badge, trois boutons (96 px de haut).</summary>
    public sealed class InfoRowRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/InfoRow.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("InfoRow", 800, 120);
            root.Pref(-1, 120, 1);
            var rim = Ui.Img(root, "Rim", Theme.Ink, Sprites.Rounded, true);
            rim.type = Image.Type.Sliced;
            rim.rectTransform.Stretch();
            var select = rim.gameObject.AddComponent<Button>();
            select.targetGraphic = rim;
            select.transition = Selectable.Transition.None;
            var nav = select.navigation;
            nav.mode = Navigation.Mode.None;
            select.navigation = nav;
            var fill = Ui.Img(rim.transform, "Fill", Theme.Surface2, Sprites.Rounded);
            fill.type = Image.Type.Sliced;
            fill.rectTransform.Stretch(5, 5, 5, 5);

            var tag = Ui.Img(fill.transform, "Tag", Theme.Info, Sprites.Circle);
            tag.rectTransform.At(0f, 0.5f, 14, 0, 70, 70);
            var tagText = Ui.Txt(tag.transform, "", 28, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            tagText.rectTransform.Stretch();
            tagText.AutoSize(28);

            // Boutons à droite (de droite à gauche : C, B, A), 190 px chacun
            var buttons = new Object[3];
            for (int i = 0; i < 3; i++)
            {
                var b = Ui.Btn(fill.transform, "", Theme.Primary, Theme.TextDark, null!, 26, FontKind.Title, "Button" + (char)('A' + i));
                b.Rect.At(1f, 0.5f, -12 - (2 - i) * 204, 0, 196, 96);
                buttons[i] = b;
            }

            var title = Ui.Txt(fill.transform, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.anchorMin = new Vector2(0, 1);
            title.rectTransform.anchorMax = new Vector2(1, 1);
            title.rectTransform.pivot = new Vector2(0, 1);
            title.rectTransform.offsetMin = new Vector2(96, -50);
            title.rectTransform.offsetMax = new Vector2(-12 - 3 * 204, -6);
            title.AutoSize(30);
            title.Inked(1.5f);
            var subtitle = Ui.Txt(fill.transform, "", 22, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Subtitle");
            subtitle.rectTransform.anchorMin = new Vector2(0, 0);
            subtitle.rectTransform.anchorMax = new Vector2(1, 1);
            subtitle.rectTransform.offsetMin = new Vector2(96, 8);
            subtitle.rectTransform.offsetMax = new Vector2(-12 - 3 * 204, -50);
            var bar = Ui.Bar(fill.transform, "Bar", Theme.Primary, Theme.SurfaceDark, true, 20);
            var barRt = (RectTransform)bar.transform;
            barRt.anchorMin = new Vector2(0, 0);
            barRt.anchorMax = new Vector2(1, 0);
            barRt.pivot = new Vector2(0, 0);
            barRt.offsetMin = new Vector2(96, 8);
            barRt.offsetMax = new Vector2(-12 - 3 * 204, 32);
            var badge = Ui.Txt(fill.transform, "", 22, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Badge");
            badge.rectTransform.At(1f, 1f, -8, 4, 90, 28);
            badge.color = Theme.Danger;
            badge.Inked();

            var view = root.gameObject.AddComponent<InfoRowView>();
            ctx.Wire(view, "rim", rim);
            ctx.Wire(view, "fill", fill);
            ctx.Wire(view, "select", select);
            ctx.Wire(view, "tag", tag);
            ctx.Wire(view, "tagText", tagText);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "subtitle", subtitle);
            ctx.Wire(view, "bar", bar);
            ctx.Wire(view, "badge", badge);
            ctx.WireArray(view, "buttons", buttons);
            return root.gameObject;
        }
    }

    /// <summary>Gabarit commun des pages « liste » : en-tête, liste défilante, colonne de droite optionnelle.</summary>
    static class ListPageKit
    {
        public const float HeaderH = 150;

        public static T Build<T>(PrefabContext ctx, string name, float sideWidth, bool sideRows, bool sideText, bool shopTabs = false) where T : ListPageView
        {
            var rt = ScreenKit.Root(name);

            // ---- En-tête
            var header = Ui.Panel(rt, "Header", Theme.WithAlpha(Theme.Frost, 0.9f), 4f, 6f, false);
            header.Frame().TopBand(HeaderH);
            var title = Ui.Txt(header.transform, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 22, -10, 900, 54);
            title.AutoSize(40);
            title.Inked();
            var line = Ui.Txt(header.transform, "", 24, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Line");
            line.rectTransform.At(0f, 1f, 22, -66, 900, 34);
            var bar = Ui.Bar(header.transform, "Bar", Theme.Primary, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 0f, 22, 14, 760, 28);
            var a = Ui.Btn(header.transform, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "ButtonA");
            a.Rect.At(1f, 0.5f, -16, 0, 420, 96);
            var b = Ui.Btn(header.transform, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "ButtonB");
            b.Rect.At(1f, 0.5f, -16 - 440, 0, 420, 96);

            // ---- Liste principale
            var mainArea = Ui.Rect(rt, "Main");
            mainArea.anchorMin = new Vector2(0, 0);
            mainArea.anchorMax = new Vector2(1, 1);
            mainArea.offsetMin = Vector2.zero;
            float tabsH = shopTabs ? ShopTabsRecipe.Height + 10 : 0f;
            mainArea.offsetMax = new Vector2(-(sideWidth > 0 ? sideWidth + 24 : 0), -(HeaderH + 14 + tabsH));
            var rows = Ui.VScroll(mainArea, out _);
            rows.VLayout(10, 0, TextAnchor.UpperCenter);
            rows.GetComponent<VerticalLayoutGroup>().childControlHeight = true;
            rows.GetComponent<VerticalLayoutGroup>().childForceExpandHeight = false;
            rows.GetComponent<VerticalLayoutGroup>().padding = new RectOffset(0, 0, 4, 12);

            var view = rt.gameObject.AddComponent<T>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "headerTitle", title);
            ctx.Wire(view, "headerLine", line);
            ctx.Wire(view, "headerBar", bar);
            ctx.Wire(view, "headerA", a);
            ctx.Wire(view, "headerB", b);
            ctx.Wire(view, "rows", rows);
            ctx.Wire(view, "rowPrefab", ctx.Asset("Widgets/InfoRow.prefab").GetComponent<InfoRowView>());
            if (shopTabs)
            {
                var tabs = ctx.Nest<ShopTabsView>("Widgets/ShopTabs.prefab", rt, "ShopTabs");
                var trt = (RectTransform)tabs.transform;
                trt.anchorMin = new Vector2(0f, 1f); trt.anchorMax = new Vector2(1f, 1f); trt.pivot = new Vector2(0.5f, 1f);
                trt.anchoredPosition = new Vector2(0f, -(HeaderH + 8)); trt.sizeDelta = new Vector2(0f, ShopTabsRecipe.Height);
                ctx.Wire(view, "shopTabs", tabs);
            }

            // ---- Colonne de droite
            if (sideWidth > 0)
            {
                var side = Ui.Panel(rt, "Side", Theme.WithAlpha(Theme.Frost, 0.9f), 4f, 6f, false);
                var frame = side.Frame();
                frame.anchorMin = new Vector2(1, 0);
                frame.anchorMax = new Vector2(1, 1);
                frame.pivot = new Vector2(1, 0.5f);
                frame.sizeDelta = new Vector2(sideWidth, -(HeaderH + 14));
                frame.anchoredPosition = new Vector2(0, -(HeaderH + 14) / 2f);
                var sTitle = Ui.Txt(side.transform, "", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
                sTitle.rectTransform.At(0f, 1f, 18, -8, sideWidth - 36, 44);
                sTitle.Inked();
                ctx.Wire(view, "sideRoot", frame.gameObject);
                ctx.Wire(view, "sideTitle", sTitle);
                if (sideRows)
                {
                    var sRows = Ui.Rect(side.transform, "Rows");
                    sRows.anchorMin = new Vector2(0, 0);
                    sRows.anchorMax = new Vector2(1, 1);
                    sRows.offsetMin = new Vector2(10, 50);
                    sRows.offsetMax = new Vector2(-10, -60);
                    var v = sRows.VLayout(10, 0, TextAnchor.UpperCenter);
                    v.childControlHeight = true;
                    v.childForceExpandHeight = false;
                    ctx.Wire(view, "sideRows", sRows);
                    var hint = Ui.Txt(side.transform, "", 22, Theme.TextMuted, TextAnchor.LowerLeft, FontKind.Body, "Text");
                    hint.rectTransform.At(0f, 0f, 18, 10, sideWidth - 36, 36);
                    ctx.Wire(view, "sideText", hint);
                }
                else if (sideText)
                {
                    var text = Ui.Txt(side.transform, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
                    text.rectTransform.Stretch(22, 16, 22, 66);
                    ctx.Wire(view, "sideText", text);
                }
            }
            return view;
        }
    }

    public sealed class PassPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Pass.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<PassPageView>(ctx, "PassPage", 760, true, false).gameObject;
    }

    public sealed class BoutiquePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Boutique.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<BoutiquePageView>(ctx, "BoutiquePage", 0, false, false, shopTabs: true).gameObject;
    }

    public sealed class OperationsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Operations.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<OperationsPageView>(ctx, "OperationsPage", 0, false, false).gameObject;
    }

    public sealed class LabPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Lab.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<LabPageView>(ctx, "LabPage", 0, false, false).gameObject;
    }

    /// <summary>Profil du joueur : tag, accès au registre et aux archives, statistiques, amis.</summary>
    public sealed class ProfilePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Profile.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<ProfilePageView>(ctx, "ProfilePage", 0, false, false).gameObject;
    }

    public sealed class ArchivesPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Archives.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<ArchivesPageView>(ctx, "ArchivesPage", 860, false, true).gameObject;
    }
}
