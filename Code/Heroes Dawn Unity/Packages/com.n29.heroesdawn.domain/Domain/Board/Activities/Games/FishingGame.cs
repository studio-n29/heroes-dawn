#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    public enum FishingState { Idle, Waiting, Bite, Caught, Escaped, Over }

    /// <summary>
    /// Pêche à la manière d'Animal Crossing : on vise un point de l'étang, on lance, on attend une touche, puis on tire au bon moment
    /// (fenêtre de touche courte). Minuteur ; le but est de pêcher le plus de poissons possible. Un lancer près d'un poisson le fait mordre plus vite.
    /// Logique pure : un pas de temps, une visée et deux boutons (lancer, tirer).
    /// </summary>
    public sealed class FishingGame
    {
        public const float AimSpeed = 4f;
        public const float AttractRadius = 2.5f;
        const float PauseSeconds = 0.9f;

        sealed class Fish { public Vec2f Position; public bool Caught; }

        readonly List<Fish> _fish = new List<Fish>();
        readonly IRandom _rng;
        readonly float _pondRadius;
        readonly int _biteWindowMs, _waitMin, _waitMax, _successFish;
        int _biting = -1;
        float _timer;

        public FishingState State { get; private set; } = FishingState.Idle;
        public Vec2f Aim { get; private set; }
        public float TimeLeft { get; private set; }
        public int Caught { get; private set; }
        public int TotalFish => _fish.Count;
        public int SuccessFish => _successFish;
        /// <summary>Fenêtre de touche restante (secondes) : 0 hors touche.</summary>
        public float BiteTimeLeft => State == FishingState.Bite ? _timer : 0f;
        /// <summary>Dernier message pour l'interface (clé : cast, bite, caught, early, escaped, none).</summary>
        public string LastEvent { get; private set; } = "none";
        public IEnumerable<Vec2f> RemainingFish { get { foreach (var f in _fish) if (!f.Caught) yield return f.Position; } }
        public bool IsOver => State == FishingState.Over;

        public FishingGame(ActivityLevel level, ActivityParameters p)
        {
            _rng = new Pcg32(level.Seed).Fork(33);
            _pondRadius = level.Fact("pond_radius_cm", 1400) / 100f;
            TimeLeft = p.Get("time_limit_s", 60);
            _biteWindowMs = p.Get("bite_window_ms", 900);
            _waitMin = p.Get("bite_wait_min_ms", 800);
            _waitMax = Math.Max(_waitMin + 1, p.Get("bite_wait_max_ms", 3000));
            _successFish = p.Get("success_fish", 4);
            foreach (var m in level.Markers)
                if (m.Name.StartsWith("fish_", StringComparison.Ordinal)) _fish.Add(new Fish { Position = new Vec2f(m.X / 100f, m.Z / 100f) });
            Aim = Vec2f.Zero;
        }

        int NearestAvailable(Vec2f p, float within)
        {
            int best = -1; float bestD = within;
            for (int i = 0; i < _fish.Count; i++)
            {
                if (_fish[i].Caught) continue;
                float d = Vec2f.Distance(p, _fish[i].Position);
                if (d <= bestD) { bestD = d; best = i; }
            }
            return best;
        }

        public void Step(float dt, Vec2f aimInput, bool cast, bool pull)
        {
            if (State == FishingState.Over || dt <= 0f) return;
            TimeLeft -= dt;
            if (TimeLeft <= 0f) { TimeLeft = 0f; State = FishingState.Over; return; }

            switch (State)
            {
                case FishingState.Idle:
                {
                    var next = Aim + aimInput.Clamped(1f) * (AimSpeed * dt);
                    Aim = next.Length > _pondRadius - 0.5f ? next.Normalized * (_pondRadius - 0.5f) : next;
                    if (cast)
                    {
                        int near = NearestAvailable(Aim, AttractRadius);
                        var available = new List<int>();
                        for (int i = 0; i < _fish.Count; i++) if (!_fish[i].Caught) available.Add(i);
                        if (available.Count == 0) { State = FishingState.Over; return; }
                        _biting = near >= 0 ? near : available[_rng.Range(0, available.Count)];
                        float wait = _rng.Range(_waitMin, _waitMax) / 1000f;
                        _timer = near >= 0 ? wait * 0.6f : wait;
                        State = FishingState.Waiting;
                        LastEvent = "cast";
                    }
                    break;
                }
                case FishingState.Waiting:
                    if (pull) { Escape("early"); break; }
                    _timer -= dt;
                    if (_timer <= 0f) { State = FishingState.Bite; _timer = _biteWindowMs / 1000f; LastEvent = "bite"; }
                    break;
                case FishingState.Bite:
                    if (pull)
                    {
                        _fish[_biting].Caught = true;
                        Caught++;
                        State = FishingState.Caught; _timer = PauseSeconds; LastEvent = "caught";
                        break;
                    }
                    _timer -= dt;
                    if (_timer <= 0f) Escape("escaped");
                    break;
                case FishingState.Caught:
                case FishingState.Escaped:
                    _timer -= dt;
                    if (_timer <= 0f)
                    {
                        State = Caught >= _fish.Count ? FishingState.Over : FishingState.Idle;
                        LastEvent = "none";
                    }
                    break;
            }
        }

        void Escape(string why)
        {
            State = FishingState.Escaped;
            _timer = PauseSeconds;
            LastEvent = why;
        }

        public ActivityResult Result() => new ActivityResult(Caught >= _successFish, Caught);
    }
}
