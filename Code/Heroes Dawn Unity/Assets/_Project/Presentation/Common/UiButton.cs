using N29.HeroesDawn.Presentation.Audio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Bouton « comics » : léger écrasement à l'appui, son de clic (relais audio de la hiérarchie).</summary>
    public sealed class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler, IPointerClickHandler
    {
        static readonly Vector3 HoverScale = new Vector3(1.04f, 1.04f, 1f);
        bool _hover;
        // Sérialisés : le bouton fonctionne aussi dans un prefab.
        [SerializeField] Button button;
        [SerializeField] Image fill;
        [SerializeField] TMP_Text label;
        [SerializeField] Color _bg;
        [SerializeField] Color _fg;
        [Tooltip("Son joué au clic (None : muet).")]
        [SerializeField] SfxId clickSound = SfxId.UiClick;
        UiSoundRelay _relay;

        public Button Button => button;
        public Image Fill => fill;
        public TMP_Text Label => label;
        public RectTransform Rect => (RectTransform)transform;

        public void Init(Button b, Image fillImage, TMP_Text text, Color bg, Color fg)
        {
            button = b; fill = fillImage; label = text; _bg = bg; _fg = fg;
            ApplyState();
        }

        public bool Interactable
        {
            get => Button.interactable;
            set
            {
                Button.interactable = value;
                ApplyState();
            }
        }

        /// <summary>Désactivé : fond éclairci vers le gris pastel et libellé marine estompé (lisible sur n'importe quel fond de bouton).</summary>
        void ApplyState()
        {
            if (fill == null) return;
            bool on = button == null || button.interactable;
            fill.color = on ? _bg : Color.Lerp(_bg, Theme.Surface3, 0.8f);
            if (label != null) label.color = on ? _fg : Theme.WithAlpha(Theme.Navy, 0.5f);
        }

        void Awake()
        {
            if (button != null) button.onClick.AddListener(PlayClick);
        }

        void PlayClick()
        {
            if (clickSound == SfxId.None) return;
            if (_relay == null) _relay = GetComponentInParent<UiSoundRelay>(true);
            if (_relay != null) _relay.Play(clickSound);
        }

        public void SetLabel(string s) => Label.text = s;

        public void SetColors(Color bg, Color fg)
        {
            _bg = bg; _fg = fg;
            ApplyState();
        }

        /// <summary>Raison affichée en toast quand on touche le bouton grisé (vide : simple secousse).</summary>
        public string DisabledReason { get; set; } = "";

        public void OnPointerDown(PointerEventData e) { if (Button.interactable) transform.localScale = new Vector3(0.95f, 0.95f, 1f); }
        public void OnPointerUp(PointerEventData e) => transform.localScale = _hover ? HoverScale : Vector3.one;

        /// <summary>Survol (souris) : léger grossissement et fond éclairci.</summary>
        public void OnPointerEnter(PointerEventData e)
        {
            if (!Button.interactable) return;
            _hover = true;
            transform.localScale = HoverScale;
            Fill.color = Color.Lerp(_bg, Color.white, 0.15f);
        }

        public void OnPointerExit(PointerEventData e) => EndHover();

        /// <summary>Toucher un bouton grisé : secousse, et sa raison en toast par le relais de la coque.</summary>
        public void OnPointerClick(PointerEventData e)
        {
            if (Button.interactable) return;
            Feedback.Fx.Shake(Rect);
            if (!string.IsNullOrEmpty(DisabledReason)) GetComponentInParent<Wip.UiTapRelay>(true)?.RaiseReason(DisabledReason);
        }

        void EndHover()
        {
            _hover = false;
            transform.localScale = Vector3.one;
            ApplyState();
        }

        void OnDisable() => EndHover();
    }
}
