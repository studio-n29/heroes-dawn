#nullable enable
using System;
using System.Collections;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;
using TMPro;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Joue la cinématique de lancement à partir des vidéos et des sous-titres de <see cref="CinematicVideoSequence"/>, avec des bandeaux noirs
    /// en haut et en bas de l'écran (même hauteur que l ancienne cinématique). Chaque pas finit par un fondu au noir ; un écran noir
    /// affiche sa narration en sous-titres. Les vidéos sont muettes et les voix off sont désactivées.
    /// Dans la popup d'intro, ses couches passent sous le toucher et « Passer » ; hors popup, il construit son propre canevas.
    /// <see cref="SkipShot"/> passe au pas suivant, <see cref="SkipAll"/> termine la cinématique.
    /// </summary>
    public sealed class CinematicVideoPlayer : MonoBehaviour
    {
        /// <summary>Hauteur de chaque bandeau, en fraction de l'écran (valeur de l ancienne cinématique).</summary>
        public const float BarHeight = 0.075f;

        const float PrepareTimeout = 20f;

        [SerializeField] float fadeSeconds = 0.8f;
        [SerializeField] bool playOnStart;

        /// <summary>Levé quand la cinématique est allée jusqu'au bout ou a été passée. Pas quand elle est arrêtée avec <see cref="Stop"/>.</summary>
        public event Action? Finished;

        public bool IsPlaying { get; private set; }

        /// <summary>Index du pas en cours dans <see cref="CinematicVideoSequence.Steps"/>, -1 avant le départ.</summary>
        public int CurrentShot { get; private set; } = -1;

        RectTransform _layer = null!;
        Canvas? _ownCanvas;
        RawImage _picture = null!;
        Image _fader = null!;
        VideoPlayer _video = null!;
        RenderTexture? _texture;
        Coroutine? _run;
        Action<float>? _mainTheme;
        bool _skipStep;
        bool _skipAll;
        bool _videoError;
        bool _frameReady;
        TMP_Text _subtitle = null!;
        RawImage _subtitleShade = null!;
        Texture2D? _subtitleShadeTexture;
        Material? _subtitleMaterial;
        UnityWebRequest? _download;
        RawImage _previousPicture = null!;
        RenderTexture? _previousTexture;
        CinematicVideoStep? _activeStep;
        float _shotStartedAt;

        void Awake() => BuildInterface();

        void Start()
        {
            if (playOnStart) Play(null);
        }

        void OnDisable()
        {
            if (IsPlaying) Stop();
        }

        void LateUpdate()
        {
            if (IsPlaying && _activeStep != null)
            {
                _subtitle.text = _activeStep.CaptionAt(Time.realtimeSinceStartup - _shotStartedAt);
                // Continue a subtle camera drift over the held last frame instead of
                // stretching a 24 fps clip into a visibly stepped slow-motion clip.
                float progress = Mathf.Clamp01((Time.realtimeSinceStartup - _shotStartedAt) / _activeStep.Seconds);
                float inset = 0.018f * (CurrentShot % 2 == 0 ? progress : 1f - progress);
                _picture.uvRect = new Rect(inset, inset, 1f - 2f * inset, 1f - 2f * inset);
            }
            // Do not leave a dark patch over the picture while a shot is loading.
            _subtitleShade.enabled = !string.IsNullOrEmpty(_subtitle.text);
        }

        void OnDestroy()
        {
            if (_previousTexture != null) { _previousTexture.Release(); Destroy(_previousTexture); }
            if (_subtitleShadeTexture != null) Destroy(_subtitleShadeTexture);
            if (_subtitleMaterial != null) Destroy(_subtitleMaterial);
            if (_video != null)
            {
                _video.errorReceived -= OnVideoError;
                _video.frameReady -= OnFrameReady;
            }
            if (_texture == null) return;
            _texture.Release();
            Destroy(_texture);
        }

        /// <summary>Lance la cinématique depuis le début. <paramref name="mainTheme"/> règle le thème principal (0 coupé, 1 plein). Sans effet si elle joue déjà.</summary>
        public void Play(Action<float>? mainTheme)
        {
            if (IsPlaying) return;
            IsPlaying = true;
            _mainTheme = mainTheme;
            _run = StartCoroutine(Run());
        }

        /// <summary>Joue la cinématique jusqu'à sa fin ou jusqu'à « Passer ». Si <paramref name="ct"/> est annulé, elle est arrêtée.</summary>
        public async UniTask PlayAsync(Action<float>? mainTheme, CancellationToken ct)
        {
            Play(mainTheme);
            try
            {
                await UniTask.WaitUntil(() => !IsPlaying, cancellationToken: ct);
            }
            finally
            {
                if (IsPlaying) Stop();
            }
        }

        /// <summary>Passe au pas suivant (toucher de l'écran).</summary>
        public void SkipShot() => _skipStep = true;

        /// <summary>Termine la cinématique (bouton « Passer »).</summary>
        public void SkipAll()
        {
            _skipAll = true;
            _skipStep = true;
        }

        /// <summary>Arrête la cinématique et revient à l'écran noir, sans lever <see cref="Finished"/>.</summary>
        public void Stop()
        {
            _download?.Abort();
            StopAllCoroutines();
            _download?.Dispose();
            _download = null;
            _run = null;
            _video.Stop();
            _activeStep = null;
            _previousPicture.enabled = false;
            _subtitle.text = "";
            _mainTheme?.Invoke(1f);
            SetFade(1f);
            IsPlaying = false;
        }

        IEnumerator Run()
        {
            _skipAll = false;
            _previousPicture.enabled = false;
            SetFade(1f);
            _mainTheme?.Invoke(1f);

            var steps = CinematicVideoSequence.Steps;
            for (int i = 0; i < steps.Count && !_skipAll; i++)
            {
                CurrentShot = i;
                _skipStep = false;
                var step = steps[i];
                yield return PlayShot(step);
            }

            _video.Stop();
            _activeStep = null;
            _subtitle.text = "";
            _previousPicture.enabled = false;
            SetFade(1f);
            IsPlaying = false;
            _run = null;
            _mainTheme?.Invoke(1f);
            Finished?.Invoke();
        }

        IEnumerator PlayShot(CinematicVideoStep step)
        {
            string path = UnityEngine.Application.streamingAssetsPath.TrimEnd('/') + "/" + CinematicVideoSequence.VideoFolder + "/" + step.Video + ".mp4";
            // Android StreamingAssets live inside the APK, not on the filesystem.
            // Extract just the current shot to a reusable cache file for the native decoder.
            if (path.StartsWith("jar:", StringComparison.OrdinalIgnoreCase))
            {
                string cached = Path.Combine(UnityEngine.Application.temporaryCachePath, "launch-cinematic.mp4");
                using (var request = UnityWebRequest.Get(path))
                {
                    _download = request;
                    try
                    {
                    request.downloadHandler = new DownloadHandlerFile(cached) { removeFileOnAbort = true };
                    request.timeout = 30;
                    var operation = request.SendWebRequest();
                    while (!operation.isDone && !_skipStep && !_skipAll) yield return null;
                    if (_skipStep || _skipAll) { request.Abort(); yield break; }
                    if (request.result != UnityWebRequest.Result.Success)
                    {
                        Debug.LogWarning($"Cinématique : {path} : {request.error}");
                        yield break;
                    }
                    }
                    finally { _download = null; }
                }
                path = cached;
            }
            if (!File.Exists(path))
            {
                Debug.LogError($"Vidéo de cinématique introuvable : {path}");
                yield break;
            }

            _videoError = false;
            _frameReady = false;
            _video.url = new Uri(path).AbsoluteUri;
            _video.Prepare();
            float waited = 0f;
            while (!_video.isPrepared && !_videoError && !_skipStep && !_skipAll && waited < PrepareTimeout)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }

            if (_skipStep || _skipAll) { _video.Stop(); yield break; }
            if (!_video.isPrepared || _videoError)
            {
                Debug.LogWarning($"Vidéo de cinématique non prête : {path}");
                _video.Stop();
                yield break;
            }

            float shotDuration = CinematicVideoSequence.ShotDurationForVideo(_video.length, step.Seconds);
            float fadeDuration = Mathf.Clamp(fadeSeconds, 0f, shotDuration / 2f);
            if (_video.canSetPlaybackSpeed)
                _video.playbackSpeed = CinematicVideoSequence.PlaybackSpeedForDuration(_video.length, shotDuration);
            _video.Play();
            waited = 0f;
            while (!_frameReady && !_videoError && !_skipStep && !_skipAll && waited < PrepareTimeout)
            {
                waited += Time.unscaledDeltaTime;
                yield return null;
            }
            if (!_frameReady || _videoError || _skipStep || _skipAll)
            {
                _video.Stop();
                yield break;
            }
            // Preparation/first-frame latency is not part of the visible shot's duration.
            _video.Pause();
            _activeStep = step;
            _shotStartedAt = Time.realtimeSinceStartup;
            _picture.uvRect = new Rect(0f, 0f, 1f, 1f);
            _subtitle.text = step.Caption ?? "";

            _video.Play();
            if (_previousPicture.enabled) yield return Dissolve(fadeDuration);
            else yield return Fade(0f);
            yield return Wait(Mathf.Max(0f, shotDuration - fadeDuration - (step.FadeToBlack ? fadeDuration : 0f)));
            _activeStep = null;
            if (step.FadeToBlack || _skipAll)
            {
                yield return Fade(1f);
                _subtitle.text = "";
                _previousPicture.enabled = false;
            }
            else
            {
                // Freeze the outgoing image while the next clip prepares, then dissolve
                // over its first decoded frame. One native decoder is enough on mobile.
                Graphics.Blit(_texture, _previousTexture);
                _previousPicture.uvRect = _picture.uvRect;
                _previousPicture.color = Color.white;
                _previousPicture.enabled = true;
            }

            _video.Stop();
        }

        IEnumerator Dissolve(float seconds)
        {
            SetFade(0f);
            for (float t = 0f; t < seconds && !_skipAll; t += Time.unscaledDeltaTime)
            {
                _previousPicture.color = new Color(1f, 1f, 1f, 1f - Mathf.SmoothStep(0f, 1f, t / seconds));
                yield return null;
            }
            _previousPicture.enabled = false;
        }

        void OnVideoError(VideoPlayer source, string message)
        {
            _videoError = true;
            Debug.LogWarning($"Cinématique : {source.url} : {message}");
        }

        void OnFrameReady(VideoPlayer source, long frame) => _frameReady = true;

        /// <summary>Attend <paramref name="seconds"/>, sauf si le joueur passe le pas ou la cinématique.</summary>
        IEnumerator Wait(float seconds)
        {
            for (float t = 0f; t < seconds && !_skipStep && !_skipAll; t += Time.unscaledDeltaTime)
                yield return null;
        }

        IEnumerator Fade(float target)
        {
            float start = _fader.color.a;
            if (fadeSeconds <= 0f)
            {
                SetFade(target);
                yield break;
            }

            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                SetFade(Mathf.Lerp(start, target, t / fadeSeconds));
                yield return null;
            }

            SetFade(target);
        }

        /// <summary>Règle l'opacité du voile noir (la couleur reste noire).</summary>
        void SetFade(float alpha)
        {
            var color = _fader.color;
            color.a = alpha;
            _fader.color = color;
        }

        void BuildInterface()
        {
            // Dans la popup d'intro, ses couches vont dans son propre conteneur (sous le toucher et « Passer ») ;
            // hors popup, il ouvre un canevas plein écran.
            if (GetComponentInParent<Canvas>() != null)
            {
                _layer = (RectTransform)transform;
            }
            else
            {
                var canvasGo = new GameObject("CinematicVideoCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
                canvasGo.transform.SetParent(transform, false);
                _ownCanvas = canvasGo.GetComponent<Canvas>();
                _ownCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _ownCanvas.sortingOrder = 900;
                var scaler = canvasGo.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
                _layer = (RectTransform)canvasGo.transform;
            }

            _picture = CreateFullscreen<RawImage>("Image");
            var aspect = _picture.gameObject.AddComponent<AspectRatioFitter>();
            aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspect.aspectRatio = 16f / 9f;
            _previousPicture = CreateFullscreen<RawImage>("ImagePrecedente");
            var previousAspect = _previousPicture.gameObject.AddComponent<AspectRatioFitter>();
            previousAspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            previousAspect.aspectRatio = 16f / 9f;
            _previousPicture.enabled = false;
            _fader = CreateFullscreen<Image>("Fondu");
            _fader.color = Color.black;
            var top = CreateBar("BandeauHaut", top: true);
            var bottom = CreateBar("BandeauBas", top: false);

            // Ordre d'affichage : image, voile, bandeaux, puis le reste du conteneur (toucher, « Passer »).
            Graphic[] layers = { _picture, _previousPicture, _fader, top, bottom };
            for (int i = 0; i < layers.Length; i++) layers[i].transform.SetSiblingIndex(i);

            _texture = new RenderTexture(1920, 1080, 0);
            _texture.Create();
            _picture.texture = _texture;
            _previousTexture = new RenderTexture(1920, 1080, 0);
            _previousTexture.Create();
            _previousPicture.texture = _previousTexture;

            _video = gameObject.AddComponent<VideoPlayer>();
            _video.playOnAwake = false;
            _video.isLooping = false;
            _video.renderMode = VideoRenderMode.RenderTexture;
            _video.targetTexture = _texture;
            _video.audioOutputMode = VideoAudioOutputMode.None;
            _video.waitForFirstFrame = true;
            _video.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;
            _video.aspectRatio = VideoAspectRatio.FitInside;
            _video.sendFrameReadyEvents = true;
            _video.errorReceived += OnVideoError;
            _video.frameReady += OnFrameReady;

            _subtitleShade = CreateFullscreen<RawImage>("OmbreSousTitres");
            _subtitleShadeTexture = CreateSubtitleShade();
            _subtitleShade.texture = _subtitleShadeTexture;
            _subtitleShade.rectTransform.anchorMin = new Vector2(0.06f, BarHeight);
            _subtitleShade.rectTransform.anchorMax = new Vector2(0.94f, 0.36f);
            _subtitleShade.enabled = false;

            _subtitle = CreateFullscreen<TextMeshProUGUI>("SousTitres");
            _subtitle.font = Ui.BodyFont;
            _subtitle.fontSize = 34f;
            _subtitle.alignment = TextAlignmentOptions.Bottom;
            _subtitle.color = Color.white;
            // A private material keeps the shadow and outline local to subtitles.
            _subtitleMaterial = new Material(_subtitle.fontSharedMaterial);
            _subtitleMaterial.EnableKeyword("UNDERLAY_ON");
            _subtitleMaterial.SetColor("_UnderlayColor", new Color(0f, 0f, 0f, 0.95f));
            _subtitleMaterial.SetFloat("_UnderlayOffsetX", 0.4f);
            _subtitleMaterial.SetFloat("_UnderlayOffsetY", -0.6f);
            _subtitleMaterial.SetFloat("_UnderlayDilate", 0.25f);
            _subtitleMaterial.SetFloat("_UnderlaySoftness", 0.35f);
            _subtitle.fontSharedMaterial = _subtitleMaterial;
            _subtitle.outlineColor = Color.black;
            _subtitle.outlineWidth = 0.16f;
            _subtitle.extraPadding = true;
            _subtitle.richText = false;
            _subtitle.rectTransform.anchorMin = new Vector2(0.1f, BarHeight + 0.02f);
            _subtitle.rectTransform.anchorMax = new Vector2(0.9f, 0.32f);

        }

        static Texture2D CreateSubtitleShade()
        {
            const int width = 128, height = 64;
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "CinematicSubtitleShade",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float v = y / (float)(height - 1);
                float vertical = Mathf.SmoothStep(0f, 1f, v / 0.07f)
                    * (1f - Mathf.SmoothStep(0f, 1f, (v - 0.65f) / 0.35f));
                for (int x = 0; x < width; x++)
                {
                    float u = x / (float)(width - 1);
                    float horizontal = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f - u) / 0.12f);
                    pixels[y * width + x] = new Color(0f, 0f, 0f, 0.82f * vertical * horizontal);
                }
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        T CreateFullscreen<T>(string name) where T : Graphic
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(T));
            go.transform.SetParent(_layer, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var graphic = go.GetComponent<T>();
            graphic.raycastTarget = false;
            return graphic;
        }

        Image CreateBar(string name, bool top)
        {
            var bar = CreateFullscreen<Image>(name);
            bar.color = Color.black;
            var rect = (RectTransform)bar.transform;
            rect.anchorMin = new Vector2(0f, top ? 1f - BarHeight : 0f);
            rect.anchorMax = new Vector2(1f, top ? 1f : BarHeight);
            return bar;
        }
    }
}
