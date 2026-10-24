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

    /// <summary>Gabarit commun des pages « grille de cartes » : en-tête, grille défilante à gauche, fiche détaillée à droite.</summary>
    static class CardGridKit
    {
        const float DetailW = 700;

        public static T Build<T>(PrefabContext ctx, string name) where T : CardGridPageView
        {
            var rt = ScreenKit.Root(name);

            // ---- En-tête (même gabarit que les pages liste)
            var header = Ui.Panel(rt, "Header", Theme.Surface, 4f, 6f, false);
            header.Frame().TopBand(ListPageKit.HeaderH);
            var title = Ui.Txt(header.transform, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 1f, 22, -10, 900, 54);
            title.AutoSize(40);
            var line = Ui.Txt(header.transform, "", 24, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Line");
            line.rectTransform.At(0f, 1f, 22, -66, 900, 34);
            var bar = Ui.Bar(header.transform, "Bar", Theme.Primary, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 0f, 22, 14, 760, 28);
            var a = Ui.Btn(header.transform, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "ButtonA");
            a.Rect.At(1f, 0.5f, -16, 0, 420, 96);
            var b = Ui.Btn(header.transform, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "ButtonB");
            b.Rect.At(1f, 0.5f, -16 - 440, 0, 420, 96);

            // ---- Grille de tuiles (quatre colonnes)
            var main = Ui.Rect(rt, "Main");
            main.anchorMin = Vector2.zero;
            main.anchorMax = Vector2.one;
            main.offsetMin = Vector2.zero;
            main.offsetMax = new Vector2(-(DetailW + 14), -(ListPageKit.HeaderH + 14));
            var content = Ui.VScroll(main, out _);
            var layout = content.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(246, 140);
            layout.spacing = new Vector2(10, 10);
            layout.padding = new RectOffset(4, 4, 4, 12);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;
            layout.childAlignment = TextAnchor.UpperLeft;

            // ---- Fiche détaillée de la tuile choisie
            var detail = Ui.Panel(rt, "Detail", Theme.Surface, 5f, 8f, false);
            var frame = detail.Frame();
            frame.anchorMin = new Vector2(1, 0);
            frame.anchorMax = new Vector2(1, 1);
            frame.pivot = new Vector2(1, 0.5f);
            frame.sizeDelta = new Vector2(DetailW, -(ListPageKit.HeaderH + 14));
            frame.anchoredPosition = new Vector2(0, -(ListPageKit.HeaderH + 14) / 2f);
            var holder = Ui.Rect(detail.transform, "ArtHolder");
            holder.At(0f, 1f, 8, -8, DetailW - 26, 190);
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", Color.white, null);
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var shade = Ui.Img(holder, "Shade", Theme.WithAlpha(Theme.Outline, 0.85f), Sprites.VerticalGradient);
            shade.rectTransform.Stretch();
            shade.rectTransform.localScale = new Vector3(1, -1, 1);
            var dTitle = Ui.Txt(holder, "", 38, Color.white, TextAnchor.LowerLeft, FontKind.Title, "Title");
            dTitle.rectTransform.At(0f, 0f, 14, 8, DetailW - 220, 52);
            dTitle.AutoSize(38);
            var dBadge = Ui.Txt(holder, "", 26, Theme.Primary, TextAnchor.UpperRight, FontKind.Title, "Badge");
            dBadge.rectTransform.At(1f, 1f, -14, -10, 220, 36);
            var dText = Ui.Txt(detail.transform, "", 24, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
            dText.rectTransform.At(0f, 1f, 20, -208, DetailW - 56, 120);
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
            ctx.Wire(view, "tilePrefab", ctx.Asset("Widgets/CardTile.prefab").GetComponent<SheetTileView>());
            ctx.Wire(view, "detailArt", art);
            ctx.Wire(view, "detailTitle", dTitle);
            ctx.Wire(view, "detailBadge", dBadge);
            ctx.Wire(view, "detailText", dText);
            ctx.WireArray(view, "detailButtons", buttons);
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
        public GameObject Build(PrefabContext ctx) => ListPageKit.Build<AgentsPageView>(ctx, "AgentsPage", 0, false, false).gameObject;
    }

    /// <summary>Page Grimoire : la collection de cartes en grille, l'ouverture des boosters.</summary>
    public sealed class GrimoirePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Grimoire.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => CardGridKit.Build<GrimoirePageView>(ctx, "GrimoirePage").gameObject;
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

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Salon des agents : entraînement et infirmerie réunis en une liste d'agents.</summary>
    public sealed class LoungePageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Lounge.prefab";
        public int Order => 10;
        public UnityEngine.GameObject Build(PrefabContext ctx) => ListPageKit.Build<N29.HeroesDawn.Presentation.Hub.LoungePageView>(ctx, "LoungePage", 0, false, false).gameObject;
    }
}
