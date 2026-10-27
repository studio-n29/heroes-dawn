#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// « Infiltration » (2D vue de dessus) : le joueur se déplace à 360° (joystick virtuel sur mobile) dans un camp, évite le champ de vision des gardes
    /// et rejoint la sortie. Le monde fait <see cref="Width"/> × 1 ; des murs bloquent le déplacement et la vue ; chaque garde balaie un cône devant lui.
    /// Être vu cumule une exposition ; au seuil <c>detect_ms</c>, une vie est perdue et le joueur retourne au départ.
    /// Logique pure et déterministe : <see cref="SetMove"/> puis <see cref="Step"/>.
    /// </summary>
    public sealed class InfiltrationGame : IMiniGame
    {
        public const float Width = 1.6f;
        public const float PlayerRadius = 0.03f;
        public const float Speed = 0.45f;
        public const float ExitRadius = 0.06f;
        /// <summary>Champ de vision total d'un garde (70°) et portée.</summary>
        public const float Fov = 1.22f;
        public const float Range = 0.32f;

        public sealed class Wall { public float X, Y, W, H; }

        public sealed class Guard
        {
            public Vec2f Pos;
            public float Facing;
            public float BaseFacing;
            public float Period;
            public float Phase;
        }

        readonly List<Wall> _walls = new List<Wall>();
        readonly List<Guard> _guards = new List<Guard>();
        readonly float _detectMs;
        Vec2f _move = Vec2f.Zero;
        float _clock;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public Vec2f Player { get; private set; }
        public Vec2f Start { get; private set; }
        public Vec2f Exit { get; private set; }
        public IReadOnlyList<Wall> Walls => _walls;
        public IReadOnlyList<Guard> Guards => _guards;
        /// <summary>Millisecondes cumulées sous le regard d'un garde.</summary>
        public float Exposure { get; private set; }
        public float TimeLeft { get; private set; }
        public int Lives { get; private set; }
        public int Target => 1;
        public int Score => Status == GameStatus.Won ? 1 : 0;

        public InfiltrationGame(ulong seed, ActivityParameters p)
        {
            TimeLeft = p.Get("time_limit_s", 60);
            Lives = p.Get("lives", 3);
            _detectMs = p.Get("detect_ms", 600);
            int guards = p.Get("guards", 3);
            var rng = new Pcg32(seed).Fork(37);
            Start = new Vec2f(0.12f, 0.5f);
            Exit = new Vec2f(Width - 0.12f, 0.15f + 0.7f * rng.Range(0, 101) / 100f);
            Player = Start;
            GenerateWalls(seed);
            for (int i = 0; i < guards; i++) _guards.Add(MakeGuard(rng));
        }

        // ---------------------------------------------------------------- Génération

        void GenerateWalls(ulong seed)
        {
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var rng = new Pcg32(seed).Fork(370 + (ulong)attempt);
                var walls = new List<Wall>();
                int count = rng.Range(3, 6);
                for (int i = 0; i < count; i++)
                {
                    bool vertical = rng.Chance(60);
                    float len = 0.3f + 0.3f * rng.Range(0, 101) / 100f;
                    var w = vertical
                        ? new Wall { X = 0.35f + 0.9f * rng.Range(0, 101) / 100f, Y = (1f - len) * rng.Range(0, 101) / 100f, W = 0.05f, H = len }
                        : new Wall { X = 0.35f + (0.9f - len) * rng.Range(0, 101) / 100f, Y = 0.1f + 0.8f * rng.Range(0, 101) / 100f, W = len, H = 0.05f };
                    if (Overlaps(w, Start, 0.15f) || Overlaps(w, Exit, 0.15f)) continue;
                    walls.Add(w);
                }
                if (!Connected(walls)) continue;
                _walls.AddRange(walls);
                return;
            }
        }

        static bool Overlaps(Wall w, Vec2f p, float pad) => p.X >= w.X - pad && p.X <= w.X + w.W + pad && p.Y >= w.Y - pad && p.Y <= w.Y + w.H + pad;

        /// <summary>Le départ rejoint la sortie en contournant les murs gonflés du rayon du joueur (grille 32×20).</summary>
        bool Connected(List<Wall> walls)
        {
            const int cols = 32, rows = 20;
            bool Blocked(int c, int r)
            {
                var p = new Vec2f((c + 0.5f) * Width / cols, (r + 0.5f) / rows);
                foreach (var w in walls) if (Overlaps(w, p, PlayerRadius)) return true;
                return false;
            }
            (int, int) CellOf(Vec2f p) => (Math.Min(cols - 1, (int)(p.X / (Width / cols))), Math.Min(rows - 1, (int)(p.Y * rows)));
            var s = CellOf(Start); var e = CellOf(Exit);
            if (Blocked(s.Item1, s.Item2) || Blocked(e.Item1, e.Item2)) return false;
            var seen = new HashSet<(int, int)> { s };
            var todo = new Queue<(int, int)>();
            todo.Enqueue(s);
            while (todo.Count > 0)
            {
                var (c, r) = todo.Dequeue();
                if ((c, r) == e) return true;
                foreach (var (dc, dr) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    int nc = c + dc, nr = r + dr;
                    if (nc < 0 || nc >= cols || nr < 0 || nr >= rows || Blocked(nc, nr) || !seen.Add((nc, nr))) continue;
                    todo.Enqueue((nc, nr));
                }
            }
            return false;
        }

        Guard MakeGuard(IRandom rng)
        {
            Vec2f pos = Vec2f.Zero;
            for (int tries = 0; tries < 30; tries++)
            {
                pos = new Vec2f(0.4f + 0.9f * rng.Range(0, 101) / 100f, 0.12f + 0.76f * rng.Range(0, 101) / 100f);
                bool inWall = false;
                foreach (var w in _walls) if (Overlaps(w, pos, 0.04f)) { inWall = true; break; }
                if (!inWall && Vec2f.Distance(pos, Start) > Range + 0.1f) break;
            }
            float baseFacing = rng.Range(0, 360) * (float)Math.PI / 180f;
            var g = new Guard { Pos = pos, BaseFacing = baseFacing, Facing = baseFacing, Period = 3f + rng.Range(0, 201) / 100f, Phase = rng.Range(0, 628) / 100f };
            return g;
        }

        // ---------------------------------------------------------------- Boucle

        public void SetMove(Vec2f move) => _move = move.Clamped(1f);

        public void Step(float dt)
        {
            if (Status != GameStatus.Running) return;
            _clock += dt;
            TimeLeft -= dt;
            foreach (var g in _guards) g.Facing = g.BaseFacing + 1.05f * (float)Math.Sin(_clock * 2.0 * Math.PI / g.Period + g.Phase);
            Move(_move * (Speed * dt));
            if (Vec2f.Distance(Player, Exit) <= ExitRadius) { Status = GameStatus.Won; return; }
            bool seen = false;
            foreach (var g in _guards) if (Sees(g, Player)) { seen = true; break; }
            Exposure = seen ? Exposure + 1000f * dt : Math.Max(0f, Exposure - 2000f * dt);
            if (Exposure >= _detectMs)
            {
                Lives--;
                Exposure = 0f;
                Player = Start;
                if (Lives <= 0) { Status = GameStatus.Lost; return; }
            }
            if (TimeLeft <= 0f) { TimeLeft = 0f; Status = GameStatus.Lost; }
        }

        void Move(Vec2f delta)
        {
            var x = new Vec2f(Player.X + delta.X, Player.Y);
            if (Free(x)) Player = x;
            var y = new Vec2f(Player.X, Player.Y + delta.Y);
            if (Free(y)) Player = y;
        }

        bool Free(Vec2f p)
        {
            if (p.X < PlayerRadius || p.X > Width - PlayerRadius || p.Y < PlayerRadius || p.Y > 1f - PlayerRadius) return false;
            foreach (var w in _walls)
            {
                float cx = Math.Max(w.X, Math.Min(p.X, w.X + w.W)), cy = Math.Max(w.Y, Math.Min(p.Y, w.Y + w.H));
                float dx = p.X - cx, dy = p.Y - cy;
                if (dx * dx + dy * dy < PlayerRadius * PlayerRadius) return false;
            }
            return true;
        }

        // ---------------------------------------------------------------- Vision

        /// <summary>Le garde voit le point : à portée, dans son cône, et sans mur sur le segment.</summary>
        public bool Sees(Guard g, Vec2f p)
        {
            var d = p - g.Pos;
            float dist = d.Length;
            if (dist > Range) return false;
            if (dist > 1e-4f)
            {
                float diff = (float)Math.Atan2(d.Y, d.X) - g.Facing;
                while (diff > Math.PI) diff -= 2f * (float)Math.PI;
                while (diff < -Math.PI) diff += 2f * (float)Math.PI;
                if (Math.Abs(diff) > Fov * 0.5f) return false;
            }
            foreach (var w in _walls) if (SegmentHitsRect(g.Pos, p, w)) return false;
            return true;
        }

        /// <summary>Intersection segment / rectangle (méthode des plans parallèles).</summary>
        static bool SegmentHitsRect(Vec2f a, Vec2f b, Wall w)
        {
            float t0 = 0f, t1 = 1f;
            float dx = b.X - a.X, dy = b.Y - a.Y;
            return Clip(-dx, a.X - w.X, ref t0, ref t1) && Clip(dx, w.X + w.W - a.X, ref t0, ref t1)
                && Clip(-dy, a.Y - w.Y, ref t0, ref t1) && Clip(dy, w.Y + w.H - a.Y, ref t0, ref t1);
        }

        static bool Clip(float p, float q, ref float t0, ref float t1)
        {
            if (Math.Abs(p) < 1e-9f) return q >= 0f;
            float r = q / p;
            if (p < 0f) { if (r > t1) return false; if (r > t0) t0 = r; }
            else { if (r < t0) return false; if (r < t1) t1 = r; }
            return true;
        }

        // ---------------------------------------------------------------- Test

        /// <summary>Place le joueur (tests uniquement).</summary>
        public void PlaceForTests(Vec2f p) => Player = p;

        /// <summary>Fixe l'exposition (tests uniquement).</summary>
        public void SetExposureForTests(float ms) => Exposure = ms;

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
