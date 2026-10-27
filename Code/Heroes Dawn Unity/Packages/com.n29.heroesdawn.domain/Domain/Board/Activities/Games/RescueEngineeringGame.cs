#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// « Ingénierie de secours » (2D) : il faut créer un outil pour se sortir de la situation. Une réserve de pièces et un manuel d'indices sont donnés ;
    /// le joueur assemble les pièces dans le bon ordre. Le manuel suffit à retrouver l'outil : la première et la dernière pièce, la pièce qui suit
    /// chaque pièce, et les pièces en trop (inutilisées). Un mauvais assemblage coûte un essai.
    /// </summary>
    public sealed class RescueEngineeringGame : IMiniGame
    {
        /// <summary>Un indice du manuel : <c>first</c> (A est la première), <c>last</c> (A est la dernière), <c>after</c> (A vient juste après B), <c>unused</c> (A est inutile).</summary>
        public sealed class Clue
        {
            public string Kind = "";
            public string A = "";
            public string B = "";
        }

        /// <summary>Identifiants des pièces (les noms affichés viennent de la localisation : <c>mg2d.rescue.part.&lt;id&gt;</c>).</summary>
        public static readonly string[] Vocabulary =
        {
            "tube", "spring", "handle", "hook", "rope", "lens", "battery", "wire", "blade", "magnet", "pulley", "plank", "bolt", "cloth",
        };

        readonly List<string> _solution = new List<string>();
        readonly List<string> _pool = new List<string>();
        readonly List<Clue> _clues = new List<Clue>();
        readonly List<string> _chosen = new List<string>();
        readonly int _parts;

        public GameStatus Status { get; private set; } = GameStatus.Running;
        public IReadOnlyList<string> Solution => _solution;
        public IReadOnlyList<string> Pool => _pool;
        public IReadOnlyList<Clue> Clues => _clues;
        public IReadOnlyList<string> Chosen => _chosen;
        public int AttemptsLeft { get; private set; }
        /// <summary>Pièces bien placées lors de la dernière validation.</summary>
        public int LastCorrect { get; private set; }
        public int Target => _parts;
        public int Score => Status == GameStatus.Won ? _parts : LastCorrect;
        public float TimeLeft => -1f;
        public int Lives => AttemptsLeft;

        public RescueEngineeringGame(ulong seed, ActivityParameters p)
        {
            var rng = new Pcg32(seed).Fork(36);
            _parts = p.Get("parts", 5);
            int extra = Math.Min(p.Get("distractors", 3), Vocabulary.Length - _parts);
            AttemptsLeft = p.Get("attempts", 3);
            var bag = Vocabulary.ToList();
            Shuffle(bag, rng);
            _solution.AddRange(bag.Take(_parts));
            var distractors = bag.Skip(_parts).Take(extra).ToList();
            _pool.AddRange(_solution);
            _pool.AddRange(distractors);
            Shuffle(_pool, rng);
            _clues.Add(new Clue { Kind = "first", A = _solution[0] });
            _clues.Add(new Clue { Kind = "last", A = _solution[_parts - 1] });
            for (int i = 1; i < _parts; i++) _clues.Add(new Clue { Kind = "after", A = _solution[i], B = _solution[i - 1] });
            foreach (var d in distractors) _clues.Add(new Clue { Kind = "unused", A = d });
            Shuffle(_clues, rng);
        }

        static void Shuffle<T>(IList<T> list, IRandom rng)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Range(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        /// <summary>Ajoute une pièce à la séquence ; refusé pour un doublon, une pièce hors réserve ou une séquence déjà pleine.</summary>
        public bool Choose(string part)
        {
            if (Status != GameStatus.Running || _chosen.Count >= _parts || _chosen.Contains(part) || !_pool.Contains(part)) return false;
            _chosen.Add(part);
            return true;
        }

        public void Undo()
        {
            if (Status == GameStatus.Running && _chosen.Count > 0) _chosen.RemoveAt(_chosen.Count - 1);
        }

        /// <summary>Valide la séquence : vrai si c'est l'outil. Une séquence incomplète est ignorée ; une mauvaise coûte un essai et se vide.</summary>
        public bool Submit()
        {
            if (Status != GameStatus.Running || _chosen.Count < _parts) return false;
            LastCorrect = 0;
            for (int i = 0; i < _parts; i++) if (_chosen[i] == _solution[i]) LastCorrect++;
            if (LastCorrect == _parts) { Status = GameStatus.Won; return true; }
            AttemptsLeft--;
            _chosen.Clear();
            if (AttemptsLeft <= 0) Status = GameStatus.Lost;
            return false;
        }

        public void Step(float dt) { }

        public void Abandon() { if (Status == GameStatus.Running) Status = GameStatus.Lost; }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Score);
    }
}
