#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    public enum LossReason { None, Cold, Time }

    /// <summary>
    /// Survie en neige : on traverse neige, herbe haute et sable avec une jauge de survie qui descend et qui se recharge près d'un totem,
    /// pour trouver l'objet recherché avant la fin du temps. Logique pure (pas de moteur) : un pas de temps, une direction.
    /// Terrains : neige = rapide mais froide (la jauge baisse plus vite), herbe haute = lente, sable = lent (moitié) et plus doux.
    /// </summary>
    public sealed class SnowSurvivalGame
    {
        public const float PlayerRadius = 0.4f;
        public const float BaseSpeed = 4.2f;
        public const float FindRadius = 1.3f;

        sealed class Patch { public Vec2f Center; public float HalfW; public float HalfD; public TerrainKind Kind; }
        sealed class Blocker { public Vec2f Center; public float Radius; }

        readonly List<Patch> _patches = new List<Patch>();
        readonly List<Blocker> _blockers = new List<Blocker>();
        readonly List<Vec2f> _totems = new List<Vec2f>();
        readonly float _drain, _refill, _totemRadius, _slow;
        readonly float _halfW, _halfD;

        public Vec2f Position { get; private set; }
        public Vec2f Target { get; }
        /// <summary>Jauge de survie sur 1000.</summary>
        public float Gauge { get; private set; } = 1000f;
        public float TimeLeft { get; private set; }
        public GameStatus Status { get; private set; } = GameStatus.Running;
        public LossReason Loss { get; private set; }
        public bool NearTotem { get; private set; }
        public TerrainKind? Terrain { get; private set; }
        public IReadOnlyList<Vec2f> Totems => _totems;
        public float TotemRadius => _totemRadius;

        public SnowSurvivalGame(ActivityLevel level, ActivityParameters p)
        {
            _halfW = level.Width / 200f; _halfD = level.Depth / 200f;
            TimeLeft = p.Get("time_limit_s", 90);
            _drain = p.Get("drain_permille_per_s", 25);
            _refill = p.Get("refill_permille_per_s", 120);
            _totemRadius = p.Get("totem_radius_m", 4);
            _slow = p.Get("terrain_slow_percent", 35) / 100f;
            foreach (var pr in level.Primitives)
            {
                var c = new Vec2f(pr.X / 100f, pr.Z / 100f);
                if (pr.Role.StartsWith("terrain_", StringComparison.Ordinal))
                {
                    var kind = pr.Role == "terrain_snow" ? TerrainKind.Snow : pr.Role == "terrain_tallgrass" ? TerrainKind.TallGrass : TerrainKind.Sand;
                    _patches.Add(new Patch { Center = c, HalfW = pr.SizeX / 200f, HalfD = pr.SizeZ / 200f, Kind = kind });
                }
                else if (pr.Role == "totem") _totems.Add(c);
                else if (pr.Role == "tree") _blockers.Add(new Blocker { Center = c, Radius = pr.SizeX / 200f });
                else if (pr.Role == "obstacle") _blockers.Add(new Blocker { Center = c, Radius = Math.Max(pr.SizeX, pr.SizeZ) / 200f });
            }
            level.TryMarker("spawn", out var spawn);
            Position = new Vec2f(spawn.X / 100f, spawn.Z / 100f);
            level.TryMarker("target", out var target);
            Target = new Vec2f(target.X / 100f, target.Z / 100f);
        }

        TerrainKind? TerrainAt(Vec2f p)
        {
            TerrainKind? kind = null;
            foreach (var t in _patches)
                if (Math.Abs(p.X - t.Center.X) <= t.HalfW && Math.Abs(p.Y - t.Center.Y) <= t.HalfD) kind = t.Kind;
            return kind;
        }

        float SpeedFactor(TerrainKind? t) => t == TerrainKind.TallGrass ? 1f - _slow : t == TerrainKind.Sand ? 1f - _slow / 2f : 1f;
        static float DrainFactor(TerrainKind? t) => t == TerrainKind.Snow ? 1.3f : t == TerrainKind.Sand ? 0.8f : 1f;

        public void Step(float dt, Vec2f move)
        {
            if (Status != GameStatus.Running || dt <= 0f) return;
            Terrain = TerrainAt(Position);
            var dir = move.Clamped(1f);
            var next = Position + dir * (BaseSpeed * SpeedFactor(Terrain) * dt);
            foreach (var b in _blockers)
            {
                var d = next - b.Center;
                float min = b.Radius + PlayerRadius, len = d.Length;
                if (len < min) next = len < 1e-4f ? new Vec2f(b.Center.X + min, b.Center.Y) : b.Center + d * (min / len);
            }
            Position = new Vec2f(Math.Max(-_halfW + 0.5f, Math.Min(_halfW - 0.5f, next.X)), Math.Max(-_halfD + 0.5f, Math.Min(_halfD - 0.5f, next.Y)));

            NearTotem = false;
            foreach (var t in _totems) if (Vec2f.Distance(Position, t) <= _totemRadius) { NearTotem = true; break; }
            if (NearTotem) Gauge = Math.Min(1000f, Gauge + _refill * dt);
            else Gauge -= _drain * DrainFactor(Terrain) * dt;
            TimeLeft -= dt;

            if (Vec2f.Distance(Position, Target) <= FindRadius) Status = GameStatus.Won;
            else if (Gauge <= 0f) { Gauge = 0f; Status = GameStatus.Lost; Loss = LossReason.Cold; }
            else if (TimeLeft <= 0f) { TimeLeft = 0f; Status = GameStatus.Lost; Loss = LossReason.Time; }
        }

        public ActivityResult Result() => Status == GameStatus.Won ? ActivityResult.Won() : ActivityResult.Lost();
    }
}
