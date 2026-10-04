#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Feedback
{
    /// <summary>Pastille flottante « +150 » (vert) / « −450 » (rouge) à côté d'une ressource : monte et s'efface, sans bloquer.</summary>
    [RequireComponent(typeof(TMP_Text))]
    public sealed class FloatDelta : MonoBehaviour
    {
        const float Rise = 46f, Duration = 0.9f;

        TMP_Text? _text;
        Vector2 _origin;
        bool _hasOrigin;
        Coroutine? _run;

        public static string Format(int delta) => delta > 0 ? "+" + delta.ToString("N0") : "−" + (-delta).ToString("N0");

        public void Play(int delta)
        {
            if (delta == 0 || !UnityEngine.Application.isPlaying) return;
            _text ??= GetComponent<TMP_Text>();
            var rt = (RectTransform)transform;
            if (!_hasOrigin) { _origin = rt.anchoredPosition; _hasOrigin = true; }
            Tween.Stop(_run);
            _text.text = Format(delta);
            var color = delta > 0 ? Theme.Success : Theme.Danger;
            gameObject.SetActive(true);
            _run = Tween.Run(Duration, k =>
            {
                if (_text == null) return;
                rt.anchoredPosition = _origin + new Vector2(0, Rise * k);
                _text.color = Theme.WithAlpha(color, k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f);
            }, () => { if (this != null) gameObject.SetActive(false); }, this, Tween.EaseOutCubic);
        }
    }
}
