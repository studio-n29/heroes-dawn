#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Mini-jeu de réflexe (QTE) : arrêter un curseur dans une zone verte, 2 réussites sur 3 essais.</summary>
    public sealed class TimingMiniGame : MonoBehaviour
    {
        const int Attempts = 3;
        const int Needed = 2;
        const float TrackWidth = 900f;

        [SerializeField] RectTransform track = null!;
        [SerializeField] RectTransform cursor = null!;
        [SerializeField] RectTransform zone = null!;
        [SerializeField] Image zoneImage = null!;
        [SerializeField] TMP_Text score = null!;
        [SerializeField] UiButton stop = null!;

        Action<bool>? _finished;
        string _scoreFormat = "{0} / {1}";
        float _t;
        float _speed = 0.9f;
        int _dir = 1;
        int _hits;
        int _tries;
        bool _running;

        void Awake() => stop.Button.onClick.AddListener(Stop);

        public void Begin(string stopLabel, string scoreFormat, Action<bool> finished)
        {
            stop.SetLabel(stopLabel);
            _scoreFormat = scoreFormat;
            _finished = finished;
            _hits = 0;
            _tries = 0;
            NewRound();
        }

        void NewRound()
        {
            float width = Mathf.Lerp(0.2f, 0.14f, _tries / (float)Attempts);
            float center = UnityEngine.Random.Range(0.2f, 0.8f);
            zone.sizeDelta = new Vector2(width * TrackWidth, 56);
            zone.anchoredPosition = new Vector2(center * TrackWidth, 0);
            _speed = 0.8f + 0.25f * _tries;
            _t = UnityEngine.Random.value;
            _running = true;
            stop.Interactable = true;
            UpdateScore();
        }

        void UpdateScore() => score.text = string.Format(_scoreFormat, _hits, Attempts - _tries);

        void Update()
        {
            if (!_running) return;
            _t += _dir * _speed * Time.unscaledDeltaTime;
            if (_t > 1f) { _t = 1f; _dir = -1; }
            if (_t < 0f) { _t = 0f; _dir = 1; }
            cursor.anchoredPosition = new Vector2(_t * TrackWidth, 0);
            if (Keys.ConfirmPressed()) Stop();
        }

        void Stop()
        {
            if (!_running) return;
            _running = false;
            stop.Interactable = false;
            float half = zone.sizeDelta.x * 0.5f;
            float x = cursor.anchoredPosition.x;
            bool hit = x >= zone.anchoredPosition.x - half && x <= zone.anchoredPosition.x + half;
            _tries++;
            if (hit) _hits++;
            zoneImage.color = hit ? Color.white : Theme.Danger;
            Tween.Punch(track, 0.05f, 0.2f);
            UpdateScore();
            bool won = _hits >= Needed;
            bool lost = _hits + (Attempts - _tries) < Needed;
            Tween.Delay(0.6f, () =>
            {
                zoneImage.color = Theme.Success;
                if (won || lost) _finished?.Invoke(won);
                else NewRound();
            }, this);
        }
    }
}
