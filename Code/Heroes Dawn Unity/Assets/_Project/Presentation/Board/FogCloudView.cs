#nullable enable
using N29.HeroesDawn.Presentation.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Nuage de brouillard de guerre : dérive lentement au-dessus d'une case cachée, se dissipe quand elle est révélée.</summary>
    public sealed class FogCloudView : MonoBehaviour
    {
        [SerializeField] Image image = null!;

        const float MaxAlpha = 0.78f;
        const float FadeSpeed = 1.4f;

        Vector2 _home;
        float _phase;
        float _alpha;
        float _target;
        Color _tint = Color.white;

        /// <summary>Vrai tant que le nuage est visible (ou en train de se dissiper).</summary>
        public bool Visible => _alpha > 0.01f || _target > 0f;
        public bool Fogged => _target > 0f;
        public float Alpha => _alpha;

        public void Init(Vector2 home, float size, float phase, Color tint, bool fogged)
        {
            _home = home;
            _phase = phase;
            _tint = tint;
            var rt = (RectTransform)transform;
            rt.anchoredPosition = home;
            rt.sizeDelta = new Vector2(size, size * 0.72f);
            _target = fogged ? 1f : 0f;
            _alpha = _target;
            Apply();
        }

        /// <summary>Cache ou dissipe le nuage (avec fondu).</summary>
        public void SetFogged(bool fogged) => _target = fogged ? 1f : 0f;

        void Update()
        {
            _alpha = Mathf.MoveTowards(_alpha, _target, FadeSpeed * Time.unscaledDeltaTime);
            if (!MotionSettings.Reduced && _alpha > 0.01f)
            {
                float t = Time.unscaledTime;
                ((RectTransform)transform).anchoredPosition = _home + new Vector2(Mathf.Sin(t * 0.30f + _phase) * 22f, Mathf.Cos(t * 0.23f + _phase * 1.3f) * 14f);
            }
            Apply();
        }

        void Apply()
        {
            var c = _tint;
            c.a = _alpha * MaxAlpha;
            image.color = c;
            if (image.enabled != (_alpha > 0.003f)) image.enabled = _alpha > 0.003f;
        }
    }
}
