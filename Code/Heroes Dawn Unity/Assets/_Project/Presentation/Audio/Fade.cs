#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Gain qui glisse vers une cible (C# pur, sans allocation). Courbes à puissance constante : une montée en sinus
    /// et une descente en cosinus gardent un volume perçu stable pendant un fondu enchaîné, sans creux au milieu.
    /// </summary>
    public struct Fade
    {
        float _from;
        float _to;
        float _duration;
        float _elapsed;

        public float Value { get; private set; }
        public float Target => _to;
        public bool Active { get; private set; }

        public Fade(float value)
        {
            _from = _to = Value = value;
            _duration = _elapsed = 0f;
            Active = false;
        }

        /// <summary>Part de la valeur actuelle vers <paramref name="target"/> ; durée nulle : saut immédiat.</summary>
        public void To(float target, float seconds)
        {
            _from = Value;
            _to = target;
            _elapsed = 0f;
            _duration = seconds;
            Active = seconds > 0f && Math.Abs(target - Value) > 0.0001f;
            if (!Active) Value = target;
        }

        /// <summary>Avance le fondu ; renvoie vrai si la valeur a changé.</summary>
        public bool Step(float deltaSeconds)
        {
            if (!Active) return false;
            _elapsed += deltaSeconds;
            float t = _elapsed >= _duration ? 1f : _elapsed / _duration;
            const float halfPi = (float)(Math.PI * 0.5);
            Value = _to >= _from
                ? _from + (_to - _from) * (float)Math.Sin(t * halfPi)
                : _to + (_from - _to) * (float)Math.Cos(t * halfPi);
            if (t >= 1f)
            {
                Value = _to;
                Active = false;
            }
            return true;
        }
    }
}
