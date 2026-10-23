#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Scène dessinée par code : plan entièrement procédural ou motif ajouté par-dessus une illustration.</summary>
    public abstract class ProcScene
    {
        protected RectTransform Root = null!;
        protected readonly List<GameObject> Objects = new List<GameObject>();
        protected readonly System.Random Rng = new System.Random(5);

        protected Vector2 Size => Root.rect.size;

        public void Attach(RectTransform root)
        {
            Root = root;
            Build();
        }

        protected abstract void Build();

        /// <summary>Met à jour la scène ; <paramref name="t"/> : secondes écoulées depuis le début du plan.</summary>
        public abstract void Tick(float t);

        public void Destroy()
        {
            foreach (var o in Objects) if (o != null) Object.Destroy(o);
            Objects.Clear();
        }

        protected float R(float a, float b) => a + (float)Rng.NextDouble() * (b - a);

        protected Image Img(string name, Sprite? sprite, Color color, Vector2 size, Vector2 pos, float rotation = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)) { layer = 5 };
            go.transform.SetParent(Root, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = pos;
            rt.localRotation = Quaternion.Euler(0f, 0f, rotation);
            var img = go.GetComponent<Image>();
            img.sprite = sprite;
            img.color = color;
            img.raycastTarget = false;
            Objects.Add(go);
            return img;
        }

        protected static void Place(Image i, Vector2 pos) => i.rectTransform.anchoredPosition = pos;
        protected static void Alpha(Image i, float a) { var c = i.color; c.a = a; i.color = c; }
        protected static float Ease(float k) => k * k * (3f - 2f * k);

        protected void Background(Color color)
        {
            var bg = Img("Background", null, color, Vector2.zero, Vector2.zero);
            var rt = bg.rectTransform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            rt.SetAsFirstSibling();
        }

        // ---------------------------------------------------------------- Fabrique

        public static ProcScene? Create(CinematicProc kind)
        {
            switch (kind)
            {
                case CinematicProc.Ember: return new EmberScene();
                case CinematicProc.Helix: return new HelixScene();
                case CinematicProc.Monitor: return new MonitorScene();
                case CinematicProc.Fireworks: return new FireworksScene();
                case CinematicProc.CoatSnow: return new CoatSnowScene();
                case CinematicProc.Rings: return new RingsScene();
                case CinematicProc.Wings: return new WingsScene();
                case CinematicProc.Supernova: return new SupernovaScene();
                case CinematicProc.Globe: return new GlobeScene();
                case CinematicProc.Star: return new StarScene();
                case CinematicProc.Crystals: return new CrystalsScene();
                default: return null;
            }
        }
    }

    /// <summary>S01 : une braise dorée dans l'obscurité absolue.</summary>
    sealed class EmberScene : ProcScene
    {
        Image _halo = null!, _core = null!, _mid = null!;

        protected override void Build()
        {
            Background(Color.black);
            _halo = Img("Halo", CinematicSprites.Glow, new Color(1f, 0.6f, 0.15f, 0.3f), new Vector2(520, 520), Vector2.zero);
            _mid = Img("Mid", CinematicSprites.Glow, new Color(1f, 0.75f, 0.3f, 0.8f), new Vector2(150, 150), Vector2.zero);
            _core = Img("Core", Sprites.Circle, new Color(1f, 0.93f, 0.7f, 1f), new Vector2(18, 18), Vector2.zero);
        }

        public override void Tick(float t)
        {
            float k = Mathf.Clamp01(t / 6f);
            var pos = new Vector2(Mathf.Sin(t * 0.8f) * 22f, -Size.y * 0.18f + k * Size.y * 0.3f);
            float pulse = 0.85f + 0.15f * Mathf.Sin(t * 3.1f) + 0.06f * Mathf.Sin(t * 8.3f);
            float fade = Mathf.Min(t / 1.2f, 1f);
            Place(_halo, pos); Place(_mid, pos); Place(_core, pos);
            _halo.rectTransform.sizeDelta = Vector2.one * 520f * pulse;
            _mid.rectTransform.sizeDelta = Vector2.one * 150f * pulse;
            Alpha(_halo, 0.3f * fade * pulse); Alpha(_mid, 0.8f * fade); Alpha(_core, fade);
        }
    }

    /// <summary>S03 : la double hélice d'ADN qui s'élève de la fiole.</summary>
    sealed class HelixScene : ProcScene
    {
        const int Rungs = 34;
        readonly Image[] _a = new Image[Rungs], _b = new Image[Rungs], _bar = new Image[Rungs];
        Image _glow = null!;

        protected override void Build()
        {
            Background(new Color(0.03f, 0.09f, 0.11f, 1f));
            _glow = Img("Glow", CinematicSprites.Glow, new Color(0.3f, 0.8f, 1f, 0.25f), new Vector2(900, 900), new Vector2(0, -60));
            for (int i = 0; i < Rungs; i++)
            {
                _bar[i] = Img("Bar", null, new Color(0.5f, 0.9f, 1f, 0.5f), new Vector2(100, 3), Vector2.zero);
                _a[i] = Img("A", Sprites.Circle, new Color(0.4f, 0.85f, 1f, 1f), new Vector2(18, 18), Vector2.zero);
                _b[i] = Img("B", Sprites.Circle, new Color(0.65f, 1f, 0.95f, 1f), new Vector2(18, 18), Vector2.zero);
            }
        }

        public override void Tick(float t)
        {
            float h = Size.y;
            float fade = Mathf.Min(t / 1f, 1f);
            for (int i = 0; i < Rungs; i++)
            {
                float u = i / (float)(Rungs - 1);
                float y = Mathf.Lerp(-h * 0.4f, h * 0.4f, u);
                float ph = t * 1.2f + u * 11f;
                float x = Mathf.Sin(ph) * h * 0.16f;
                float z = Mathf.Cos(ph);
                float grow = Mathf.Clamp01(t * 0.5f - u * 0.6f + 0.2f);
                float s = (14f + 9f * z) * grow;
                _a[i].rectTransform.sizeDelta = Vector2.one * s; _b[i].rectTransform.sizeDelta = Vector2.one * (14f - 9f * z) * grow;
                Place(_a[i], new Vector2(x, y)); Place(_b[i], new Vector2(-x, y));
                Alpha(_a[i], (0.6f + 0.4f * z) * fade * grow); Alpha(_b[i], (0.6f - 0.4f * z) * fade * grow);
                Place(_bar[i], new Vector2(0, y));
                _bar[i].rectTransform.sizeDelta = new Vector2(Mathf.Abs(x) * 2f, 3f);
                Alpha(_bar[i], 0.35f * fade * grow);
            }
            Alpha(_glow, (0.18f + 0.07f * Mathf.Sin(t * 2f)) * fade);
        }
    }

    /// <summary>S16 : le moniteur cardiaque (une seule impulsion) sous le givre.</summary>
    sealed class MonitorScene : ProcScene
    {
        const int Points = 140;
        readonly Image[] _seg = new Image[Points];
        Image _glow = null!;

        protected override void Build()
        {
            Background(new Color(0.02f, 0.07f, 0.1f, 1f));
            for (int i = -6; i <= 6; i++) Img("GridV", null, new Color(0.3f, 0.7f, 0.8f, 0.07f), new Vector2(2, Size.y), new Vector2(i * Size.x / 12f, 0));
            for (int i = -3; i <= 3; i++) Img("GridH", null, new Color(0.3f, 0.7f, 0.8f, 0.07f), new Vector2(Size.x, 2), new Vector2(0, i * Size.y / 6f));
            _glow = Img("Glow", CinematicSprites.Glow, new Color(0.2f, 1f, 0.6f, 0.12f), new Vector2(1100, 500), Vector2.zero);
            for (int i = 0; i < Points; i++) _seg[i] = Img("Seg", null, new Color(0.4f, 1f, 0.65f, 1f), new Vector2(Size.x / Points + 2f, 4f), Vector2.zero);
        }

        static float Pulse(float u)
        {
            // Une seule impulsion : pic, creux, retour à plat.
            float d = (u - 0.58f) / 0.018f;
            return 150f * Mathf.Exp(-d * d) - 70f * Mathf.Exp(-((u - 0.605f) / 0.02f) * ((u - 0.605f) / 0.02f));
        }

        public override void Tick(float t)
        {
            float w = Size.x;
            float cursor = Mathf.Clamp01(t / 4.4f) * 1.02f;
            for (int i = 0; i < Points; i++)
            {
                float u = i / (float)(Points - 1);
                float y = (Mathf.PerlinNoise(u * 40f, 1.7f) - 0.5f) * 5f + Pulse(u);
                _seg[i].rectTransform.anchoredPosition = new Vector2((u - 0.5f) * w, y);
                float age = cursor - u;
                Alpha(_seg[i], age < 0f ? 0f : Mathf.Clamp(1f - age * 1.1f, 0.15f, 1f));
            }
            Alpha(_glow, 0.1f + (t > 2.5f && t < 3.3f ? 0.22f * (1f - (t - 2.5f) / 0.8f) : 0f));
        }
    }

    /// <summary>S12 : le bloc soviétique s'effondre, la foule fête sur un mur de béton.</summary>
    sealed class FireworksScene : ProcScene
    {
        sealed class Spark { public Image Img = null!; public Vector2 Pos, Vel; public float Age, Life; public Color Color; }
        readonly List<Spark> _sparks = new List<Spark>();
        readonly List<Image> _flags = new List<Image>();
        readonly List<Vector2> _flagPos = new List<Vector2>();
        float _next;

        protected override void Build()
        {
            Background(new Color(0.03f, 0.04f, 0.1f, 1f));
            float w = Size.x, h = Size.y;
            Img("Glow", CinematicSprites.Glow, new Color(0.5f, 0.2f, 0.2f, 0.25f), new Vector2(w * 1.2f, h * 0.9f), new Vector2(0, -h * 0.3f));
            for (int i = 0; i < 70; i++)
            {
                float x = R(-w * 0.55f, w * 0.55f), s = R(46f, 80f), baseY = -h * 0.5f + h * 0.085f;
                Img("Head", Sprites.Circle, new Color(0.01f, 0.01f, 0.03f, 1f), new Vector2(s * 0.45f, s * 0.45f), new Vector2(x, baseY + s * 1.15f));
                Img("Body", Sprites.Rounded, new Color(0.01f, 0.01f, 0.03f, 1f), new Vector2(s, s * 1.4f), new Vector2(x, baseY + s * 0.45f));
            }
            Img("Wall", null, new Color(0.01f, 0.01f, 0.025f, 1f), new Vector2(w, h * 0.1f), new Vector2(0, -h * 0.5f + h * 0.04f));
            for (int i = 0; i < 16; i++)
            {
                var f = Img("Flag", null, new Color(0.65f, 0.08f, 0.08f, 0.9f), new Vector2(R(36f, 60f), R(22f, 38f)), Vector2.zero, R(-30f, 30f));
                _flags.Add(f); _flagPos.Add(new Vector2(R(-w * 0.5f, w * 0.5f), R(-h * 0.1f, h * 0.7f)));
            }
        }

        public override void Tick(float t)
        {
            float w = Size.x, h = Size.y;
            if (t >= _next)
            {
                _next = t + R(0.45f, 0.9f);
                var c = Rng.Next(3) == 0 ? new Color(1f, 0.85f, 0.4f) : Rng.Next(2) == 0 ? new Color(1f, 0.25f, 0.2f) : new Color(1f, 1f, 1f);
                var o = new Vector2(R(-w * 0.35f, w * 0.35f), R(h * 0.0f, h * 0.35f));
                for (int i = 0; i < 34; i++)
                {
                    float a = i / 34f * Mathf.PI * 2f, sp = R(130f, 290f);
                    _sparks.Add(new Spark { Img = Img("Spark", Sprites.Circle, c, Vector2.one * 9f, o), Pos = o, Vel = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * sp, Life = R(1.3f, 2.1f), Color = c });
                }
                Img("Flash", CinematicSprites.Glow, new Color(c.r, c.g, c.b, 0.5f), Vector2.one * 640f, o).gameObject.AddComponent<TimedDestroy>().Life = 0.6f;
            }
            for (int i = _sparks.Count - 1; i >= 0; i--)
            {
                var s = _sparks[i];
                s.Age += Time.unscaledDeltaTime;
                if (s.Age >= s.Life) { if (s.Img != null) Object.Destroy(s.Img.gameObject); _sparks.RemoveAt(i); continue; }
                s.Vel += new Vector2(0f, -70f) * Time.unscaledDeltaTime;
                s.Pos += s.Vel * Time.unscaledDeltaTime;
                Place(s.Img, s.Pos);
                Alpha(s.Img, 1f - s.Age / s.Life);
            }
            for (int i = 0; i < _flags.Count; i++)
            {
                var p = _flagPos[i];
                float y = p.y - ((t * (28f + i * 3f)) % (h * 0.8f));
                Place(_flags[i], new Vector2(p.x + Mathf.Sin(t * 1.4f + i) * 30f, y + h * 0.1f));
                _flags[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(t * 1.8f + i) * 40f);
            }
        }
    }

    /// <summary>S13 : le manteau de Volkov abandonné dans la neige fraîche, ses pas qui s'effacent.</summary>
    sealed class CoatSnowScene : ProcScene
    {
        readonly List<Image> _cover = new List<Image>();
        readonly List<Image> _prints = new List<Image>();
        readonly List<Image> _drifts = new List<Image>();

        protected override void Build()
        {
            Background(new Color(0.05f, 0.08f, 0.14f, 1f));
            float w = Size.x, h = Size.y;
            Img("Ground", CinematicSprites.Glow, new Color(0.7f, 0.8f, 0.95f, 0.65f), new Vector2(w * 1.3f, h * 0.9f), new Vector2(0, -h * 0.28f));
            Img("Shadow", CinematicSprites.Glow, new Color(0f, 0f, 0.05f, 0.5f), new Vector2(560, 220), new Vector2(10, -h * 0.17f));
            var coat = new Color(0.30f, 0.29f, 0.33f, 1f);
            var lining = new Color(0.55f, 0.08f, 0.08f, 1f);
            var parts = new (string, Vector2, Vector2, float, Color)[]
            {
                ("SleeveL", new Vector2(230, 64), new Vector2(-250, -h * 0.20f), 22f, coat),
                ("SleeveR", new Vector2(230, 64), new Vector2(255, -h * 0.10f), -28f, coat),
                ("CuffL", new Vector2(48, 66), new Vector2(-352, -h * 0.245f), 22f, lining),
                ("CuffR", new Vector2(48, 66), new Vector2(360, -h * 0.045f), -28f, lining),
                ("Body", new Vector2(330, 190), new Vector2(0, -h * 0.14f), -5f, coat),
                ("Lining", new Vector2(46, 190), new Vector2(0, -h * 0.14f), -5f, lining),
                ("Collar", new Vector2(190, 70), new Vector2(-4, -h * 0.14f + 112f), -5f, new Color(0.2f, 0.19f, 0.22f, 1f)),
            };
            foreach (var (n, size, pos, rot, color) in parts)
            {
                Img(n, Sprites.Rounded, color, size, pos, rot);
                _cover.Add(Img("Snow" + n, Sprites.Rounded, new Color(0.95f, 0.97f, 1f, 0f), size, pos, rot));
            }
            for (int i = 0; i < 9; i++) _drifts.Add(Img("Drift", CinematicSprites.Glow, new Color(0.95f, 0.97f, 1f, 0f), new Vector2(R(120f, 230f), R(50f, 90f)), new Vector2(R(-320f, 320f), -h * 0.14f + R(-70f, 90f))));
            for (int i = 0; i < 11; i++) _prints.Add(Img("Print", Sprites.Circle, new Color(0.08f, 0.1f, 0.2f, 0.8f), new Vector2(36, 16), new Vector2(-440f - i * 105f, -h * 0.30f + (i % 2) * 30f), (i % 2) * 8f));
        }

        public override void Tick(float t)
        {
            float cover = Mathf.Clamp01(t / 6f) * 0.55f;
            foreach (var c in _cover) Alpha(c, cover);
            for (int i = 0; i < _drifts.Count; i++) Alpha(_drifts[i], Mathf.Clamp01(t / 5f - i * 0.04f) * 0.6f);
            for (int i = 0; i < _prints.Count; i++) Alpha(_prints[i], Mathf.Clamp01(0.85f - t * 0.17f - i * 0.03f));
        }
    }

    /// <summary>S23 : les cercles de pierre et de runes de la machine occulte.</summary>
    sealed class RingsScene : ProcScene
    {
        readonly List<Image> _rings = new List<Image>();
        readonly List<Image> _runes = new List<Image>();

        protected override void Build()
        {
            float h = Size.y;
            float[] sizes = { 1.5f, 1.05f, 0.7f, 0.42f };
            for (int i = 0; i < sizes.Length; i++)
                _rings.Add(Img("Ring", Sprites.Ring, new Color(1f, 0.35f + 0.2f * i, 0.15f, 0.3f), Vector2.one * h * sizes[i], Vector2.zero));
            for (int i = 0; i < 18; i++)
                _runes.Add(Img("Rune", Sprites.Diamond, new Color(1f, 0.5f, 0.2f, 0.8f), new Vector2(26, 50), Vector2.zero));
            Img("Core", CinematicSprites.Glow, new Color(1f, 0.3f, 0.1f, 0.5f), Vector2.one * h * 0.9f, Vector2.zero);
        }

        public override void Tick(float t)
        {
            for (int i = 0; i < _rings.Count; i++) _rings[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, t * (i % 2 == 0 ? 8f : -14f) * (1f + i * 0.4f));
            float r = Size.y * 0.36f;
            for (int i = 0; i < _runes.Count; i++)
            {
                float a = t * 0.35f + i / (float)_runes.Count * Mathf.PI * 2f;
                Place(_runes[i], new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r * (i % 2 == 0 ? 1f : 1.4f));
                _runes[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, a * Mathf.Rad2Deg - 90f);
                Alpha(_runes[i], 0.5f + 0.4f * Mathf.Sin(t * 2f + i));
            }
        }
    }

    /// <summary>S37 : les ailes de feu doré du Phoenix qui se déploient dans son dos.</summary>
    sealed class WingsScene : ProcScene
    {
        const int Feathers = 9;
        readonly List<Image> _front = new List<Image>(), _back = new List<Image>();
        readonly List<float> _side = new List<float>();
        Image _glow = null!;

        protected override void Build()
        {
            float h = Size.y;
            _glow = Img("Glow", CinematicSprites.Glow, new Color(1f, 0.65f, 0.15f, 0.5f), Vector2.one * h * 1.5f, new Vector2(0, h * 0.02f));
            // Deux ailes (gauche et droite), chacune en deux rangées de plumes : la rangée arrière est plus courte et plus pâle.
            foreach (float side in new[] { -1f, 1f })
            for (int i = 0; i < Feathers; i++)
            {
                float u = i / (float)(Feathers - 1);
                float len = h * (0.34f + 0.2f * (1f - u * 0.55f));
                var cb = Color.Lerp(new Color(1f, 0.45f, 0.08f, 0.55f), new Color(1f, 0.7f, 0.2f, 0.55f), u);
                var cf = Color.Lerp(new Color(1f, 0.72f, 0.18f, 0.85f), new Color(1f, 0.95f, 0.65f, 0.9f), u);
                _back.Add(Img("WingBack", Sprites.Diamond, cb, new Vector2(96f, len * 0.8f), Vector2.zero));
                _front.Add(Img("WingFront", Sprites.Diamond, cf, new Vector2(70f, len), Vector2.zero));
                _side.Add(side);
            }
        }

        public override void Tick(float t)
        {
            float h = Size.y;
            float open = Ease(Mathf.Clamp01((t - 0.2f) / 3.4f));
            for (int i = 0; i < _front.Count; i++)
            {
                float u = (i % Feathers) / (float)(Feathers - 1);
                float side = _side[i];
                float flutter = Mathf.Sin(t * 1.4f + u * 3f) * 1.6f;
                SetFeather(_back[i], side, Mathf.Lerp(8f, 84f, u) * open + 5f + flutter, 0.8f, h);
                SetFeather(_front[i], side, Mathf.Lerp(14f, 74f, u) * open + flutter, 1f, h);
                float a = Mathf.Clamp01(open * 1.5f);
                Alpha(_front[i], (0.55f + 0.4f * u) * a);
                Alpha(_back[i], 0.45f * a);
            }
            Alpha(_glow, 0.25f + 0.3f * open + 0.05f * Mathf.Sin(t * 3f));
        }

        static void SetFeather(Image f, float side, float angDeg, float lenScale, float h)
        {
            // Plume centrée à mi-longueur sur un rayon qui part de l'épaule ; angle 0 = vers le haut, 90 = à l'horizontale.
            float rad = (90f - angDeg) * Mathf.Deg2Rad;
            var dir = new Vector2(side * Mathf.Cos(rad), Mathf.Sin(rad));
            float half = f.rectTransform.sizeDelta.y * 0.5f;
            f.rectTransform.anchoredPosition = new Vector2(side * 40f, h * 0.02f) + dir * half;
            f.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -side * angDeg);
        }
    }

    /// <summary>S40 : la supernova blanche et dorée dans la dimension infernale.</summary>
    sealed class SupernovaScene : ProcScene
    {
        readonly List<Image> _rings = new List<Image>();
        readonly List<Image> _shards = new List<Image>();
        readonly List<Vector2> _dir = new List<Vector2>();
        Image _core = null!, _burst = null!, _bg = null!, _warm = null!;

        protected override void Build()
        {
            Background(new Color(0.18f, 0.02f, 0.04f, 1f));
            _bg = Img("Sky", CinematicSprites.Glow, new Color(0.7f, 0.1f, 0.1f, 0.5f), Vector2.one * Size.x, Vector2.zero);
            for (int i = 0; i < 34; i++)
            {
                var s = Img("Shard", Sprites.Diamond, new Color(0.08f, 0.02f, 0.03f, 0.9f), new Vector2(R(30f, 90f), R(60f, 200f)), Vector2.zero, R(0f, 360f));
                _shards.Add(s); _dir.Add(Random.insideUnitCircle.normalized * R(0.25f, 1f));
            }
            for (int i = 0; i < 6; i++) _rings.Add(Img("Ring", Sprites.Ring, new Color(1f, 0.9f, 0.55f, 0f), Vector2.one * 40f, Vector2.zero));
            _burst = Img("Burst", Sprites.Burst, new Color(1f, 0.96f, 0.8f, 0f), Vector2.one * 100f, Vector2.zero);
            _warm = Img("Warm", CinematicSprites.Glow, new Color(1f, 0.55f, 0.15f, 0f), Vector2.one * 30f, Vector2.zero);
            _core = Img("Core", CinematicSprites.Glow, new Color(1f, 0.93f, 0.7f, 1f), Vector2.one * 30f, Vector2.zero);
        }

        public override void Tick(float t)
        {
            float w = Size.x;
            float k = Mathf.Clamp01(t / 4.2f);
            _warm.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(30f, w * 2.4f, Ease(k));
            Alpha(_warm, Mathf.Clamp01(k * 2f) * 0.7f);
            _core.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(30f, w * 1.5f, Ease(k));
            Alpha(_core, Mathf.Clamp01(0.4f + k * 1.3f) * (1f - Mathf.Clamp01((t - 4.4f) / 0.6f) * 0.2f));
            _burst.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(100f, w * 1.1f, Ease(Mathf.Clamp01(t / 2.4f)));
            _burst.rectTransform.localRotation = Quaternion.Euler(0f, 0f, t * 18f);
            Alpha(_burst, Mathf.Clamp01(1.1f - t / 3.5f) * 0.85f * Mathf.Min(t * 3f, 1f));
            for (int i = 0; i < _rings.Count; i++)
            {
                float rk = Mathf.Clamp01((t - 0.35f - i * 0.42f) / 2.2f);
                _rings[i].rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(40f, w * 1.6f, 1f - (1f - rk) * (1f - rk));
                Alpha(_rings[i], rk <= 0f || rk >= 1f ? 0f : 0.85f * (1f - rk));
            }
            for (int i = 0; i < _shards.Count; i++)
            {
                Place(_shards[i], _dir[i] * w * 0.6f * Ease(Mathf.Clamp01(t / 4f)) + _dir[i] * 30f);
                Alpha(_shards[i], Mathf.Clamp01(1f - t / 3.8f) * 0.9f);
            }
            Alpha(_bg, Mathf.Clamp01(0.5f - t * 0.1f));
        }
    }

    /// <summary>S41 : la Terre vue de l'orbite, l'onde dorée éteint les villes continent par continent.</summary>
    sealed class GlobeScene : ProcScene
    {
        readonly List<Image> _lights = new List<Image>();
        readonly List<float> _off = new List<float>();
        readonly List<Image> _stars = new List<Image>();
        Image _ring = null!, _aurora = null!, _rim = null!;

        protected override void Build()
        {
            Background(new Color(0.008f, 0.01f, 0.03f, 1f));
            float h = Size.y, d = h * 0.82f, r = d * 0.5f;
            for (int i = 0; i < 90; i++) _stars.Add(Img("Star", Sprites.Circle, new Color(1f, 1f, 1f, R(0.2f, 0.7f)), Vector2.one * R(1.5f, 3.5f), new Vector2(R(-Size.x * 0.5f, Size.x * 0.5f), R(-h * 0.5f, h * 0.5f))));
            Img("Atmosphere", CinematicSprites.Glow, new Color(0.25f, 0.5f, 1f, 0.55f), Vector2.one * d * 1.3f, Vector2.zero);
            Img("Planet", Sprites.Circle, new Color(0.05f, 0.11f, 0.25f, 1f), Vector2.one * d, Vector2.zero);
            _rim = Img("Rim", CinematicSprites.Glow, new Color(0.4f, 0.65f, 1f, 0.3f), Vector2.one * d * 1.06f, Vector2.zero);
            Img("Shade", CinematicSprites.Glow, new Color(0.1f, 0.2f, 0.45f, 0.35f), Vector2.one * d * 1.12f, Vector2.zero);
            for (int i = 0; i < 520; i++)
            {
                Vector2 p;
                do { p = new Vector2(R(-1f, 1f), R(-1f, 1f)); } while (p.magnitude > 0.94f);
                float band = Mathf.PerlinNoise(p.x * 3f + 4f, p.y * 3f + 2f);
                if (band < 0.42f) { i--; continue; }
                var l = Img("City", Sprites.Circle, new Color(1f, 0.8f, 0.4f, 0.95f), Vector2.one * R(4f, 9f), p * r * 0.96f);
                _lights.Add(l); _off.Add(1.6f + (p.x * 0.5f + 0.5f) * 3.2f + R(-0.25f, 0.25f));
            }
            _ring = Img("Wave", Sprites.Ring, new Color(1f, 0.85f, 0.4f, 0f), Vector2.one * 30f, Vector2.zero);
            _aurora = Img("Aurora", CinematicSprites.Glow, new Color(1f, 0.8f, 0.3f, 0f), new Vector2(d * 1.3f, d * 0.5f), new Vector2(0, r * 0.82f));
        }

        public override void Tick(float t)
        {
            float h = Size.y;
            for (int i = 0; i < _lights.Count; i++)
            {
                float a = Mathf.Clamp01(1f - (t - _off[i]) * 3f);
                float flick = t > _off[i] - 0.5f && t < _off[i] ? (Mathf.Sin(t * 40f + i) > 0f ? 1f : 0.3f) : 1f;
                Alpha(_lights[i], 0.95f * a * flick);
            }
            float k = Mathf.Clamp01((t - 0.4f) / 4.6f);
            _ring.rectTransform.sizeDelta = Vector2.one * Mathf.Lerp(30f, h * 2.4f, Ease(k));
            Alpha(_ring, k <= 0f ? 0f : 0.9f * (1f - k));
            Alpha(_aurora, Mathf.Clamp01((t - 1.8f) / 2f) * (0.3f + 0.12f * Mathf.Sin(t * 2f)));
            Alpha(_rim, 0.3f + 0.2f * Mathf.Clamp01((t - 2f) / 2f));
            for (int i = 0; i < _stars.Count; i++) Alpha(_stars[i], 0.35f + 0.35f * Mathf.Sin(t * 1.3f + i));
        }
    }

    /// <summary>S46 : l'étoile d'or à quatre branches qui s'embrase, puis le logo.</summary>
    sealed class StarScene : ProcScene
    {
        readonly List<Image> _rays = new List<Image>();
        Image _v = null!, _hz = null!, _d1 = null!, _d2 = null!, _glow = null!, _logo = null!;

        protected override void Build()
        {
            Background(new Color(0.01f, 0.01f, 0.02f, 1f));
            float h = Size.y;
            for (int i = 0; i < 12; i++)
            {
                var r = Img("Ray", CinematicSprites.Beam, new Color(1f, 0.82f, 0.4f, 0.2f), new Vector2(120f, h * 0.9f), new Vector2(0, h * 0.08f), i * 30f);
                r.rectTransform.pivot = new Vector2(0.5f, 0f);
                _rays.Add(r);
            }
            _glow = Img("Glow", CinematicSprites.Glow, new Color(1f, 0.75f, 0.25f, 0.6f), Vector2.one * h * 1.1f, new Vector2(0, h * 0.08f));
            _d1 = Img("D1", Sprites.Diamond, new Color(1f, 0.85f, 0.45f, 0.6f), new Vector2(60f, h * 0.5f), new Vector2(0, h * 0.08f), 45f);
            _d2 = Img("D2", Sprites.Diamond, new Color(1f, 0.85f, 0.45f, 0.6f), new Vector2(60f, h * 0.5f), new Vector2(0, h * 0.08f), -45f);
            _v = Img("V", Sprites.Diamond, new Color(1f, 0.93f, 0.65f, 1f), new Vector2(90f, h * 0.78f), new Vector2(0, h * 0.08f));
            _hz = Img("H", Sprites.Diamond, new Color(1f, 0.93f, 0.65f, 1f), new Vector2(90f, h * 0.78f), new Vector2(0, h * 0.08f), 90f);
            var sprite = Resources.Load<Sprite>("Cinematic/Art/logo");
            _logo = Img("Logo", sprite, new Color(1f, 1f, 1f, 0f), new Vector2(h * 0.62f, h * 0.36f), new Vector2(0, -h * 0.17f));
            _logo.preserveAspect = true;
        }

        public override void Tick(float t)
        {
            float ignite = Mathf.Clamp01(t / 1.1f);
            float over = 1f + 0.25f * Mathf.Sin(ignite * Mathf.PI);
            float s = Ease(ignite) * over;
            float pulse = 1f + 0.04f * Mathf.Sin(t * 3f);
            foreach (var i in new[] { _v, _hz, _d1, _d2 }) i.rectTransform.localScale = Vector3.one * s * pulse;
            _glow.rectTransform.localScale = Vector3.one * (0.5f + ignite * 0.6f) * pulse;
            Alpha(_glow, 0.55f * ignite);
            for (int i = 0; i < _rays.Count; i++)
            {
                _rays[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, i * 30f + t * 6f);
                Alpha(_rays[i], 0.16f * ignite * (0.7f + 0.3f * Mathf.Sin(t * 2f + i)));
            }
            Alpha(_logo, Mathf.Clamp01((t - 2.4f) / 1.4f));
        }
    }

    /// <summary>S42 : les cristaux d'edenite, turquoise et or, qui poussent dans les gravats.</summary>
    sealed class CrystalsScene : ProcScene
    {
        readonly List<Image> _crystals = new List<Image>();
        readonly List<float> _start = new List<float>();
        readonly List<float> _height = new List<float>();

        protected override void Build()
        {
            float w = Size.x, h = Size.y;
            Img("Base", CinematicSprites.Glow, new Color(0.4f, 1f, 0.8f, 0.25f), new Vector2(w * 1.1f, h * 0.5f), new Vector2(0, -h * 0.45f));
            for (int i = 0; i < 14; i++)
            {
                bool gold = i % 3 == 0;
                var c = gold ? new Color(1f, 0.82f, 0.35f, 0.9f) : new Color(0.25f, 0.95f, 0.85f, 0.85f);
                float x = R(-w * 0.48f, w * 0.48f), ht = R(h * 0.14f, h * 0.38f);
                var cr = Img("Crystal", Sprites.Diamond, c, new Vector2(ht * R(0.28f, 0.42f), ht), new Vector2(x, -h * 0.5f + ht * 0.35f), R(-12f, 12f));
                cr.rectTransform.pivot = new Vector2(0.5f, 0.2f);
                _crystals.Add(cr); _start.Add(R(0.2f, 2.6f)); _height.Add(ht);
            }
        }

        public override void Tick(float t)
        {
            for (int i = 0; i < _crystals.Count; i++)
            {
                float k = Ease(Mathf.Clamp01((t - _start[i]) / 1.4f));
                _crystals[i].rectTransform.localScale = new Vector3(1f, k, 1f);
                var c = _crystals[i].color; c.a = (0.55f + 0.35f * Mathf.Sin(t * 1.7f + i)) * k; _crystals[i].color = c;
            }
        }
    }

    /// <summary>Détruit son objet après <see cref="Life"/> secondes (éclairs de feux d'artifice).</summary>
    sealed class TimedDestroy : MonoBehaviour
    {
        public float Life = 1f;
        float _age;

        void Update()
        {
            _age += Time.unscaledDeltaTime;
            var img = GetComponent<Image>();
            if (img != null) { var c = img.color; c.a = Mathf.Max(0f, c.a - Time.unscaledDeltaTime * 0.7f); img.color = c; }
            if (_age >= Life) Destroy(gameObject);
        }
    }
}
