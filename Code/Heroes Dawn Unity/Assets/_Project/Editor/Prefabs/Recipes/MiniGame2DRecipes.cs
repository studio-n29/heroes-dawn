#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Fenêtre commune des mini-jeux 2D du lot H : bandeau (score, temps, vies), grande zone tactile avec fond, joystick flottant, Abandonner.</summary>
    public sealed class MiniGame2DPopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/MiniGame2DPopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("MiniGame2DPopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1280, 900, Theme.Primary);

            var score = Ui.Txt(content, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Score");
            score.rectTransform.At(0f, 1f, 0, 0, 620, 56);
            score.Inked();
            var time = Ui.Txt(content, "", 44, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Time");
            time.rectTransform.At(0.5f, 1f, 0, 0, 240, 56);
            time.Inked();
            var lives = Ui.Txt(content, "", 44, Theme.Danger, TextAnchor.MiddleRight, FontKind.Title, "Lives");
            lives.rectTransform.At(1f, 1f, 0, 0, 420, 56);

            // ---- Zone de jeu : fond (image du jeu, teintée), éléments créés par le pilote, joystick, texte de début et de fin
            var field = Ui.Img(content, "Field", new Color(0.07f, 0.06f, 0.16f, 1f), Sprites.Panel, true);
            field.rectTransform.anchorMin = new Vector2(0f, 0f);
            field.rectTransform.anchorMax = new Vector2(1f, 1f);
            field.rectTransform.offsetMin = new Vector2(0, 120);
            field.rectTransform.offsetMax = new Vector2(0, -66);
            field.gameObject.AddComponent<RectMask2D>();
            var background = Ui.Img(field.transform, "Background", Color.white, null);
            background.raycastTarget = false;
            background.rectTransform.Stretch();
            background.preserveAspect = false;

            var ring = Ui.Img(field.transform, "StickRing", Theme.WithAlpha(Color.white, 0.35f), Sprites.Ring);
            ring.raycastTarget = false;
            ring.rectTransform.anchorMin = ring.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            ring.rectTransform.sizeDelta = new Vector2(250, 250);
            var knob = Ui.Img(field.transform, "StickKnob", Theme.WithAlpha(Theme.Primary, 0.8f), Sprites.Circle);
            knob.raycastTarget = false;
            knob.rectTransform.anchorMin = knob.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            knob.rectTransform.sizeDelta = new Vector2(110, 110);
            ring.gameObject.SetActive(false);
            knob.gameObject.SetActive(false);

            var rescue = BuildRescuePanel(ctx, field.transform);

            var overlay = Ui.Txt(field.transform, "", 40, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Overlay");
            overlay.rectTransform.Stretch(60, 40, 60, 40);
            overlay.Inked();
            overlay.transform.SetAsLastSibling();

            var abandon = Ui.Btn(content, "", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Abandon");
            abandon.Rect.At(0.5f, 0f, 0, 6, 340, 96);

            var view = root.gameObject.AddComponent<MiniGame2DPopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "scoreText", score);
            ctx.Wire(view, "timeText", time);
            ctx.Wire(view, "livesText", lives);
            ctx.Wire(view, "overlay", overlay);
            ctx.Wire(view, "field", field.rectTransform);
            ctx.Wire(view, "backgroundImage", background);
            ctx.Wire(view, "abandon", abandon);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            ctx.Wire(view, "stickRing", ring.rectTransform);
            ctx.Wire(view, "stickKnob", knob.rectTransform);
            ctx.Wire(view, "rescuePanel", rescue);
            return root.gameObject;
        }

        /// <summary>
        /// Panneau d'« Ingénierie de secours » (masqué hors de ce jeu) pour une zone de jeu de 1208 × 604 : à gauche le manuel, les emplacements de l'outil
        /// (2 rangées de 4) et la ligne d'état ; à droite 16 boutons de pièces (4 colonnes de 96 px de haut) puis Annuler et Valider.
        /// </summary>
        static RescuePanelView BuildRescuePanel(PrefabContext ctx, Transform field)
        {
            const float w = 1208f, h = 604f;
            const int poolCapacity = 16, slotCapacity = 8;
            var rt = Ui.Rect(field, "RescuePanel");
            rt.Stretch();
            var view = rt.gameObject.AddComponent<RescuePanelView>();

            float left = -w / 2f + 24f, leftW = w * 0.40f;
            var manualBack = Ui.Img(rt, "ManualBack", new Color(0f, 0f, 0f, 0.45f), Sprites.Panel);
            manualBack.raycastTarget = false;
            manualBack.rectTransform.At(0.5f, 0.5f, left + leftW / 2f, 79f, leftW, 399f);
            var manual = Ui.Txt(rt, "", 28, Color.white, TextAnchor.UpperLeft, FontKind.Body, "Manual");
            manual.rectTransform.At(0.5f, 0.5f, left + leftW / 2f, 79f, leftW - 36f, 375f);

            var boxes = new Image[slotCapacity];
            var texts = new TMPro.TMP_Text[slotCapacity];
            float slotW = (leftW - 3f * 12f) / 4f;
            for (int i = 0; i < slotCapacity; i++)
            {
                float x = left + slotW / 2f + (i % 4) * (slotW + 12f);
                float y = i < 4 ? -155f : -230f;
                boxes[i] = Ui.Img(rt, "Slot" + i, new Color(1f, 1f, 1f, 0.16f), Sprites.Rounded);
                boxes[i].raycastTarget = false;
                boxes[i].rectTransform.At(0.5f, 0.5f, x, y, slotW, 66f);
                texts[i] = Ui.Txt(boxes[i].transform, "", 24, Color.white, TextAnchor.MiddleCenter, FontKind.Body, "Text");
                texts[i].rectTransform.Stretch(4, 2, 4, 2);
            }
            var status = Ui.Txt(rt, "", 28, Theme.Accent, TextAnchor.MiddleLeft, FontKind.Body, "Status");
            status.rectTransform.At(0.5f, 0.5f, left + leftW / 2f, -285f, leftW, 40f);

            float rightX = -w / 2f + w * 0.43f, rightW = w * 0.55f;
            float bw = (rightW - 3f * 10f) / 4f;
            var pool = new UiButton[poolCapacity];
            for (int i = 0; i < poolCapacity; i++)
            {
                pool[i] = Ui.Btn(rt, "", Theme.Surface2, Theme.Text, null!, 26, FontKind.Body, "Part" + i);
                pool[i].Rect.At(0.5f, 0.5f, rightX + bw / 2f + (i % 4) * (bw + 10f), 230f - (i / 4) * 104f, bw, 96f);
            }
            var undo = Ui.Btn(rt, "", Theme.Surface3, Theme.Text, null!, 28, FontKind.Title, "Undo");
            undo.Rect.At(0.5f, 0.5f, rightX + rightW * 0.25f, -232f, rightW * 0.46f, 96f);
            var submit = Ui.Btn(rt, "", Theme.Primary, Theme.TextDark, null!, 28, FontKind.Title, "Submit");
            submit.Rect.At(0.5f, 0.5f, rightX + rightW * 0.75f, -232f, rightW * 0.46f, 96f);

            ctx.Wire(view, "manual", manual);
            ctx.Wire(view, "status", status);
            ctx.WireArray(view, "poolButtons", pool);
            ctx.WireArray(view, "slotBoxes", boxes);
            ctx.WireArray(view, "slotTexts", texts);
            ctx.Wire(view, "undo", undo);
            ctx.Wire(view, "submit", submit);
            rt.gameObject.SetActive(false);
            return view;
        }
    }
}
