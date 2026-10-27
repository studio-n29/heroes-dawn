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
    /// « Engrenage » : des engrenages sur une grille 5×5. Un toucher inverse le sens d'un engrenage libre ; le moteur (cerclé d'or) ne bouge pas.
    /// Un engrenage entraîné tourne en vert, un engrenage hors synchronisation reste rouge et tourne dans son sens.
    /// </summary>
    public sealed class GearsDriver : MiniGameDriverBase
    {
        readonly GearsGame _game;
        readonly ILocalization _loc;
        Image[] _gears = new Image[0];
        Image[] _rings = new Image[0];
        bool[] _synced = new bool[0];
        float _angle;

        public GearsDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new GearsGame(plan.Level.Seed, plan.Parameters);
            _loc = loc;
        }

        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.gears.score", _game.Score, _game.Target, _game.MovesLeft), -1f, -1);

        float Cell => Mathf.Min(FieldWidth, FieldHeight) / GearsGame.Grid;

        Vector2 CenterOf(GearsGame.Gear g) => new Vector2((g.Col - (GearsGame.Grid - 1) * 0.5f) * Cell, ((GearsGame.Grid - 1) * 0.5f - g.Row) * Cell);

        protected override void OnBuild()
        {
            int n = _game.Gears.Count;
            _gears = new Image[n];
            _rings = new Image[n];
            _synced = new bool[n];
            for (int i = 0; i < n; i++)
            {
                _rings[i] = Img("Ring" + i, Sprites.Ring, Theme.Accent, Vector2.one);
                _gears[i] = Img("Gear" + i, Sprites.Burst, Theme.Danger, Vector2.one);
            }
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            _game.Step(dt);
            _angle += dt * 60f;
            if (input.Down)
            {
                var tap = ToField(input.Pos.x, input.Pos.y);
                foreach (var g in _game.Gears)
                {
                    if (Vector2.Distance(CenterOf(g), tap) > Cell * 0.5f) continue;
                    int before = _game.MovesLeft;
                    if (_game.Toggle(g.Id)) Sparks(CenterOf(g), Theme.Accent, 6);
                    else if (!g.Motor || before >= 0) ShakeField();
                    break;
                }
            }
            MarkSynced();
            float size = Cell * 0.92f;
            for (int i = 0; i < _gears.Length; i++)
            {
                var g = _game.Gears[i];
                var pos = CenterOf(g);
                _gears[i].rectTransform.anchoredPosition = pos;
                _gears[i].rectTransform.sizeDelta = Vector2.one * size;
                _gears[i].rectTransform.localRotation = Quaternion.Euler(0f, 0f, (g.Clockwise ? -1f : 1f) * _angle + (g.Col + g.Row) * 11f);
                _gears[i].color = g.Motor ? Theme.Accent : _synced[i] ? Theme.Success : Theme.Danger;
                _rings[i].rectTransform.anchoredPosition = pos;
                _rings[i].rectTransform.sizeDelta = Vector2.one * size * 1.05f;
                _rings[i].gameObject.SetActive(g.Motor);
            }
        }

        /// <summary>Quels engrenages sont entraînés depuis le moteur (même parcours que le domaine, pour la couleur).</summary>
        void MarkSynced()
        {
            var seen = _synced;
            System.Array.Clear(seen, 0, seen.Length);
            seen[0] = true;
            bool grew = true;
            while (grew)
            {
                grew = false;
                foreach (var (a, b) in _game.Edges)
                {
                    if (seen[a] == seen[b]) continue;
                    int from = seen[a] ? a : b, to = seen[a] ? b : a;
                    if (_game.Gears[from].Clockwise == _game.Gears[to].Clockwise) continue;
                    seen[to] = true;
                    grew = true;
                }
            }
        }
    }
}
