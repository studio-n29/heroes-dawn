using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Sprites générés au démarrage (aucun asset requis) : rectangle arrondi 9-slice, disque, anneau, dégradé.</summary>
    public static class Sprites
    {
        static Sprite _rounded, _roundedSmall, _circle, _ring, _gradient;

        public static Sprite Rounded => _rounded != null ? _rounded : (_rounded = MakeRounded(64, 18));
        public static Sprite RoundedSmall => _roundedSmall != null ? _roundedSmall : (_roundedSmall = MakeRounded(32, 7));
        public static Sprite Circle => _circle != null ? _circle : (_circle = MakeCircle(128, 0f));
        public static Sprite Ring => _ring != null ? _ring : (_ring = MakeCircle(128, 0.8f));
        public static Sprite VerticalGradient => _gradient != null ? _gradient : (_gradient = MakeGradient());

        static readonly System.Collections.Generic.Dictionary<string, Sprite> _art = new System.Collections.Generic.Dictionary<string, Sprite>();

        /// <summary>Icône « H » (Resources/Art/HD_Logo_png.bytes).</summary>
        public static Sprite Logo => Art("HD_Logo_png");
        /// <summary>Logo-titre « Heroes Dawn ».</summary>
        public static Sprite Title => Art("HD_Title_png");
        public static Sprite BackgroundLandscape => Art("HD_BG_Landscape_jpg");
        public static Sprite BackgroundPortrait => Art("HD_BG_Portrait_jpg");

        /// <summary>
        /// Charge une image stockée en octets bruts (.bytes) et la décode à l'exécution : la résolution d'origine est conservée
        /// quel que soit le réglage d'import de Unity (pas de redimensionnement en puissance de 2). Renvoie null si absente.
        /// </summary>
        public static Sprite Art(string name)
        {
            if (_art.TryGetValue(name, out var cached)) return cached;
            Sprite sprite = null;
            var asset = Resources.Load<TextAsset>("Art/" + name);
            if (asset != null)
            {
                var tex = new Texture2D(2, 2, TextureFormat.RGBA32, true) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Trilinear, anisoLevel = 2 };
                if (tex.LoadImage(asset.bytes, true))
                    sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                Resources.UnloadAsset(asset);
            }
            if (sprite == null) Debug.LogWarning("[Heroes Dawn] Image introuvable : Resources/Art/" + name + ".bytes");
            _art[name] = sprite;
            return sprite;
        }

        static Texture2D NewTex(int w, int h)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            t.hideFlags = HideFlags.DontSave;
            return t;
        }

        static Sprite MakeRounded(int size, int radius)
        {
            var t = NewTex(size, size);
            var px = new Color32[size * size];
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float cx = Mathf.Clamp(x + 0.5f, radius, size - radius);
                float cy = Mathf.Clamp(y + 0.5f, radius, size - radius);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, cy));
                float a = Mathf.Clamp01(radius - d + 0.5f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            t.SetPixels32(px);
            t.Apply();
            int b = radius + 1;
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(b, b, b, b));
        }

        static Sprite MakeCircle(int size, float innerRatio)
        {
            var t = NewTex(size, size);
            var px = new Color32[size * size];
            float r = size * 0.5f;
            float inner = r * innerRatio;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(r, r));
                float a = Mathf.Clamp01(r - d);
                if (innerRatio > 0f) a *= Mathf.Clamp01(d - inner);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            t.SetPixels32(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        static Sprite MakeGradient()
        {
            var t = NewTex(2, 64);
            var px = new Color32[2 * 64];
            for (int y = 0; y < 64; y++)
            {
                byte a = (byte)(255 * (y / 63f));
                px[y * 2] = px[y * 2 + 1] = new Color32(255, 255, 255, a);
            }
            t.SetPixels32(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, 2, 64), new Vector2(0.5f, 0.5f), 100f);
        }
    }
}
