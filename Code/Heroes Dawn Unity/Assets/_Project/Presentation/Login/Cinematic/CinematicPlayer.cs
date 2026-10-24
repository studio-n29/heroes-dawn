#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Lecteur de la cinématique de lancement : joue les plans de <see cref="CinematicScript"/> en pilotant la caméra (poussée, recul, panoramique,
    /// recadrage) sur des illustrations fixes, avec ambiance (particules, lumières), étalonnage par époque, transitions (fondu, fondu au noir,
    /// brûlure dorée), titres, voix off sous-titrée, nappes musicales et effets sonores. Aucun clip vidéo : tout est calculé en temps réel.
    /// Chaque plan dure au moins le temps de sa voix (« caler les images sur la voix »).
    /// </summary>
    public sealed class CinematicPlayer : MonoBehaviour
    {
        sealed class Slot
        {
            public RectTransform Root = null!;
            public CanvasGroup Group = null!;
            public RectTransform Art = null!;
            public Image ArtImage = null!;
            public RectTransform Parts = null!;
            public RectTransform Scene = null!;
            public ProcScene? Proc;
            public readonly List<GameObject> Extras = new List<GameObject>();
            public readonly List<(Image Image, CinematicGlow Glow)> Glows = new List<(Image, CinematicGlow)>();
            public CinematicShot? Shot;
            public Vector2 ArtSize;
            public float Time;
            public float Duration;
        }

        const float BarHeight = 0.075f;

        RectTransform _viewport = null!;
        RectTransform _fxLayer = null!;
        Image _grade = null!, _fade = null!, _burn = null!, _titleLine = null!;
        RawImage _grain = null!;
        TextMeshProUGUI _subtitle = null!, _title = null!;
        readonly Slot[] _slots = new Slot[2];
        int _front;
        CinematicParticles _particles = null!;
        AudioSource _voice = null!, _sfx = null!, _bedA = null!, _bedB = null!;
        bool _bedOnA = true;
        string _bedName = "";
        float _bedVolume, _bedTarget;
        bool _built, _skipShot, _skipAll;
        float _grainTimer;

        public int CurrentShot { get; private set; } = -1;
        public string CurrentSubtitle => _subtitle != null ? _subtitle.text : "";
        public string CurrentTitle => _title != null ? _title.text : "";
        public bool Playing { get; private set; }

        /// <summary>Passe au plan suivant (toucher de l'écran).</summary>
        public void SkipShot() => _skipShot = true;

        /// <summary>Termine la cinématique (bouton Passer).</summary>
        public void SkipAll() { _skipAll = true; _skipShot = true; }

        // ---------------------------------------------------------------- Construction

        void Build()
        {
            if (_built) return;
            _built = true;
            var rt = (RectTransform)transform;
            var bg = Child(rt, "Black", Color.black);
            Stretch(bg.rectTransform);
            var vp = new GameObject("Viewport", typeof(RectTransform), typeof(RectMask2D)) { layer = 5 };
            vp.transform.SetParent(rt, false);
            _viewport = (RectTransform)vp.transform;
            Stretch(_viewport);
            for (int i = 0; i < 2; i++) _slots[i] = BuildSlot(i);
            var fx = new GameObject("Fx", typeof(RectTransform)) { layer = 5 };
            fx.transform.SetParent(rt, false);
            _fxLayer = (RectTransform)fx.transform;
            Stretch(_fxLayer);
            _particles = new CinematicParticles(_fxLayer);
            _grade = Child(rt, "Grade", Color.clear); Stretch(_grade.rectTransform);
            var vig = Child(rt, "Vignette", new Color(0f, 0f, 0f, 0.62f)); vig.sprite = CinematicSprites.Vignette; Stretch(vig.rectTransform);
            var grainGo = new GameObject("Grain", typeof(RectTransform), typeof(RawImage)) { layer = 5 };
            grainGo.transform.SetParent(rt, false);
            _grain = grainGo.GetComponent<RawImage>();
            _grain.texture = CinematicSprites.Grain;
            _grain.color = new Color(1f, 1f, 1f, 0f);
            _grain.raycastTarget = false;
            Stretch((RectTransform)grainGo.transform);
            foreach (bool top in new[] { true, false })
            {
                var bar = Child(rt, top ? "BarTop" : "BarBottom", Color.black);
                var brt = bar.rectTransform;
                brt.anchorMin = new Vector2(0f, top ? 1f - BarHeight : 0f);
                brt.anchorMax = new Vector2(1f, top ? 1f : BarHeight);
                brt.offsetMin = brt.offsetMax = Vector2.zero;
            }
            _title = Text(rt, "Title", Ui.TitleFont, 150, new Color(1f, 0.82f, 0.4f, 0f), TextAlignmentOptions.MidlineLeft);
            _title.fontSharedMaterial = Ui.InkedMaterial(Ui.TitleFont);
            var trt = _title.rectTransform;
            trt.anchorMin = new Vector2(0.07f, 0.52f); trt.anchorMax = new Vector2(0.7f, 0.74f); trt.offsetMin = trt.offsetMax = Vector2.zero;
            _titleLine = Child(rt, "TitleLine", new Color(1f, 0.82f, 0.4f, 0f));
            var lrt = _titleLine.rectTransform;
            lrt.anchorMin = lrt.anchorMax = new Vector2(0.07f, 0.5f); lrt.pivot = new Vector2(0f, 0.5f); lrt.sizeDelta = new Vector2(260f, 5f); lrt.anchoredPosition = Vector2.zero;
            _subtitle = Text(rt, "Subtitle", Ui.BodyFont, 54, new Color(1f, 1f, 1f, 0f), TextAlignmentOptions.Center);
            _subtitle.fontSharedMaterial = Ui.InkedMaterial(Ui.BodyFont);
            var srt = _subtitle.rectTransform;
            srt.anchorMin = new Vector2(0.1f, BarHeight + 0.02f); srt.anchorMax = new Vector2(0.9f, BarHeight + 0.2f); srt.offsetMin = srt.offsetMax = Vector2.zero;
            _fade = Child(rt, "Fade", new Color(0f, 0f, 0f, 1f)); Stretch(_fade.rectTransform);
            _burn = Child(rt, "Burn", new Color(1f, 0.82f, 0.4f, 0f)); Stretch(_burn.rectTransform);
            _voice = AddSource(false); _sfx = AddSource(false); _bedA = AddSource(true); _bedB = AddSource(true);
            // Les contrôles du prefab (toucher, Passer) restent au-dessus de la cinématique.
            foreach (Transform c in rt) if (c.name == "Tap" || c.name == "Skip") c.SetAsLastSibling();
        }

        Slot BuildSlot(int index)
        {
            var go = new GameObject("Slot" + index, typeof(RectTransform), typeof(CanvasGroup)) { layer = 5 };
            go.transform.SetParent(_viewport, false);
            var s = new Slot { Root = (RectTransform)go.transform, Group = go.GetComponent<CanvasGroup>() };
            Stretch(s.Root);
            s.Group.alpha = 0f;
            s.Group.blocksRaycasts = false;
            s.Scene = NewRect(s.Root, "Scene"); Stretch(s.Scene);
            var art = Child(s.Root, "Art", Color.white);
            s.Art = art.rectTransform; s.ArtImage = art; art.enabled = false;
            s.Art.anchorMin = s.Art.anchorMax = new Vector2(0.5f, 0.5f);
            s.Parts = NewRect(s.Root, "Parts"); Stretch(s.Parts);
            return s;
        }

        AudioSource AddSource(bool loop)
        {
            var a = gameObject.AddComponent<AudioSource>();
            a.playOnAwake = false; a.loop = loop; a.spatialBlend = 0f;
            return a;
        }

        // ---------------------------------------------------------------- Lecture

        /// <summary>
        /// Joue les plans de <paramref name="startShot"/> à <paramref name="endShot"/> (inclus ; -1 : jusqu'au dernier). <paramref name="mainTheme"/> règle
        /// le niveau (0 à 1) du thème principal du jeu, tu pendant la cinématique et rouvert à la dernière séquence.
        /// </summary>
        public async UniTask PlayAsync(IReadOnlyList<CinematicShot> shots, Action<float>? mainTheme, CancellationToken ct, int startShot = 0, int endShot = -1)
        {
            Build();
            Playing = true;
            _skipAll = false;
            mainTheme?.Invoke(0f);
            _fade.color = Color.black;
            if (endShot < 0 || endShot >= shots.Count) endShot = shots.Count - 1;
            try
            {
                for (int i = startShot; i <= endShot && !_skipAll; i++)
                    await RunShot(shots[i], i, mainTheme, ct);
            }
            finally
            {
                _voice.Stop(); _sfx.Stop();
                _particles.Clear();
                foreach (var s in _slots) ClearSlot(s);
                Playing = false;
            }
            await Fade(_fade, 1f, _skipAll ? 0.5f : 1.2f, ct);
            await StopBeds(0.8f, ct);
            await UniTask.Delay(250, DelayType.UnscaledDeltaTime, cancellationToken: ct);
        }

        async UniTask RunShot(CinematicShot shot, int index, Action<float>? mainTheme, CancellationToken ct)
        {
            CurrentShot = index;
            _skipShot = false;
            var prev = _slots[_front];
            int next = 1 - _front;
            var slot = _slots[next];
            ClearSlot(slot);
            Prepare(slot, shot);
            SetGrade(shot.Grade);

            var voiceClip = shot.Voice ? Resources.Load<AudioClip>("Cinematic/Voice/vo_" + shot.Id) : null;
            float voiceLen = voiceClip != null ? voiceClip.length : 0f;
            float voiceStart = 0.4f;
            slot.Duration = Mathf.Max(shot.Seconds, voiceStart + voiceLen + 0.4f);
            string subtitle = shot.Voice ? Loc.T("cinematic.vo." + shot.Id) : "";
            ApplyMusic(shot, mainTheme);

            // Entrée dans le plan.
            switch (shot.In)
            {
                case CinematicIn.Cut:
                    SwapIn(slot, prev);
                    break;
                case CinematicIn.FromBlack:
                    if (_fade.color.a < 0.99f) await Fade(_fade, 1f, 0.35f, ct, () => Tick(slot, prev, 0f, false));
                    SwapIn(slot, prev);
                    break;
                case CinematicIn.GoldBurn:
                case CinematicIn.WhiteFlash:
                    _burn.color = shot.In == CinematicIn.GoldBurn ? new Color(1f, 0.82f, 0.4f, 0f) : new Color(1f, 1f, 1f, 0f);
                    await Fade(_burn, 1f, 0.22f, ct, () => Tick(slot, prev, 0f, false));
                    SwapIn(slot, prev);
                    break;
                default:
                    slot.Group.alpha = 0f;
                    break;
            }
            _front = next;
            _particles.Set(shot.Fx, shot.SmokeColor);

            float elapsed = 0f;
            bool voiceStarted = false, sfxDone = shot.Sfx == null;
            float fadeIn = shot.In == CinematicIn.FromBlack ? 1.2f : shot.In == CinematicIn.GoldBurn || shot.In == CinematicIn.WhiteFlash ? 0.9f : 1.1f;
            float titleStart = 0.5f;
            _title.text = shot.Title ?? "";
            while (elapsed < slot.Duration && !_skipShot && !ct.IsCancellationRequested)
            {
                float dt = Time.unscaledDeltaTime;
                elapsed += dt;
                slot.Time = elapsed;
                if (!voiceStarted && elapsed >= voiceStart && voiceClip != null) { _voice.clip = voiceClip; _voice.Play(); voiceStarted = true; }
                if (!sfxDone && elapsed >= shot.SfxAt) { PlaySfx(shot); sfxDone = true; }

                // Fondus d'entrée.
                if (shot.In == CinematicIn.Dissolve) { slot.Group.alpha = Mathf.Clamp01(elapsed / fadeIn); prev.Group.alpha = 1f - slot.Group.alpha; }
                else if (shot.In == CinematicIn.FromBlack) _fade.color = new Color(0f, 0f, 0f, 1f - Mathf.Clamp01(elapsed / fadeIn));
                else if (shot.In == CinematicIn.GoldBurn || shot.In == CinematicIn.WhiteFlash) _burn.color = new Color(_burn.color.r, _burn.color.g, _burn.color.b, 1f - Mathf.Clamp01(elapsed / fadeIn));
                if (shot.In == CinematicIn.Dissolve && elapsed > fadeIn) prev.Group.alpha = 0f;

                // Fondu au noir de fin de séquence.
                float left = slot.Duration - elapsed;
                if (shot.FadeOut && left < 0.9f) _fade.color = new Color(0f, 0f, 0f, 1f - Mathf.Clamp01(left / 0.9f));

                Tick(slot, prev, dt, true);
                UpdateText(shot, elapsed, voiceStarted, voiceLen, voiceStart, subtitle, titleStart);
                UpdateBeds(dt, voiceStarted && _voice.isPlaying);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _voice.Stop();
            prev.Group.alpha = 0f;
            if (shot.In != CinematicIn.FromBlack && !shot.FadeOut && _fade.color.a > 0.01f && _skipShot) _fade.color = new Color(0f, 0f, 0f, 0f);
        }

        void SwapIn(Slot slot, Slot prev)
        {
            slot.Group.alpha = 1f;
            prev.Group.alpha = 0f;
        }

        void Tick(Slot slot, Slot prev, float dt, bool particles)
        {
            ApplyCamera(slot);
            if (prev.Shot != null && prev.Group.alpha > 0.01f) { prev.Time += dt; ApplyCamera(prev); }
            if (slot.Proc != null) slot.Proc.Tick(slot.Time);
            foreach (var (img, glow) in slot.Glows)
            {
                float k = slot.Duration > 0f ? Mathf.Clamp01(slot.Time / slot.Duration) : 0f;
                var c = glow.ColorTo.HasValue ? Color.Lerp(glow.Color, glow.ColorTo.Value, Mathf.SmoothStep(0f, 1f, k)) : glow.Color;
                float pulse = 0.75f + 0.25f * Mathf.Sin(slot.Time * glow.Pulse * 2f);
                c.a *= pulse * Mathf.Min(slot.Time / 0.8f, 1f);
                img.color = c;
                img.rectTransform.localScale = Vector3.one * (0.9f + 0.15f * pulse);
            }
            if (particles) _particles.Tick(dt);
            _grainTimer -= Time.unscaledDeltaTime;
            if (_grainTimer <= 0f)
            {
                _grainTimer = 0.07f;
                _grain.uvRect = new Rect(UnityEngine.Random.value, UnityEngine.Random.value, _viewport.rect.width / 128f, _viewport.rect.height / 128f);
            }
        }

        // ---------------------------------------------------------------- Mise en place d'un plan

        void ClearSlot(Slot s)
        {
            foreach (var o in s.Extras) if (o != null) Destroy(o);
            s.Extras.Clear();
            foreach (var (g, _) in s.Glows) if (g != null) Destroy(g.gameObject);
            s.Glows.Clear();
            s.Proc?.Destroy();
            s.Proc = null;
            s.Shot = null;
            s.Group.alpha = 0f;
            s.ArtImage.enabled = false;
            s.Time = 0f;
        }

        void Prepare(Slot s, CinematicShot shot)
        {
            s.Shot = shot;
            s.Time = 0f;
            var tint = GradeTint(shot.Grade);
            var vp = _viewport.rect.size;
            if (shot.Layout == CinematicLayout.Single && shot.Art != null)
            {
                var sprite = Resources.Load<Sprite>("Cinematic/Art/" + shot.Art);
                s.ArtImage.enabled = sprite != null;
                s.ArtImage.sprite = sprite;
                s.ArtImage.color = tint;
                if (sprite != null)
                {
                    var r = sprite.rect.size;
                    float k = Mathf.Max(vp.x / r.x, vp.y / r.y);
                    s.ArtSize = r * k;
                    s.Art.sizeDelta = s.ArtSize;
                }
                if (shot.Glows != null) foreach (var g in shot.Glows) s.Glows.Add((MakeGlow(s.Art, g, s.ArtSize), g));
            }
            else if (shot.Parts != null)
            {
                BuildParts(s, shot, tint, vp);
                if (shot.Glows != null) foreach (var g in shot.Glows) s.Glows.Add((MakeGlow(s.Parts, g, vp), g));
            }
            else
            {
                s.ArtSize = vp;
                if (shot.Glows != null) foreach (var g in shot.Glows) s.Glows.Add((MakeGlow(s.Scene, g, vp), g));
            }
            if (shot.Proc != CinematicProc.None)
            {
                s.Proc = ProcScene.Create(shot.Proc);
                s.Proc?.Attach(s.Scene);
                s.Scene.SetAsLastSibling();
            }
            if (s.Parts != null && shot.Parts == null) foreach (Transform c in s.Parts) Destroy(c.gameObject);
            ApplyCamera(s);
        }

        void BuildParts(Slot s, CinematicShot shot, Color tint, Vector2 vp)
        {
            foreach (Transform c in s.Parts) Destroy(c.gameObject);
            s.Parts.sizeDelta = Vector2.zero;
            int n = shot.Parts!.Length;
            for (int i = 0; i < n; i++)
            {
                var sprite = Resources.Load<Sprite>("Cinematic/Art/" + shot.Parts[i]);
                var holder = new GameObject("Part" + i, typeof(RectTransform), typeof(RectMask2D)) { layer = 5 };
                holder.transform.SetParent(s.Parts, false);
                var hr = (RectTransform)holder.transform;
                hr.anchorMin = new Vector2(i / (float)n, 0f);
                hr.anchorMax = new Vector2((i + 1) / (float)n, 1f);
                hr.offsetMin = hr.offsetMax = Vector2.zero;
                var img = Child(hr, "Image", tint);
                img.sprite = sprite;
                if (sprite != null)
                {
                    var r = sprite.rect.size;
                    float k = Mathf.Max(vp.x / n / r.x, vp.y / r.y) * 1.02f;
                    var size = r * k;
                    img.rectTransform.anchorMin = img.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                    img.rectTransform.sizeDelta = size;
                    float fx = shot.PartFocus != null && i < shot.PartFocus.Length ? shot.PartFocus[i] : 0.5f;
                    float range = Mathf.Max(0f, (size.x - vp.x / n) * 0.5f);
                    img.rectTransform.anchoredPosition = new Vector2(Mathf.Clamp((0.5f - fx) * size.x, -range, range), 0f);
                }
                // Fine bande sombre entre deux illustrations.
                if (i > 0) { var sep = Child(hr, "Sep", new Color(0f, 0f, 0f, 0.55f)); var sr = sep.rectTransform; sr.anchorMin = new Vector2(0f, 0f); sr.anchorMax = new Vector2(0f, 1f); sr.sizeDelta = new Vector2(6f, 0f); sr.anchoredPosition = Vector2.zero; }
            }
            s.ArtSize = vp;
        }

        Image MakeGlow(RectTransform parent, CinematicGlow g, Vector2 area)
        {
            var img = Child(parent, "Glow", g.Color);
            img.sprite = CinematicSprites.Glow;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = Vector2.one * g.Size;
            rt.anchoredPosition = new Vector2((g.Pos.x - 0.5f) * area.x, (0.5f - g.Pos.y) * area.y);
            return img;
        }

        void ApplyCamera(Slot s)
        {
            var shot = s.Shot;
            if (shot == null) return;
            float k = s.Duration > 0f ? Mathf.Clamp01(s.Time / s.Duration) : 0f;
            if (MotionSettings.Reduced) k = 1f;
            float e = Mathf.Lerp(k, k * k * (3f - 2f * k), 0.45f);
            float scale = Mathf.Lerp(shot.From.Scale, shot.To.Scale, e);
            var focus = Vector2.Lerp(shot.From.Focus, shot.To.Focus, e);
            var vp = _viewport.rect.size;
            if (shot.Layout == CinematicLayout.Single && shot.Art != null)
            {
                var size = s.ArtSize * scale;
                float rx = Mathf.Max(0f, (size.x - vp.x) * 0.5f), ry = Mathf.Max(0f, (size.y - vp.y) * 0.5f);
                s.Art.localScale = Vector3.one * scale;
                s.Art.anchoredPosition = new Vector2(Mathf.Clamp((0.5f - focus.x) * size.x, -rx, rx), Mathf.Clamp((focus.y - 0.5f) * size.y, -ry, ry));
            }
            else if (shot.Parts != null)
            {
                float rx = (scale - 1f) * vp.x * 0.5f, ry = (scale - 1f) * vp.y * 0.5f;
                s.Parts.localScale = Vector3.one * scale;
                s.Parts.anchoredPosition = new Vector2(Mathf.Clamp((0.5f - focus.x) * vp.x * scale, -rx, rx), Mathf.Clamp((focus.y - 0.5f) * vp.y * scale, -ry, ry));
            }
            s.Scene.localScale = Vector3.one * Mathf.Lerp(1f, 1.07f, e);
        }

        static Color GradeTint(CinematicGrade g)
        {
            switch (g)
            {
                case CinematicGrade.Archive: return new Color(0.86f, 0.76f, 0.6f, 1f);
                case CinematicGrade.Cold: return new Color(0.74f, 0.84f, 1f, 1f);
                case CinematicGrade.Hell: return new Color(1f, 0.86f, 0.82f, 1f);
                case CinematicGrade.Dawn: return new Color(1f, 0.95f, 0.82f, 1f);
                default: return Color.white;
            }
        }

        void SetGrade(CinematicGrade g)
        {
            switch (g)
            {
                case CinematicGrade.Archive: _grade.color = new Color(0.12f, 0.36f, 0.36f, 0.2f); _grain.color = new Color(1f, 1f, 1f, 0.11f); break;
                case CinematicGrade.Cold: _grade.color = new Color(0.05f, 0.14f, 0.34f, 0.2f); _grain.color = new Color(1f, 1f, 1f, 0.05f); break;
                case CinematicGrade.Hell: _grade.color = new Color(0.55f, 0.03f, 0.08f, 0.13f); _grain.color = new Color(1f, 1f, 1f, 0.04f); break;
                case CinematicGrade.Dawn: _grade.color = new Color(1f, 0.7f, 0.25f, 0.1f); _grain.color = new Color(1f, 1f, 1f, 0.03f); break;
                default: _grade.color = Color.clear; _grain.color = new Color(1f, 1f, 1f, 0.03f); break;
            }
        }

        // ---------------------------------------------------------------- Textes

        void UpdateText(CinematicShot shot, float t, bool voiceStarted, float voiceLen, float voiceStart, string subtitle, float titleStart)
        {
            // Sous-titre : visible pendant la voix.
            float a = 0f;
            if (voiceStarted && !string.IsNullOrEmpty(subtitle))
            {
                float local = t - voiceStart;
                a = Mathf.Clamp01(local / 0.3f) * Mathf.Clamp01((voiceLen + 0.4f - local) / 0.4f);
                if (_subtitle.text != subtitle) _subtitle.text = subtitle;
            }
            _subtitle.color = new Color(1f, 1f, 1f, a);
            // Titre d'époque : apparaît, tient, disparaît.
            float ta = 0f;
            if (!string.IsNullOrEmpty(shot.Title)) ta = Mathf.Clamp01((t - titleStart) / 0.7f) * Mathf.Clamp01((titleStart + 3.6f - t) / 0.8f);
            _title.color = new Color(1f, 0.82f, 0.4f, ta);
            _titleLine.color = new Color(1f, 0.82f, 0.4f, ta * 0.9f);
            _titleLine.rectTransform.sizeDelta = new Vector2(Mathf.Lerp(40f, 260f, Mathf.Clamp01((t - titleStart) / 1.2f)), 5f);
        }

        // ---------------------------------------------------------------- Audio

        void PlaySfx(CinematicShot shot)
        {
            var clip = Resources.Load<AudioClip>("Cinematic/Audio/" + shot.Sfx);
            if (clip != null) _sfx.PlayOneShot(clip, shot.SfxVolume);
        }

        void ApplyMusic(CinematicShot shot, Action<float>? mainTheme)
        {
            switch (shot.Music)
            {
                case CinematicMusic.Keep: return;
                case CinematicMusic.Silence: _bedTarget = 0f; return;
                case CinematicMusic.MainTheme: _bedTarget = 0f; mainTheme?.Invoke(1f); return;
                case CinematicMusic.Cold: SetBed("mus_cold", 0.34f); return;
                case CinematicMusic.War: SetBed("mus_war", 0.42f); return;
                case CinematicMusic.Piano: SetBed("mus_piano", 0.5f); return;
            }
        }

        void SetBed(string name, float volume)
        {
            _bedTarget = volume;
            if (_bedName == name) return;
            _bedName = name;
            var clip = Resources.Load<AudioClip>("Cinematic/Audio/" + name);
            var from = _bedOnA ? _bedA : _bedB;
            var to = _bedOnA ? _bedB : _bedA;
            _bedOnA = !_bedOnA;
            to.clip = clip; to.volume = 0f;
            if (clip != null) to.Play();
            _fading = from;

        }

        AudioSource? _fading;

        void UpdateBeds(float dt, bool voicePlaying)
        {
            float target = _bedTarget * (voicePlaying ? 0.55f : 1f);
            var cur = _bedOnA ? _bedA : _bedB;
            cur.volume = Mathf.MoveTowards(cur.volume, target, dt * 0.25f);
            if (_fading != null)
            {
                _fading.volume = Mathf.MoveTowards(_fading.volume, 0f, dt * 0.3f);
                if (_fading.volume <= 0.001f) { _fading.Stop(); _fading = null; }
            }
        }

        async UniTask StopBeds(float seconds, CancellationToken ct)
        {
            _bedTarget = 0f;
            float t = 0f;
            while (t < seconds && !ct.IsCancellationRequested)
            {
                t += Time.unscaledDeltaTime;
                _bedA.volume = Mathf.MoveTowards(_bedA.volume, 0f, Time.unscaledDeltaTime * 1.2f);
                _bedB.volume = Mathf.MoveTowards(_bedB.volume, 0f, Time.unscaledDeltaTime * 1.2f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _bedA.Stop(); _bedB.Stop();
            _bedName = "";
        }

        void OnDestroy()
        {
            if (_bedA != null) _bedA.Stop();
            if (_bedB != null) _bedB.Stop();
        }

        // ---------------------------------------------------------------- Aides

        async UniTask Fade(Image img, float to, float seconds, CancellationToken ct, Action? each = null)
        {
            float from = img.color.a, t = 0f;
            while (t < seconds && !ct.IsCancellationRequested)
            {
                t += Time.unscaledDeltaTime;
                var c = img.color; c.a = Mathf.Lerp(from, to, Mathf.Clamp01(t / seconds)); img.color = c;
                each?.Invoke();
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            var f = img.color; f.a = to; img.color = f;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one; rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        static RectTransform NewRect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform)) { layer = 5 };
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static Image Child(Transform parent, string name, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)) { layer = 5 };
            go.transform.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        static TextMeshProUGUI Text(Transform parent, string name, TMP_FontAsset font, int size, Color color, TextAlignmentOptions align)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)) { layer = 5 };
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<TextMeshProUGUI>();
            t.font = font; t.fontSize = size; t.color = color; t.alignment = align; t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            return t;
        }
    }
}
