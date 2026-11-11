#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>
    /// Case Mini-jeu (fenêtre UI Toolkit) : consignes et jeu de réflexe (QTE) — arrêter un curseur dans la zone verte, 2 réussites sur 3 essais.
    /// Le bouton, la barre Espace / Entrée et la validation de la manette arrêtent le curseur.
    /// </summary>
    public sealed class MiniGamePopupView : HdPopup, IMiniGamePopupView
    {
        const int Attempts = 3;
        const int Needed = 2;

        Label _title = null!, _body = null!, _rules = null!, _score = null!;
        VisualElement _track = null!, _zone = null!, _cursor = null!;
        UnityEngine.UIElements.Button _stop = null!;
        MiniGamePopupData? _data;
        UniTaskCompletionSource<bool>? _done;

        string _scoreFormat = "{0} / {1}";
        float _t, _speed = 0.9f, _zoneCenter, _zoneWidth;
        int _dir = 1, _hits, _tries;
        bool _running;

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _stop;

        public HdButtonRef StopButton { get; private set; } = null!;
        public int Hits => _hits;
        public int Tries => _tries;

        protected override void Bind()
        {
            _title = Q<Label>("title"); _body = Q<Label>("body"); _rules = Q<Label>("rules"); _score = Q<Label>("score");
            _track = Q<VisualElement>("track"); _zone = Q<VisualElement>("zone"); _cursor = Q<VisualElement>("cursor");
            _stop = Q<UnityEngine.UIElements.Button>("stop");
            StopButton = new HdButtonRef(_stop, this);
            _stop.clicked += Stop;
            if (_data.HasValue) Fill(_data.Value);
        }

        void Fill(MiniGamePopupData d)
        {
            _title.text = d.Title; _body.text = d.Body; _rules.text = d.Rules; _stop.text = d.Stop;
            _scoreFormat = d.ScoreFormat;
            NewRound();
        }

        public UniTask<bool> PlayAsync(MiniGamePopupData d, CancellationToken ct)
        {
            _data = d;
            _done = new UniTaskCompletionSource<bool>();
            ct.Register(() => _done.TrySetCanceled());
            _hits = 0; _tries = 0;
            if (_title != null) Fill(d);
            return _done.Task;
        }

        void NewRound()
        {
            _zoneWidth = Mathf.Lerp(0.2f, 0.14f, _tries / (float)Attempts) * (Feedback.MotionSettings.ExtendedMiniGame ? 1.4f : 1f);
            _zoneCenter = Random.Range(0.2f, 0.8f);
            _zone.style.left = Length.Percent((_zoneCenter - _zoneWidth * 0.5f) * 100f);
            _zone.style.width = Length.Percent(_zoneWidth * 100f);
            _zone.style.backgroundColor = Theme.Success;
            _speed = (0.8f + 0.25f * _tries) * (Feedback.MotionSettings.ExtendedMiniGame ? 0.7f : 1f);
            _t = Random.value;
            _running = true;
            _stop.SetEnabled(true);
            UpdateScore();
        }

        void UpdateScore() => _score.text = string.Format(_scoreFormat, _hits, Attempts - _tries);

        void Update()
        {
            if (!_running || _cursor == null) return;
            _t += _dir * _speed * Time.unscaledDeltaTime;
            if (_t > 1f) { _t = 1f; _dir = -1; }
            if (_t < 0f) { _t = 0f; _dir = 1; }
            _cursor.style.left = Length.Percent(_t * 100f);
            if (Keys.ConfirmPressed()) Stop();
        }

        void Stop()
        {
            if (!_running) return;
            _running = false;
            _stop.SetEnabled(false);
            bool hit = _t >= _zoneCenter - _zoneWidth * 0.5f && _t <= _zoneCenter + _zoneWidth * 0.5f;
            _tries++;
            if (hit) _hits++;
            _zone.style.backgroundColor = hit ? Color.white : Theme.Danger;
            HudAnim.Punch(_track, 0.05f, 0.2f);
            UpdateScore();
            bool won = _hits >= Needed;
            bool lost = _hits + (Attempts - _tries) < Needed;
            HudAnim.After(_track, 0.6f, () =>
            {
                _zone.style.backgroundColor = Theme.Success;
                if (won || lost) _done?.TrySetResult(won);
                else NewRound();
            });
        }
    }
}
