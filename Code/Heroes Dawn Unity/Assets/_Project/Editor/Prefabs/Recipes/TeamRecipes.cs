#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Carte de la grille Équipe (M20 ③), 270 × 300 : toucher la carte ouvre la fiche ; bouton d'escouade de 96 px.</summary>
    public sealed class MemberTileRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/MemberTile.prefab";
        public int Order => 6;

        public GameObject Build(PrefabContext ctx)
        {
            var fill = RecipeKit.PanelRoot("MemberTile", Theme.Surface, 4f, 6f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(270, 330);
            var tap = frame.gameObject.AddComponent<Button>();
            tap.targetGraphic = fill;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;
            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.Rounded);
            strip.rectTransform.TopBand(10, 0);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 1f, 12, -20, 80, 80);
            portrait.gameObject.SetActive(false); // remplacé par la grande illustration cadrée sur le visage ; repli : silhouette (voir MemberTileView)
            var holder = Ui.Rect(fill.transform, "ArtHolder").At(0.5f, 1f, 0, -10, 262, 190);
            holder.gameObject.AddComponent<RectMask2D>();
            var bigArt = Ui.Img(holder, "Art", Color.white, null);
            bigArt.raycastTarget = false;
            bigArt.gameObject.SetActive(false);
            var shade = Ui.Img(holder, "Shade", Theme.WithAlpha(Theme.Outline, 0.85f), Sprites.VerticalGradient);
            shade.raycastTarget = false;
            shade.rectTransform.BottomBand(90);
            strip.transform.SetAsLastSibling();
            var name = Ui.Txt(fill.transform, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 16, -142, 238, 38);
            name.AutoSize(30);
            name.Inked(1.5f);
            var level = Ui.Txt(fill.transform, "", 24, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Level");
            level.rectTransform.At(0f, 1f, 16, -176, 84, 28);
            var rarity = Ui.Txt(fill.transform, "", 20, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0f, 1f, 104, -178, 156, 26);
            rarity.AutoSize(20);
            rarity.alignment = TMPro.TextAlignmentOptions.MidlineRight;
            var status = Ui.Txt(fill.transform, "", 22, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Status");
            status.rectTransform.At(0f, 1f, 14, -206, 242, 26);
            var bar = Ui.Bar(fill.transform, "Fatigue", Theme.Success, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 1f, 14, -236, 242, 24);
            var squad = Ui.Btn(fill.transform, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Title, "Squad");
            squad.Rect.At(0.5f, 0f, 0, 10, 242, 56);
            RecipeKit.Mark(frame, TutorialTarget.TeamTile);
            var view = frame.gameObject.AddComponent<MemberTileView>();
            ctx.Wire(view, "tap", tap);
            ctx.Wire(view, "fill", fill);
            ctx.Wire(view, "strip", strip);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "level", level);
            ctx.Wire(view, "rarity", rarity);
            ctx.Wire(view, "status", status);
            ctx.Wire(view, "fatigue", bar);
            ctx.Wire(view, "squad", squad);
            ctx.Wire(view, "bigArt", bigArt);
            return frame.gameObject;
        }
    }

    /// <summary>Page Équipe (UI Toolkit) : filtres et tri, grille de cartes, escouade, synergies et compositions à droite.</summary>
    public sealed class TeamPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Team.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<TeamPageView>("TeamPage", "Team/Team.uxml", 10);
    }

    /// <summary>
    /// Fiche d'un membre (maquette) : grande illustration à gauche, niveau, histoire, stats et paliers au centre, quatre compétences,
    /// trois objets et agents à forte affinité à droite. Le panneau d'inventaire (maquettes « cartes » et « objets ») recouvre la grille de compétences.
    /// </summary>
    public sealed class MemberSheetRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/MemberSheet.prefab";
        public int Order => 10;
        public GameObject Build(PrefabContext ctx) => N29.HeroesDawn.EditorTools.UiToolkit.UiToolkitSetup.NewViewObject<MemberSheetView>("MemberSheet", "MemberSheet/MemberSheet.uxml", 10);
    }
}
