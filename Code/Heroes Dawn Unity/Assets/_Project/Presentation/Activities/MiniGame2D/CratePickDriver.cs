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
    /// « Caisse au choix » : trois caisses scellées au sol, un curseur qui les survole ; maintenir charge (barre sous le curseur),
    /// relâcher frappe la caisse du dessous. La caisse récompense scintille au début de chaque manche.
    /// </summary>
    public sealed class CratePickDriver : MiniGameDriverBase
    {
        const float CrateY = 0.64f, CursorY = 0.28f;
        static readonly float[] CrateX = { 1f / 6f, 0.5f, 5f / 6f };

        readonly CratePickGame _game;
        readonly ILocalization _loc;
        Image[] _crates = new Image[0];
        Image[] _locks = new Image[0];
        Image? _cursor, _beam, _chargeBack, _chargeFill;
        float _flash;
        int _flashCrate = -1;
        Color _flashColor;
        float _clock;

        public CratePickDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new CratePickGame(plan.Level.Seed, plan.Parameters);
            _loc = loc;
        }

        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.cratepick.score", _game.Score, _game.Target), -1f, _game.Lives);

        protected override void OnBuild()
        {
            _crates = new Image[CratePickGame.Crates];
            _locks = new Image[CratePickGame.Crates];
            for (int i = 0; i < _crates.Length; i++)
            {
                _crates[i] = Img("Crate" + i, Sprites.Rounded, new Color(0.62f, 0.42f, 0.22f), new Vector2(260, 260));
                _locks[i] = Img("Lock" + i, Sprites.Circle, Theme.Accent, new Vector2(60, 60));
            }
            _beam = Img("Beam", null, Theme.WithAlpha(Theme.Accent, 0.35f), Vector2.one);
            _cursor = Img("Cursor", Sprites.Diamond, Theme.Accent, new Vector2(90, 90));
            _chargeBack = Img("ChargeBack", null, new Color(0f, 0f, 0f, 0.45f), new Vector2(160, 20));
            _chargeFill = Img("ChargeFill", null, Theme.Danger, new Vector2(160, 20));
            Layout();
        }

        void Layout()
        {
            for (int i = 0; i < _crates.Length; i++)
            {
                var pos = ToField(CrateX[i], CrateY);
                _crates[i].rectTransform.anchoredPosition = pos;
                _locks[i].rectTransform.anchoredPosition = pos;
            }
            var cursorPos = ToField(_game.Cursor, CursorY);
            _cursor!.rectTransform.anchoredPosition = cursorPos;
            float beamTop = cursorPos.y, beamBottom = ToField(0f, CrateY).y;
            _beam!.rectTransform.sizeDelta = new Vector2(8f, Mathf.Abs(beamTop - beamBottom));
            _beam.rectTransform.anchoredPosition = new Vector2(cursorPos.x, (beamTop + beamBottom) * 0.5f);
            var barPos = cursorPos + new Vector2(0f, -70f);
            _chargeBack!.rectTransform.anchoredPosition = barPos;
            var fill = _chargeFill!.rectTransform;
            fill.sizeDelta = new Vector2(160f * _game.Charge, 20f);
            fill.anchoredPosition = barPos + new Vector2(-80f + 80f * _game.Charge, 0f);
            _chargeBack.gameObject.SetActive(_game.Charging);
            _chargeFill.gameObject.SetActive(_game.Charging);
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            _clock += dt;
            _game.Step(dt);
            if (input.Down) _game.BeginCharge();
            if (_game.Charging && (input.Up || !input.Held))
            {
                int round = _game.Round;
                int lives = _game.Lives;
                bool hit = _game.Release();
                if (_game.Round > round)
                {
                    var at = ToField(CrateX[_game.LastStruck], CrateY);
                    _flashCrate = _game.LastStruck;
                    _flash = 0.5f;
                    _flashColor = hit ? Theme.Success : Theme.Danger;
                    Sparks(at, _flashColor, hit ? 12 : 7);
                    if (_game.Lives < lives) ShakeField();
                }
            }
            if (_flash > 0f) _flash -= dt;
            Layout();
            for (int i = 0; i < _crates.Length; i++)
            {
                var color = new Color(0.62f, 0.42f, 0.22f);
                if (_game.Revealed && i == _game.Reward)
                    color = Color.Lerp(color, Theme.Accent, 0.5f + 0.5f * Mathf.Sin(_clock * 12f));
                if (_flash > 0f && i == _flashCrate) color = Color.Lerp(color, _flashColor, 0.85f);
                _crates[i].color = color;
            }
        }
    }
}
