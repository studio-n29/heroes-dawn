using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Pulsation continue (destinations du plateau, bouton Combo, unité prête).</summary>
    public sealed class Pulse : MonoBehaviour
    {
        public float Amplitude = 0.08f;
        public float Speed = 5f;
        float _phase;

        void OnEnable() { _phase = 0f; }
        void Update()
        {
            _phase += Time.unscaledDeltaTime * Speed;
            transform.localScale = Vector3.one * (1f + Amplitude * (0.5f + 0.5f * Mathf.Sin(_phase)));
        }
        void OnDisable() { transform.localScale = Vector3.one; }
    }
}
