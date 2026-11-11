#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Fenêtre commune des mini-jeux 2D (UI Toolkit) : bandeau score / temps / vies, grande zone tactile, joystick flottant, Abandonner.
    /// Le jeu lui-même vit dans un <see cref="IMiniGameDriver"/>. La fenêtre rend toujours la main : fin de partie, abandon ou annulation.
    /// </summary>
    public sealed class MiniGame2DPopupView : HdPopup, IMiniGame2DPopupView
    {
        Label _title = null!, _score = null!, _time = null!, _lives = null!, _overlay = null!;
        VisualElement _field = null!, _background = null!, _briefing = null!, _ring = null!, _knob = null!, _rescueHost = null!;
        UnityEngine.UIElements.Button _abandon = null!;
        FieldSurface? _surface;
        MiniGameField? _input;
        RescuePanelHud? _rescue;

        MiniGame2DRequest? _request;
        IMiniGameDriver? _driver;
        UniTaskCompletionSource<ActivityResult>? _done;
        bool _started, _finished;

        /// <summary>Retour (bouton matériel, Échap) pendant la partie = Abandonner : on peut toujours sortir.</summary>
        public override bool Cancelable => _driver != null && !_finished;
        public override void RequestCancel() => OnAbandon();
        protected override VisualElement DefaultFocus => _abandon;

        public FieldSurface Field => _surface!;
        public RescuePanelHud? Rescue => _rescue;
        public HdButtonRef AbandonButton { get; private set; } = null!;
        public bool Started => _started;
        public bool Finished => _finished;
        public IMiniGameDriver? Driver => _driver;
        public MiniGameField Input => _input!;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _score = Q<Label>("score"); _time = Q<Label>("time"); _lives = Q<Label>("lives"); _overlay = Q<Label>("overlay");
            _field = Q<VisualElement>("field"); _background = Q<VisualElement>("background"); _briefing = Q<VisualElement>("briefing");
            _ring = Q<VisualElement>("stickRing"); _knob = Q<VisualElement>("stickKnob"); _rescueHost = Q<VisualElement>("rescueHost");
            _abandon = Q<UnityEngine.UIElements.Button>("abandon");
            AbandonButton = new HdButtonRef(_abandon, this);
            _overlay.enableRichText = true;
            _ring.pickingMode = PickingMode.Ignore; _knob.pickingMode = PickingMode.Ignore; _background.pickingMode = PickingMode.Ignore;
            _briefing.pickingMode = PickingMode.Ignore; _overlay.pickingMode = PickingMode.Ignore; _rescueHost.pickingMode = PickingMode.Position;
            _surface = new FieldSurface(_field);
            _input = new MiniGameField(_field);
            _rescue = new RescuePanelHud();
            _rescueHost.Clear(); _rescueHost.Add(_rescue.Root);
            _abandon.clicked += OnAbandon;
            if (_request != null) Present(_request);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _driver?.Dispose();
        }

        public async UniTask<ActivityResult> PlayAsync(MiniGame2DRequest r, CancellationToken ct)
        {
            _driver?.Dispose();
            _request = r;
            _driver = r.Driver;
            _started = false; _finished = false;
            _done = new UniTaskCompletionSource<ActivityResult>();
            var registration = ct.Register(() => _done.TrySetCanceled());
            if (_surface == null) await UniTask.WaitUntil(() => _surface != null, cancellationToken: ct); // le document se monte au premier affichage
            Present(r);
            try { return await _done.Task; }
            finally { registration.Dispose(); }
        }

        /// <summary>Met la fenêtre en place pour la partie et construit le jeu ; la mise en page est attendue une image pour que la zone ait sa taille.</summary>
        void Present(MiniGame2DRequest r)
        {
            _input!.ResetState();
            _title.text = r.Title;
            _abandon.text = r.Abandon;
            _background.style.backgroundImage = r.Background != null ? new StyleBackground(Background.FromSprite(r.Background)) : new StyleBackground(StyleKeyword.None);
            _background.style.unityBackgroundImageTintColor = r.Background != null ? r.BackgroundTint : Color.clear;
            // Un pilote qui a besoin du panneau de secours le reçoit avant de construire ; sinon le panneau reste masqué.
            var panelUser = _driver as IRescuePanelUser;
            _rescue!.Root.style.display = panelUser != null ? DisplayStyle.Flex : DisplayStyle.None;
            panelUser?.Bind(_rescue);
            _surface!.Clear();
            _driver!.Build(_surface);
            SetOverlay(true);
            _overlay.text = r.Instructions + (r.Theme.Length > 0 ? "\n\n<size=70%><color=#7FE3FF>" + r.Theme + "</color></size>" : "") + "\n\n<size=70%><color=#FFCF33>" + r.Start + "</color></size>";
            _ring.style.display = DisplayStyle.None; _knob.style.display = DisplayStyle.None;
            // Les éléments créés avant la première mise en page sont replacés quand la zone a sa taille.
            _field.RegisterCallback<GeometryChangedEvent>(_ => RelayoutField());
            Refresh();
        }

        void RelayoutField() { /* les pilotes recalculent leurs positions à chaque image */ }

        void OnAbandon()
        {
            if (_driver == null || _finished) return;
            // Abandonner avant le début compte comme une partie perdue : Update conclut à la prochaine image.
            _started = true;
            SetOverlay(false);
            _driver.Game.Abandon();
        }

        void Update()
        {
            if (_request == null || _driver == null || _done == null || _finished || _input == null) return;
            var input = _input.Poll();
            if (!_started)
            {
                if (!input.Down) { Refresh(); return; }
                // Le premier appui démarre la partie ; il n'est pas transmis au jeu.
                _started = true;
                SetOverlay(false);
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
            SetOverlay(true);
            _overlay.text = result.Success ? _request!.WonText + (_request.Learned.Length > 0 ? "\n<size=60%>" + _request.Learned + "</size>" : "") : _request!.LostText;
            _ring.style.display = DisplayStyle.None; _knob.style.display = DisplayStyle.None;
            _done!.TrySetResult(result);
        }

        void SetOverlay(bool visible)
        {
            _briefing.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (visible) _briefing.BringToFront();
        }

        void UpdateStick()
        {
            bool show = _driver!.UsesStick && _input!.Held;
            _ring.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            _knob.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            if (!show) return;
            var size = _surface!.Size;
            var o = _input.StickOrigin;
            var ringPos = new Vector2(o.x * size.x, o.y * size.y);
            _ring.style.left = ringPos.x - 125f; _ring.style.top = ringPos.y - 125f;
            var p = _input.Position;
            var knob = new Vector2(p.x * size.x, p.y * size.y);
            float reach = MiniGameField.StickRadius * size.x;
            var at = ringPos + Vector2.ClampMagnitude(knob - ringPos, reach);
            _knob.style.left = at.x - 55f; _knob.style.top = at.y - 55f;
        }

        void Refresh()
        {
            if (_request == null || _driver == null) return;
            var hud = _driver.Hud;
            _score.text = hud.Score;
            _time.style.display = hud.TimeLeft >= 0f ? DisplayStyle.Flex : DisplayStyle.None;
            _time.text = string.Format(_request.TimeFormat, Mathf.Max(0, Mathf.CeilToInt(hud.TimeLeft)));
            _lives.style.display = hud.Lives >= 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _lives.text = new string('♥', Mathf.Max(0, hud.Lives));
        }
    }
}
