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
            var focus = Ui.Img(root, "Focus", Color.white, Sprites.Ring);
            focus.rectTransform.At(0.5f, 0.5f, 0, 0, NodeSize + 58, NodeSize + 58);
            focus.raycastTarget = false;
            focus.gameObject.AddComponent<Pulse>().Amplitude = 0.06f;
            focus.gameObject.SetActive(false);
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
            ctx.Wire(view, "focus", focus);
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

    /// <summary>Nuage de brouillard de guerre (posé par la carte sur chaque case cachée).</summary>
    public sealed class FogCloudRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/FogCloud.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("FogCloud", 470, 340);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var img = root.gameObject.AddComponent<Image>();
            img.sprite = Sprites.Cloud;
            img.color = new Color(0.78f, 0.84f, 0.93f, 0.78f);
            img.raycastTarget = false;
            var view = root.gameObject.AddComponent<FogCloudView>();
            ctx.Wire(view, "image", img);
            return root.gameObject;
        }
    }

    /// <summary>Pastille du coût en PM d'un chemin (au milieu de son tracé).</summary>
    public sealed class BoardLinkCostRecipe : IPrefabRecipe
    {
        public string RelativePath => "Widgets/BoardLinkCost.prefab";
        public int Order => 0;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("BoardLinkCost", 44, 44);
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(0.5f, 0.5f);
            var bg = Ui.Img(root, "Background", Theme.Surface3, Sprites.Circle, true);
            bg.rectTransform.Stretch();
            bg.raycastTarget = false;
            var label = Ui.Txt(bg.transform, "1", 26, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Cost");
            label.rectTransform.Stretch();
            label.raycastTarget = false;
            var view = root.gameObject.AddComponent<BoardLinkCostView>();
            ctx.Wire(view, "background", bg);
            ctx.Wire(view, "label", label);
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
            shadow.effectColor = Theme.WithAlpha(Theme.Navy, 0.6f);
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
            ScreenKit.Backdrop(rt, Theme.SkyTop, Theme.SkyBottom);
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
            // Fenêtre de carte agrandie (le plateau déborde : caméra à glisser / zoomer, voir BoardMapView).
            var mapArea = Ui.Rect(rt, "MapArea").At(0.5f, 0.5f, 0, 50, 1880, 780);
            mapArea.gameObject.AddComponent<RectMask2D>();
            var mapRt = Ui.Rect(mapArea, "Map").Stretch();
            var mapHit = mapRt.gameObject.AddComponent<Image>(); // reçoit le glisser et la molette
            mapHit.color = new Color(0f, 0f, 0f, 0f);
            var content = Ui.Rect(mapRt, "Content").At(0.5f, 0.5f, 0, 0, 0, 0);
            var linkLayer = Ui.Rect(content, "Links").Stretch();
            var nodeLayer = Ui.Rect(content, "Nodes").Stretch();
            var map = mapRt.gameObject.AddComponent<BoardMapView>();
            var zoomIn = Ui.Btn(mapArea, "+", Theme.Surface3, Theme.Text, null!, 40, FontKind.Title, "ZoomIn");
            zoomIn.Rect.At(0f, 0f, 20, 170, 64, 64);
            var zoomOut = Ui.Btn(mapArea, "-", Theme.Surface3, Theme.Text, null!, 40, FontKind.Title, "ZoomOut");
            zoomOut.Rect.At(0f, 0f, 20, 244, 64, 64);
            var recenter = Ui.Btn(mapArea, "PION", Theme.Surface3, Theme.Text, null!, 22, FontKind.Title, "Recenter");
            recenter.Rect.At(0f, 0f, 20, 318, 64, 64);
            var prevCase = Ui.Btn(mapArea, "< CASE", Theme.Surface3, Theme.Text, null!, 24, FontKind.Title, "PrevCase");
            prevCase.Rect.At(0f, 0f, 20, 90, 140, 64);
            var nextCase = Ui.Btn(mapArea, "CASE >", Theme.Surface3, Theme.Text, null!, 24, FontKind.Title, "NextCase");
            nextCase.Rect.At(0f, 0f, 170, 90, 140, 64);
            ctx.Wire(map, "content", content);
            ctx.Wire(map, "zoomIn", zoomIn.Button);
            ctx.Wire(map, "zoomOut", zoomOut.Button);
            ctx.Wire(map, "recenter", recenter.Button);
            ctx.Wire(map, "prevCase", prevCase.Button);
            ctx.Wire(map, "nextCase", nextCase.Button);
            ctx.Wire(map, "linkLayer", linkLayer);
            ctx.Wire(map, "nodeLayer", nodeLayer);
            ctx.Wire(map, "nodePrefab", ctx.Asset("Widgets/BoardNode.prefab").GetComponent<BoardNodeView>());
            ctx.Wire(map, "linkPrefab", ctx.Asset("Widgets/BoardLink.prefab").GetComponent<Image>());
            ctx.Wire(map, "linkCostPrefab", ctx.Asset("Widgets/BoardLinkCost.prefab").GetComponent<BoardLinkCostView>());
            ctx.Wire(map, "fogPrefab", ctx.Asset("Widgets/FogCloud.prefab").GetComponent<FogCloudView>());
            ctx.Wire(map, "pawnPrefab", ctx.Asset("Widgets/Pawn.prefab").GetComponent<PortraitView>());

            // ---- Pop-up d'information de la case visée (déplacement) : se dézoome sur le chemin, « S'y rendre » valide
            var previewBox = Ui.Panel(rt, "MovePreview", Theme.WithAlpha(Theme.Frost, 0.96f), 5f, 8f, false);
            var previewFrame = previewBox.Frame().At(1f, 0.5f, -30, 20, 520, 480);
            var previewTitle = Ui.Txt(previewBox.transform, "", 36, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            previewTitle.rectTransform.At(0f, 1f, 18, -12, 484, 52);
            previewTitle.AutoSize(36);
            previewTitle.Inked();
            var previewBody = Ui.Txt(previewBox.transform, "", 25, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Body");
            previewBody.rectTransform.At(0f, 1f, 18, -70, 484, 170);
            var previewDetail = Ui.Txt(previewBox.transform, "", 24, Theme.TextMuted, TextAnchor.UpperLeft, FontKind.Body, "Detail");
            previewDetail.rectTransform.At(0f, 1f, 18, -250, 484, 110);
            var previewConfirm = Ui.LocBtn(previewBox.transform, "board.preview.go", Theme.Primary, Theme.TextDark, null!, 38, FontKind.Title, "Go");
            previewConfirm.Rect.At(0f, 0f, 18, 18, 320, 84);
            RecipeKit.Shine(previewConfirm);
            var previewCancel = Ui.LocBtn(previewBox.transform, "board.preview.cancel", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "Cancel");
            previewCancel.Rect.At(1f, 0f, -18, 18, 150, 84);
            previewFrame.gameObject.SetActive(false);

            // ---- HUD du bas
            var hud =Ui.Panel(rt, "Hud", Theme.Surface, 5f, 8f, true).Frame().BottomBand(170, 0);
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

            // La carte déborde de sa fenêtre : barre du haut, objectifs, HUD et fiche passent au-dessus d'elle.
            top.SetAsLastSibling();
            obj.Frame().SetAsLastSibling();
            hud.SetAsLastSibling();
            previewFrame.SetAsLastSibling();

            // Onomatopée de bande dessinée au centre, par-dessus tout (jouée avant un combat ou un événement).
            var burstHolder = new GameObject("ComicBurst", typeof(RectTransform)).GetComponent<RectTransform>();
            burstHolder.SetParent(rt, false);
            burstHolder.Stretch();
            var burstRoot = new GameObject("Root", typeof(RectTransform)).GetComponent<RectTransform>();
            burstRoot.SetParent(burstHolder, false);
            burstRoot.anchorMin = burstRoot.anchorMax = burstRoot.pivot = new Vector2(0.5f, 0.5f);
            burstRoot.anchoredPosition = Vector2.zero;
            burstRoot.sizeDelta = new Vector2(760, 520);
            var burstShadow = Ui.Img(burstRoot, "Shadow", Theme.Ink, Sprites.Burst);
            burstShadow.rectTransform.At(0.5f, 0.5f, 10, -10, 760, 520);
            burstShadow.raycastTarget = false;
            var burstImg = Ui.Img(burstRoot, "Burst", Theme.Primary, Sprites.Burst);
            burstImg.rectTransform.At(0.5f, 0.5f, 0, 0, 740, 500);
            burstImg.raycastTarget = false;
            var burstLabel = Ui.Txt(burstRoot, "COMBAT !", 120, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Label");
            burstLabel.rectTransform.Stretch(120, 120, 120, 120);
            burstLabel.raycastTarget = false;
            var burstView = burstHolder.gameObject.AddComponent<ComicBurstView>();
            ctx.Wire(burstView, "root", burstRoot);
            ctx.Wire(burstView, "shadow", burstShadow);
            ctx.Wire(burstView, "burst", burstImg);
            ctx.Wire(burstView, "label", burstLabel);
            burstHolder.SetAsLastSibling();

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
            ctx.Wire(view, "comicBurst", burstView);
            ctx.Wire(view, "previewRoot", previewFrame);
            ctx.Wire(view, "previewTitle", previewTitle);
            ctx.Wire(view, "previewBody", previewBody);
            ctx.Wire(view, "previewDetail", previewDetail);
            ctx.Wire(view, "previewConfirm", previewConfirm);
            ctx.Wire(view, "previewCancel", previewCancel);
            ctx.Wire(view, "objectiveLinePrefab", ctx.Asset("Widgets/TextLine.prefab").GetComponent<TextLineView>());
            ctx.Wire(view, "squadSlotPrefab", ctx.Asset("Widgets/SquadSlot.prefab").GetComponent<SquadSlotView>());
            return rt.gameObject;
        }
    }
}
