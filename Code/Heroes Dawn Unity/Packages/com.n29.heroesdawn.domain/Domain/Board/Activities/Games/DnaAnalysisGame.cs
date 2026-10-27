#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>Ce que donne un toucher dans « Analyse ADN » : à côté, parasite retiré, ou nœud sain touché (une vie perdue).</summary>
    public enum DnaTap { Miss, Removed, Healthy }

    /// <summary>
    /// « Analyse ADN » (2D, nœuds) : un brin d'ADN est étalé devant le joueur, avec des molécules parasites cachées parmi des nœuds sains.
    /// Toucher un parasite le retire ; toucher un nœud sain coûte une vie. On gagne en retirant tous les parasites avant la fin du temps.
    /// Les positions sont normalisées (0..1, y vers le bas). Logique pure et déterministe.
    /// </summary>
    public sealed class DnaAnalysisGame : IMiniGame
    {
        public sealed class Node
        {
            public int Id;
            public float X, Y;
            public bool Parasite;
            public bool Removed;
        }

        /// <summary>Rayon de toucher (en largeur de zone) ; la tolérance verticale est élargie car la zone est plus large que haute.</summary>
        public const float HitRadius = 0.06f;

        readonly List<Node> _nodes = new List<Node>();
        readonly int _parasites;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public int Score { get; private set; }
        public int Target => _parasites;
        public float TimeLeft { get; private set; }
        public int Lives { get; private set; }
        public IReadOnlyList<Node> Nodes => _nodes;

        public DnaAnalysisGame(ulong seed, ActivityParameters p)
        {
            var rng = new Pcg32(seed).Fork(34);
            _parasites = p.Get("parasites", 6);
            TimeLeft = p.Get("time_limit_s", 45);
            Lives = p.Get("lives", 3);
            int n = 2 * _parasites + 6;
            // Double brin : les nœuds pairs sont sur un brin, les impairs sur l'autre (sinusoïdes en opposition).
            for (int i = 0; i < n; i++)
            {
                float x = 0.04f + 0.92f * (i + 0.5f) / n;
                float wave = (float)Math.Sin(i * 0.9);
                float y = 0.5f + (i % 2 == 0 ? 1f : -1f) * 0.26f * wave;
                _nodes.Add(new Node { Id = i, X = x, Y = Math.Max(0.12f, Math.Min(0.88f, y)) });
            }
            var order = new List<int>();
            for (int i = 0; i < n; i++) order.Add(i);
            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                (order[i], order[j]) = (order[j], order[i]);
            }
            for (int i = 0; i < _parasites; i++) _nodes[order[i]].Parasite = true;
        }

        public void Step(float dt)
        {
            if (Status != GameStatus.Running) return;
            TimeLeft -= dt;
            if (TimeLeft <= 0f) { TimeLeft = 0f; Status = GameStatus.Lost; }
        }

        public DnaTap Tap(float x, float y)
        {
            if (Status != GameStatus.Running) return DnaTap.Miss;
            Node? best = null; float bestD = HitRadius * HitRadius;
            foreach (var n in _nodes)
            {
                if (n.Removed) continue;
                float dx = n.X - x, dy = (n.Y - y) * 0.6f;
                float d = dx * dx + dy * dy;
                if (d <= bestD) { bestD = d; best = n; }
            }
            if (best == null) return DnaTap.Miss;
            if (best.Parasite)
            {
                best.Removed = true;
                Score++;
                if (Score >= _parasites) Status = GameStatus.Won;
                return DnaTap.Removed;
            }
            Lives--;
            if (Lives <= 0) Status = GameStatus.Lost;
            return DnaTap.Healthy;
        }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
