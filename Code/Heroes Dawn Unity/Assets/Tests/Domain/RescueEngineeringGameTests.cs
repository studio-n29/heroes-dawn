using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Ingénierie de secours » : un manuel d'indices, une réserve de pièces, un outil à assembler dans le bon ordre.</summary>
    public sealed class RescueEngineeringGameTests
    {
        static ActivityParameters P(int parts = 5, int distractors = 3, int attempts = 3)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.RescueEngineering, "n", "d",
                new ActivityParameters(new Dictionary<string, int> { ["parts"] = parts, ["distractors"] = distractors, ["attempts"] = attempts })));

        /// <summary>Reconstruit la solution depuis les seuls indices du manuel (solveur indépendant du domaine).</summary>
        static List<string> SolveFromManual(RescueEngineeringGame g)
        {
            var seq = new List<string> { g.Clues.Single(c => c.Kind == "first").A };
            while (true)
            {
                var next = g.Clues.FirstOrDefault(c => c.Kind == "after" && c.B == seq.Last());
                if (next == null) break;
                seq.Add(next.A);
            }
            return seq;
        }

        [Test, Description("Vérifie que la même graine donne la même solution et les mêmes indices — parce que le niveau doit être rejouable à l'identique")]
        public void SameSeed_SameSolutionAndClues()
        {
            var a = new RescueEngineeringGame(3, P()); var b = new RescueEngineeringGame(3, P());
            Assert.That(a.Solution, Is.EqualTo(b.Solution));
            Assert.That(a.Clues.Select(c => c.Kind + c.A + c.B), Is.EqualTo(b.Clues.Select(c => c.Kind + c.A + c.B)));
        }

        [Test, Description("Vérifie que la réserve contient la solution et les pièces en trop, sans doublon, et que le nombre de pièces suit les réglages — parce que la réserve est ce que le joueur voit")]
        public void Pool_ContainsSolutionAndDistractors_NoDuplicates()
        {
            foreach (var (parts, extra) in new[] { (3, 0), (5, 3), (8, 6) })
            {
                var g = new RescueEngineeringGame(7, P(parts, extra));
                Assert.That(g.Solution.Count, Is.EqualTo(parts));
                Assert.That(g.Pool.Count, Is.EqualTo(parts + extra));
                Assert.That(g.Pool.Distinct().Count(), Is.EqualTo(g.Pool.Count));
                Assert.That(g.Pool, Is.SupersetOf(g.Solution));
                Assert.That(g.Target, Is.EqualTo(parts));
            }
        }

        [Test, Description("Vérifie que sur 200 graines et aux réglages extrêmes le manuel seul suffit à retrouver la solution, et que chaque pièce en trop y est déclarée inutile — parce qu'un manuel qui ne détermine pas l'outil rendrait le jeu injouable")]
        public void ManualDeterminesTheSolution()
        {
            for (ulong seed = 1; seed <= 200; seed++)
                foreach (var (parts, extra) in new[] { (3, 0), (8, 6), (5, 3) })
                {
                    var g = new RescueEngineeringGame(seed, P(parts, extra));
                    Assert.That(SolveFromManual(g), Is.EqualTo(g.Solution), $"graine {seed}, {parts}+{extra}");
                    var unused = g.Clues.Where(c => c.Kind == "unused").Select(c => c.A).ToList();
                    Assert.That(unused.Count, Is.EqualTo(extra));
                    Assert.That(unused.Intersect(g.Solution), Is.Empty);
                    Assert.That(g.Clues.Single(c => c.Kind == "last").A, Is.EqualTo(g.Solution.Last()));
                }
        }

        [Test, Description("Vérifie que les indices ne sont pas listés dans l'ordre de la solution — parce que le manuel ne doit pas donner la réponse d'un coup d'œil")]
        public void Clues_AreShuffled()
        {
            int ordered = 0;
            for (ulong seed = 1; seed <= 50; seed++)
            {
                var g = new RescueEngineeringGame(seed, P(8, 0));
                var afters = g.Clues.Where(c => c.Kind == "after").Select(c => c.A).ToList();
                if (afters.SequenceEqual(g.Solution.Skip(1))) ordered++;
            }
            Assert.That(ordered, Is.LessThan(10));
        }

        [Test, Description("Vérifie que soumettre la bonne séquence donne la victoire avec toutes les pièces bien placées — parce que c'est le but")]
        public void Submit_CorrectSequence_Wins()
        {
            var g = new RescueEngineeringGame(3, P());
            foreach (var part in g.Solution) Assert.That(g.Choose(part), Is.True);
            Assert.That(g.Submit(), Is.True);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Score, Is.EqualTo(5));
            Assert.That(g.Result().Success, Is.True);
        }

        [Test, Description("Vérifie qu'une mauvaise séquence consomme un essai, vide la séquence et compte les pièces bien placées, et que l'épuisement des essais donne la défaite — parce que les essais sont limités")]
        public void Submit_Wrong_ConsumesAnAttempt_AndAttemptsExhaustedLoses()
        {
            var g = new RescueEngineeringGame(3, P(attempts: 2));
            var wrong = g.Solution.Reverse().ToList();
            foreach (var part in wrong) g.Choose(part);
            Assert.That(g.Submit(), Is.False);
            Assert.That(g.AttemptsLeft, Is.EqualTo(1));
            Assert.That(g.Chosen, Is.Empty);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Running));
            foreach (var part in wrong) g.Choose(part);
            g.Submit();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(g.AttemptsLeft, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'on ne peut pas choisir deux fois la même pièce, une pièce hors réserve, ni plus de pièces que l'outil n'en demande, et qu'Annuler retire la dernière — parce que la séquence doit rester cohérente")]
        public void Choose_Rules_AndUndo()
        {
            var g = new RescueEngineeringGame(3, P());
            Assert.That(g.Choose(g.Pool[0]), Is.True);
            Assert.That(g.Choose(g.Pool[0]), Is.False, "doublon");
            Assert.That(g.Choose("nonexistent"), Is.False, "hors réserve");
            g.Undo();
            Assert.That(g.Chosen, Is.Empty);
            g.Undo();
            foreach (var part in g.Pool.Take(5)) g.Choose(part);
            Assert.That(g.Chosen.Count, Is.EqualTo(5));
            Assert.That(g.Choose(g.Pool[5]), Is.False, "séquence pleine");
        }

        [Test, Description("Vérifie qu'une validation avec une séquence incomplète ne coûte pas d'essai — parce qu'un appui trop tôt ne doit pas être puni")]
        public void Submit_Incomplete_IsIgnored()
        {
            var g = new RescueEngineeringGame(3, P());
            g.Choose(g.Pool[0]);
            Assert.That(g.Submit(), Is.False);
            Assert.That(g.AttemptsLeft, Is.EqualTo(3));
            Assert.That(g.Chosen.Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que l'abandon perd la partie et que le jeu n'a pas de minuteur — parce que le joueur prend le temps de lire le manuel")]
        public void Abandon_Loses_NoTimer()
        {
            var g = new RescueEngineeringGame(3, P());
            Assert.That(g.TimeLeft, Is.EqualTo(-1f));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }
    }
}
