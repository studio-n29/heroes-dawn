using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Bouton « comics » : léger écrasement à l'appui.</summary>
    public sealed class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        // Sérialisés : le bouton fonctionne aussi dans un prefab.
        [SerializeField] Button button;
        [SerializeField] Image fill;
        [SerializeField] TMP_Text label;
        [SerializeField] Color _bg;
        [SerializeField] Color _fg;

        public Button Button => button;
        public Image Fill => fill;
        public TMP_Text Label => label;
        public RectTransform Rect => (RectTransform)transform;

        public void Init(Button b, Image fillImage, TMP_Text text, Color bg, Color fg)
        {
            button = b; fill = fillImage; label = text; _bg = bg; _fg = fg;
            fillImage.color = bg;
        }

        public bool Interactable
        {
            get => Button.interactable;
            set
            {
                Button.interactable = value;
                Label.color = value ? _fg : Theme.WithAlpha(_fg, 0.55f);
            }
        }

        public void SetLabel(string s) => Label.text = s;

        public void SetColors(Color bg, Color fg)
        {
            _bg = bg; _fg = fg;
            Fill.color = bg;
            Label.color = Button.interactable ? fg : Theme.WithAlpha(fg, 0.55f);
        }

        public void OnPointerDown(PointerEventData e) { if (Button.interactable) transform.localScale = new Vector3(0.95f, 0.95f, 1f); }
        public void OnPointerUp(PointerEventData e) => transform.localScale = Vector3.one;
        public void OnPointerExit(PointerEventData e) => transform.localScale = Vector3.one;
        void OnDisable() => transform.localScale = Vector3.one;
    }
}
