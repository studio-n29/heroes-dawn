#nullable enable
using N29.HeroesDawn.Domain.Maps;
using UnityEngine;

namespace N29.HeroesDawn.Authoring
{
    /// <summary>Dessine un fond de carte vectoriel dans une texture (rectangles, disques, traits épais, mélange alpha) ; origine en bas à gauche.</summary>
    public static class MapRasterizer
    {
        public static Texture2D Render(MapBackground bg)
        {
            int w = bg.Width, h = bg.Height;
            var px = new Color32[w * h];
            foreach (var s in bg.Shapes)
            {
                var c = ToColor(s.Color);
                switch (s.Kind)
                {
                    case MapShapeKind.Rect: FillRect(px, w, h, s.X0, s.Y0, s.X1, s.Y1, c); break;
                    case MapShapeKind.Circle: FillCircle(px, w, h, s.X0, s.Y0, s.Size, c); break;
                    default: DrawLine(px, w, h, s.X0, s.Y0, s.X1, s.Y1, s.Size, c); break;
                }
            }
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { name = "MapBackground", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            tex.SetPixels32(px);
            tex.Apply(false);
            return tex;
        }

        static Color32 ToColor(int argb) => new Color32((byte)((argb >> 16) & 0xFF), (byte)((argb >> 8) & 0xFF), (byte)(argb & 0xFF), (byte)((argb >> 24) & 0xFF));

        static void Blend(Color32[] px, int i, Color32 c)
        {
            if (c.a == 255) { px[i] = c; return; }
            var d = px[i];
            int a = c.a;
            px[i] = new Color32((byte)((c.r * a + d.r * (255 - a)) / 255), (byte)((c.g * a + d.g * (255 - a)) / 255),
                                (byte)((c.b * a + d.b * (255 - a)) / 255), 255);
        }

        static void FillRect(Color32[] px, int w, int h, int x0, int y0, int x1, int y1, Color32 c)
        {
            x0 = Mathf.Max(0, x0); y0 = Mathf.Max(0, y0); x1 = Mathf.Min(w, x1); y1 = Mathf.Min(h, y1);
            for (int y = y0; y < y1; y++)
            {
                int row = y * w;
                for (int x = x0; x < x1; x++) Blend(px, row + x, c);
            }
        }

        static void FillCircle(Color32[] px, int w, int h, int cx, int cy, int r, Color32 c)
        {
            int r2 = r * r;
            for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(h - 1, cy + r); y++)
            {
                int dy = y - cy;
                int span = (int)Mathf.Sqrt(Mathf.Max(0, r2 - dy * dy));
                for (int x = Mathf.Max(0, cx - span); x <= Mathf.Min(w - 1, cx + span); x++) Blend(px, y * w + x, c);
            }
        }

        /// <summary>Trait épais : opaque = disques le long du trait ; translucide = bande par distance au segment (évite le double mélange).</summary>
        static void DrawLine(Color32[] px, int w, int h, int x0, int y0, int x1, int y1, int thickness, Color32 c)
        {
            int half = Mathf.Max(1, thickness) / 2;
            if (c.a == 255)
            {
                int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
                if (steps == 0) { FillCircle(px, w, h, x0, y0, half, c); return; }
                for (int i = 0; i <= steps; i++)
                    FillCircle(px, w, h, x0 + (x1 - x0) * i / steps, y0 + (y1 - y0) * i / steps, half, c);
                return;
            }
            int minX = Mathf.Max(0, Mathf.Min(x0, x1) - half), maxX = Mathf.Min(w - 1, Mathf.Max(x0, x1) + half);
            int minY = Mathf.Max(0, Mathf.Min(y0, y1) - half), maxY = Mathf.Min(h - 1, Mathf.Max(y0, y1) + half);
            long dx = x1 - x0, dy = y1 - y0, len2 = dx * dx + dy * dy;
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                {
                    long t = len2 == 0 ? 0 : ((x - x0) * dx + (y - y0) * dy) * 1000 / len2;
                    t = t < 0 ? 0 : t > 1000 ? 1000 : t;
                    long qx = x0 + dx * t / 1000 - x, qy = y0 + dy * t / 1000 - y;
                    if (qx * qx + qy * qy <= (long)half * half) Blend(px, y * w + x, c);
                }
        }
    }
}
