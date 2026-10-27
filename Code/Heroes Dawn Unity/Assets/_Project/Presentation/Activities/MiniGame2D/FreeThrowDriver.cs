#nullable enable
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// « Tir franc » : une jauge verticale à gauche (la zone verte est la puissance à viser), un projectile en bas à gauche, une cible à droite
    /// d'autant plus loin que la puissance requise est grande. Maintenir charge la jauge, relâcher lance le projectile en parabole.
    /// </summary>
    public sealed class FreeThrowDriver : MiniGameDriverBase
    {
        const float GaugeX = 0.08f, GaugeTop = 0.15f, GaugeBottom = 0.85f;
        const float FlightSeconds = 0.6f;

        readonly FreeThrowGame _game;
        readonly ILocalization _loc;
        Image? _track, _zone, _cursor, _ball, _target, _targetCore;
        float _flight = -1f;
        float _landing;
        bool _flightHit;
        float _shownCenter, _shownHalf;

        public FreeThrowDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new FreeThrowGame(plan.Level.Seed, plan.Parameters);
            _loc = loc;
            _shownCenter = _game.TargetCenter;
            _shownHalf = _game.TargetHalfWidth;
        }

        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.freethrow.score", _game.Score, _game.Target), -1f, _game.ThrowsLeft);

        static float GaugeY(float power) => GaugeBottom - power * (GaugeBottom - GaugeTop);
        static float TargetX(float power) => 0.45f + 0.45f * power;

        protected override void OnBuild()
        {
            _track = Img("Track", null, new Color(1f, 1f, 1f, 0.18f), Vector2.one);
            _zone = Img("Zone", null, Theme.WithAlpha(Theme.Success, 0.75f), Vector2.one);
            _cursor = Img("Cursor", null, Theme.Accent, Vector2.one);
            _target = Img("Target", Sprites.Ring, Theme.Danger, new Vector2(150, 150));
            _targetCore = Img("TargetCore", Sprites.Circle, Theme.WithAlpha(Theme.Danger, 0.85f), new Vector2(60, 60));
            _ball = Img("Ball", Sprites.Circle, Theme.Accent, new Vector2(70, 70));
            Layout();
        }

        void Layout()
        {
            float w = FieldWidth, h = FieldHeight;
            _track!.rectTransform.sizeDelta = new Vector2(40f, (GaugeBottom - GaugeTop) * h);
            _track.rectTransform.anchoredPosition = ToField(GaugeX, (GaugeTop + GaugeBottom) * 0.5f);
            _zone!.rectTransform.sizeDelta = new Vector2(56f, Mathf.Max(18f, 2f * _shownHalf * (GaugeBottom - GaugeTop) * h));
            _zone.rectTransform.anchoredPosition = ToField(GaugeX, GaugeY(_shownCenter));
            _cursor!.rectTransform.sizeDelta = new Vector2(84f, 14f);
            _cursor.rectTransform.anchoredPosition = ToField(GaugeX, GaugeY(_game.Charging ? _game.Gauge : 0f));
            var targetPos = ToField(TargetX(_shownCenter), 0.62f);
            float size = Mathf.Max(110f, 2f * _shownHalf * w * 0.9f);
            _target!.rectTransform.sizeDelta = new Vector2(size, size);
            _target.rectTransform.anchoredPosition = targetPos;
            _targetCore!.rectTransform.anchoredPosition = targetPos;
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            _game.Step(dt);
            if (_flight < 0f)
            {
                if (input.Down) _game.BeginCharge();
                if (_game.Charging && (input.Up || !input.Held))
                {
                    int before = _game.ThrowsLeft;
                    _game.Release();
                    if (_game.ThrowsLeft < before)
                    {
                        _flight = 0f;
                        _landing = _game.LastPower;
                        _flightHit = _game.LastHit;
                    }
                }
            }
            if (_flight >= 0f)
            {
                _flight += dt;
                float k = Mathf.Clamp01(_flight / FlightSeconds);
                float x = Mathf.Lerp(0.18f, TargetX(_landing), k);
                float y = Mathf.Lerp(0.78f, 0.62f, k) - 0.35f * Mathf.Sin(k * Mathf.PI);
                _ball!.rectTransform.anchoredPosition = ToField(x, y);
                if (k >= 1f)
                {
                    var at = ToField(TargetX(_landing), 0.62f);
                    if (_flightHit) Sparks(at, Theme.Success, 10); else { Sparks(at, Theme.Danger, 5); ShakeField(); }
                    _flight = -1f;
                    _shownCenter = _game.TargetCenter;
                    _shownHalf = _game.TargetHalfWidth;
                }
            }
            else _ball!.rectTransform.anchoredPosition = ToField(0.18f, 0.78f);
            Layout();
        }
    }
}
