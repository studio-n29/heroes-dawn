#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Case de pièce du QG (M10 ③) : bouton plein, nom, info ; bandeau BIENTÔT par-dessus quand la pièce est à venir.</summary>
    public sealed class RoomCardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/RoomCard.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("RoomCard", 350, 168);
            var button = Ui.Btn(root, "", Theme.Surface2, Theme.Text, null!, 34, FontKind.Title, "Button");
            button.Rect.Stretch();
            button.Label.gameObject.SetActive(false);
            var holder = Ui.Rect(button.Fill.transform, "ArtHolder").Stretch();
            holder.gameObject.AddComponent<RectMask2D>();
            var art = Ui.Img(holder, "Art", Color.white, null);
            art.raycastTarget = false;
            art.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var shade = Ui.Img(holder, "Shade", Theme.WithAlpha(Theme.Outline, 0.88f), Sprites.VerticalGradient);
            shade.raycastTarget = false;
            shade.rectTransform.Stretch();
            var caption = Ui.Img(button.Fill.transform, "Caption", Theme.WithAlpha(Theme.Background, 0.96f), Sprites.Panel);
            caption.rectTransform.BottomBand(164);
            var title = Ui.Txt(button.Fill.transform, "", 36, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "Title");
            title.rectTransform.At(0f, 0f, 18, 74, 171, 82);
            title.AutoSize(36);
            title.Inked();
            var info = Ui.Txt(button.Fill.transform, "", 24, Theme.Text, TextAnchor.LowerLeft, FontKind.Body, "Info");
            info.rectTransform.At(0f, 0f, 18, 14, 171, 52);
            // État (accessible ou nouveau) : étiquette en haut à droite, l'objectif reste en bas.
            var state = Ui.Txt(button.Fill.transform, "", 22, Theme.Primary, TextAnchor.UpperRight, FontKind.Title, "State");
            state.rectTransform.At(1f, 1f, -12, -10, 180, 34);
            state.Inked();
            var icon = Ui.Img(button.Fill.transform, "FunctionIcon", Theme.Accent, Sprites.Glyph("Register"));
            icon.rectTransform.At(0f, 1f, 12, -10, 32, 32);
            var wip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", root, "Wip");
            ((RectTransform)wip.transform).Stretch();
            var band = (RectTransform)wip.transform.Find("Band");
            band.localScale = new Vector3(0.8f, 0.8f, 1f);
            band.anchoredPosition = new Vector2(70, -40); // sous le titre, jamais dessus
            wip.gameObject.SetActive(false);
            var badgeImg = Ui.Img(root, "Badge", Theme.Danger, Sprites.Circle);
            badgeImg.rectTransform.At(1f, 1f, 10, 10, 44, 44);
            var badgeText = Ui.Txt(badgeImg.transform, "", 26, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            badgeText.rectTransform.Stretch();
            badgeImg.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;
            badgeImg.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<RoomCardView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "art", art);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "info", info);
            ctx.Wire(view, "state", state);
            ctx.Wire(view, "icon", icon);
            ctx.Wire(view, "wip", wip);
            ctx.Wire(view, "badge", badgeImg.gameObject);
            ctx.Wire(view, "badgeText", badgeText);
            return root.gameObject;
        }
    }

    /// <summary>
    /// Page QG (M10) : pièces en cases de BD à gauche, fil du QG dessous ; à droite missions du jour (BIENTÔT), alertes
    /// (compte à rebours de la boutique, objectifs secondaires BIENTÔT) et MISSION en rouge en bas.
    /// </summary>
    public sealed class QgPageRecipe : IPrefabRecipe
    {
        public string RelativePath => "Pages/Qg.prefab";
        public int Order => 10;

        const float LeftW = 1100, RightW = 590;


        /// <summary>Icône d'alerte ronde (108 px) avec pastille ; renvoie le bouton et le texte de la pastille.</summary>
        static Button AlertIcon(RectTransform parent, string name, string glyph, Color color, out TMPro.TextMeshProUGUI badgeText)
        {
            var fill = Ui.Panel(parent, name, Theme.SurfaceDark, 4f, 3f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(260, 96);
            frame.Pref(260, 96);
            var icon = Ui.Img(fill.transform, "Glyph", Theme.Accent, Sprites.Glyph(glyph));
            icon.rectTransform.At(0f, 0.5f, 16, 0, 40, 40);
            var label = Ui.LocTxt(fill.transform, glyph == "Shop" ? "nav.shop" : "qg.daily", 26, Theme.Text, TextAnchor.MiddleLeft, FontKind.Title, "Label");
            label.rectTransform.Stretch(68, 8, 16, 8);
            var button = frame.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var dot = Ui.Img(frame, "Badge", Theme.Danger, Sprites.Circle);
            dot.rectTransform.At(1f, 1f, 4, 4, 40, 40);
            badgeText = Ui.Txt(dot.transform, "", 24, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            badgeText.rectTransform.Stretch();
            dot.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;
            return button;
        }
        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("QgPage");
            RecipeKit.Mark(rt, TutorialTarget.QgPage);

            // ---- Quête principale : bandeau compact en haut (touché, il lance la mission comme le bouton MISSION)
            var banner = Ui.Panel(rt, "MainBanner", Theme.Surface, 5f, 10f, true);
            banner.Frame().At(0f, 1f, 0, 0, LeftW, 96);
            var bannerTitle = Ui.Txt(banner.transform, "", 28, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "BannerTitle");
            bannerTitle.rectTransform.At(0f, 1f, 24, -12, 1000, 36);
            bannerTitle.Inked();
            var bannerLine = Ui.Txt(banner.transform, "", 26, Theme.Text, TextAnchor.LowerLeft, FontKind.Body, "BannerLine");
            bannerLine.rectTransform.At(0f, 0f, 24, 10, LeftW - 48, 34);
            var bannerButton = banner.Frame().gameObject.AddComponent<Button>();
            bannerButton.targetGraphic = banner;
            bannerButton.transition = Selectable.Transition.None;

            // ---- Pièces (sous le bandeau)
            var roomsTitle = Ui.LocTxt(rt, "qg.rooms", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "RoomsTitle");
            roomsTitle.rectTransform.At(0f, 0f, 0, 340, LeftW, 46);
            roomsTitle.Inked();
            // Pièces en cartes verticales, rangées à l'horizontale : cinq colonnes de 207 sur 1100, hauteur 400 sous le titre.
            var grid = Ui.Rect(rt, "Rooms").At(0f, 0f, 0, 0, LeftW, 320);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(207, 320);
            layout.spacing = new Vector2(16, 0);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 5;
            var rooms = new List<Object>();
            for (int i = 0; i < 5; i++) rooms.Add(ctx.Nest<RoomCardView>("Widgets/RoomCard.prefab", grid, "Room" + i)); // cinq pièces : le doublon Équipe est retiré (L3)

            // ---- Activités du jour : bandeau avec leur nombre ; un toucher ouvre le panneau latéral (fermé par défaut)
            var daily = Ui.Panel(rt, "Daily", Theme.Surface, 5f, 10f, true);
            daily.Frame().At(1f, 1f, 0, 0, RightW, 96);
            Ui.LocTxt(daily.transform, "qg.daily", 30, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title").rectTransform.At(0f, 1f, 22, -10, 540, 40);
            var dailyCount = Ui.Txt(daily.transform, "", 26, Theme.Text, TextAnchor.LowerLeft, FontKind.Title, "Count");
            dailyCount.rectTransform.At(0f, 0f, 22, 10, 540, 36);
            var drawer = Ui.Panel(rt, "DailyDrawer", Theme.Surface, 5f, 10f, true);
            drawer.Frame().At(1f, 1f, 0, -112, RightW, 250);
            Ui.LocTxt(drawer.transform, "qg.daily", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "DrawerTitle").gameObject.SetActive(false); // le bandeau au-dessus porte déjà le titre
            var close = Ui.LocBtn(drawer.transform, "nav.back", Theme.Surface3, Theme.Text, null!, 26, FontKind.Title, "Close");
            close.Rect.At(1f, 1f, -14, -14, 160, 64);
            close.Rect.gameObject.SetActive(false); // la liste du jour reste affichée en permanence (planche 3.1)
            var dailyBody = Ui.Txt(drawer.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Body");
            dailyBody.rectTransform.Stretch(22, 16, 22, 16);
            dailyBody.fontSize = 24;
            dailyBody.lineSpacing = 8;
            var dailyButton = daily.Frame().gameObject.AddComponent<Button>();
            dailyButton.targetGraphic = daily;
            dailyButton.transition = Selectable.Transition.None;
            var dailyBadge = Ui.Img(daily.Frame(), "Badge", Theme.Danger, Sprites.Circle);
            dailyBadge.rectTransform.At(1f, 1f, -10, -10, 44, 44);
            var dailyBadgeText = Ui.Txt(dailyBadge.transform, "", 26, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            dailyBadgeText.rectTransform.Stretch();
            dailyBadge.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;

            // ---- Alertes : deux icônes (boutique, objectif secondaire) ; le détail s'ouvre dans un pop-up
            var alertRow = Ui.Rect(rt, "Alerts").At(0f, 1f, 0, -112, LeftW, 96);
            alertRow.HLayout(20, 0, TextAnchor.MiddleLeft);
            var shopIcon = AlertIcon(alertRow, "ShopAlert", "Shop", Theme.Pr, out var shopBadgeText);
            var secondaryIcon = AlertIcon(alertRow, "SecondaryAlert", "Quest", Theme.Fatigue, out var secondaryDotText);
            var secondaryButton = secondaryIcon;
            var shopAlertButton = shopIcon;
            var secondaryDot = secondaryDotText.transform.parent.gameObject;

            // ---- Prochaine mission : carte illustrée au-dessus du bouton MISSION (planche 3.1)
            var next = Ui.Panel(rt, "NextMission", Theme.Surface, 5f, 10f, false);
            next.Frame().At(1f, 0f, 0, 130, RightW, 300);
            var nextHolder = Ui.Rect(next.transform, "ArtHolder").Stretch(6, 6, 6, 6);
            nextHolder.gameObject.AddComponent<RectMask2D>();
            var nextArt = Ui.Img(nextHolder, "Art", Color.white, TacticalArt.Load("combat-red-hook"));
            nextArt.raycastTarget = false;
            nextArt.gameObject.AddComponent<AspectRatioFitter>().aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            var nextShade = Ui.Img(nextHolder, "Shade", Theme.WithAlpha(Theme.Outline, 0.85f), Sprites.VerticalGradient);
            nextShade.raycastTarget = false;
            nextShade.rectTransform.Stretch();

            // ---- MISSION
            var hint = Ui.Txt(rt, "", 22, Theme.Fatigue, TextAnchor.MiddleRight, FontKind.Body, "MissionHint");
            hint.rectTransform.At(1f, 0f, 0, 182, RightW, 30);
            var sub = Ui.Txt(rt, "", 26, Theme.Text, TextAnchor.MiddleRight, FontKind.Title, "MissionSub");
            sub.rectTransform.At(1f, 0f, 0, 138, RightW, 38);
            sub.Inked(1.5f);
            var mission = Ui.LocBtn(rt, "qg.mission", Theme.Primary, Theme.TextDark, null!, 64, FontKind.Title, "Mission");
            mission.Rect.At(1f, 0f, 0, 0, RightW, 120);
            mission.Label.Inked();
            RecipeKit.Shine(mission);
            RecipeKit.Mark(mission, TutorialTarget.QgMission);

            var view = rt.gameObject.AddComponent<QgPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "rooms", rooms);
            ctx.Wire(view, "dailyCard", dailyButton);
            ctx.Wire(view, "dailyBody", dailyBody);
            ctx.Wire(view, "dailyCount", dailyCount);
            ctx.Wire(view, "dailyDrawer", drawer.Frame().gameObject);
            ctx.Wire(view, "dailyClose", close.Button);
            drawer.Frame().SetAsLastSibling(); // le panneau passe au-dessus des alertes
            ctx.Wire(view, "dailyBadge", dailyBadgeText);
            ctx.Wire(view, "secondaryCard", secondaryButton);
            ctx.Wire(view, "shopAlertButton", shopAlertButton);
            ctx.Wire(view, "shopAlertBadge", shopBadgeText);
            ctx.Wire(view, "secondaryDot", secondaryDot);
            ctx.Wire(view, "mission", mission);
            ctx.Wire(view, "missionSub", sub);
            ctx.Wire(view, "missionHint", hint);
            ctx.Wire(view, "banner", bannerButton);
            ctx.Wire(view, "bannerTitle", bannerTitle);
            ctx.Wire(view, "bannerLine", bannerLine);
            return rt.gameObject;
        }
    }
}
