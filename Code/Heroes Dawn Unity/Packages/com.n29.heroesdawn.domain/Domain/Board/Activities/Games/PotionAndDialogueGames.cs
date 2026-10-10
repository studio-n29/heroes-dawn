#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>Retour sur une étape d'une potion ratée (façon « mastermind ») : de quoi déduire la recette sans la donner.</summary>
    public enum StepFeedback { Correct, TooMuch, TooLittle, WrongPlace, Wrong }

    /// <summary>
    /// La potion magique : on ajoute des ingrédients (avec leur quantité) dans l'ordre, puis on prépare. Réussite si la préparation suit la recette
    /// du livre ; sinon un essai est perdu et chaque étape indique si elle est juste, trop / pas assez dosée, au mauvais endroit ou fausse.
    /// </summary>
    public sealed class PotionGame
    {
        readonly PotionRecipe _recipe;
        readonly List<PotionStep> _current = new List<PotionStep>();

        public IReadOnlyList<PotionStep> Current => _current;
        public int AttemptsLeft { get; private set; }
        public int Brewed { get; private set; }
        public GameStatus Status { get; private set; } = GameStatus.Running;
        public IReadOnlyList<StepFeedback> LastFeedback { get; private set; } = Array.Empty<StepFeedback>();
        /// <summary>Préparation ratée avant remise à zéro (pour l'afficher avec son retour).</summary>
        public IReadOnlyList<PotionStep> LastAttempt { get; private set; } = Array.Empty<PotionStep>();
        public PotionRecipe Recipe => _recipe;
        public int StepsNeeded => _recipe.Steps.Count;

        public PotionGame(PotionRecipe recipe)
        {
            _recipe = recipe;
            AttemptsLeft = Math.Max(1, recipe.Attempts);
        }

        public bool CanAdd => Status == GameStatus.Running && _current.Count < _recipe.Steps.Count;

        public bool Add(int ingredient, int quantity)
        {
            if (!CanAdd || ingredient < 0 || ingredient >= _recipe.Ingredients.Count) return false;
            _current.Add(new PotionStep(ingredient, Math.Max(1, Math.Min(_recipe.MaxQuantity, quantity))));
            return true;
        }

        public bool RemoveLast()
        {
            if (Status != GameStatus.Running || _current.Count == 0) return false;
            _current.RemoveAt(_current.Count - 1);
            return true;
        }

        public void Clear() { if (Status == GameStatus.Running) _current.Clear(); }

        /// <summary>Prépare la potion ; faux si la préparation est incomplète (aucun essai perdu).</summary>
        public bool Brew()
        {
            if (Status != GameStatus.Running || _current.Count != _recipe.Steps.Count) return false;
            Brewed++;
            LastAttempt = new List<PotionStep>(_current);
            if (_recipe.Evaluate(_current))
            {
                LastFeedback = new StepFeedback[_current.Count];
                Status = GameStatus.Won;
                return true;
            }
            var feedback = new StepFeedback[_current.Count];
            for (int i = 0; i < _current.Count; i++)
            {
                var got = _current[i]; var want = _recipe.Steps[i];
                if (got.Ingredient == want.Ingredient)
                {
                    int diff = got.Quantity - want.Quantity;
                    feedback[i] = Math.Abs(diff) <= _recipe.Tolerance ? StepFeedback.Correct : diff > 0 ? StepFeedback.TooMuch : StepFeedback.TooLittle;
                }
                else feedback[i] = _recipe.Steps.Exists(s => s.Ingredient == got.Ingredient) ? StepFeedback.WrongPlace : StepFeedback.Wrong;
            }
            LastFeedback = feedback;
            _current.Clear();
            if (--AttemptsLeft <= 0) { AttemptsLeft = 0; Status = GameStatus.Lost; }
            return true;
        }

        public ActivityResult Result() => new ActivityResult(Status == GameStatus.Won, Status == GameStatus.Won ? Math.Max(1, _recipe.Attempts - Brewed + 1) : 0);
    }

    static class StepListExtensions
    {
        public static bool Exists(this IReadOnlyList<PotionStep> steps, Func<PotionStep, bool> match)
        {
            foreach (var s in steps) if (match(s)) return true;
            return false;
        }
    }

    /// <summary>Un dialogue en cours : une réplique à la fois, un choix de réponse par réplique ; le total décide de la réussite.</summary>
    public sealed class DialogueGame
    {
        readonly DialogueScript _script;
        readonly List<int> _chosen = new List<int>();

        public DialogueScript Script => _script;
        public int Index => _chosen.Count;
        public int Score { get; private set; }
        public IReadOnlyList<int> Chosen => _chosen;
        public bool Finished => _chosen.Count >= _script.Lines.Count;
        public DialogueLine? Current => Finished ? null : _script.Lines[_chosen.Count];
        public bool Success => Finished && _script.Evaluate(_chosen).Success;

        public DialogueGame(DialogueScript script) { _script = script; }

        /// <summary>Répond à la réplique courante par le choix <paramref name="choice"/> ; renvoie les points gagnés (0 à 2), -1 si refusé.</summary>
        public int Choose(int choice)
        {
            var line = Current;
            if (line == null || choice < 0 || choice >= line.Choices.Count) return -1;
            _chosen.Add(choice);
            int gained = line.Choices[choice].Score;
            Score += gained;
            return gained;
        }

        public ActivityResult Result() => new ActivityResult(Success, Score);
    }
}
