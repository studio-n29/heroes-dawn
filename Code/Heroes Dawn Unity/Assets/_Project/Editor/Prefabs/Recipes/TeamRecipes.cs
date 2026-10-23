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
            var name = Ui.Txt(fill.transform, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Name");
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
            return frame.gameObject;
        }
    }

    /// <summary>Page Équipe (M20) : filtres par famille et tri en haut, grille défilante, escouade et synergies à droite.</summary>
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

            // ---- Escouade, synergies, mission
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
            Ui.LocTxt(side.transform, "team.synergies", 28, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "SynergiesTitle").rectTransform.At(0f, 1f, 22, -232, 520, 38);
            var synergies = Ui.Txt(side.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Synergies");
            synergies.rectTransform.Stretch(22, 196, 22, 278);
            var none = Ui.LocTxt(side.transform, "team.synergies.none", 22, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "SynergiesNone");
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
            ctx.Wire(view, "synergies", synergies);
            ctx.Wire(view, "synergiesNone", none.gameObject);
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

    /// <summary>
    /// Fiche d'un membre (maquette) : grande illustration à gauche, niveau, histoire, stats et paliers au centre, quatre compétences,
    /// trois objets et agents à forte affinité à droite. Le panneau d'inventaire (maquettes « cartes » et « objets ») recouvre la grille de compétences.
    /// </summary>
    public sealed class MemberSheetRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/MemberSheet.prefab";
        public int Order => 10;

        const float LeftW = 520, MidX = 544, MidW = 560, RightX = 1128, RightW = 616;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("MemberSheet");

            // ---- Illustration large, en opacité réduite, derrière toute la fiche
            var wideHolder = Ui.Rect(rt, "WideArt").Stretch();
            wideHolder.gameObject.AddComponent<RectMask2D>();
            var wide = Ui.Img(wideHolder, "Image", new Color(1f, 1f, 1f, 0.28f), null);
            wide.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;

            // ---- Colonne gauche : illustration de l'agent, nom, rareté, rôle, fatigue, flèches
            var id = Column(rt, "Identity", 0, LeftW);
            var holder = Ui.Rect(id.transform, "ArtHolder").Stretch(6, 6, 6, 6);
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", Color.white, null);
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var shade = Ui.Img(id.transform, "Shade", Theme.WithAlpha(Theme.Outline, 0.9f), Sprites.VerticalGradient);
            shade.rectTransform.At(0.5f, 0f, 0, 6, LeftW - 12, 300);
            shade.rectTransform.localScale = new Vector3(1, -1, 1);
            var strip = Ui.Img(id.transform, "Family", Theme.Primary, Sprites.Panel);
            strip.rectTransform.TopBand(14, 0);
            var name = Ui.Txt(id.transform, "", 60, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Name");
            name.rectTransform.At(0.5f, 0f, 0, 228, LeftW - 40, 70);
            name.AutoSize(60);
            var rarity = Ui.Txt(id.transform, "", 28, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0.5f, 0f, 0, 196, LeftW - 40, 32);
            var role = Ui.Txt(id.transform, "", 24, Theme.TextMuted, TextAnchor.MiddleCenter, FontKind.Regular, "Role");
            role.rectTransform.At(0.5f, 0f, 0, 164, LeftW - 40, 30);
            var fatigue = Ui.Bar(id.transform, "Fatigue", Theme.Success, Theme.Outline, true, 22);
            ((RectTransform)fatigue.transform).At(0.5f, 0f, 0, 126, LeftW - 60, 30);
            var previous = Ui.LocBtn(id.transform, "sheet.previous", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "Previous");
            previous.Rect.At(0f, 0f, 20, 14, 150, 96);
            var next = Ui.LocBtn(id.transform, "sheet.next", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "Next");
            next.Rect.At(1f, 0f, -20, 14, 150, 96);

            // ---- Colonne centrale : niveau, histoire, stats, paliers, action principale
            var levelBar = Ui.Bar(rt, "Level", Theme.Accent, Theme.Outline, true, 26);
            ((RectTransform)levelBar.transform).At(0f, 1f, MidX, 0, MidW, 46);

            var bioFill = Ui.Panel(rt, "Bio", Theme.Surface, 4f, 6f, false);
            bioFill.Frame().At(0f, 1f, MidX, -56, MidW, 130);
            var bio = Ui.Txt(bioFill.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "BioText");
            bio.rectTransform.Stretch(18, 10, 18, 22);

            var statsFill = Ui.Panel(rt, "Stats", Theme.Surface, 4f, 6f, false);
            statsFill.Frame().At(0f, 1f, MidX, -190, MidW, 54);
            var stats = Ui.Txt(statsFill.transform, "", 26, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "StatsText");
            stats.rectTransform.Stretch(12, 4, 12, 8);
            stats.AutoSize(26);

            var tiersFill = Ui.Panel(rt, "Tiers", Theme.Surface, 4f, 6f, false);
            tiersFill.Frame().At(0f, 1f, MidX, -258, MidW, 196);
            var tiersTitle = Ui.Txt(tiersFill.transform, "", 28, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "TiersTitle");
            tiersTitle.rectTransform.At(0f, 1f, 20, -14, MidW - 40, 36);
            var tiers = Ui.Txt(tiersFill.transform, "", 24, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "TiersText");
            tiers.rectTransform.Stretch(20, 12, 20, 58);
            tiers.lineSpacing = 8;

            var reason = Ui.Txt(rt, "", 24, Theme.Danger, TextAnchor.MiddleCenter, FontKind.Body, "PrimaryReason");
            reason.rectTransform.At(0f, 0f, MidX, 108, MidW, 32);
            var primary = Ui.Btn(rt, "", Theme.Primary, Theme.TextDark, null!, 40, FontKind.Title, "Primary");
            primary.Rect.At(0f, 0f, MidX, 0, MidW, 104);
            RecipeKit.Mark(primary, TutorialTarget.SheetTrain);

            // ---- Colonne droite : compétences, objets, affinités, raccourcis
            var skillTiles = new List<Object>();
            for (int i = 0; i < 4; i++)
            {
                var t = Tile(ctx, rt, "Skill" + i, 304, 140, false);
                ((RectTransform)t.transform).At(0f, 1f, RightX + (i % 2) * 312, -(i / 2) * 148, 304, 140);
                skillTiles.Add(t);
            }
            var itemTiles = new List<Object>();
            for (int i = 0; i < 3; i++)
            {
                var t = Tile(ctx, rt, "Item" + i, 200, 104, false);
                ((RectTransform)t.transform).At(0f, 1f, RightX + i * 208, -304, 200, 104);
                itemTiles.Add(t);
                if (i == 0) RecipeKit.Mark(t, TutorialTarget.SheetSlot);
            }

            var affinityFill = Ui.Panel(rt, "Affinity", Theme.Surface, 4f, 6f, false);
            affinityFill.Frame().At(0f, 1f, RightX, -416, RightW, 92);
            var affinityTitle = Ui.Txt(affinityFill.transform, "", 22, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "AffinityTitle");
            affinityTitle.rectTransform.At(0f, 1f, 18, -8, 330, 28);
            var affinity = new List<Object>();
            for (int i = 0; i < 3; i++)
            {
                var p = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", affinityFill.transform, "Affinity" + i);
                ((RectTransform)p.transform).At(0f, 1f, 18 + i * 70, -34, 56, 56);
                affinity.Add(p);
            }
            var synergies = Ui.Txt(affinityFill.transform, "", 20, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Synergies");
            synergies.rectTransform.At(1f, 0.5f, -12, 0, 330, 80);
            synergies.overflowMode = TMPro.TextOverflowModes.Ellipsis;

            var salon = Ui.LocBtn(rt, "sheet.salon", Theme.Surface3, Theme.Text, null!, 26, FontKind.Title, "Salon");
            salon.Rect.At(0f, 0f, RightX, 0, 200, 96);
            var infirmary = Ui.LocBtn(rt, "sheet.infirmary", Theme.Surface3, Theme.Text, null!, 26, FontKind.Title, "Infirmary");
            infirmary.Rect.At(0f, 0f, RightX + 208, 0, 200, 96);
            var loadout = Ui.LocBtn(rt, "sheet.loadout", Theme.Info, Theme.TextDark, null!, 22, FontKind.Title, "Loadout");
            loadout.Rect.At(0f, 0f, RightX + 416, 0, 200, 96);

            // ---- Panneau d'inventaire (recouvre la grille de compétences)
            var pickerFill = Ui.Panel(rt, "Picker", Theme.Surface, 5f, 10f, true);
            var pickerFrame = pickerFill.Frame();
            pickerFrame.At(0f, 1f, RightX, 0, RightW, 304);
            var pickerTitle = Ui.Txt(pickerFill.transform, "", 24, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "PickerTitle");
            pickerTitle.rectTransform.At(0f, 1f, 16, -10, 370, 44);
            pickerTitle.AutoSize(24);
            var pickerHint = Ui.Txt(pickerFill.transform, "", 20, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "PickerHint");
            pickerHint.rectTransform.At(0f, 1f, 16, -56, 380, 30);
            var pickerRemove = Ui.LocBtn(pickerFill.transform, "sheet.picker_remove", Theme.Danger, Theme.Text, null!, 22, FontKind.Title, "Remove");
            pickerRemove.Rect.At(1f, 1f, -112, 0, 130, 96);
            var pickerClose = Ui.LocBtn(pickerFill.transform, "sheet.picker_close", Theme.Surface3, Theme.Text, null!, 22, FontKind.Title, "Close");
            pickerClose.Rect.At(1f, 1f, -8, 0, 100, 96);
            var pickerTiles = new List<Object>();
            for (int i = 0; i < 8; i++)
            {
                var t = Tile(ctx, pickerFill.transform, "Option" + i, 144, 96, true);
                ((RectTransform)t.transform).At(0f, 1f, 12 + (i % 4) * 150, -100 - (i / 4) * 102, 144, 96);
                pickerTiles.Add(t);
                if (i == 0) RecipeKit.Mark(t, TutorialTarget.SheetPick);
            }

            pickerFrame.gameObject.SetActive(false);
            var view = rt.gameObject.AddComponent<MemberSheetView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "wideArt", wide);
            ctx.Wire(view, "strip", strip);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "rarity", rarity);
            ctx.Wire(view, "role", role);
            ctx.Wire(view, "levelBar", levelBar);
            ctx.Wire(view, "bio", bio);
            ctx.Wire(view, "stats", stats);
            ctx.Wire(view, "tiersTitle", tiersTitle);
            ctx.Wire(view, "tiers", tiers);
            ctx.Wire(view, "fatigue", fatigue);
            ctx.WireArray(view, "skillTiles", skillTiles);
            ctx.WireArray(view, "itemTiles", itemTiles);
            ctx.WireArray(view, "affinity", affinity);
            ctx.Wire(view, "affinityTitle", affinityTitle);
            ctx.Wire(view, "synergies", synergies);
            ctx.Wire(view, "previous", previous);
            ctx.Wire(view, "next", next);
            ctx.Wire(view, "salon", salon);
            ctx.Wire(view, "infirmary", infirmary);
            ctx.Wire(view, "loadout", loadout);
            ctx.Wire(view, "primary", primary);
            ctx.Wire(view, "primaryReason", reason);
            ctx.Wire(view, "pickerRoot", pickerFrame.gameObject);
            ctx.Wire(view, "pickerTitle", pickerTitle);
            ctx.Wire(view, "pickerHint", pickerHint);
            ctx.WireArray(view, "pickerTiles", pickerTiles);
            ctx.Wire(view, "pickerRemove", pickerRemove);
            ctx.Wire(view, "pickerClose", pickerClose);
            return rt.gameObject;
        }

        /// <summary>Case de la fiche : cadre coloré, illustration, titre en bas, sous-titre en haut, voile quand elle est verrouillée.</summary>
        static SheetTileView Tile(PrefabContext ctx, Transform parent, string name, float w, float h, bool compact)
        {
            var fill = Ui.Panel(parent, name, Theme.Surface3, 4f, 6f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(w, h);
            var frameImg = frame.GetComponent<Image>();
            var holder = Ui.Rect(fill.transform, "ArtHolder").Stretch();
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", new Color(1f, 1f, 1f, 0.6f), null);
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var shade = Ui.Img(fill.transform, "Shade", Theme.WithAlpha(Theme.Outline, 0.75f), Sprites.VerticalGradient);
            shade.rectTransform.Stretch();
            shade.rectTransform.localScale = new Vector3(1, -1, 1);
            var sub = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Sub");
            sub.rectTransform.Stretch(10, 4, 8, 6);
            var title = Ui.Txt(fill.transform, "", compact ? 21 : 30, Theme.Text, TextAnchor.LowerLeft, FontKind.Title, "Title");
            title.rectTransform.Stretch(10, 6, 8, compact ? 18 : 24);
            title.AutoSize(compact ? 21 : 30);
            var veil = Ui.Img(fill.transform, "Locked", Theme.WithAlpha(Theme.Outline, 0.55f), Sprites.Panel);
            veil.rectTransform.Stretch();
            var button = frame.gameObject.AddComponent<Button>();
            button.targetGraphic = frameImg;
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var view = frame.gameObject.AddComponent<SheetTileView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "frame", frameImg);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "sub", sub);
            ctx.Wire(view, "locked", veil.gameObject);
            return view;
        }

        /// <summary>Colonne pleine hauteur (panneau) placée à <paramref name="x"/> depuis le bord gauche.</summary>
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
