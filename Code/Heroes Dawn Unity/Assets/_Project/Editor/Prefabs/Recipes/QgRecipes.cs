#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
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
            var root = PrefabContext.NewRoot("RoomCard", 350, 190);
            var button = Ui.Btn(root, "", Theme.Surface2, Theme.Text, null!, 34, FontKind.Title, "Button");
            button.Rect.Stretch();
            button.Label.gameObject.SetActive(false);
            var title = Ui.Txt(button.Fill.transform, "", 36, Theme.Primary, TextAnchor.UpperLeft, FontKind.Title, "Title");
            title.rectTransform.Stretch(18, 70, 18, 14);
            title.AutoSize(36);
            title.Inked();
            var info = Ui.Txt(button.Fill.transform, "", 24, Theme.Text, TextAnchor.LowerLeft, FontKind.Body, "Info");
            info.rectTransform.Stretch(18, 14, 18, 100);
            var wip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", root, "Wip");
            ((RectTransform)wip.transform).Stretch();
            wip.gameObject.SetActive(false);
            var view = root.gameObject.AddComponent<RoomCardView>();
            ctx.Wire(view, "button", button);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "info", info);
            ctx.Wire(view, "wip", wip);
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

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("QgPage");

            // ---- Pièces
            var roomsTitle = Ui.LocTxt(rt, "qg.rooms", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "RoomsTitle");
            roomsTitle.rectTransform.At(0f, 1f, 0, 0, LeftW, 46);
            roomsTitle.Inked();
            var grid = Ui.Rect(rt, "Rooms").At(0f, 1f, 0, -56, LeftW, 404);
            var layout = grid.gameObject.AddComponent<GridLayoutGroup>();
            layout.cellSize = new Vector2(350, 190);
            layout.spacing = new Vector2(25, 24);
            layout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            layout.constraintCount = 3;
            var rooms = new List<Object>();
            for (int i = 0; i < 5; i++) rooms.Add(ctx.Nest<RoomCardView>("Widgets/RoomCard.prefab", grid, "Room" + i));

            // ---- Fil du QG
            var fil = Ui.Panel(rt, "Fil", Theme.Surface, 5f, 10f, true);
            fil.Frame().At(0f, 0f, 0, 0, LeftW, 330);
            Ui.LocTxt(fil.transform, "qg.fil", 26, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Title, "FilTitle").rectTransform.At(0f, 1f, 24, -14, 600, 34);
            var speaker = Ui.Txt(fil.transform, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Speaker");
            speaker.rectTransform.At(0f, 1f, 24, -52, 600, 50);
            speaker.Inked();
            var filText = Ui.Txt(fil.transform, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Text");
            filText.rectTransform.Stretch(24, 20, 24, 112);

            // ---- Missions du jour (à venir)
            var daily = Ui.Panel(rt, "Daily", Theme.Surface, 5f, 10f, true);
            daily.Frame().At(1f, 1f, 0, 0, RightW, 190);
            Ui.LocTxt(daily.transform, "qg.daily", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title").rectTransform.At(0f, 1f, 22, -14, 540, 46);
            Ui.LocTxt(daily.transform, "qg.daily.body", 24, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Body").rectTransform.Stretch(22, 16, 22, 70);
            var dailyWip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", daily.transform, "Wip");
            ((RectTransform)dailyWip.transform).Stretch();
            dailyWip.FeatureId = WipCatalog.DailyMissions.Id;

            // ---- Alertes
            var alerts = Ui.Panel(rt, "Alerts", Theme.Surface, 5f, 10f, true);
            alerts.Frame().At(1f, 1f, 0, -214, RightW, 250);
            Ui.LocTxt(alerts.transform, "qg.alerts", 34, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title").rectTransform.At(0f, 1f, 22, -14, 540, 46);
            var shopAlert = Ui.Txt(alerts.transform, "", 28, Theme.Text, TextAnchor.MiddleLeft, FontKind.Body, "ShopAlert");
            shopAlert.rectTransform.At(0f, 1f, 22, -66, 540, 44);
            var secondary = Ui.Img(alerts.transform, "Secondary", Theme.Surface2, Sprites.RoundedSmall, true);
            secondary.type = Image.Type.Sliced;
            secondary.rectTransform.At(0.5f, 0f, 0, 18, 546, 104);
            Ui.LocTxt(secondary.transform, "qg.secondary", 24, Theme.TextMuted, TextAnchor.MiddleLeft, FontKind.Body, "Label").rectTransform.Stretch(16, 0, 16, 0);
            var secondaryWip = ctx.Nest<WipMark>("Widgets/WipMark.prefab", secondary.transform, "Wip");
            ((RectTransform)secondaryWip.transform).Stretch();
            secondaryWip.FeatureId = WipCatalog.SecondaryObjectives.Id;

            // ---- MISSION
            var hint = Ui.Txt(rt, "", 22, Theme.Fatigue, TextAnchor.MiddleRight, FontKind.Body, "MissionHint");
            hint.rectTransform.At(1f, 0f, 0, 216, RightW, 30);
            var sub = Ui.Txt(rt, "", 26, Theme.Text, TextAnchor.MiddleRight, FontKind.Title, "MissionSub");
            sub.rectTransform.At(1f, 0f, 0, 176, RightW, 38);
            sub.Inked(1.5f);
            var mission = Ui.LocBtn(rt, "qg.mission", Theme.Danger, Color.white, null!, 72, FontKind.Title, "Mission");
            mission.Rect.At(1f, 0f, 0, 0, RightW, 160);
            mission.Label.Inked();

            var view = rt.gameObject.AddComponent<QgPageView>();
            ScreenKit.WireFade(ctx, view);
            ctx.WireArray(view, "rooms", rooms);
            ctx.Wire(view, "dailyMissionsWip", dailyWip);
            ctx.Wire(view, "secondaryWip", secondaryWip);
            ctx.Wire(view, "shopAlert", shopAlert);
            ctx.Wire(view, "filSpeaker", speaker);
            ctx.Wire(view, "filText", filText);
            ctx.Wire(view, "mission", mission);
            ctx.Wire(view, "missionSub", sub);
            ctx.Wire(view, "missionHint", hint);
            return rt.gameObject;
        }
    }
}
