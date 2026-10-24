#nullable enable
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>Fenêtre des mini-jeux 2D de héros : bandeau (score, temps, vies), grande zone de jeu tactile, flèches de couloir, Abandonner.</summary>
    public sealed class HeroGamePopupRecipe : IPrefabRecipe
    {
        public string RelativePath => "Popups/HeroGamePopup.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("HeroGamePopup");
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var (window, content, title) = PopupKit.Window(root, 1280, 900, Theme.Danger);

            var score = Ui.Txt(content, "", 40, Theme.Primary, TextAnchor.MiddleLeft, FontKind.Title, "Score");
            score.rectTransform.At(0f, 1f, 0, 0, 520, 56);
            score.Inked();
            var time = Ui.Txt(content, "", 44, Theme.Text, TextAnchor.MiddleCenter, FontKind.Title, "Time");
            time.rectTransform.At(0.5f, 1f, 0, 0, 240, 56);
            time.Inked();
            var lives = Ui.Txt(content, "", 44, Theme.Danger, TextAnchor.MiddleRight, FontKind.Title, "Lives");
            lives.rectTransform.At(1f, 1f, 0, 0, 420, 56);

            // ---- Zone de jeu : un ciel nocturne de ville, la ligne du sol en bas
            var field = Ui.Img(content, "Field", new Color(0.07f, 0.06f, 0.16f, 1f), Sprites.Panel, true);
            field.rectTransform.anchorMin = new Vector2(0f, 0f);
            field.rectTransform.anchorMax = new Vector2(1f, 1f);
            field.rectTransform.offsetMin = new Vector2(0, 120);
            field.rectTransform.offsetMax = new Vector2(0, -66);
            field.gameObject.AddComponent<RectMask2D>();
            var sky = Ui.Img(field.transform, "Sky", Theme.WithAlpha(Theme.Surface, 0.9f), Sprites.VerticalGradient);
            sky.raycastTarget = false;
            sky.rectTransform.Stretch();
            sky.rectTransform.localScale = new Vector3(1, -1, 1);
            var ground = Ui.Img(field.transform, "Ground", Theme.WithAlpha(Theme.Danger, 0.55f), null);
            ground.raycastTarget = false;
            ground.rectTransform.anchorMin = new Vector2(0f, 0f);
            ground.rectTransform.anchorMax = new Vector2(1f, 0f);
            ground.rectTransform.pivot = new Vector2(0.5f, 0f);
            ground.rectTransform.sizeDelta = new Vector2(0, 14);
            ground.rectTransform.anchoredPosition = Vector2.zero;
            var overlay = Ui.Txt(field.transform, "", 40, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Overlay");
            overlay.rectTransform.Stretch(60, 40, 60, 40);
            overlay.Inked();
            overlay.transform.SetAsLastSibling();

            // ---- Commandes
            var left = Ui.Btn(content, "◄", Theme.Primary, Theme.TextDark, null!, 64, FontKind.Title, "Left");
            left.Rect.At(0f, 0f, 0, 0, 260, 108);
            var right = Ui.Btn(content, "►", Theme.Primary, Theme.TextDark, null!, 64, FontKind.Title, "Right");
            right.Rect.At(1f, 0f, 0, 0, 260, 108);
            var abandon = Ui.Btn(content, "", Theme.Surface3, Theme.Text, null!, 30, FontKind.Title, "Abandon");
            abandon.Rect.At(0.5f, 0f, 0, 6, 340, 96);

            var view = root.gameObject.AddComponent<HeroGamePopupView>();
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "scoreText", score);
            ctx.Wire(view, "timeText", time);
            ctx.Wire(view, "livesText", lives);
            ctx.Wire(view, "overlay", overlay);
            ctx.Wire(view, "field", field.rectTransform);
            ctx.Wire(view, "leftButton", left.Button);
            ctx.Wire(view, "rightButton", right.Button);
            ctx.Wire(view, "abandon", abandon);
            ctx.Wire(view, "window", window);
            ctx.Wire(view, "group", group);
            return root.gameObject;
        }
    }
}
