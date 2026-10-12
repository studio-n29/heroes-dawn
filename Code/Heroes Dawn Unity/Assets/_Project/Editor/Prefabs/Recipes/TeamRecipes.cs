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
            frame.sizeDelta = new Vector2(270, 300);
            var tap = frame.gameObject.AddComponent<Button>();
            tap.targetGraphic = fill;
            var nav = tap.navigation;
            nav.mode = Navigation.Mode.None;
            tap.navigation = nav;
            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.Rounded);
            strip.rectTransform.TopBand(10, 0);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 1f, 12, -20, 80, 80);
            var name = Ui.Txt(fill.transform, "", 30, Theme.Navy, TextAnchor.MiddleLeft, FontKind.Title, "Name");
            name.rectTransform.At(0f, 1f, 102, -18, 158, 38);
            name.AutoSize(30);
            name.Inked(1.5f);
            var level = Ui.Txt(fill.transform, "", 24, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Level");
            level.rectTransform.At(0f, 1f, 102, -58, 158, 28);
            var rarity = Ui.Txt(fill.transform, "", 20, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0f, 1f, 102, -86, 158, 26);
            var status = Ui.Txt(fill.transform, "", 22, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Status");
            status.rectTransform.At(0f, 1f, 14, -112, 242, 30);
            var bar = Ui.Bar(fill.transform, "Fatigue", Theme.Success, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 1f, 14, -146, 242, 26);
            var squad = Ui.Btn(fill.transform, "", Theme.Primary, Theme.TextDark, null!, 30, FontKind.Title, "Squad");
            squad.Rect.At(0.5f, 0f, 0, 12, 242, 96);
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
            return frame.gameObject;
        }
    }

    /// <summary>Page Équipe (M20) : filtres par famille et tri en haut, grille défilante, escouade et combos à droite.</summary>
    public sealed class TeamPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Team.prefab";
        public int Order => 10;

        const float GridW = 1150, RightW = 566;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("TeamPage");

            // ---- Filtres (famille : libellé + bande de couleur) et tri
            var filterRow = Ui.Rect(rt, "Filters").At(0f, 1f, 0, 0, GridW, 96);
            filterRow.HLayout(14, 0, TextAnchor.MiddleLeft);
            var filters = new List<Object> { Chip(filterRow, "team.filter.all", "FilterAll", 170, null) };
            foreach (Family f in System.Enum.GetValues(typeof(Family)))
                filters.Add(Chip(filterRow, "family." + f, "Filter" + f, 228, Theme.FamilyColor(f)));
            var sortRow = Ui.Rect(rt, "Sorts").At(0f, 1f, 0, -110, GridW, 96);
            sortRow.HLayout(14, 0, TextAnchor.MiddleLeft);
            var sortLabel = Ui.LocTxt(sortRow, "team.sort", 30, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Title, "Label");
            sortLabel.Pref(90, 96);
            var sorts = new List<Object>
            {
                Chip(sortRow, "team.sort.level", "SortLevel", 210, null),
                Chip(sortRow, "team.sort.rarity", "SortRarity", 210, null),
                Chip(sortRow, "team.sort.fatigue", "SortFatigue", 210, null),
            };

            // ---- Grille
            var gridArea = Ui.Rect(rt, "GridArea");
            gridArea.anchorMin = new Vector2(0, 0);
            gridArea.anchorMax = new Vector2(0, 1);
            gridArea.pivot = new Vector2(0, 0.5f);
            gridArea.offsetMin = new Vector2(0, 0);
            gridArea.offsetMax = new Vector2(GridW, -222);
            var grid = Ui.VScroll(gridArea, out _);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(270, 300);
            layout.spacing = new Vector2(20, 20);
            layout.padding = new RectOffset(2, 2, 4, 16);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 4;

            // ---- Escouade, combos, mission
            var side = Ui.Panel(rt, "Squad", Theme.Surface, 5f, 10f, true);
            var sideFrame = side.Frame();
            sideFrame.anchorMin = new Vector2(1, 0);
            sideFrame.anchorMax = new Vector2(1, 1);
            sideFrame.pivot = new Vector2(1, 0.5f);
            sideFrame.offsetMin = new Vector2(-RightW, 0);
            sideFrame.offsetMax = new Vector2(0, 0);
            var squadTitle = Ui.Txt(side.transform, "", 36, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "SquadTitle");
            squadTitle.rectTransform.At(0f, 1f, 22, -14, 520, 46);
            squadTitle.Inked();
            var squadRow = Ui.Rect(side.transform, "SquadRow").At(0.5f, 1f, 0, -66, 536, 150);
            squadRow.HLayout(12, 0, TextAnchor.UpperCenter);
            Ui.LocTxt(side.transform, "team.combos", 28, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "CombosTitle").rectTransform.At(0f, 1f, 22, -232, 520, 38);
            var combos = Ui.Txt(side.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Combos");
            combos.rectTransform.Stretch(22, 196, 22, 278);
            var none = Ui.LocTxt(side.transform, "team.combos.none", 22, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "CombosNone");
            none.rectTransform.Stretch(22, 196, 22, 278);
            none.gameObject.SetActive(false);
            var hint = Ui.Txt(side.transform, "", 22, Theme.Fatigue, TextAnchor.MiddleCenter, FontKind.Body, "MissionHint");
            hint.rectTransform.At(0.5f, 0f, 0, 140, 520, 30);
            RecipeKit.Mark(side.Frame(), TutorialTarget.TeamSquad);
            var mission = Ui.LocBtn(side.transform, "team.mission", Theme.Danger, Color.white, null!, 40, FontKind.Title, "Mission");
            RecipeKit.Mark(mission, TutorialTarget.TeamMission);
            mission.Rect.At(0.5f, 0f, 0, 18, 520, 112);
            mission.Label.Inked();

            var view = rt.gameObject.AddComponent<TeamPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "filters", filters);
            ctx.WireArray(view, "sorts", sorts);
            ctx.Wire(view, "grid", grid);
            ctx.Wire(view, "squadTitle", squadTitle);
            ctx.Wire(view, "squadRow", squadRow);
            ctx.Wire(view, "combos", combos);
            ctx.Wire(view, "combosNone", none.gameObject);
            ctx.Wire(view, "mission", mission);
            ctx.Wire(view, "missionHint", hint);
            ctx.Wire(view, "tilePrefab", ctx.Asset("Widgets/MemberTile.prefab").GetComponent<MemberTileView>());
            ctx.Wire(view, "squadSlotPrefab", ctx.Asset("Widgets/SquadSlot.prefab").GetComponent<SquadSlotView>());
            return rt.gameObject;
        }

        /// <summary>Puce de filtre ou de tri (96 px de haut) ; une bande de couleur de famille sous le libellé.</summary>
        static UiButton Chip(RectTransform row, string key, string name, float width, Color? family)
        {
            var b = Ui.LocBtn(row, key, Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, name);
            b.Rect.Pref(width, 96);
            if (family != null)
            {
                var band = Ui.Img(b.Fill.transform, "FamilyBand", family.Value, Sprites.RoundedSmall);
                band.rectTransform.BottomBand(8, 6);
            }
            return b;
        }
    }

    /// <summary>Fiche d'un membre (M21) : identité à gauche, stats et compétences au centre, combos, raccourcis et action principale à droite.</summary>
    public sealed class MemberSheetRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/MemberSheet.prefab";
        public int Order => 10;

        const float LeftW = 560, MidW = 600, RightW = 536, Gap = 24;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("MemberSheet");

            // ---- Identité
            var id = Column(rt, "Identity", 0, LeftW);
            var strip = Ui.Img(id.transform, "Family", Theme.Primary, Sprites.Rounded);
            strip.rectTransform.TopBand(12, 0);
            var art = Ui.Img(id.transform, "Art", Theme.SurfaceDark, Sprites.RoundedSmall, true);
            art.type = Image.Type.Sliced;
            art.rectTransform.At(0.5f, 1f, 0, -30, 320, 240);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", art.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 0.5f, 0, 0, 180, 180);
            var artWip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", art.transform, "Wip");
            ((RectTransform)artWip.transform).Stretch();
            artWip.transform.Find("Veil").GetComponent<Image>().color = Theme.WithAlpha(Theme.Navy, 0.2f);
            ((RectTransform)artWip.transform.Find("Band")).anchoredPosition = new Vector2(0, -86);
            artWip.FeatureId = WipCatalog.MemberArt.Id;
            var name = Ui.Txt(id.transform, "", 56, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 1f, 0, -280, 520, 66);
            name.AutoSize(56);
            name.Inked();
            var rarity = Ui.Txt(id.transform, "", 26, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0.5f, 1f, 0, -348, 520, 32);
            var role = Ui.Txt(id.transform, "", 24, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Regular, "Role");
            role.rectTransform.At(0.5f, 1f, 0, -382, 520, 30);
            var level = Ui.Txt(id.transform, "", 40, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Level");
            level.rectTransform.At(0.5f, 1f, 0, -418, 520, 46);
            var fatigue = Ui.Bar(id.transform, "Fatigue", Theme.Success, Theme.SurfaceDark, true, 22);
            ((RectTransform)fatigue.transform).At(0.5f, 1f, 0, -474, 480, 32);
            var previous = Ui.LocBtn(id.transform, "sheet.previous", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Previous");
            previous.Rect.At(0f, 0f, 20, 20, 200, 96);
            var next = Ui.LocBtn(id.transform, "sheet.next", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Next");
            next.Rect.At(1f, 0f, -20, 20, 200, 96);

            // ---- Stats et compétences
            var mid = Column(rt, "Skills", LeftW + Gap, MidW);
            var stats = Ui.Txt(mid.transform, "", 26, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Stats");
            stats.rectTransform.At(0f, 1f, 22, -20, 556, 40);
            Ui.LocTxt(mid.transform, "sheet.skills", 32, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "SkillsTitle").rectTransform.At(0f, 1f, 22, -74, 556, 42);
            var skills = Ui.Txt(mid.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Regular, "SkillList");
            skills.rectTransform.Stretch(22, 20, 22, 128);

            // ---- Combos, raccourcis, action principale
            var side = Column(rt, "Side", LeftW + Gap + MidW + Gap, RightW);
            Ui.LocTxt(side.transform, "sheet.combos", 32, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "CombosTitle").rectTransform.At(0f, 1f, 22, -20, 492, 42);
            var combos = Ui.Txt(side.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Combos");
            combos.rectTransform.Stretch(22, 468, 22, 72);
            var salon = Ui.LocBtn(side.transform, "sheet.salon", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Salon");
            salon.Rect.At(0f, 0f, 20, 252, 236, 96);
            var infirmary = Ui.LocBtn(side.transform, "sheet.infirmary", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Infirmary");
            infirmary.Rect.At(1f, 0f, -20, 252, 236, 96);
            var loadout = Ui.LocBtn(side.transform, "sheet.loadout", Theme.Info, Theme.TextDark, null!, 30, FontKind.Title, "Loadout");
            loadout.Rect.At(0.5f, 0f, 0, 356, 492, 96);
            var reason = Ui.Txt(side.transform, "", 24, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Body, "PrimaryReason");
            reason.rectTransform.At(0.5f, 0f, 0, 140, 492, 34);
            var primary = Ui.Btn(side.transform, "", Theme.Primary, Theme.TextDark, null!, 40, FontKind.Title, "Primary");
            primary.Rect.At(0.5f, 0f, 0, 20, 492, 112);

            var view = rt.gameObject.AddComponent<MemberSheetView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "strip", strip);
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "rarity", rarity);
            ctx.Wire(view, "role", role);
            ctx.Wire(view, "level", level);
            ctx.Wire(view, "stats", stats);
            ctx.Wire(view, "fatigue", fatigue);
            ctx.Wire(view, "skills", skills);
            ctx.Wire(view, "combos", combos);
            ctx.Wire(view, "previous", previous);
            ctx.Wire(view, "next", next);
            ctx.Wire(view, "salon", salon);
            ctx.Wire(view, "infirmary", infirmary);
            ctx.Wire(view, "loadout", loadout);
            ctx.Wire(view, "primary", primary);
            ctx.Wire(view, "primaryReason", reason);
            return rt.gameObject;
        }

        /// <summary>Colonne pleine hauteur (case de BD) placée à <paramref name="x"/> depuis le bord gauche.</summary>
        static Image Column(RectTransform parent, string name, float x, float width)
        {
            var fill = Ui.Panel(parent, name, Theme.Surface, 5f, 10f, true);
            var frame = fill.Frame();
            frame.anchorMin = new Vector2(0, 0);
            frame.anchorMax = new Vector2(0, 1);
            frame.pivot = new Vector2(0, 0.5f);
            frame.offsetMin = new Vector2(x, 0);
            frame.offsetMax = new Vector2(x + width, 0);
            return fill;
        }
    }
}
