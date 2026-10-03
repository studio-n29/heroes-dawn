#nullable enable
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Base des popups de case : titre, fenêtre animée à l'ouverture ; non annulables (un choix est requis).</summary>
    public abstract class PopupViewBase : MonoBehaviour, IPopupView
    {
        [SerializeField] protected TMP_Text title = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;

        public virtual bool Cancelable => false;
        public virtual void RequestCancel() { }

        protected void Open(string titleText)
        {
            title.text = titleText;
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
        }
    }
}
