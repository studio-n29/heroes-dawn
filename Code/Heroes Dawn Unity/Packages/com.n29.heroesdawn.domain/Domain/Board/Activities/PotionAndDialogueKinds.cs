#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>Propriété d'un ingrédient (ce que le livre de potions en dit).</summary>
    public enum PotionTag { Heat, Cold, Calm, Vigor, Antidote, Binding, Light, Earth }

    /// <summary>Mal à soigner chez le patient (« cas à aider de l'équipe »).</summary>
    public enum Ailment { Fever, Wound, Poison, Exhaustion, Curse }

    public sealed class PotionIngredient
    {
        public int Index { get; }
        public PotionTag Tag { get; }
        public PotionIngredient(int index, PotionTag tag) { Index = index; Tag = tag; }
    }

    /// <summary>Une étape de la recette : quel ingrédient, en quelle quantité (dans l'ordre).</summary>
    public readonly struct PotionStep
    {
        public int Ingredient { get; }
        public int Quantity { get; }
        public PotionStep(int ingredient, int quantity) { Ingredient = ingredient; Quantity = quantity; }
    }

    /// <summary>Indice du livre : « effect » (ingrédient A a la propriété B), « order » (A est ajouté à l'étape B), « quantity » (A se dose en B).</summary>
    public readonly struct PotionHint
    {
        public string Kind { get; }
        public int A { get; }
        public int B { get; }
        public PotionHint(string kind, int a, int b) { Kind = kind; A = a; B = b; }
    }

    /// <summary>Recette secrète d'une potion : ingrédients, étapes à respecter (ordre, quantités) et indices du livre.</summary>
    public sealed class PotionRecipe
    {
        public Ailment Ailment { get; }
        public IReadOnlyList<PotionIngredient> Ingredients { get; }
        public IReadOnlyList<PotionStep> Steps { get; }
        public IReadOnlyList<PotionHint> Hints { get; }
        public int MaxQuantity { get; }
        public int Attempts { get; }
        public int Tolerance { get; }

        public PotionRecipe(Ailment ailment, IReadOnlyList<PotionIngredient> ingredients, IReadOnlyList<PotionStep> steps,
                            IReadOnlyList<PotionHint> hints, int maxQuantity, int attempts, int tolerance)
        {
            Ailment = ailment; Ingredients = ingredients; Steps = steps; Hints = hints;
            MaxQuantity = maxQuantity; Attempts = attempts; Tolerance = tolerance;
        }

        /// <summary>Vrai si la préparation suit la recette : mêmes ingrédients dans le même ordre, quantités à la tolérance près.</summary>
        public bool Evaluate(IReadOnlyList<PotionStep> attempt)
        {
            if (attempt.Count != Steps.Count) return false;
            for (int i = 0; i < Steps.Count; i++)
            {
                if (attempt[i].Ingredient != Steps[i].Ingredient) return false;
                if (Math.Abs(attempt[i].Quantity - Steps[i].Quantity) > Tolerance) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// La potion magique (2D) : un membre de l'équipe est malade ou blessé ; un livre donne des indices sur les ingrédients, leur ordre et leurs quantités.
    /// Réussite : gain ; échec : malus sur l'agent pendant tout le plateau (défini dans l'ActivityDef).
    /// </summary>
    public sealed class PotionKind : IActivityKind
    {
        public string Id => ActivityKinds.Potion;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("ingredients", 6, 3, 8),
            new ParameterDef("steps", 3, 2, 5),
            new ParameterDef("max_quantity", 3, 1, 5),
            new ParameterDef("attempts", 2, 1, 6),
            new ParameterDef("quantity_tolerance", 0, 0, 2),
        };

        public ActivityLevel Generate(ActivityContext c)
            => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0], payload: Recipe(c));

        public static PotionRecipe Recipe(ActivityContext c)
        {
            var rng = new Pcg32(c.Seed).Fork(14);
            int count = Math.Min(c.Param("ingredients"), Enum.GetValues(typeof(PotionTag)).Length);
            int steps = Math.Min(c.Param("steps"), count);
            int maxQty = c.Param("max_quantity");

            var tags = new List<PotionTag>((PotionTag[])Enum.GetValues(typeof(PotionTag)));
            Gen.Shuffle(rng, tags);
            var ingredients = new List<PotionIngredient>();
            for (int i = 0; i < count; i++) ingredients.Add(new PotionIngredient(i, tags[i]));

            var order = new List<int>();
            for (int i = 0; i < count; i++) order.Add(i);
            Gen.Shuffle(rng, order);
            var recipe = new List<PotionStep>();
            for (int i = 0; i < steps; i++) recipe.Add(new PotionStep(order[i], rng.Range(1, maxQty + 1)));

            // Le livre : la propriété de chaque ingrédient, l'ordre de chaque étape, et la dose de la plupart.
            var hints = new List<PotionHint>();
            foreach (var ing in ingredients) hints.Add(new PotionHint("effect", ing.Index, (int)ing.Tag));
            for (int i = 0; i < steps; i++) hints.Add(new PotionHint("order", recipe[i].Ingredient, i + 1));
            for (int i = 0; i < steps; i++) if (rng.Chance(70)) hints.Add(new PotionHint("quantity", recipe[i].Ingredient, recipe[i].Quantity));

            var ailments = (Ailment[])Enum.GetValues(typeof(Ailment));
            return new PotionRecipe(ailments[rng.Range(0, ailments.Length)], ingredients, recipe, hints, maxQty,
                                    c.Param("attempts"), c.Param("quantity_tolerance"));
        }
    }

    public sealed class DialogueChoice
    {
        public int Index { get; }
        /// <summary>0 = mauvaise réponse, 1 = acceptable, 2 = bonne.</summary>
        public int Score { get; }
        public string TextKey { get; }
        public DialogueChoice(int index, int score, string textKey) { Index = index; Score = score; TextKey = textKey; }
    }

    public sealed class DialogueLine
    {
        public int Index { get; }
        public string PromptKey { get; }
        public IReadOnlyList<DialogueChoice> Choices { get; }
        public DialogueLine(int index, string promptKey, IReadOnlyList<DialogueChoice> choices) { Index = index; PromptKey = promptKey; Choices = choices; }
    }

    /// <summary>Enchaînement de répliques avec choix ; les réponses font réussir ou non le dialogue.</summary>
    public sealed class DialogueScript
    {
        public IReadOnlyList<DialogueLine> Lines { get; }
        public int RequiredPercent { get; }
        public int MaxScore => Lines.Count * 2;

        public DialogueScript(IReadOnlyList<DialogueLine> lines, int requiredPercent) { Lines = lines; RequiredPercent = requiredPercent; }

        /// <summary>Score obtenu et réussite pour les choix faits (un indice de choix par réplique).</summary>
        public (int Score, bool Success) Evaluate(IReadOnlyList<int> chosen)
        {
            int score = 0;
            for (int i = 0; i < Lines.Count && i < chosen.Count; i++)
            {
                int k = chosen[i];
                if (k >= 0 && k < Lines[i].Choices.Count) score += Lines[i].Choices[k].Score;
            }
            return (score, MaxScore > 0 && score * 100 >= RequiredPercent * MaxScore);
        }
    }

    /// <summary>
    /// Dialogue : des répliques et des réponses au choix ; le résultat dépend des réponses. Un échec peut lancer une activité liée
    /// (combat ou autre) via <see cref="ActivityDef.FailureFollowUp"/>.
    /// </summary>
    public sealed class DialogueKind : IActivityKind
    {
        public string Id => ActivityKinds.Dialogue;

        public IReadOnlyList<ParameterDef> Schema { get; } = new[]
        {
            new ParameterDef("lines", 4, 2, 8),
            new ParameterDef("choices_per_line", 3, 2, 4),
            new ParameterDef("required_percent", 60, 0, 100),
        };

        public ActivityLevel Generate(ActivityContext c)
            => new ActivityLevel(c.Seed, c.Environment, 0, 0, new LevelPrimitive[0], new LevelMarker[0], payload: Script(c));

        public static DialogueScript Script(ActivityContext c)
        {
            var rng = new Pcg32(c.Seed).Fork(15);
            int lines = c.Param("lines"), perLine = c.Param("choices_per_line");
            var result = new List<DialogueLine>();
            string id = c.Def.Id.Value;
            for (int i = 0; i < lines; i++)
            {
                // Une bonne, une mauvaise, le reste acceptable ; l'ordre est mélangé.
                var scores = new List<int> { 2, 0 };
                while (scores.Count < perLine) scores.Add(1);
                Gen.Shuffle(rng, scores);
                var choices = new List<DialogueChoice>();
                int ok = 0;
                for (int k = 0; k < perLine; k++)
                {
                    // Le texte suit la qualité de la réponse (.good / .ok1 / .ok2 / .bad), pas sa position : les répliques se rédigent par palier.
                    string tier = scores[k] == 2 ? "good" : scores[k] == 0 ? "bad" : "ok" + (++ok);
                    choices.Add(new DialogueChoice(k, scores[k], "dialogue." + id + "." + i + "." + tier));
                }
                result.Add(new DialogueLine(i, "dialogue." + id + "." + i, choices));
            }
            return new DialogueScript(result, c.Param("required_percent"));
        }
    }
}
