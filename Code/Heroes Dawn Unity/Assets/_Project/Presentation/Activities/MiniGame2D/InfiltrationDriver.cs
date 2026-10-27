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
    /// « Infiltration » : vue de dessus du camp (murs gris, gardes avec leur cône de vision en rayons, sortie verte). Le joystick flottant déplace le joueur.
    /// Les rayons s'arrêtent sur les murs comme la vue des gardes ; ils virent au rouge quand le garde voit le joueur.
    /// </summary>
    public sealed class InfiltrationDriver : MiniGameDriverBase
    {
        const int Rays = 5;
        readonly InfiltrationGame _game;
        readonly ILocalization _loc;
        readonly float _detectMs;
        Image[] _walls = new Image[0];
        Image[][] _rays = new Image[0][];
        Image[] _guardDiscs = new Image[0];
        Image? _player, _exit, _barBack, _barFill;
        float _scale;

        public InfiltrationDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new InfiltrationGame(plan.Level.Seed, plan.Parameters);
            _detectMs = plan.Parameters.Get("detect_ms", 600);
            _loc = loc;
        }

        public override bool UsesStick => true;
        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.infiltration.score", Mathf.RoundToInt(100f * Mathf.Clamp01(_game.Exposure / _detectMs))), _game.TimeLeft, _game.Lives);

        Vector2 ToWorld(Vec2f p) => new Vector2((p.X - InfiltrationGame.Width * 0.5f) * _scale, (0.5f - p.Y) * _scale);

        protected override void OnBuild()
        {
            _exit = Img("Exit", Sprites.Ring, Theme.Success, Vector2.one);
            _walls = new Image[_game.Walls.Count];
            for (int i = 0; i < _walls.Length; i++) _walls[i] = Img("Wall" + i, Sprites.Rounded, new Color(0.55f, 0.55f, 0.62f), Vector2.one);
            _rays = new Image[_game.Guards.Count][];
            _guardDiscs = new Image[_game.Guards.Count];
            for (int g = 0; g < _rays.Length; g++)
            {
                _rays[g] = new Image[Rays];
                for (int r = 0; r < Rays; r++) _rays[g][r] = Img("Ray" + g + "_" + r, null, new Color(1f, 0.9f, 0.3f, 0.35f), new Vector2(10, 8));
                _guardDiscs[g] = Img("Guard" + g, Sprites.Circle, Theme.Danger, Vector2.one * 46f);
            }
            _player = Img("Player", Sprites.Circle, Theme.Accent, Vector2.one * 44f);
            _barBack = Img("BarBack", null, new Color(0f, 0f, 0f, 0.5f), new Vector2(80, 12));
            _barFill = Img("BarFill", null, Theme.Danger, new Vector2(80, 12));
            Layout();
            Tick(0f, default);   // gardes et rayons à leur place avant le début
        }

        void Layout()
        {
            _scale = Mathf.Min(FieldWidth / InfiltrationGame.Width, FieldHeight);
            float diameter = 2f * InfiltrationGame.ExitRadius * _scale;
            _exit!.rectTransform.sizeDelta = Vector2.one * diameter;
            _exit.rectTransform.anchoredPosition = ToWorld(_game.Exit);
            for (int i = 0; i < _walls.Length; i++)
            {
                var w = _game.Walls[i];
                _walls[i].rectTransform.sizeDelta = new Vector2(w.W * _scale, w.H * _scale);
                _walls[i].rectTransform.anchoredPosition = ToWorld(new Vec2f(w.X + w.W * 0.5f, w.Y + w.H * 0.5f));
            }
            _player!.rectTransform.sizeDelta = Vector2.one * (2f * InfiltrationGame.PlayerRadius * _scale);
        }

        float RayLength(Vec2f origin, float angle)
        {
            float cos = Mathf.Cos(angle), sin = Mathf.Sin(angle);
            for (float d = 0.01f; d < InfiltrationGame.Range; d += 0.01f)
            {
                float x = origin.X + cos * d, y = origin.Y + sin * d;
                foreach (var w in _game.Walls)
                    if (x >= w.X && x <= w.X + w.W && y >= w.Y && y <= w.Y + w.H) return d;
            }
            return InfiltrationGame.Range;
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            int livesBefore = _game.Lives;
            _game.SetMove(new Vec2f(input.Stick.x, input.Stick.y));
            _game.Step(dt);
            if (_game.Lives < livesBefore) { Sparks(ToWorld(_game.Player), Theme.Danger, 10); ShakeField(); }
            Layout();
            for (int g = 0; g < _game.Guards.Count; g++)
            {
                var guard = _game.Guards[g];
                bool alert = _game.Sees(guard, _game.Player);
                _guardDiscs[g].rectTransform.anchoredPosition = ToWorld(guard.Pos);
                for (int r = 0; r < Rays; r++)
                {
                    float angle = guard.Facing + (r / (float)(Rays - 1) - 0.5f) * InfiltrationGame.Fov;
                    float len = RayLength(guard.Pos, angle) * _scale;
                    var img = _rays[g][r];
                    img.rectTransform.sizeDelta = new Vector2(len, 8f);
                    var a = ToWorld(guard.Pos);
                    img.rectTransform.anchoredPosition = a + new Vector2(Mathf.Cos(angle), -Mathf.Sin(angle)) * len * 0.5f;
                    img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
                    img.color = alert ? new Color(1f, 0.25f, 0.2f, 0.7f) : new Color(1f, 0.9f, 0.3f, 0.35f);
                }
            }
            var p = ToWorld(_game.Player);
            _player!.rectTransform.anchoredPosition = p;
            float k = Mathf.Clamp01(_game.Exposure / _detectMs);
            _barBack!.rectTransform.anchoredPosition = p + new Vector2(0f, 46f);
            _barFill!.rectTransform.sizeDelta = new Vector2(80f * k, 12f);
            _barFill.rectTransform.anchoredPosition = p + new Vector2(0f, 46f) + new Vector2(-40f + 40f * k, 0f);
            _barBack.gameObject.SetActive(k > 0.01f);
            _barFill.gameObject.SetActive(k > 0.01f);
        }
    }
}
