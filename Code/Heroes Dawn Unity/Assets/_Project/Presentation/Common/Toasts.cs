using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Messages éphémères en haut de l'écran (style « récitatif » jaune des comics).</summary>
    public sealed class Toasts : MonoBehaviour
    {
        RectTransform _stack;
        const int Max = 4;

        public static Toasts Create(RectTransform layer)
        {
            var rt = Ui.Rect(layer, "Toasts").At(0.5f, 1f, 0, -110, 900, 400);
            var t = rt.gameObject.AddComponent<Toasts>();
            t._stack = rt;
            var v = rt.VLayout(10, 0, TextAnchor.UpperCenter);
            v.childForceExpandWidth = false;
            v.childControlWidth = true;
            return t;
        }

        public void Show(string text, Color accent)
        {
            if (string.IsNullOrEmpty(text)) return;
            while (_stack.childCount >= Max)
            {
                var old = _stack.GetChild(0).gameObject;
                old.transform.SetParent(null);
                Destroy(old);
            }
            var fill = Ui.Panel(_stack, "Toast", Theme.Primary, 4f, 6f, false);
            var frame = fill.Frame();
            frame.Pref(-1, 64);
            var h = frame.HLayout(0, 0);
            h.padding = new RectOffset(34, 34, 12, 12);
            fill.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
            var strip = Ui.Img(fill.transform, "Accent", accent, Sprites.RoundedSmall);
            strip.rectTransform.anchorMin = new Vector2(0, 0);
            strip.rectTransform.anchorMax = new Vector2(0, 1);
            strip.rectTransform.pivot = new Vector2(0, 0.5f);
            strip.rectTransform.sizeDelta = new Vector2(14, -8);
            strip.rectTransform.anchoredPosition = new Vector2(6, 0);
            var label = Ui.Txt(frame, text, 30, Theme.TextDark, TextAnchor.MiddleCenter, FontKind.Body, "Label");
            label.textWrappingMode = TextWrappingModes.NoWrap;
            var cg = frame.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            Tween.Fade(cg, 1f, 0.18f);
            Tween.PopIn(frame, 0.25f);
            Tween.Delay(2.4f, () => Tween.Fade(cg, 0f, 0.35f, () => { if (frame != null) Destroy(frame.gameObject); }), frame);
        }
    }
}
