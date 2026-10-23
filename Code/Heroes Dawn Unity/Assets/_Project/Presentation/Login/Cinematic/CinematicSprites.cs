#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Sprites doux fabriqués au lancement pour la cinématique : halo, faisceau, vignettage, grain.</summary>
    public static class CinematicSprites
    {
        static Sprite? _glow, _beam, _vignette;
        static Texture2D? _grain;

        /// <summary>Halo rond à bord très doux (alpha 1 au centre, 0 au bord).</summary>
        public static Sprite Glow => _glow != null ? _glow : (_glow = MakeRadial(128, 2.2f, false));

        /// <summary>Vignettage : transparent au centre, sombre vers les bords.</summary>
        public static Sprite Vignette => _vignette != null ? _vignette : (_vignette = MakeRadial(256, 2.6f, true));

        /// <summary>Faisceau vertical : lumineux au centre de sa largeur, qui s'éteint vers le haut (à poser par le bas).</summary>
        public static Sprite Beam => _beam != null ? _beam : (_beam = MakeBeam());

        /// <summary>Bruit de grain répétable (à tuiler avec un RawImage).</summary>
        public static Texture2D Grain => _grain != null ? _grain : (_grain = MakeGrain());

        static Sprite MakeRadial(int size, float power, bool inverse)
        {
            var t = NewTex(size, size);
            var px = new Color32[size * size];
            float c = (size - 1) * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c)) / c;
                float a = Mathf.Clamp01(1f - d);
                a = Mathf.Pow(a, power * 0.6f);
                if (inverse) a = Mathf.Clamp01(Mathf.Pow(Mathf.Clamp01(d), 2.4f) * 1.1f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            t.SetPixels32(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeBeam()
        {
            const int w = 64, h = 256;
            var t = NewTex(w, h);
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float u = Mathf.Abs((x + 0.5f) / w * 2f - 1f);
                float across = Mathf.Pow(1f - u, 1.6f);
                float along = Mathf.Pow(1f - y / (float)(h - 1), 0.9f); // opaque en bas, transparent en haut
                px[y * w + x] = new Color32(255, 255, 255, (byte)(across * along * 255));
            }
            t.SetPixels32(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 100f);
        }

        static Texture2D MakeGrain()
        {
            var t = NewTex(128, 128);
            t.wrapMode = TextureWrapMode.Repeat;
            var rnd = new System.Random(11);
            var px = new Color32[128 * 128];
            for (int i = 0; i < px.Length; i++)
            {
                byte v = (byte)rnd.Next(0, 256);
                px[i] = new Color32(v, v, v, (byte)rnd.Next(30, 130));
            }
            t.SetPixels32(px);
            t.Apply();
            return t;
        }

        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            t.hideFlags = HideFlags.DontSave;
            return t;
        }
    }
}
