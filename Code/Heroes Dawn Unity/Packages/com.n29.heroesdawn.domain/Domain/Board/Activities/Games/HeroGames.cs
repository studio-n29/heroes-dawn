#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>Ce que donne un toucher du joueur dans « Rempart ».</summary>
    public enum DeflectTap { Miss, Deflected, Civilian }

    /// <summary>
    /// « Rempart » (2D, dynamique) : des projectiles plongent vers la ville, le joueur les intercepte du doigt avant qu'ils touchent le sol.
    /// Des drones de civils se mêlent à la pluie : les toucher coûte une vie. Trois vies, un minuteur ; le but est d'intercepter assez de projectiles.
    /// Les positions sont normalisées (0..1, y vers le bas). Logique pure et déterministe : un pas de temps, un toucher.
    /// </summary>
    public sealed class DeflectGame
    {
        public sealed class Item
        {
            public int Id;
            public float X, Y, Speed;
            public bool Civilian;
        }

        public const float HitRadius = 0.085f;
        public const float GroundY = 0.93f;

        readonly IRandom _rng;
        readonly List<Item> _items = new List<Item>();
        readonly int _target;
        readonly int _speedPercent;
        float _spawnIn;
        int _nextId;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public float TimeLeft { get; private set; }
        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int Target => _target;
        public IReadOnlyList<Item> Items => _items;
        /// <summary>Dernier événement (« deflect », « civilian », « leak », « miss », « none ») : pour les effets visuels.</summary>
        public string LastEvent { get; private set; } = "none";

        public DeflectGame(ulong seed, ActivityParameters p)
        {
            _rng = new Pcg32(seed).Fork(21);
            TimeLeft = p.Get("time_limit_s");
            _target = p.Get("target");
            Lives = p.Get("lives");
            _speedPercent = p.Get("speed_percent");
            _spawnIn = 0.4f;
        }

        public void Step(float dt)
        {
            if (Status != GameStatus.Running) return;
            LastEvent = "none";
            TimeLeft -= dt;
            _spawnIn -= dt;
            // La cadence s'accélère avec le temps écoulé : le début est un échauffement, la fin un déluge.
            while (_spawnIn <= 0f)
            {
                Spawn();
                _spawnIn += Math.Max(0.28f, 0.85f - Score * 0.025f) * (0.75f + 0.5f * (float)_rng.Range(0, 100) / 100f);
            }
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var it = _items[i];
                it.Y += it.Speed * dt;
                if (it.Y < GroundY) continue;
                _items.RemoveAt(i);
                if (!it.Civilian) { Lives--; LastEvent = "leak"; }
            }
            if (Lives <= 0) Status = GameStatus.Lost;
            else if (Score >= _target) Status = GameStatus.Won;
            else if (TimeLeft <= 0f) Status = GameStatus.Lost;
        }

        void Spawn()
        {
            bool civilian = _rng.Range(0, 100) < 16;
            float speed = (0.22f + 0.0125f * Score + (float)_rng.Range(0, 100) / 100f * 0.12f) * _speedPercent / 100f;
            _items.Add(new Item { Id = _nextId++, X = 0.07f + (float)_rng.Range(0, 860) / 1000f, Y = 0.02f, Speed = civilian ? speed * 0.8f : speed, Civilian = civilian });
        }

        /// <summary>Un toucher en (x, y) normalisés : intercepte le projectile le plus proche, ou blesse si c'est un civil.</summary>
        public DeflectTap Tap(float x, float y)
        {
            if (Status != GameStatus.Running) return DeflectTap.Miss;
            int best = -1; float bestD = HitRadius * HitRadius;
            for (int i = 0; i < _items.Count; i++)
            {
                float dx = _items[i].X - x, dy = (_items[i].Y - y) * 0.6f; // le champ est plus large que haut : la tolérance verticale est élargie
                float d = dx * dx + dy * dy;
                if (d <= bestD) { bestD = d; best = i; }
            }
            if (best < 0) { LastEvent = "miss"; return DeflectTap.Miss; }
            var hit = _items[best];
            _items.RemoveAt(best);
            if (hit.Civilian)
            {
                Lives--;
                LastEvent = "civilian";
                if (Lives <= 0) Status = GameStatus.Lost;
                return DeflectTap.Civilian;
            }
            Score++;
            LastEvent = "deflect";
            if (Score >= _target) Status = GameStatus.Won;
            return DeflectTap.Deflected;
        }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }

    /// <summary>« Course des toits » (2D, dynamique) : trois couloirs, le héros esquive les obstacles et sauve les civils qui tombent vers lui.</summary>
    public sealed class RooftopRunGame
    {
        public sealed class Item
        {
            public int Id;
            public int Lane;
            public float Y, Speed;
            public bool Civilian;
        }

        public const int Lanes = 3;
        /// <summary>Ligne du héros (y normalisé) et épaisseur de la zone de contact.</summary>
        public const float PlayerY = 0.82f;
        public const float Reach = 0.07f;

        readonly IRandom _rng;
        readonly List<Item> _items = new List<Item>();
        readonly int _target;
        readonly int _speedPercent;
        float _spawnIn;
        int _nextId;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public float TimeLeft { get; private set; }
        public int Lane { get; private set; } = 1;
        public int Rescued { get; private set; }
        public int Lives { get; private set; }
        public int Target => _target;
        public IReadOnlyList<Item> Items => _items;
        public string LastEvent { get; private set; } = "none";

        public RooftopRunGame(ulong seed, ActivityParameters p)
        {
            _rng = new Pcg32(seed).Fork(22);
            TimeLeft = p.Get("time_limit_s");
            _target = p.Get("target");
            Lives = p.Get("lives");
            _speedPercent = p.Get("speed_percent");
            _spawnIn = 0.5f;
        }

        public void MoveLeft() { if (Status == GameStatus.Running) Lane = Math.Max(0, Lane - 1); }
        public void MoveRight() { if (Status == GameStatus.Running) Lane = Math.Min(Lanes - 1, Lane + 1); }

        /// <summary>Place le héros dans le couloir correspondant à une position horizontale normalisée (toucher / glisser).</summary>
        public void MoveToward(float x01)
        {
            if (Status != GameStatus.Running) return;
            Lane = Math.Max(0, Math.Min(Lanes - 1, (int)(x01 * Lanes)));
        }

        public void Step(float dt)
        {
            if (Status != GameStatus.Running) return;
            LastEvent = "none";
            TimeLeft -= dt;
            _spawnIn -= dt;
            float elapsed = 1f - 0f;
            while (_spawnIn <= 0f)
            {
                Spawn();
                _spawnIn += Math.Max(0.32f, 0.75f - Rescued * 0.03f) * (0.8f + 0.4f * (float)_rng.Range(0, 100) / 100f);
            }
            for (int i = _items.Count - 1; i >= 0; i--)
            {
                var it = _items[i];
                float before = it.Y;
                it.Y += it.Speed * dt;
                bool crossed = before < PlayerY + Reach && it.Y >= PlayerY - Reach;
                if (crossed && it.Lane == Lane)
                {
                    _items.RemoveAt(i);
                    if (it.Civilian) { Rescued++; LastEvent = "rescue"; }
                    else { Lives--; LastEvent = "hit"; }
                    continue;
                }
                if (it.Y > 1.05f)
                {
                    _items.RemoveAt(i);
                    if (it.Civilian) LastEvent = "lost"; // un civil manqué ne coûte rien, mais ne compte pas
                }
            }
            if (Lives <= 0) Status = GameStatus.Lost;
            else if (Rescued >= _target) Status = GameStatus.Won;
            else if (TimeLeft <= 0f) Status = GameStatus.Lost;
            _ = elapsed;
        }

        void Spawn()
        {
            bool civilian = _rng.Range(0, 100) < 45;
            float speed = (0.34f + 0.015f * Rescued + (float)_rng.Range(0, 100) / 100f * 0.1f) * _speedPercent / 100f;
            _items.Add(new Item { Id = _nextId++, Lane = _rng.Range(0, Lanes), Y = -0.04f, Speed = speed, Civilian = civilian });
        }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Rescued);
    }
}
