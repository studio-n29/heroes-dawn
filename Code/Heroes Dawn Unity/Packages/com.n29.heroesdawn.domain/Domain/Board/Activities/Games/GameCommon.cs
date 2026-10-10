#nullable enable
using System;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>Vecteur plan (mètres) des jeux d'activité ; le niveau généré est en centimètres, les jeux en mètres.</summary>
    public readonly struct Vec2f
    {
        public float X { get; }
        public float Y { get; }
        public Vec2f(float x, float y) { X = x; Y = y; }

        public static readonly Vec2f Zero = new Vec2f(0f, 0f);
        public float Length => (float)Math.Sqrt(X * X + Y * Y);
        public Vec2f Normalized { get { float l = Length; return l < 1e-5f ? Zero : new Vec2f(X / l, Y / l); } }
        public static Vec2f operator +(Vec2f a, Vec2f b) => new Vec2f(a.X + b.X, a.Y + b.Y);
        public static Vec2f operator -(Vec2f a, Vec2f b) => new Vec2f(a.X - b.X, a.Y - b.Y);
        public static Vec2f operator *(Vec2f a, float k) => new Vec2f(a.X * k, a.Y * k);
        public static float Distance(Vec2f a, Vec2f b) => (a - b).Length;
        public float Dot(Vec2f o) => X * o.X + Y * o.Y;
        /// <summary>Même vecteur borné à une longueur maximale.</summary>
        public Vec2f Clamped(float max) { float l = Length; return l > max && l > 0f ? this * (max / l) : this; }
        public override string ToString() => "(" + X.ToString("0.00") + ", " + Y.ToString("0.00") + ")";
    }

    /// <summary>État d'une partie d'activité jouable.</summary>
    public enum GameStatus { Running, Won, Lost }
}
