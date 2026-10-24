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
            var title = Ui.Txt(button.Fill.transform, "", 36, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "Title");
            title.rectTransform.Stretch(18, 64, 18, 12);
            title.AutoSize(36);
            title.Inked();
            var info = Ui.Txt(button.Fill.transform, "", 24, Theme.Text, TextAnchor.LowerLeft, FontKind.Body, "Info");
            info.rectTransform.Stretch(18, 12, 18, 96);
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
            var fill = Ui.Panel(parent, name, color, 5f, 54f, true);
            var frame = fill.Frame();
            frame.sizeDelta = new Vector2(108, 108);
            frame.Pref(108, 108);
            var g = Ui.Txt(fill.transform, glyph, 64, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
            g.rectTransform.Stretch();
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

            // ---- Pièces
            var roomsTitle = Ui.LocTxt(rt, "qg.rooms", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "RoomsTitle");
            roomsTitle.rectTransform.At(0f, 1f, 0, 0, LeftW, 46);
            roomsTitle.Inked();
            var grid = Ui.Rect(rt, "Rooms").At(0f, 1f, 0, -52, LeftW, 356);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(350, 168);
            layout.spacing = new Vector2(25, 20);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            var rooms = new List<Object>();
            for (int i = 0; i < 6; i++) rooms.Add(ctx.Nest<RoomCardView>("Widgets/RoomCard.prefab", grid, "Room" + i));

            // ---- Fil du QG
            var fil = Ui.Panel(rt, "Fil", Theme.Surface, 5f, 10f, true);
            fil.Frame().At(0f, 0f, 0, 0, LeftW, 250);
            Ui.LocTxt(fil.transform, "qg.fil", 26, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Title, "FilTitle").rectTransform.At(0f, 1f, 24, -14, 600, 34);
            var speaker = Ui.Txt(fil.transform, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Speaker");
            speaker.rectTransform.At(0f, 1f, 24, -52, 600, 50);
            speaker.Inked();
            var filText = Ui.Txt(fil.transform, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
            filText.rectTransform.Stretch(24, 16, 24, 104);
            var filButton = fil.Frame().gameObject.AddComponent<Button>();
            filButton.targetGraphic = fil;
            filButton.transition = Selectable.Transition.None;
            Ui.LocTxt(fil.transform, "qg.fil.open", 24, Theme.Primary, TextAnchor.MiddleRight, FontKind.Title, "OpenHint").rectTransform.At(1f, 0f, -24, 14, 520, 32);
            var filBadge = Ui.Img(fil.Frame(), "Badge", Theme.Danger, Sprites.Circle);
            filBadge.rectTransform.At(1f, 1f, -10, -10, 44, 44);
            var filBadgeText = Ui.Txt(filBadge.transform, "", 26, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Text");
            filBadgeText.rectTransform.Stretch();
            filBadge.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;

            // ---- Missions du jour (à venir)
            var daily = Ui.Panel(rt, "Daily", Theme.Surface, 5f, 10f, true);
            daily.Frame().At(1f, 1f, 0, 0, RightW, 300);
            Ui.LocTxt(daily.transform, "qg.daily", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title").rectTransform.At(0f, 1f, 22, -14, 540, 46);
            var dailyBody = Ui.Txt(daily.transform, "", 22, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Body");
            dailyBody.rectTransform.Stretch(22, 12, 22, 62);
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
            var alertRow = Ui.Rect(rt, "Alerts").At(1f, 1f, 0, -316, RightW, 108);
            alertRow.HLayout(20, 0, TextAnchor.MiddleRight);
            var shopIcon = AlertIcon(alertRow, "ShopAlert", "B", Theme.Pr, out var shopBadgeText);
            var secondaryIcon = AlertIcon(alertRow, "SecondaryAlert", "!", Theme.Fatigue, out var secondaryDotText);
            var secondaryButton = secondaryIcon;
            var shopAlertButton = shopIcon;
            var secondaryDot = secondaryDotText.transform.parent.gameObject;

            // ---- MISSION
            var hint = Ui.Txt(rt, "", 22, Theme.Fatigue, TextAnchor.MiddleRight, FontKind.Body, "MissionHint");
            hint.rectTransform.At(1f, 0f, 0, 216, RightW, 30);
            var sub = Ui.Txt(rt, "", 26, Theme.Text, TextAnchor.MiddleRight, FontKind.Title, "MissionSub");
            sub.rectTransform.At(1f, 0f, 0, 176, RightW, 38);
            sub.Inked(1.5f);
            var mission = Ui.LocBtn(rt, "qg.mission", Theme.Danger, Color.white, null!, 72, FontKind.Title, "Mission");
            mission.Rect.At(1f, 0f, 0, 0, RightW, 160);
            mission.Label.Inked();
            RecipeKit.Shine(mission);
            RecipeKit.Mark(mission, TutorialTarget.QgMission);

            var view = rt.gameObject.AddComponent<QgPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "rooms", rooms);
            ctx.Wire(view, "dailyCard", dailyButton);
            ctx.Wire(view, "dailyBody", dailyBody);
            ctx.Wire(view, "dailyBadge", dailyBadgeText);
            ctx.Wire(view, "secondaryCard", secondaryButton);
            ctx.Wire(view, "shopAlertButton", shopAlertButton);
            ctx.Wire(view, "shopAlertBadge", shopBadgeText);
            ctx.Wire(view, "secondaryDot", secondaryDot);
            ctx.Wire(view, "filSpeaker", speaker);
            ctx.Wire(view, "filText", filText);
            ctx.Wire(view, "filCard", filButton);
            ctx.Wire(view, "questsBadge", filBadgeText);
            ctx.Wire(view, "mission", mission);
            ctx.Wire(view, "missionSub", sub);
            ctx.Wire(view, "missionHint", hint);
            return rt.gameObject;
        }
    }
}
