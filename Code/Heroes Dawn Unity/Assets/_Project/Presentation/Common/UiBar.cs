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
        float _target = 1f;
        float _shown = 1f;
        public bool Smooth = true;
        public float Speed = 2.5f;

        public TMP_Text Label => _label;

        public void Init(RectTransform fill, Image fillImage, TMP_Text label)
        {
            _fill = fill; _fillImage = fillImage; _label = label;
            Apply();
        }

        public void Set(float ratio, bool instant = false)
        {
            _target = Mathf.Clamp01(ratio);
            if (instant || !Smooth || !isActiveAndEnabled) { _shown = _target; Apply(); }
        }

        public void SetColor(Color c) => _fillImage.color = c;
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
        }
    }
}
