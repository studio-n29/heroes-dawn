#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Ligne de texte de liste (objectifs, journal).</summary>
    public sealed class TextLineRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/TextLine.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("TextLine", 540, 32);
            var t = Ui.Txt(root, "", 24, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Label");
            t.rectTransform.Stretch();
            root.Pref(540, 32);
            var view = root.gameObject.AddComponent<TextLineView>();
            ctx.Wire(view, "label", t);
            return root.gameObject;
        }
    }

    /// <summary>Membre de l'escouade : portrait, nom, jauge de fatigue (reprise de HubScreen.RefreshOps).</summary>
    public sealed class SquadSlotRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/SquadSlot.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var cell = PrefabContext.NewRoot("SquadSlot", 125, 150);
            cell.Pref(125, 150);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", cell, "Portrait");
            ((RectTransform)portrait.transform).At(0.5f, 1f, 0, 0, 90, 90);
            var name = Ui.Txt(cell, "", 22, Color.white, TextAnchor.MiddleCenter, FontKind.Body, "Name");
            name.rectTransform.At(0.5f, 1f, 0, -92, 125, 26);
            var bar = Ui.Bar(cell, "Fatigue", Theme.Success, Theme.SurfaceDark);
            ((RectTransform)bar.transform).At(0.5f, 1f, 0, -122, 110, 16);
            var view = cell.gameObject.AddComponent<SquadSlotView>();
            ctx.Wire(view, "portrait", portrait);
            ctx.Wire(view, "memberName", name);
            ctx.Wire(view, "fatigue", bar);
            return cell.gameObject;
        }
    }

    /// <summary>En-tête commun des cartes de membre (reprise de HubScreen.CardBase), 390 × 380.</summary>
    static class MemberCardKit
    {
        public static (RectTransform Frame, Image Fill) Build(PrefabContext ctx, string name, out System.Action<MemberCardView> wire)
        {
            var fill = RecipeKit.PanelRoot(name, Theme.Surface, 4f, 7f);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(390, 380);
            var strip = Ui.Img(fill.transform, "Family", Theme.Primary, Sprites.Rounded);
            strip.rectTransform.TopBand(12, 0);
            var portrait = ctx.Nest<PortraitView>("Widgets/Portrait.prefab", fill.transform, "Portrait");
            ((RectTransform)portrait.transform).At(0f, 1f, 16, -24, 96, 96);
            var memberName = Ui.Txt(fill.transform, "", 40, Color.white, TextAnchor.UpperLeft, FontKind.Title, "Name");
            memberName.rectTransform.At(0f, 1f, 124, -22, 250, 48);
            memberName.Inked();
            var rarity = Ui.Txt(fill.transform, "", 22, Theme.Primary, TextAnchor.UpperLeft, FontKind.Body, "Rarity");
            rarity.rectTransform.At(0f, 1f, 126, -70, 250, 28);
            var role = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular, "Role");
            role.rectTransform.At(0f, 1f, 126, -96, 250, 26);
            var level = Ui.Txt(fill.transform, "", 30, Theme.Primary, TextAnchor.UpperRight, FontKind.Title, "Level");
            level.rectTransform.At(1f, 1f, -14, -22, 110, 36);
            var stats = Ui.Txt(fill.transform, "", 20, Theme.Text, TextAnchor.UpperLeft, FontKind.Regular, "Stats");
            stats.rectTransform.At(0f, 1f, 16, -130, 360, 26);
            var skills = Ui.Txt(fill.transform, "", 20, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular, "Skills");
            skills.rectTransform.At(0f, 1f, 16, -160, 360, 50);
            wire = v =>
            {
                ctx.Wire(v, "fill", fill);
                ctx.Wire(v, "strip", strip);
                ctx.Wire(v, "portrait", portrait);
                ctx.Wire(v, "memberName", memberName);
                ctx.Wire(v, "rarity", rarity);
                ctx.Wire(v, "role", role);
                ctx.Wire(v, "level", level);
                ctx.Wire(v, "stats", stats);
                ctx.Wire(v, "skills", skills);
            };
            return (frame, fill);
        }
    }

    /// <summary>Carte d'effectif : fatigue, escouade, entraînement (reprise de HubScreen.TeamCard).</summary>
    public sealed class TeamCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/TeamCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var (frame, fill) = MemberCardKit.Build(ctx, "TeamCard", out var wire);
            var bar = Ui.Bar(fill.transform, "Fatigue", Theme.Success, Theme.SurfaceDark, true, 20);
            ((RectTransform)bar.transform).At(0f, 1f, 16, -214, 356, 28);
            var squad = Ui.Btn(fill.transform, "", Theme.Surface3, Theme.Text, null!, 24, FontKind.Title, "Squad");
            squad.Rect.At(0f, 0f, 14, 74, 356, 52);
            var train = Ui.Btn(fill.transform, "", Theme.Info, Theme.TextDark, null!, 24, FontKind.Title, "Train");
            train.Rect.At(0f, 0f, 14, 14, 356, 52);
            var view = frame.gameObject.AddComponent<TeamCardView>();
            wire(view);
            ctx.Wire(view, "fatigue", bar);
            ctx.Wire(view, "squad", squad);
            ctx.Wire(view, "train", train);
            return frame.gameObject;
        }
    }

    /// <summary>Carte de recrue : coût en PR, bouton Recruter (reprise de HubScreen.RecruitCard).</summary>
    public sealed class RecruitCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/RecruitCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var (frame, fill) = MemberCardKit.Build(ctx, "RecruitCard", out var wire);
            ((RectTransform)fill.transform.Find("Skills")).sizeDelta = new Vector2(360, 80);
            var recruit = Ui.Btn(fill.transform, "", Theme.Pr, Theme.TextDark, null!, 28, FontKind.Title, "Recruit");
            recruit.Rect.At(0f, 0f, 14, 16, 356, 62);
            // Bande de rareté (Agent / Élite / Légende) en bas de carte.
            var rarityBand = Ui.Img(fill.transform, "RarityBand", Theme.Primary, Sprites.Rounded);
            rarityBand.rectTransform.BottomBand(8, 0);
            // État « Recruté » : corps de carte estompé, tampon net par-dessus (hors du groupe estompé).
            var group = fill.gameObject.AddComponent<CanvasGroup>();
            var stamp = Ui.Txt(frame, "", 64, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Stamp");
            stamp.rectTransform.At(0.5f, 0.5f, 0, 20, 360, 90);
            stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, 12);
            stamp.Inked();
            stamp.gameObject.SetActive(false);
            var view = frame.gameObject.AddComponent<RecruitCardView>();
            wire(view);
            ctx.Wire(view, "recruit", recruit);
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "stamp", stamp);
            ctx.Wire(view, "rarityBand", rarityBand);
            return frame.gameObject;
        }
    }

    /// <summary>QG (reprise de HubScreen.Build).</summary>
    public sealed class HubRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Hub.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Hub");
            ScreenKit.Backdrop(rt, Theme.WithAlpha(Theme.Matter, 0.18f), Theme.Background);
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.74f);

            // ---- Barre du haut
            var top = Ui.Panel(rt, "TopBar", Theme.SurfaceDark, 0f, 0f, true).Frame().TopBand(110);
            var logo = Sprites.Logo;
            if (logo != null)
            {
                var l = Ui.Img(top, "Logo", Color.white, logo);
                l.preserveAspect = true;
                l.rectTransform.At(0f, 0.5f, 14, -4, 130, 130);
            }
            var title = Ui.LocTxt(top, "hub.title", 56, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 0.5f, 150, 12, 700, 70);
            title.Inked();
            var player = Ui.Txt(top, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Player");
            player.rectTransform.At(0f, 0.5f, 152, -32, 700, 34);
            var wallet = ctx.Nest<WalletView>("Widgets/Wallet.prefab", top, "Wallet");
            ((RectTransform)wallet.transform).At(1f, 0.5f, -330, 0, 700, 70);
            var logout = Ui.LocBtn(top, "hub.logout", Theme.Surface3, Theme.Text, null!, 22, FontKind.Body, "Logout");
            logout.Rect.At(1f, 0.5f, -20, 22, 290, 46);
            var reset = Ui.LocBtn(top, "login.reset", Theme.Surface2, Theme.TextMuted, null!, 20, FontKind.Body, "Reset");
            reset.Rect.At(1f, 0.5f, -20, -28, 290, 40);

            // ---- Colonne gauche : onglets + grille
            var left = Ui.Rect(rt, "Left");
            left.anchorMin = new Vector2(0, 0);
            left.anchorMax = new Vector2(0, 1);
            left.pivot = new Vector2(0, 0.5f);
            left.offsetMin = new Vector2(30, 30);
            left.offsetMax = new Vector2(1250, -130);
            var tabTeam = Ui.LocBtn(left, "hub.tab.team", Theme.Primary, Theme.TextDark, null!, 36, FontKind.Title, "TabTeam");
            tabTeam.Rect.At(0f, 1f, 0, 0, 300, 76);
            var tabRecruit = Ui.LocBtn(left, "hub.tab.recruit", Theme.Surface3, Theme.Text, null!, 36, FontKind.Title, "TabRecruit");
            tabRecruit.Rect.At(0f, 1f, 320, 0, 340, 76);
            var hint = Ui.Txt(left, "", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Hint");
            hint.rectTransform.At(0f, 1f, 690, -10, 530, 60);
            var gridArea = Ui.Rect(left, "GridArea").Stretch(0, 0, 0, 96);
            var grid = Ui.VScroll(gridArea, out var scroll);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(390, 380);
            layout.spacing = new Vector2(20, 22);
            layout.padding = new RectOffset(4, 4, 6, 20);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            var empty = Ui.Txt(gridArea, "", 30, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "EmptyRecruits");
            empty.rectTransform.At(0.5f, 1f, 0, -20, 1180, 200);
            empty.gameObject.SetActive(false);

            // ---- Barre de la boutique éphémère (onglet Boutique) : sous la rangée de 3 cartes.
            var shopBar = Ui.Rect(left, "ShopBar");
            shopBar.At(0f, 1f, 0, -500, 1200, 84);
            var countdown = Ui.Txt(shopBar, "", 30, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "Countdown");
            countdown.rectTransform.At(0f, 0.5f, 8, 0, 720, 70);
            var reroll = Ui.Btn(shopBar, "", Theme.Credits, Theme.TextDark, null!, 30, FontKind.Title, "Reroll");
            reroll.Rect.At(1f, 0.5f, -4, 0, 440, 76);
            shopBar.gameObject.SetActive(false);

            // ---- Colonne droite : opérations
            var ops = Ui.Panel(rt, "Ops", Theme.Surface, 5f, 10f, true);
            var opsFrame = ops.Frame();
            opsFrame.anchorMin = new Vector2(1, 0);
            opsFrame.anchorMax = new Vector2(1, 1);
            opsFrame.pivot = new Vector2(1, 0.5f);
            opsFrame.offsetMin = new Vector2(-640, 30);
            opsFrame.offsetMax = new Vector2(-30, -140);
            var opsArt = Sprites.BackgroundPortrait;
            if (opsArt != null)
            {
                var holder = Ui.Rect(ops.transform, "OpsArt").Stretch();
                holder.gameObject.AddComponent<RectMask2D>();
                var art = Ui.Img(holder, "Art", new Color(1f, 1f, 1f, 0.3f), opsArt);
                var fit = art.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = opsArt.rect.width / opsArt.rect.height;
            }
            var opsBand = Ui.Panel(opsFrame, "OpsBand", Theme.Primary, 4f, 0f, false);
            opsBand.Frame().At(0.5f, 1f, 0, 26, 520, 70);
            Ui.LocTxt(opsBand.transform, "hub.ops", 40, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Title").rectTransform.Stretch();
            Ui.LocTxt(ops.transform, "zones.ny_1_1.code", 26, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "ZoneCode").rectTransform.At(0f, 1f, 30, -64, 540, 34);
            var zoneName = Ui.LocTxt(ops.transform, "zones.ny_1_1.name", 46, Theme.Text, TextAnchor.UpperLeft, FontKind.Title, "ZoneName");
            zoneName.rectTransform.At(0f, 1f, 30, -96, 560, 56);
            zoneName.Inked();
            Ui.LocTxt(ops.transform, "zones.ny_1_1.desc", 24, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Regular, "ZoneDesc").rectTransform.At(0f, 1f, 30, -156, 550, 90);
            var objectives = Ui.Rect(ops.transform, "Objectives").At(0f, 1f, 30, -250, 550, 110);
            objectives.VLayout(4, 0, TextAnchor.UpperLeft);
            var zoneStatus = Ui.Txt(ops.transform, "", 24, Theme.Success, TextAnchor.UpperLeft, FontKind.Body, "ZoneStatus");
            zoneStatus.rectTransform.At(0f, 1f, 30, -366, 550, 34);
            var squadTitle = Ui.Txt(ops.transform, "", 30, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "SquadTitle");
            squadTitle.rectTransform.At(0f, 1f, 30, -410, 550, 40);
            var squadRow = Ui.Rect(ops.transform, "Squad").At(0f, 1f, 30, -455, 550, 150);
            squadRow.HLayout(12, 0, TextAnchor.UpperLeft);
            var heal = Ui.Btn(ops.transform, "", Theme.Success, Theme.TextDark, null!, 28, FontKind.Title, "Heal");
            heal.Rect.At(0.5f, 0f, 0, 150, 540, 70);
            var deployHint = Ui.Txt(ops.transform, "", 22, Theme.Fatigue, TextAnchor.MiddleCenter, FontKind.Body, "DeployHint");
            deployHint.rectTransform.At(0.5f, 0f, 0, 118, 540, 30);
            var deploy = Ui.LocBtn(ops.transform, "hub.deploy", Theme.Danger, Color.white, null!, 54, FontKind.Title, "Deploy");
            deploy.Rect.At(0.5f, 0f, 0, 20, 540, 96);
            deploy.Label.Inked();

            var view = rt.gameObject.AddComponent<HubView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "player", player);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "emptyRecruits", empty);
            ctx.Wire(view, "zoneStatus", zoneStatus);
            ctx.Wire(view, "squadTitle", squadTitle);
            ctx.Wire(view, "deployHint", deployHint);
            ctx.Wire(view, "tabTeam", tabTeam);
            ctx.Wire(view, "tabRecruit", tabRecruit);
            ctx.Wire(view, "logout", logout);
            ctx.Wire(view, "reset", reset);
            ctx.Wire(view, "heal", heal);
            ctx.Wire(view, "deploy", deploy);
            ctx.Wire(view, "shopBar", shopBar.gameObject);
            ctx.Wire(view, "shopCountdown", countdown);
            ctx.Wire(view, "reroll", reroll);
            ctx.Wire(view, "grid", grid);
            ctx.Wire(view, "scroll", scroll);
            ctx.Wire(view, "objectives", objectives);
            ctx.Wire(view, "squadRow", squadRow);
            ctx.Wire(view, "wallet", wallet);
            ctx.Wire(view, "teamCardPrefab", ctx.Asset("Widgets/TeamCard.prefab").GetComponent<TeamCardView>());
            ctx.Wire(view, "recruitCardPrefab", ctx.Asset("Widgets/RecruitCard.prefab").GetComponent<RecruitCardView>());
            ctx.Wire(view, "squadSlotPrefab", ctx.Asset("Widgets/SquadSlot.prefab").GetComponent<SquadSlotView>());
            ctx.Wire(view, "objectiveLinePrefab", ctx.Asset("Widgets/TextLine.prefab").GetComponent<TextLineView>());
            return rt.gameObject;
        }
    }
}
