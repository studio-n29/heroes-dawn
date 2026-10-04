#nullable enable
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using N29.HeroesDawn.Presentation.Hub;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Case du plateau (reprise de l'ancien BoardMapView.Build, partie cases).</summary>
    public sealed class BoardNodeRecipe : IPrefabRecipe
    {
        const float NodeSize = 86f;
        public string RelativePath => "Widgets/BoardNode.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("BoardNode", NodeSize + 40, NodeSize + 40);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var highlight = Ui.Img(root, "Highlight", Theme.Primary, Sprites.Ring);
            highlight.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize + 36, NodeSize + 36);
            highlight.gameObject.AddComponent<Pulse>().Amplitude = 0.1f;
            highlight.gameObject.SetActive(false);
            var objective = Ui.Img(root, "Objective", Theme.Primary, Sprites.Ring);
            objective.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize + 18, NodeSize + 18);
            var frame = Ui.Img(root, "Frame", Theme.Ink, Sprites.Circle, true);
            frame.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize, NodeSize);
            var button = frame.gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.None;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var disc = Ui.Img(frame.transform, "Disc", Theme.Surface3, Sprites.Circle);
            disc.rectTransform.Stretch(5, 5, 5, 5);
            var label = Ui.Txt(disc.transform, "?", 34, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Glyph");
            label.rectTransform.Stretch();
            label.Inked();
            var sub = Ui.Txt(root, "", 20, Theme.TextMuted, TextAnchor.UpperCenter, FontKind.Body, "Sub");
            sub.rectTransform.At(0.5f, 0f, 0, -14, 160, 26);
            sub.Inked();
            var tag = Ui.Txt(root, "OBJ", 20, Theme.Primary, TextAnchor.LowerCenter, FontKind.Title, "ObjTag");
            tag.rectTransform.At(0.5f, 1f, 0, 12, 120, 26);
            tag.Inked();
            var badge = Ui.Img(root, "CostBadge", Theme.Primary, Sprites.Circle);
            badge.rectTransform.At(1f, 1f, -2, -2, 40, 40);
            var cost = Ui.Txt(badge.transform, "", 24, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Cost");
            cost.rectTransform.Stretch();
            badge.gameObject.SetActive(false);

            RecipeKit.Mark(frame, TutorialTarget.BoardDestination, enabled: false);
            var view = root.gameObject.AddComponent<BoardNodeView>();
            ctx.Wire(view, "highlight", highlight);
            ctx.Wire(view, "objective", objective);
            ctx.Wire(view, "frame", frame);
            ctx.Wire(view, "disc", disc);
            ctx.Wire(view, "label", label);
            ctx.Wire(view, "sub", sub);
            ctx.Wire(view, "objectiveTag", tag);
            ctx.Wire(view, "costBadge", badge.gameObject);
            ctx.Wire(view, "cost", cost);
            ctx.Wire(view, "button", button);
            return root.gameObject;
        }
    }

    /// <summary>Lien entre deux cases (positionné et orienté par la carte).</summary>
    public sealed class BoardLinkRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/BoardLink.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("BoardLink", 100, 12);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var img = root.gameObject.AddComponent<Image>();
            img.color = Theme.Surface3;
            img.raycastTarget = false;
            var outline = root.gameObject.AddComponent<Outline>();
            outline.effectColor = Theme.Ink;
            outline.effectDistance = new Vector2(2, 2);
            return root.gameObject;
        }
    }

    /// <summary>Pion : portrait du chef d'escouade (imbriqué) avec ombre portée, ancré par le bas.</summary>
    public sealed class PawnRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/Pawn.prefab";
        public int Order => 5;

        public GameObject Build(PrefabContext ctx)
        {
            var holder = PrefabContext.NewRoot("holder");
            var pawn = (RectTransform)ctx.Nest("Widgets/Portrait.prefab", holder, "Pawn").transform;
            pawn.SetParent(null, false);
            Object.DestroyImmediate(holder.gameObject);
            pawn.anchorMin = pawn.anchorMax = new Vector2(0.5f, 0.5f);
            pawn.pivot = new Vector2(0.5f, 0f);
            pawn.sizeDelta = new Vector2(70, 70);
            var shadow = pawn.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            shadow.effectDistance = new Vector2(6, -6);
            return pawn.gameObject;
        }
    }

    /// <summary>Plateau (reprise de l'ancien BoardScreen.Build).</summary>
    public sealed class BoardRecipe : IPrefabRecipe
    {
        public string RelativePath => "Screens/Board.prefab";
        public int Order => 20;

        public GameObject Build(PrefabContext ctx)
        {
            var rt = ScreenKit.Root("Board");
            ScreenKit.Backdrop(rt, Theme.WithAlpha(Theme.Info, 0.16f), Theme.Hex(0x0E1420));
            ScreenKit.ArtBackdrop(rt, Sprites.BackgroundLandscape, 1f, 0.86f);

            // ---- Barre du haut
            var top = Ui.Panel(rt, "TopBar", Theme.SurfaceDark, 0f, 0f, true).Frame().TopBand(96);
            var zoneTitle = Ui.Txt(top, "", 46, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Zone");
            zoneTitle.rectTransform.At(0f, 0.5f, 30, 0, 900, 70);
            zoneTitle.Inked();
            var wallet = ctx.Nest<WalletView>("Widgets/Wallet.prefab", top, "Wallet");
            ((RectTransform)wallet.transform).At(1f, 0.5f, -290, 0, 700, 66);
            var back = Ui.LocBtn(top, "board.back_hq", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "BackHq");
            back.Rect.At(1f, 0.5f, -20, 0, 250, 66);

            // ---- Objectifs (récitatif en haut à gauche)
            var obj = Ui.Panel(rt, "Objectives", Theme.Primary, 4f, 6f, false);
            obj.Frame().At(0f, 1f, 24, -112, 520, 156);
            RecipeKit.Mark(obj.Frame(), TutorialTarget.BoardObjectives);
            Ui.LocTxt(obj.transform, "board.objectives", 30, Theme.TextDark, TextAnchor.UpperLeft, FontKind.Title, "Title").rectTransform.At(0f, 1f, 16, -8, 480, 36);
            var objectiveList = Ui.Rect(obj.transform, "List").At(0f, 1f, 16, -46, 490, 120);
            objectiveList.VLayout(2, 0, TextAnchor.UpperLeft);
            Ui.LocTxt(rt, "board.legend", 22, Theme.TextMuted, TextAnchor.UpperRight, FontKind.Regular, "Legend").rectTransform.At(1f, 1f, -30, -112, 900, 30);

            // ---- Carte
            var mapArea = Ui.Rect(rt, "MapArea").At(0.5f, 0.5f, 0, -20, 1840, 600);
            var mapRt = Ui.Rect(mapArea, "Map").Stretch();
            var linkLayer = Ui.Rect(mapRt, "Links").Stretch();
            var nodeLayer = Ui.Rect(mapRt, "Nodes").Stretch();
            var map = mapRt.gameObject.AddComponent<BoardMapView>();
            ctx.Wire(map, "linkLayer", linkLayer);
            ctx.Wire(map, "nodeLayer", nodeLayer);
            ctx.Wire(map, "nodePrefab", ctx.Asset("Widgets/BoardNode.prefab").GetComponent<BoardNodeView>());
            ctx.Wire(map, "linkPrefab", ctx.Asset("Widgets/BoardLink.prefab").GetComponent<Image>());
            ctx.Wire(map, "pawnPrefab", ctx.Asset("Widgets/Pawn.prefab").GetComponent<PortraitView>());

            // ---- HUD du bas
            var hud = Ui.Panel(rt, "Hud", Theme.Surface, 5f, 8f, true).Frame().BottomBand(170, 0);
            hud.offsetMin = new Vector2(20, 16);
            hud.offsetMax = new Vector2(-20, 186);
            var hudFill = hud.GetChild(0);
            var squadStrip = Ui.Rect(hudFill, "Squad").At(0f, 0.5f, 20, 0, 560, 140);
            squadStrip.HLayout(10, 0, TextAnchor.MiddleLeft);
            var hint = Ui.Txt(hudFill, "", 28, Theme.Text, TextAnchor.MiddleCenter, FontKind.Body, "Hint");
            hint.rectTransform.At(0.5f, 0.5f, -120, 24, 560, 70);
            var pm = Ui.Txt(hudFill, "", 40, Theme.Primary, TextAnchor.MiddleCenter, FontKind.Title, "Pm");
            pm.rectTransform.At(0.5f, 0.5f, -120, -34, 300, 50);
            pm.Inked();
            var reserve = Ui.Bar(hudFill, "Reserve", Theme.Primary, Theme.SurfaceDark, true, 20);
            ((RectTransform)reserve.transform).At(0.5f, 0f, -120, 10, 420, 26);
            var dieFill = Ui.Panel(hudFill, "Die", Color.white, 5f, 6f, false);
            var die = dieFill.Frame().At(1f, 0.5f, -640, 0, 124, 124);
            var dieFace = Ui.Txt(dieFill.transform, "-", 86, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Face");
            dieFace.rectTransform.Stretch();
            var roll = Ui.LocBtn(hudFill, "board.roll", Theme.Primary, Theme.TextDark, null!, 54, FontKind.Title, "Roll");
            roll.Rect.At(1f, 0.5f, -250, 26, 360, 90);
            RecipeKit.Mark(roll, TutorialTarget.BoardRoll);
            RecipeKit.Shine(roll);
            var bonus = Ui.Btn(hudFill, "", Theme.Info, Theme.TextDark, null!, 26, FontKind.Title, "Bonus");
            bonus.Rect.At(1f, 0.5f, -20, 26, 220, 90);
            var end = Ui.LocBtn(hudFill, "board.end_move", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "EndMove");
            end.Rect.At(1f, 0.5f, -20, -46, 590, 52); // sous Lancer et Dé bonus, jamais sous le dé

            var view = rt.gameObject.AddComponent<BoardView>();
            ScreenKit.WireFade(ctx, view);
            ctx.Wire(view, "zoneTitle", zoneTitle);
            ctx.Wire(view, "objectiveList", objectiveList);
            ctx.Wire(view, "squadStrip", squadStrip);
            ctx.Wire(view, "hint", hint);
            ctx.Wire(view, "pm", pm);
            ctx.Wire(view, "reserve", reserve);
            ctx.Wire(view, "die", die);
            ctx.Wire(view, "dieFace", dieFace);
            ctx.Wire(view, "roll", roll);
            ctx.Wire(view, "bonus", bonus);
            ctx.Wire(view, "end", end);
            ctx.Wire(view, "back", back);
            ctx.Wire(view, "wallet", wallet);
            ctx.Wire(view, "map", map);
            ctx.Wire(view, "objectiveLinePrefab", ctx.Asset("Widgets/TextLine.prefab").GetComponent<TextLineView>());
            ctx.Wire(view, "squadSlotPrefab", ctx.Asset("Widgets/SquadSlot.prefab").GetComponent<SquadSlotView>());
            return rt.gameObject;
        }
    }
}
