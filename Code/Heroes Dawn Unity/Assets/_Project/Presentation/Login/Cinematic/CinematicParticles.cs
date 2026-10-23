#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Ambiance d'un plan : braises, neige, pluie, poussière, étincelles, fumées, flammes (particules recyclées) et lumières
    /// (faisceaux de projecteurs, rayons, pulsation d'alarme, vacillement, éclairs, givre). Tout est dessiné par code sur une couche d'images.
    /// </summary>
    public sealed class CinematicParticles
    {
        sealed class Particle
        {
            public RectTransform Rt = null!;
            public Image Img = null!;
            public Vector2 Pos, Vel;
            public float Age, Life, Size, Size1, Alpha, Wobble, WobbleSpeed, Spin;
            public Color Color;
            public bool FadeIn;
        }

        readonly RectTransform _root;
        readonly List<Particle> _live = new List<Particle>();
        readonly Stack<Particle> _pool = new Stack<Particle>();
        readonly Dictionary<CinematicFx, float> _acc = new Dictionary<CinematicFx, float>();
        readonly List<GameObject> _overlays = new List<GameObject>();
        readonly List<RectTransform> _beams = new List<RectTransform>();
        Image? _redPulse, _flicker, _flash;
        CinematicFx _fx;
        Color _smoke = new Color(0.5f, 0.52f, 0.58f);
        float _time, _nextFlash;

        public CinematicParticles(RectTransform root) { _root = root; }

        Vector2 Size => _root.rect.size;

        public void Set(CinematicFx fx, Color? smoke)
        {
            Clear();
            _fx = fx;
            _smoke = smoke ?? new Color(0.5f, 0.52f, 0.58f);
            _nextFlash = Random.Range(0.8f, 2.4f);
            if ((fx & CinematicFx.Searchlights) != 0) BuildBeams(3, new Color(1f, 0.95f, 0.8f, 0.2f), true);
            if ((fx & CinematicFx.Rays) != 0) BuildBeams(4, new Color(1f, 0.85f, 0.5f, 0.13f), false);
            if ((fx & CinematicFx.RedPulse) != 0) _redPulse = FullScreen(new Color(0.9f, 0.05f, 0.05f, 0f));
            if ((fx & CinematicFx.Flicker) != 0) _flicker = FullScreen(new Color(0f, 0f, 0f, 0f));
            if ((fx & CinematicFx.Lightning) != 0) _flash = FullScreen(new Color(1f, 1f, 1f, 0f));
            if ((fx & CinematicFx.Frost) != 0) BuildFrost();
        }

        public void Clear()
        {
            foreach (var p in _live) Recycle(p);
            _live.Clear();
            foreach (var o in _overlays) if (o != null) Object.Destroy(o);
            _overlays.Clear();
            _beams.Clear();
            _acc.Clear();
            _redPulse = _flicker = _flash = null;
            _fx = CinematicFx.None;
        }

        public void Tick(float dt)
        {
            _time += dt;
            var size = Size;
            if (size.x < 10f) return;
            Emit(CinematicFx.Embers, 14f, dt, () => Spawn(Sprites.Circle, new Vector2(Rand(0f, size.x), Rand(-30f, size.y * 0.25f)), new Vector2(Rand(-14f, 14f), Rand(45f, 115f)),
                Rand(5f, 9f), Rand(5f, 9f) * 0.4f, Rand(5f, 8.5f), new Color(1f, Rand(0.55f, 0.8f), 0.25f), 0.9f, true));
            Emit(CinematicFx.GoldSparks, 26f, dt, () => Spawn(Sprites.Circle, new Vector2(Rand(0f, size.x), Rand(-30f, size.y * 0.5f)), new Vector2(Rand(-30f, 30f), Rand(90f, 240f)),
                Rand(3f, 7f), 1.5f, Rand(2f, 4f), new Color(1f, Rand(0.78f, 0.92f), 0.4f), 0.95f, true));
            Emit(CinematicFx.Snow, 42f, dt, () => Spawn(Sprites.Circle, new Vector2(Rand(-50f, size.x + 50f), size.y + 20f), new Vector2(Rand(-26f, 8f), -Rand(55f, 120f)),
                Rand(3f, 8f), Rand(3f, 8f), 9f, new Color(0.95f, 0.97f, 1f), Rand(0.45f, 0.85f), false));
            Emit(CinematicFx.Rain, 95f, dt, () =>
            {
                var p = Spawn(null, new Vector2(Rand(-100f, size.x + 200f), size.y + 40f), new Vector2(-170f, -Rand(820f, 1100f)), 2f, 2f, 1.5f, new Color(0.8f, 0.9f, 1f), Rand(0.2f, 0.4f), false);
                p.Rt.sizeDelta = new Vector2(2f, Rand(26f, 48f));
                p.Rt.localRotation = Quaternion.Euler(0f, 0f, 11f);
                p.Size = -1f;
            });
            Emit(CinematicFx.Dust, 11f, dt, () => Spawn(Sprites.Circle, new Vector2(Rand(0f, size.x), Rand(0f, size.y)), new Vector2(Rand(-8f, 8f), Rand(-4f, 12f)),
                Rand(2f, 5f), Rand(2f, 5f), Rand(6f, 9f), new Color(1f, 0.95f, 0.8f), Rand(0.2f, 0.42f), true));
            Emit(CinematicFx.BlueSparks, 16f, dt, () =>
            {
                var o = new Vector2(Rand(size.x * 0.25f, size.x * 0.75f), Rand(size.y * 0.25f, size.y * 0.7f));
                var d = Random.insideUnitCircle.normalized * Rand(60f, 220f);
                Spawn(Sprites.Circle, o, d, Rand(3f, 6f), 1f, Rand(0.4f, 1.0f), new Color(0.55f, 0.82f, 1f), 1f, false);
            });
            Emit(CinematicFx.Smoke, 1.1f, dt, () => Spawn(CinematicSprites.Glow, new Vector2(Rand(0f, size.x), Rand(-60f, size.y * 0.3f)), new Vector2(Rand(-12f, 12f), Rand(18f, 45f)),
                Rand(420f, 700f), Rand(700f, 900f), Rand(9f, 12f), _smoke, 0.25f, true));
            Emit(CinematicFx.RedSmoke, 1.4f, dt, () => Spawn(CinematicSprites.Glow, new Vector2(Rand(0f, size.x), Rand(-60f, size.y * 0.35f)), new Vector2(Rand(-10f, 10f), Rand(30f, 70f)),
                Rand(380f, 620f), Rand(640f, 900f), Rand(7f, 10f), new Color(0.75f, 0.08f, 0.1f), 0.28f, true));
            Emit(CinematicFx.Fire, 46f, dt, () => Spawn(CinematicSprites.Glow, new Vector2(Rand(0f, size.x), Rand(-40f, 20f)), new Vector2(Rand(-25f, 25f), Rand(130f, 290f)),
                Rand(90f, 170f), 20f, Rand(1.1f, 1.9f), new Color(1f, Rand(0.35f, 0.6f), 0.1f), 0.7f, false));
            Emit(CinematicFx.GreenFire, 32f, dt, () =>
            {
                bool green = Random.value < 0.6f;
                Spawn(CinematicSprites.Glow, new Vector2(Rand(0f, size.x), Rand(-40f, 30f)), new Vector2(Rand(-20f, 20f), Rand(90f, 220f)),
                    Rand(100f, 200f), 30f, Rand(1.3f, 2.2f), green ? new Color(0.25f, 1f, 0.4f) : new Color(0.02f, 0.06f, 0.03f), green ? 0.55f : 0.65f, false);
            });

            var live = _live;
            for (int i = live.Count - 1; i >= 0; i--)
            {
                var p = live[i];
                p.Age += dt;
                if (p.Age >= p.Life) { Recycle(p); live.RemoveAt(i); continue; }
                float k = p.Age / p.Life;
                p.Pos += p.Vel * dt;
                float sway = p.Wobble > 0f ? Mathf.Sin(_time * p.WobbleSpeed + p.Wobble) * 14f : 0f;
                p.Rt.anchoredPosition = p.Pos + new Vector2(sway, 0f);
                float fade = p.FadeIn ? Mathf.Min(k * 5f, 1f) * Mathf.Min((1f - k) * 3.5f, 1f) : 1f - k;
                var c = p.Color;
                c.a = p.Alpha * Mathf.Clamp01(fade);
                p.Img.color = c;
                if (p.Size >= 0f) { float s = Mathf.Lerp(p.Size, p.Size1, k); p.Rt.sizeDelta = new Vector2(s, s); }
            }
            TickOverlays();
        }

        void TickOverlays()
        {
            var size = Size;
            if (_redPulse != null) _redPulse.color = new Color(0.9f, 0.04f, 0.04f, 0.04f + 0.1f * (0.5f + 0.5f * Mathf.Sin(_time * 3.2f)));
            if (_flicker != null) _flicker.color = new Color(0f, 0f, 0f, Mathf.Clamp01(0.08f + 0.18f * Mathf.PerlinNoise(_time * 9f, 3.1f)));
            if (_flash != null)
            {
                _nextFlash -= Time.unscaledDeltaTime;
                if (_nextFlash <= 0f) { _nextFlash = Random.Range(1.6f, 4.2f); _flash.color = new Color(0.9f, 0.85f, 1f, 0.45f); }
                else _flash.color = new Color(_flash.color.r, _flash.color.g, _flash.color.b, Mathf.MoveTowards(_flash.color.a, 0f, Time.unscaledDeltaTime * 1.8f));
            }
            for (int i = 0; i < _beams.Count; i++)
            {
                var b = _beams[i];
                bool search = (_fx & CinematicFx.Searchlights) != 0;
                if (search)
                {
                    float a = 24f * Mathf.Sin(_time * (0.55f + 0.17f * i) + i * 2.1f) + (i - 1) * 16f;
                    b.localRotation = Quaternion.Euler(0f, 0f, 180f + a);
                    b.anchoredPosition = new Vector2(size.x * (0.2f + 0.3f * i), 0f);
                }
                else
                {
                    float a = -18f + 36f * ((i + 0.5f) / _beams.Count) + 5f * Mathf.Sin(_time * 0.3f + i);
                    b.localRotation = Quaternion.Euler(0f, 0f, 180f + a);
                    b.anchoredPosition = new Vector2(size.x * 0.82f, 40f);
                }
            }
        }

        // ---------------------------------------------------------------- Construction des lumières

        Image FullScreen(Color color)
        {
            var go = NewObject("Overlay");
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            _overlays.Add(go);
            return img;
        }

        void BuildBeams(int count, Color color, bool searchlight)
        {
            for (int i = 0; i < count; i++)
            {
                var go = NewObject(searchlight ? "Searchlight" : "Ray");
                var rt = (RectTransform)go.transform;
                // Le sprite est opaque en bas : pivot en bas, retourné de 180° pour que la source soit en haut du cadre.
                rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0.5f, 0f);
                rt.sizeDelta = new Vector2(searchlight ? 230f : 260f, Size.y * (searchlight ? 1.5f : 1.9f));
                rt.localRotation = Quaternion.Euler(0f, 0f, 180f);
                var img = go.GetComponent<Image>();
                img.sprite = CinematicSprites.Beam;
                img.color = color;
                _overlays.Add(go);
                _beams.Add(rt);
            }
        }

        void BuildFrost()
        {
            var size = Size;
            for (int i = 0; i < 6; i++)
            {
                var go = NewObject("Frost");
                var rt = (RectTransform)go.transform;
                bool left = i % 2 == 0;
                rt.anchorMin = rt.anchorMax = new Vector2(left ? 0f : 1f, i < 4 ? 0f : 1f);
                float s = Rand(420f, 640f);
                rt.sizeDelta = new Vector2(s, s);
                rt.anchoredPosition = new Vector2(left ? 40f : -40f, i < 4 ? 40f : -40f);
                var img = go.GetComponent<Image>();
                img.sprite = CinematicSprites.Glow;
                img.color = new Color(0.82f, 0.93f, 1f, 0.22f);
                img.raycastTarget = false;
                _overlays.Add(go);
            }
        }

        // ---------------------------------------------------------------- Particules

        void Emit(CinematicFx kind, float rate, float dt, System.Action spawn)
        {
            if ((_fx & kind) == 0) return;
            _acc.TryGetValue(kind, out float a);
            a += rate * dt;
            while (a >= 1f) { spawn(); a -= 1f; }
            _acc[kind] = a;
        }

        Particle Spawn(Sprite? sprite, Vector2 pos, Vector2 vel, float size, float size1, float life, Color color, float alpha, bool fadeIn)
        {
            var p = _pool.Count > 0 ? _pool.Pop() : Create();
            p.Rt.gameObject.SetActive(true);
            p.Img.sprite = sprite;
            p.Img.color = new Color(color.r, color.g, color.b, 0f);
            p.Pos = pos; p.Vel = vel; p.Size = size; p.Size1 = size1; p.Life = life; p.Age = 0f; p.Alpha = alpha; p.Color = color; p.FadeIn = fadeIn;
            p.Wobble = fadeIn && sprite == Sprites.Circle ? Random.value * 6f : 0f;
            p.WobbleSpeed = Rand(0.8f, 2.2f);
            p.Rt.sizeDelta = new Vector2(size, size);
            p.Rt.localRotation = Quaternion.identity;
            _live.Add(p);
            return p;
        }

        Particle Create()
        {
            var go = NewObject("Particle");
            var rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            return new Particle { Rt = rt, Img = go.GetComponent<Image>() };
        }

        void Recycle(Particle p)
        {
            p.Rt.gameObject.SetActive(false);
            _pool.Push(p);
        }

        GameObject NewObject(string name)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)) { layer = 5 };
            go.transform.SetParent(_root, false);
            go.GetComponent<Image>().raycastTarget = false;
            return go;
        }

        static float Rand(float a, float b) => Random.Range(a, b);
    }
}
