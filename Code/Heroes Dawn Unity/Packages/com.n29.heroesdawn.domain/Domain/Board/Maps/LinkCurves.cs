#nullable enable
using System;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Maps
{
    /// <summary>Point de la carte en unités de grille.</summary>
    public readonly struct PointF
    {
        public float X { get; }
        public float Y { get; }
        public PointF(float x, float y) { X = x; Y = y; }
    }

    /// <summary>Courbe de Bézier cubique d'un chemin, de la case A vers la case B.</summary>
    public readonly struct BezierCurve
    {
        public PointF P0 { get; }
        public PointF C1 { get; }
        public PointF C2 { get; }
        public PointF P3 { get; }

        public BezierCurve(PointF p0, PointF c1, PointF c2, PointF p3) { P0 = p0; C1 = c1; C2 = c2; P3 = p3; }

        public PointF At(float t)
        {
            float u = 1f - t, a = u * u * u, b = 3f * u * u * t, c = 3f * u * t * t, d = t * t * t;
            return new PointF(a * P0.X + b * C1.X + c * C2.X + d * P3.X, a * P0.Y + b * C1.Y + c * C2.Y + d * P3.Y);
        }

        /// <summary>Point au milieu de la courbe (là où s'affiche le coût).</summary>
        public PointF Middle => At(0.5f);

        /// <summary>Points régulièrement espacés en t, extrémités comprises (au moins 2).</summary>
        public PointF[] Sample(int segments)
        {
            if (segments < 1) segments = 1;
            var pts = new PointF[segments + 1];
            for (int i = 0; i <= segments; i++) pts[i] = At((float)i / segments);
            return pts;
        }

        /// <summary>Longueur approchée par segments.</summary>
        public float Length(int segments = 24)
        {
            var pts = Sample(segments);
            float sum = 0f;
            for (int i = 1; i < pts.Length; i++)
            {
                float dx = pts[i].X - pts[i - 1].X, dy = pts[i].Y - pts[i - 1].Y;
                sum += (float)Math.Sqrt(dx * dx + dy * dy);
            }
            return sum;
        }
    }

    /// <summary>Courbes des chemins : celle de la définition si elle existe, sinon une courbe organique déterministe (selon les identifiants).</summary>
    public static class LinkCurves
    {
        /// <summary>Écart latéral maximal des courbes générées, en part de la longueur du chemin.</summary>
        public const float AutoBend = 0.18f;

        public static BezierCurve Resolve(NodeDef a, NodeDef b, LinkDef link)
        {
            var p0 = new PointF(a.X, a.Y);
            var p3 = new PointF(b.X, b.Y);
            if (link.Curve.HasValue)
            {
                var c = link.Curve.Value;
                return new BezierCurve(p0, new PointF(c.C1X, c.C1Y), new PointF(c.C2X, c.C2Y), p3);
            }
            return Auto(p0, p3, link.A.Value, link.B.Value);
        }

        /// <summary>Deux points de contrôle décalés sur la perpendiculaire, de signe et d'ampleur propres au chemin.</summary>
        public static BezierCurve Auto(PointF p0, PointF p3, string idA, string idB)
        {
            float dx = p3.X - p0.X, dy = p3.Y - p0.Y;
            float len = (float)Math.Sqrt(dx * dx + dy * dy);
            if (len < 1e-4f) return new BezierCurve(p0, p0, p3, p3);
            ulong h = Fnv(idA + ">" + idB);
            float s1 = ((h & 0xFF) / 255f - 0.5f) * 2f;          // -1..1
            float s2 = (((h >> 8) & 0xFF) / 255f - 0.5f) * 2f;
            if (Math.Abs(s1) < 0.35f) s1 = s1 < 0f ? -0.35f : 0.35f; // toujours une vraie courbe
            float nx = -dy / len, ny = dx / len;
            float o1 = s1 * AutoBend * len, o2 = s2 * AutoBend * len;
            var c1 = new PointF(p0.X + dx / 3f + nx * o1, p0.Y + dy / 3f + ny * o1);
            var c2 = new PointF(p0.X + 2f * dx / 3f + nx * o2, p0.Y + 2f * dy / 3f + ny * o2);
            return new BezierCurve(p0, c1, c2, p3);
        }

        static ulong Fnv(string s)
        {
            ulong h = 1469598103934665603UL;
            foreach (char ch in s) { h ^= ch; h *= 1099511628211UL; }
            return h;
        }
    }
}
