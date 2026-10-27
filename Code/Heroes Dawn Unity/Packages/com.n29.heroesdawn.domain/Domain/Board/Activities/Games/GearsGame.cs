#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// « Engrenage » (2D) : plusieurs engrenages posés sur une grille 5×5, en contact quand leurs cases sont voisines. Le moteur tourne dans un sens fixe ;
    /// le joueur choisit le sens de chacun des autres. Deux engrenages en contact doivent tourner en sens contraires : un engrenage « entraîné »
    /// est relié au moteur par une chaîne de contacts bien orientés. On gagne quand tous le sont, avant d'épuiser les coups.
    /// Le niveau est toujours soluble : les contacts d'une grille sont bipartis, le sens attendu d'un engrenage suit donc la parité de sa case.
    /// </summary>
    public sealed class GearsGame : IMiniGame
    {
        public const int Grid = 5;

        public sealed class Gear
        {
            public int Id;
            public int Col, Row;
            public bool Clockwise;
            public bool Motor;
        }

        readonly List<Gear> _gears = new List<Gear>();
        readonly List<(int A, int B)> _edges = new List<(int A, int B)>();

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public IReadOnlyList<Gear> Gears => _gears;
        public IReadOnlyList<(int A, int B)> Edges => _edges;
        public int MovesLeft { get; private set; }
        public int Target => _gears.Count;
        public int Score => InSync;
        public float TimeLeft => -1f;
        /// <summary>Pas de vies : seuls les coups comptent (voir <see cref="MovesLeft"/>).</summary>
        public int Lives => -1;

        /// <summary>Nombre d'engrenages entraînés depuis le moteur par des contacts de sens contraires.</summary>
        public int InSync { get; private set; }

        public GearsGame(ulong seed, ActivityParameters p)
        {
            var rng = new Pcg32(seed).Fork(35);
            int count = p.Get("gears", 6);
            int moves = p.Get("moves", 12);
            var taken = new HashSet<(int, int)>();
            _gears.Add(new Gear { Id = 0, Col = rng.Range(0, Grid), Row = rng.Range(0, Grid), Clockwise = true, Motor = true });
            taken.Add((_gears[0].Col, _gears[0].Row));
            while (_gears.Count < count)
            {
                var candidates = new List<(int, int)>();
                foreach (var g in _gears)
                    foreach (var (dc, dr) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                    {
                        var cell = (g.Col + dc, g.Row + dr);
                        if (cell.Item1 < 0 || cell.Item1 >= Grid || cell.Item2 < 0 || cell.Item2 >= Grid) continue;
                        if (!taken.Contains(cell) && !candidates.Contains(cell)) candidates.Add(cell);
                    }
                var pick = candidates[rng.Range(0, candidates.Count)];
                taken.Add(pick);
                _gears.Add(new Gear { Id = _gears.Count, Col = pick.Item1, Row = pick.Item2, Clockwise = rng.Chance(50) });
            }
            for (int i = 0; i < _gears.Count; i++)
                for (int j = i + 1; j < _gears.Count; j++)
                    if (Math.Abs(_gears[i].Col - _gears[j].Col) + Math.Abs(_gears[i].Row - _gears[j].Row) == 1) _edges.Add((i, j));
            // Jamais résolu au départ : si le tirage tombe juste, on retourne le dernier engrenage libre.
            if (Wrong() == 0 && count > 1) _gears[count - 1].Clockwise = !_gears[count - 1].Clockwise;
            MovesLeft = Math.Max(moves, Wrong());
            Evaluate();
        }

        bool Required(Gear g)
        {
            var motor = _gears[0];
            return (g.Col + g.Row) % 2 == (motor.Col + motor.Row) % 2 ? motor.Clockwise : !motor.Clockwise;
        }

        int Wrong()
        {
            int n = 0;
            foreach (var g in _gears) if (!g.Motor && g.Clockwise != Required(g)) n++;
            return n;
        }

        void Evaluate()
        {
            var seen = new HashSet<int> { 0 };
            var todo = new Queue<int>();
            todo.Enqueue(0);
            while (todo.Count > 0)
            {
                int cur = todo.Dequeue();
                foreach (var (a, b) in _edges)
                {
                    int other = a == cur ? b : b == cur ? a : -1;
                    if (other < 0 || seen.Contains(other)) continue;
                    if (_gears[cur].Clockwise == _gears[other].Clockwise) continue;
                    seen.Add(other);
                    todo.Enqueue(other);
                }
            }
            InSync = seen.Count;
            if (Status != GameStatus.Running) return;
            if (InSync == _gears.Count) Status = GameStatus.Won;
            else if (MovesLeft <= 0) Status = GameStatus.Lost;
        }

        /// <summary>Inverse le sens d'un engrenage libre (coût : un coup). Refusé pour le moteur, sans coup restant ou partie finie.</summary>
        public bool Toggle(int gearId)
        {
            if (Status != GameStatus.Running || gearId < 0 || gearId >= _gears.Count) return false;
            var g = _gears[gearId];
            if (g.Motor || MovesLeft <= 0) return false;
            g.Clockwise = !g.Clockwise;
            MovesLeft--;
            Evaluate();
            return true;
        }

        public void Step(float dt) { }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
