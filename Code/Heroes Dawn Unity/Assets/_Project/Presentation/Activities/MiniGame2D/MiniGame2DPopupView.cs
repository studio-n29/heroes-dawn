#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Fenêtre commune des mini-jeux 2D (prefab Popups/MiniGame2DPopup) : bandeau score / temps / vies, grande zone tactile, joystick flottant, Abandonner.
    /// Le jeu lui-même vit dans un <see cref="IMiniGameDriver"/>. La fenêtre rend toujours la main : fin de partie, abandon ou annulation.
    /// </summary>
    public sealed class MiniGame2DPopupView : MonoBehaviour, IMiniGame2DPopupView
    {
        [SerializeField] TMP_Text title = null!;
        [SerializeField] TMP_Text scoreText = null!;
        [SerializeField] TMP_Text timeText = null!;
        [SerializeField] TMP_Text livesText = null!;
        [SerializeField] TMP_Text overlay = null!;
        [SerializeField] RectTransform field = null!;
        [SerializeField] Image backgroundImage = null!;
        [SerializeField] UiButton abandon = null!;
        [SerializeField] RectTransform window = null!;
        [SerializeField] CanvasGroup group = null!;
        [SerializeField] RectTransform stickRing = null!;
        [SerializeField] RectTransform stickKnob = null!;
        [SerializeField] RescuePanelView rescuePanel = null!;

        MiniGame2DRequest? _request;
        IMiniGameDriver? _driver;
        MiniGameField? _input;
        UniTaskCompletionSource<ActivityResult>? _done;
        bool _started, _finished;

        public bool Cancelable => false;
        public void RequestCancel() { }

        public RectTransform Field => field;
        public UiButton AbandonButton => abandon;
        public bool Started => _started;
        public bool Finished => _finished;
        public IMiniGameDriver? Driver => _driver;
        public MiniGameField Input => _input != null ? _input : (_input = EnsureInput());

        MiniGameField EnsureInput() => field.gameObject.GetComponent<MiniGameField>() ?? field.gameObject.AddComponent<MiniGameField>();

        void Awake()
        {
            _input = EnsureInput();
            abandon.Button.onClick.AddListener(OnAbandon);
        }

        void OnDestroy() => _driver?.Dispose();

        public async UniTask<ActivityResult> PlayAsync(MiniGame2DRequest r, CancellationToken ct)
        {
            _driver?.Dispose();
            _request = r;
            _driver = r.Driver;
            _started = false; _finished = false;
            Input.ResetState();
            title.text = r.Title;
            abandon.SetLabel(r.Abandon);
            backgroundImage.sprite = r.Background;
            backgroundImage.color = r.Background != null ? r.BackgroundTint : new Color(0f, 0f, 0f, 0f);
            // Un pilote qui a besoin d'un panneau du prefab (Ingénierie de secours) le reçoit avant de construire ; sinon le panneau reste masqué.
            var panelUser = _driver as IRescuePanelUser;
            rescuePanel.gameObject.SetActive(panelUser != null);
            panelUser?.Bind(rescuePanel);
            _driver.Build(field);
            overlay.gameObject.SetActive(true);
            overlay.text = r.Instructions + "\n\n<size=70%><color=#FFCF33>" + r.Start + "</color></size>";
            overlay.transform.SetAsLastSibling();
            stickRing.gameObject.SetActive(false);
            stickKnob.gameObject.SetActive(false);
            group.alpha = 0f;
            Tween.Fade(group, 1f, 0.15f);
            Tween.PopIn(window, 0.26f);
            _done = new UniTaskCompletionSource<ActivityResult>();
            ct.Register(() => _done.TrySetCanceled());
            Refresh();
            return await _done.Task;
        }

        void OnAbandon()
        {
            if (_driver == null || _finished) return;
            // Abandonner avant le début compte comme une partie perdue : Update conclut à la prochaine image.
            _started = true;
            overlay.gameObject.SetActive(false);
            _driver.Game.Abandon();
        }

        void Update()
        {
            if (_request == null || _driver == null || _done == null || _finished) return;
            var input = Input.Poll();
            if (!_started)
            {
                if (!input.Down) { Refresh(); return; }
                // Le premier appui démarre la partie ; il n'est pas transmis au jeu.
                _started = true;
                overlay.gameObject.SetActive(false);
            }
            else if (_driver.Game.Status == GameStatus.Running)
            {
                _driver.Tick(Mathf.Min(Time.unscaledDeltaTime, 0.05f), input);
            }
            UpdateStick();
            Refresh();
            if (_driver.Game.Status != GameStatus.Running) Finish();
        }

        void Finish()
        {
            if (_finished) return;
            _finished = true;
            var result = _driver!.Game.Result();
            overlay.gameObject.SetActive(true);
            overlay.text = result.Success ? _request!.WonText : _request!.LostText;
            overlay.transform.SetAsLastSibling();
            stickRing.gameObject.SetActive(false);
            stickKnob.gameObject.SetActive(false);
            _done!.TrySetResult(result);
        }

        void UpdateStick()
        {
            bool show = _driver!.UsesStick && Input.Held;
            stickRing.gameObject.SetActive(show);
            stickKnob.gameObject.SetActive(show);
            if (!show) return;
            var r = field.rect;
            var o = Input.StickOrigin;
            var ringPos = new Vector2(r.xMin + o.x * r.width, r.yMax - o.y * r.height);
            stickRing.anchoredPosition = ringPos;
            var p = Input.Position;
            var knob = new Vector2(r.xMin + p.x * r.width, r.yMax - p.y * r.height);
            float reach = MiniGameField.StickRadius * r.width;
            stickKnob.anchoredPosition = ringPos + Vector2.ClampMagnitude(knob - ringPos, reach);
        }

        void Refresh()
        {
            if (_request == null || _driver == null) return;
            var hud = _driver.Hud;
            scoreText.text = hud.Score;
            timeText.gameObject.SetActive(hud.TimeLeft >= 0f);
            timeText.text = string.Format(_request.TimeFormat, Mathf.Max(0, Mathf.CeilToInt(hud.TimeLeft)));
            livesText.gameObject.SetActive(hud.Lives >= 0);
            livesText.text = new string('♥', Mathf.Max(0, hud.Lives));
        }
    }
}
