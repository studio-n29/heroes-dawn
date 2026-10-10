using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Sprites générés au démarrage (aucun asset requis) : rectangle arrondi 9-slice, disque, anneau, dégradé.</summary>
    public static class Sprites
    {
        static Sprite _rounded, _roundedSmall, _circle, _ring, _gradient, _burst, _cloud;

        // Assets générés (Heroes Dawn ▸ Setup ▸ Générer les assets d'interface → Resources/UI) ; repli procédural sinon.
        public static Sprite Rounded => _rounded != null ? _rounded : (_rounded = Load("Rounded"));
        public static Sprite RoundedSmall => _roundedSmall != null ? _roundedSmall : (_roundedSmall = Load("RoundedSmall"));
        public static Sprite Circle => _circle != null ? _circle : (_circle = Load("Circle"));
        public static Sprite Ring => _ring != null ? _ring : (_ring = Load("Ring"));
        public static Sprite VerticalGradient => _gradient != null ? _gradient : (_gradient = Load("VerticalGradient"));
        /// <summary>Étoile de bande dessinée (bulle d'impact) : « BANG ! », « COMBAT ! ».</summary>
        public static Sprite Burst => _burst != null ? _burst : (_burst = Load("Burst"));
        /// <summary>Nuage doux aux bords irréguliers (brouillard de guerre).</summary>
        public static Sprite Cloud => _cloud != null ? _cloud : (_cloud = Load("Cloud"));

        /// <summary>Noms des sprites d'interface générés (Resources/UI/&lt;nom&gt;.png).</summary>
        public static readonly string[] UiSpriteNames = { "Rounded", "RoundedSmall", "Circle", "Ring", "VerticalGradient", "Burst", "Cloud" };

        static Sprite Load(string name) { var s = Resources.Load<Sprite>("UI/" + name); return s != null ? s : Procedural(name); }

        /// <summary>Version fabriquée par code d'un sprite d'interface (source du baker éditeur, repli si l'asset manque).</summary>
        public static Sprite Procedural(string name)
        {
            switch (name)
            {
                case "Rounded": return MakeRounded(64, 18);
                case "RoundedSmall": return MakeRounded(32, 7);
                case "Circle": return MakeCircle(128, 0f);
                case "Ring": return MakeCircle(128, 0.8f);
                case "VerticalGradient": return MakeGradient();
                case "Burst": return MakeBurst(256, 14);
                case "Cloud": return MakeCloud(128);
                default: throw new System.ArgumentException("Sprite d'interface inconnu : " + name);
            }
        }

        /// <summary>Dossier des illustrations importées en textures (générées depuis Resources/Art/*.bytes).</summary>
        public const string ArtAssetFolder = "Assets/_Project/Art/UI";

#if UNITY_EDITOR
        // Illustrations pour les recettes de prefabs (éditeur seulement) : en jeu, les prefabs portent les références.
        public static Sprite Logo => Art("HD_Logo_png");
        public static Sprite Title => Art("HD_Title_png");
        public static Sprite BackgroundLandscape => Art("HD_BG_Landscape_jpg");
        public static Sprite BackgroundPortrait => Art("HD_BG_Portrait_jpg");

        /// <summary>Texture importée d'une illustration (« HD_Logo_png » → Art/UI/HD_Logo.png) ; null si absente.</summary>
        public static Sprite Art(string name)
        {
            int dot = name.LastIndexOf('_');
            var path = ArtAssetFolder + "/" + name.Substring(0, dot) + "." + name.Substring(dot + 1);
            var sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null) Debug.LogWarning("[Heroes Dawn] Illustration non importée : " + path + " (Heroes Dawn ▸ Setup ▸ Générer les assets d'interface)");
            return sprite;
        }
#endif

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

        /// <summary>Étoile à <paramref name="points"/> pointes irrégulières (la même à chaque génération).</summary>
        static Sprite MakeBurst(int size, int points)
        {
            var t = NewTex(size, size);
            var px = new Color32[size * size];
            float c = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - c, dy = y + 0.5f - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx) / (2f * Mathf.PI) + 0.5f; // 0..1
                float k = ang * points;
                int tooth = Mathf.FloorToInt(k);
                float frac = k - tooth;
                float tip = 0.82f + 0.18f * (((tooth * 37) % 7) / 6f);   // pointes de hauteurs variées
                float zig = frac < 0.5f ? frac * 2f : (1f - frac) * 2f;  // 0 creux, 1 pointe
                float edge = c * Mathf.Lerp(0.58f, tip, zig);
                float a = Mathf.Clamp01(edge - d + 0.5f);
                px[y * size + x] = new Color32(255, 255, 255, (byte)(a * 255));
            }
            t.SetPixels32(px);
            t.Apply();
            return Sprite.Create(t, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>Nuage : disque flou dont le rayon varie avec l'angle.</summary>
        static Sprite MakeCloud(int size)
        {
            var t = NewTex(size, size);
            var px = new Color32[size * size];
            float c = size * 0.5f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - c, dy = y + 0.5f - c;
                float d = Mathf.Sqrt(dx * dx + dy * dy) / c;
                float ang = Mathf.Atan2(dy, dx);
                float wobble = 0.82f + 0.10f * Mathf.Sin(ang * 3f + 0.7f) + 0.06f * Mathf.Sin(ang * 5f + 2.1f);
                float a = Mathf.Clamp01(1f - d / wobble);
                a = a * a * (3f - 2f * a);
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
