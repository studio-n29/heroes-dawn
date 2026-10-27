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
            return root.gameObject;
        }
    }
}
