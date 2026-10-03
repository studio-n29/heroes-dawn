#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Un message éphémère (widget prefab) : apparaît, reste 2,4 s, s'efface puis se détruit.</summary>
    public sealed class ToastItemView : MonoBehaviour
    {
        [SerializeField] TMP_Text label = null!;
        [SerializeField] Image accent = null!;
        [SerializeField] CanvasGroup group = null!;

        public void Play(string text, Color accentColor)
        {
            label.text = text;
            accent.color = accentColor;
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.18f);
            Tween.PopIn(transform, 0.25f);
            Tween.Delay(2.4f, () => Tween.Fade(group, 0f, 0.35f, () => { if (this != null) Destroy(gameObject); }), this);
        }
    }
}
