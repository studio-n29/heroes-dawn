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
    /// « Analyse ADN » : un double brin de nœuds bleus reliés par des barreaux ; les molécules parasites (vert acide, qui pulsent) se retirent d'un toucher.
    /// </summary>
    public sealed class DnaDriver : MiniGameDriverBase
    {
        static readonly Color Healthy = Theme.Info;
        static readonly Color Acid = new Color(0.6f, 1f, 0.2f);

        readonly DnaAnalysisGame _game;
        readonly ILocalization _loc;
        Image[] _nodes = new Image[0];
        Image[] _links = new Image[0];
        float _clock;

        public DnaDriver(ActivityPlan plan, ILocalization loc)
        {
            _game = new DnaAnalysisGame(plan.Level.Seed, plan.Parameters);
            _loc = loc;
        }

        public override IMiniGame Game => _game;
        public override MiniGameHud Hud => new MiniGameHud(_loc.F("mg2d.dna.score", _game.Score, _game.Target), _game.TimeLeft, _game.Lives);

        protected override void OnBuild()
        {
            int n = _game.Nodes.Count;
            // Les liens (brins et barreaux) sont sous les nœuds.
            _links = new Image[(n - 2) + (n - 1) / 2 + 1];
            for (int i = 0; i < _links.Length; i++) _links[i] = Img("Link" + i, null, new Color(1f, 1f, 1f, 0.35f), new Vector2(10, 6));
            _nodes = new Image[n];
            for (int i = 0; i < n; i++) _nodes[i] = Img("Node" + i, Sprites.Circle, Healthy, Vector2.one * 60f);
            Tick(0f, default);   // mise en page avant le début
        }

        Vector2 PosOf(int i) => ToField(_game.Nodes[i].X, _game.Nodes[i].Y);

        static void Line(Image img, Vector2 a, Vector2 b)
        {
            var d = b - a;
            img.rectTransform.sizeDelta = new Vector2(d.magnitude, 6f);
            img.rectTransform.anchoredPosition = (a + b) * 0.5f;
            img.rectTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg);
        }

        public override void Tick(float dt, MiniGameInput input)
        {
            _clock += dt;
            _game.Step(dt);
            if (input.Down)
            {
                int lives = _game.Lives;
                var tapAt = ToField(input.Pos.x, input.Pos.y);
                var r = _game.Tap(input.Pos.x, input.Pos.y);
                if (r == DnaTap.Removed) Sparks(tapAt, Acid, 10);
                else if (r == DnaTap.Healthy) { Sparks(tapAt, Theme.Danger, 6); ShakeField(); }
            }
            int n = _nodes.Length, k = 0;
            // Brins : chaque nœud est relié au deuxième suivant (même brin) ; barreaux : nœud pair vers le suivant.
            for (int i = 0; i + 2 < n; i++) Line(_links[k++], PosOf(i), PosOf(i + 2));
            for (int i = 0; i + 1 < n && k < _links.Length; i += 2) Line(_links[k++], PosOf(i), PosOf(i + 1));
            while (k < _links.Length) _links[k++].gameObject.SetActive(false);
            float spacing = FieldWidth * 0.92f / n;
            float size = Mathf.Clamp(spacing * 1.6f, 36f, 80f);
            for (int i = 0; i < n; i++)
            {
                var node = _game.Nodes[i];
                var img = _nodes[i];
                img.gameObject.SetActive(!node.Removed);
                if (node.Removed) continue;
                img.rectTransform.anchoredPosition = PosOf(i);
                float pulse = node.Parasite ? 1f + 0.15f * Mathf.Sin(_clock * 8f + i) : 1f;
                img.rectTransform.sizeDelta = Vector2.one * size * pulse;
                img.color = node.Parasite ? Acid : Healthy;
            }
        }
    }
}
