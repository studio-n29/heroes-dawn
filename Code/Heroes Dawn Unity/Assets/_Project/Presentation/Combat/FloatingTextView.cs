#nullable enable
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Nombre flottant (widget prefab) : dégâts, soins, statut… monte, s'efface puis se détruit.</summary>
    public sealed class FloatingTextView : MonoBehaviour
    {
        [SerializeField] TMP_Text label = null!;
        [SerializeField] CanvasGroup group = null!;

        public void Play(string text, Color color, int size)
        {
            label.text = text;
            label.color = color;
            label.fontSize = size;
            var rt = (RectTransform)transform;
            rt.anchoredPosition = new Vector2(Random.Range(-60f, 60f), 10);
            Vector2 start = rt.anchoredPosition;
            Tween.PopIn(rt, 0.18f);
            Tween.Run(0.9f, k =>
            {
                if (rt == null) return;
                rt.anchoredPosition = start + new Vector2(0, 90f * k);
                group.alpha = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            }, () => { if (rt != null) Destroy(rt.gameObject); }, rt, Tween.Linear);
        }
    }
}
