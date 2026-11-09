#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Tuile de la grille de cartes (Grimoire, Équipement) : même gabarit que les cases de la fiche d'agent.</summary>
    public sealed class CardTileRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/CardTile.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var scratch = PrefabContext.NewRoot("Scratch", 246, 140);
            var tile = MemberSheetRecipe.Tile(ctx, scratch, "CardTile", 246, 140, false);
            tile.transform.SetParent(null, false);
            Object.DestroyImmediate(scratch.gameObject);
            return tile.gameObject;
        }
    }

    /// <summary>Tuile d'offre de la boutique : illustration pleine, nom au-dessus d'un bandeau ivoire qui porte le prix (planches 10.2 et 10.3).</summary>
    public sealed class ShopTileRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/ShopTile.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var scratch = PrefabContext.NewRoot("Scratch", 282, 300);
            var tile = MemberSheetRecipe.Tile(ctx, scratch, "ShopTile", 282, 300, false);
            var fill = tile.transform.Find("Fill");
            var band = Ui.Img(fill, "PriceBand", Theme.Primary, Sprites.PanelSmall);
            band.raycastTarget = false;
            band.rectTransform.BottomBand(58, 8);
            var sub = (RectTransform)fill.Find("Sub");
            sub.SetAsLastSibling();
            sub.anchorMin = new Vector2(0f, 0f);
            sub.anchorMax = new Vector2(1f, 0f);
            sub.offsetMin = new Vector2(12f, 10f);
            sub.offsetMax = new Vector2(-12f, 60f);
            var subText = sub.GetComponent<TMPro.TMP_Text>();
            subText.color = Theme.TextDark;
            subText.fontSize = 30;
            subText.alignment = TMPro.TextAlignmentOptions.Center;
            var title = (RectTransform)fill.Find("Title");
            title.Stretch(10, 72, 8, 6);
            tile.transform.SetParent(null, false);
            Object.DestroyImmediate(scratch.gameObject);
            return tile.gameObject;
        }
    }

    /// <summary>Gabarit commun des pages « grille de cartes » : en-tête, grille défilante à gauche, fiche détaillée à droite.</summary>
    static class CardGridKit
    {


        /// <param name="shopTabs">Boutique : les onglets de la boutique sont posés sous l'en-tête et la grille comme la fiche descendent d'autant.</param>
        public static T Build<T>(PrefabContext ctx, string name, bool shopTabs = false) where T : CardGridPageView
        {
            var rt = ScreenKit.Root(name);
            rt.gameObject.AddComponent<TacticalPageLayout>();
            float tabsH = 0f;
            float DetailW = shopTabs ? 520f : 700f; // boutique : colonnes d'offres plus larges et fiche plus étroite (planches 10.2 à 10.4)
            float artH = shopTabs ? 320f : 230f;
            float headerH = shopTabs ? 110f : ListPageKit.HeaderH; // boutique : en-tête compact, l'espace va aux offres

            // ---- En-tête (même gabarit que les pages liste)
            var header = Ui.Panel(rt, "Header", Theme.Surface, 4f, 6f, false);
            header.Frame().TopBand(headerH);
            var title = Ui.Txt(header.transform, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 22, -10, 900, 54);
            title.AutoSize(40);
            var line = Ui.Txt(header.transform, "", 24, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Line");
            line.rectTransform.At(0f, 1f, 22, -66, 900, 34);
            var bar = Ui.Bar(header.transform, "Bar", Theme.Primary, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 0f, 22, 14, 760, 28);
            var a = Ui.Btn(header.transform, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "ButtonA");
            a.Rect.At(1f, 0.5f, -16, 0, 420, shopTabs ? 96 : 96);
            var b = Ui.Btn(header.transform, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "ButtonB");
            b.Rect.At(1f, 0.5f, -16 - 440, 0, 420, 96);

            // ---- Grille de tuiles (quatre colonnes)
            var main = Ui.Rect(rt, "Main");
            main.anchorMin = Vector2.zero;
            main.anchorMax = Vector2.one;
            main.offsetMin = new Vector2(shopTabs ? 274 : 0, 0);
            main.offsetMax = new Vector2(-(DetailW + 14), -(headerH + 14 + tabsH));
            var content = Ui.VScroll(main, out _);
            var layout = content.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = shopTabs ? new Vector2(282, 300) : new Vector2(246, 140);
            layout.spacing = new Vector2(10, 10);
            layout.padding = new RectOffset(4, 4, 4, 12);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = shopTabs ? 3 : 4;
            layout.childAlignment = TextAnchor.UpperLeft;

            // ---- Barre de filtres : au-dessus de la grille, masquée tant que la page n'a pas de filtres (la grille descend sous elle)
            var filters = Ui.Rect(rt, "Filters");
            filters.anchorMin = new Vector2(0, 1);
            filters.anchorMax = new Vector2(1, 1);
            filters.pivot = new Vector2(0.5f, 1);
            filters.sizeDelta = new Vector2(-(DetailW + 14), 0);
            filters.anchoredPosition = new Vector2(-(DetailW + 14) / 2f, -(headerH + 14 + tabsH));
            if (shopTabs) { filters.offsetMin = new Vector2(274, filters.offsetMin.y); }

            // ---- Fiche détaillée de la tuile choisie
            var detail = Ui.Panel(rt, "Detail", Theme.Surface, 5f, 8f, false);
            var frame = detail.Frame();
            frame.anchorMin = new Vector2(1, 0);
            frame.anchorMax = new Vector2(1, 1);
            frame.pivot = new Vector2(1, 0.5f);
            frame.sizeDelta = new Vector2(DetailW, -(headerH + 14 + tabsH));
            frame.anchoredPosition = new Vector2(0, -(headerH + 14 + tabsH) / 2f);
            if (shopTabs) { frame.anchorMin = frame.anchorMax = new Vector2(1, 1); frame.pivot = new Vector2(1, 1); frame.sizeDelta = new Vector2(DetailW, 600); frame.anchoredPosition = new Vector2(0, -(headerH + 14)); } // fiche compacte, sans vide sous le texte
            var holder = Ui.Rect(detail.transform, "ArtHolder");
            holder.At(0f, 1f, 8, -8, DetailW - 26, artH);
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", Color.white, null);
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = shopTabs ? AspectRatioFitter.AspectMode.FitInParent : AspectRatioFitter.AspectMode.EnvelopeParent;
            var shade = Ui.Img(holder, "Shade", Theme.WithAlpha(Theme.Outline, 0.85f), Sprites.VerticalGradient);
            shade.rectTransform.Stretch();
            shade.rectTransform.localScale = new Vector3(1, -1, 1);
            var dTitle = Ui.Txt(holder, "", 38, Color.white, TextAnchor.LowerLeft, FontKind.Title, "Title");
            dTitle.rectTransform.At(0f, 0f, 14, 8, DetailW - 220, 52);
            dTitle.AutoSize(38);
            var dBadge = Ui.Txt(holder, "", 26, Theme.Primary, TextAnchor.UpperRight, FontKind.Title, "Badge");
            dBadge.rectTransform.At(1f, 1f, -14, -10, 220, 36);
            var textArea = Ui.Rect(detail.transform, "TextArea").Stretch(20, 124, 20, artH + 20);
            var textContent = Ui.VScroll(textArea, out _);
            textContent.VLayout(0, 0, TextAnchor.UpperLeft).childControlHeight = true;
            var dText = Ui.Txt(textContent, "", 26, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
            dText.rectTransform.Stretch();
            var buttons = new Object[3];
            for (int i = 0; i < 3; i++)
            {
                var btn = Ui.Btn(detail.transform, "", Theme.Primary, Theme.TextDark, null!, 26, FontKind.Title, "Button" + (char)('A' + i));
                btn.Rect.At(0f, 0f, 12 + i * 224, 12, 216, 96);
                buttons[i] = btn;
            }

            var view = rt.gameObject.AddComponent<T>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "headerTitle", title);
            ctx.Wire(view, "headerLine", line);
            ctx.Wire(view, "headerBar", bar);
            ctx.Wire(view, "headerA", a);
            ctx.Wire(view, "headerB", b);
            ctx.Wire(view, "grid", content);
            ctx.Wire(view, "tilePrefab", ctx.Asset(shopTabs ? "Widgets/ShopTile.prefab" : "Widgets/CardTile.prefab").GetComponent<SheetTileView>());
            ctx.Wire(view, "detailArt", art);
            ctx.Wire(view, "detailTitle", dTitle);
            ctx.Wire(view, "detailBadge", dBadge);
            ctx.Wire(view, "detailText", dText);
            ctx.WireArray(view, "detailButtons", buttons);
            ctx.Wire(view, "filterBar", filters);
            ctx.Wire(view, "gridArea", main);
            if (shopTabs)
            {
                var tabs = ctx.Nest<ShopTabsView>("Widgets/ShopTabs.prefab", rt, "ShopTabs");
                var trt = (RectTransform)tabs.transform;
                trt.anchorMin = trt.anchorMax = Vector2.up; trt.pivot = Vector2.up;
                trt.anchoredPosition = new Vector2(0f, -(headerH + 18)); trt.sizeDelta = new Vector2(250f, 474);
                ctx.Wire(view, "shopTabs", tabs);
            }
            return view;
        }
    }

    /// <summary>Page Équipement d'un agent : ses objets et cartes équipés, puis la réserve, en grille avec fiche détaillée.</summary>
    public sealed class LoadoutPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Loadout.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => CardGridKit.Build<LoadoutPageView>(ctx, "LoadoutPage").gameObject;
    }

    /// <summary>Registre des agents : les 30 agents et leur voie d obtention.</summary>
    public sealed class AgentsPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Agents.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => CardGridKit.Build<AgentsPageView>(ctx, "AgentsPage").gameObject;
    }

    /// <summary>Page Grimoire : la collection de cartes en grille, l'ouverture des boosters, et la recherche par nom dans l'en-tête.</summary>
    public sealed class GrimoirePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Grimoire.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var page = CardGridKit.Build<GrimoirePageView>(ctx, "GrimoirePage");
            // Champ de recherche à droite de l'en-tête, à gauche des boutons A et B.
            var header = (RectTransform)page.transform.Find("Header/Fill");
            var box = Ui.Img(header, "Search", Theme.Surface3, Sprites.Panel, true);
            box.rectTransform.At(1f, 0.5f, -896, 0, 320, 70);
            var viewport = Ui.Rect(box.transform, "Viewport").Stretch(16, 8, 16, 8);
            viewport.gameObject.AddComponent<RectMask2D>();
            var text = Ui.Txt(viewport, "", 28, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Text");
            text.rectTransform.Stretch();
            var hint = Ui.LocTxt(viewport, "grimoire.search", 28, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Hint");
            hint.rectTransform.Stretch();
            var field = box.gameObject.AddComponent<TMPro.TMP_InputField>();
            field.textViewport = viewport;
            field.textComponent = text;
            field.placeholder = hint;
            field.lineType = TMPro.TMP_InputField.LineType.SingleLine;
            field.characterLimit = 32;
            ctx.Wire(page, "search", field);
            return page.gameObject;
        }
    }
}

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Page Inventaire : les objets possédés en grille avec fiche détaillée.</summary>
    public sealed class InventoryPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Inventory.prefab";
        public int Order => 10;
        public UnityEngine.GameObject Build(PrefabContext ctx) => CardGridKit.Build<N29.HeroesDawn.Presentation.Hub.InventoryPageView>(ctx, "InventoryPage").gameObject;
    }
}

