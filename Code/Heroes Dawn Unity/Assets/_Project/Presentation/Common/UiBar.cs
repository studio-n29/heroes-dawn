using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Barre de progression (PV, jauge ATB, fatigue) avec interpolation douce.</summary>
    public sealed class UiBar : MonoBehaviour
    {
        [SerializeField] RectTransform _fill;
        [SerializeField] Image _fillImage;
        [SerializeField, Mvp.OptionalRef] TMP_Text _label;
        [SerializeField, Mvp.OptionalRef] Image _glow;
        float _target = 1f;
        float _shown = 1f;
        public bool Smooth = true;
        public float Speed = 2.5f;

        public TMP_Text Label => _label;

        public void Init(RectTransform fill, Image fillImage, TMP_Text label, Image glow = null)
        {
            _fill = fill; _fillImage = fillImage; _label = label; _glow = glow;
            Apply();
        }

        public void Set(float ratio, bool instant = false)
        {
            _target = Mathf.Clamp01(ratio);
            if (instant || !Smooth || !isActiveAndEnabled) { _shown = _target; Apply(); }
        }

        public void SetColor(Color c) { _fillImage.color = c; if (_glow != null) _glow.color = Theme.WithAlpha(c, 0.35f); }
        public void SetLabel(string s) { if (_label != null) _label.text = s; }

        void Update()
        {
            if (Mathf.Abs(_shown - _target) < 0.0005f) return;
            _shown = Mathf.MoveTowards(_shown, _target, Time.unscaledDeltaTime * Speed);
            Apply();
        }

        void Apply()
        {
            if (_fill == null) return;
            _fill.anchorMax = new Vector2(_shown, 1f);
            _fillImage.enabled = _shown > 0.001f;
            if (_glow != null) _glow.enabled = _fillImage.enabled;
        }
    }
}
