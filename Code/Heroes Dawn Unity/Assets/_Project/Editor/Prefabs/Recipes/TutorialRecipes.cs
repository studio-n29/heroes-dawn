#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Tutorial;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.EditorTools.Prefabs.Recipes
{
    /// <summary>
    /// Couche du tutoriel : voile à découpe (4 rectangles), anneau + onde + flèche sur la cible, bulle de Rayner (médaillon, titre jaune,
    /// texte, progression, Compris / Passer) et carte « Astuce ».
    /// </summary>
    public sealed class TutorialOverlayRecipe : IPrefabRecipe
    {
        public string RelativePath => "Common/TutorialOverlay.prefab";
        public int Order => 10;

        public GameObject Build(PrefabContext ctx)
        {
            var root = PrefabContext.NewRoot("TutorialOverlay");

            // ---- Voile et désignation
            var veilRoot = Ui.Rect(root, "Veil").Stretch();
            var veilColor = Theme.WithAlpha(Theme.Navy, 0.5f);
            Image Veil(string n) { var i = Ui.Img(veilRoot, n, veilColor, null, true); return i; }
            var top = Veil("Top");
            var bottom = Veil("Bottom");
            var left = Veil("Left");
            var right = Veil("Right");
            var ring = Ui.Img(veilRoot, "Ring", Theme.Primary, Sprites.PanelFrame);
            ring.type = Image.Type.Sliced;
            ring.rectTransform.anchorMin = ring.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            var ripple = Ui.Img(veilRoot, "Ripple", Theme.Primary, Sprites.PanelFrame);
            ripple.rectTransform.anchorMin = ripple.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            var brackets = new Object[4];
            for (int i = 0; i < 4; i++)
            {
                var br = Ui.Img(veilRoot, "Bracket" + (i + 1), Theme.FrostEdge, Sprites.Bracket);
                br.rectTransform.anchorMin = br.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                br.rectTransform.pivot = new Vector2(0f, 0f);
                br.rectTransform.sizeDelta = new Vector2(34f, 34f);
                brackets[i] = br.rectTransform;
            }

            var pointerGo = new GameObject("Pointer", typeof(RectTransform), typeof(CanvasRenderer), typeof(UiPolygon)) { layer = 5 };
            var pointer = (RectTransform)pointerGo.transform;
            pointer.SetParent(veilRoot, false);
            pointer.anchorMin = pointer.anchorMax = new Vector2(0.5f, 0.5f);
            pointer.sizeDelta = new Vector2(56, 64);
            var poly = pointerGo.GetComponent<UiPolygon>();
            poly.raycastTarget = false;
            poly.color = Theme.Primary;
            poly.OutlineColor = Color.black;
            poly.OutlineWidth = 5f;
            poly.SetPoints(new[] { new Vector2(0.5f, 0.02f), new Vector2(0.98f, 0.98f), new Vector2(0.02f, 0.98f) });

            // ---- Bulle de Rayner
            var bubblePanel = Ui.Panel(root, "Bubble", Theme.Surface, 5f, 10f, true);
            var bubble = bubblePanel.Frame();
            bubble.anchorMin = bubble.anchorMax = new Vector2(0.5f, 0f);
            bubble.pivot = new Vector2(0.5f, 0f);
            bubble.sizeDelta = new Vector2(1060, 250);
            var medal = Ui.Img(bubblePanel.transform, "Medallion", Theme.Perception, Sprites.Circle);
            medal.rectTransform.At(0f, 0.5f, 22, 6, 128, 128);
            var medalLetter = Ui.Txt(medal.transform, "R", 84, Color.white, TextAnchor.MiddleCenter, FontKind.Title, "Letter");
            medalLetter.rectTransform.Stretch();
            medalLetter.Inked();
            var band = Ui.Panel(bubblePanel.transform, "TitleBand", Theme.Primary, 4f, 4f, false);
            band.Frame().At(0f, 1f, 170, -14, 640, 56);
            var title = Ui.Txt(band.transform, "", 34, Theme.TextDark, TextAnchor.MiddleLeft, FontKind.Title, "Title");
            title.rectTransform.Stretch(14, 0, 14, 0);
            title.AutoSize(34);
            var body = Ui.Txt(bubblePanel.transform, "", 28, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Body");
            body.rectTransform.At(0f, 1f, 170, -80, 850, 100);
            var progress = Ui.Txt(bubblePanel.transform, "", 22, Theme.TextMuted, TextAnchor.MiddleRight, FontKind.Body, "Progress");
            progress.rectTransform.At(1f, 1f, -20, -18, 200, 32);
            var skip = Ui.Btn(bubblePanel.transform, "", Theme.Surface3, Theme.TextMuted, null!, 22, FontKind.Body, "Skip");
            skip.Rect.At(0f, 0f, 22, 14, 330, 52);
            var ok = Ui.Btn(bubblePanel.transform, "", Theme.Primary, Theme.TextDark, null!, 36, FontKind.Title, "Ok");
            ok.Rect.At(1f, 0f, -20, 14, 300, 74);
            RecipeKit.Shine(ok);

            // ---- Carte « Astuce » (non bloquante, en bas)
            var hintPanel = Ui.Panel(root, "Hint", Theme.Surface, 5f, 10f, true);
            var hint = hintPanel.Frame();
            hint.anchorMin = hint.anchorMax = new Vector2(0.5f, 0f);
            hint.pivot = new Vector2(0.5f, 0f);
            hint.sizeDelta = new Vector2(1000, 190);
            hint.anchoredPosition = new Vector2(0, 150);
            var hBand = Ui.Panel(hintPanel.transform, "Band", Theme.Info, 4f, 4f, false);
            hBand.Frame().At(0f, 1f, 24, -14, 240, 50);
            var hTitle = Ui.Txt(hBand.transform, "", 30, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Title, "Title");
            hTitle.rectTransform.Stretch(8, 0, 8, 0);
            var hBody = Ui.Txt(hintPanel.transform, "", 26, Theme.Text, TextAnchor.UpperLeft, FontKind.Body, "Body");
            hBody.rectTransform.At(0f, 1f, 24, -72, 700, 100);
            var hOk = Ui.Btn(hintPanel.transform, "", Theme.Primary, Theme.TextDark, null!, 34, FontKind.Title, "Ok");
            hOk.Rect.At(1f, 0f, -22, 22, 210, 70);

            var view = root.gameObject.AddComponent<TutorialOverlayView>();
            ctx.Wire(view, "veilRoot", veilRoot);
            ctx.Wire(view, "veilTop", top);
            ctx.Wire(view, "veilBottom", bottom);
            ctx.Wire(view, "veilLeft", left);
            ctx.Wire(view, "veilRight", right);
            ctx.Wire(view, "ring", ring);
            ctx.Wire(view, "ripple", ripple);
            ctx.WireArray(view, "brackets", brackets);
            ctx.Wire(view, "pointer", pointer);
            ctx.Wire(view, "bubble", bubble);
            ctx.Wire(view, "title", title);
            ctx.Wire(view, "body", body);
            ctx.Wire(view, "progress", progress);
            ctx.Wire(view, "ok", ok);
            ctx.Wire(view, "skip", skip);
            ctx.Wire(view, "hintCard", hint);
            ctx.Wire(view, "hintTitle", hTitle);
            ctx.Wire(view, "hintBody", hBody);
            ctx.Wire(view, "hintOk", hOk);
            return root.gameObject;
        }
    }
}
