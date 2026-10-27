using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Analyse ADN » : un brin d'ADN, des molécules parasites à retirer, sans toucher les nœuds sains.</summary>
    public sealed class DnaAnalysisGameTests
    {
        static ActivityParameters P(int time = 45, int parasites = 6, int lives = 3)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.DnaAnalysis, "n", "d",
                new ActivityParameters(new Dictionary<string, int> { ["time_limit_s"] = time, ["parasites"] = parasites, ["lives"] = lives })));

        [Test, Description("Vérifie que la même graine donne les mêmes parasites et qu'une autre graine en donne d'autres — parce que le niveau doit être rejouable à l'identique")]
        public void SameSeed_SameParasites()
        {
            var a = new DnaAnalysisGame(1, P()); var b = new DnaAnalysisGame(1, P()); var c = new DnaAnalysisGame(2, P());
            Assert.That(a.Nodes.Select(n => n.Parasite), Is.EqualTo(b.Nodes.Select(n => n.Parasite)));
            Assert.That(a.Nodes.Select(n => n.Parasite), Is.Not.EqualTo(c.Nodes.Select(n => n.Parasite)));
        }

        [Test, Description("Vérifie que le nombre de parasites est celui du réglage (3 à 16) et que le brin a 2 nœuds sains de plus par parasite plus 6 — parce que le réglage doit se voir à l'écran")]
        public void ParasiteCount_MatchesTheParameter()
        {
            foreach (int n in new[] { 3, 6, 16 })
            {
                var g = new DnaAnalysisGame(1, P(parasites: n));
                Assert.That(g.Nodes.Count(x => x.Parasite), Is.EqualTo(n));
                Assert.That(g.Nodes.Count, Is.EqualTo(2 * n + 6));
                Assert.That(g.Target, Is.EqualTo(n));
            }
        }

        [Test, Description("Vérifie que tous les nœuds sont à l'intérieur du cadre de jeu — parce qu'un nœud hors de l'écran serait injouable")]
        public void Positions_AreInsideTheField()
        {
            var g = new DnaAnalysisGame(1, P(parasites: 16));
            foreach (var n in g.Nodes)
            {
                Assert.That(n.X, Is.InRange(0.02f, 0.98f));
                Assert.That(n.Y, Is.InRange(0.1f, 0.9f));
            }
        }

        [Test, Description("Vérifie que toucher un parasite le retire et marque un point — parce que c'est le but du jeu")]
        public void TapParasite_RemovesIt_AndScores()
        {
            var g = new DnaAnalysisGame(1, P());
            var p = g.Nodes.First(n => n.Parasite);
            Assert.That(g.Tap(p.X, p.Y), Is.EqualTo(DnaTap.Removed));
            Assert.That(p.Removed, Is.True);
            Assert.That(g.Score, Is.EqualTo(1));
            Assert.That(g.Tap(p.X, p.Y), Is.Not.EqualTo(DnaTap.Removed), "un parasite déjà retiré ne se retire pas deux fois");
            Assert.That(g.Score, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que toucher un nœud sain coûte une vie sans rien retirer — parce que l'erreur doit se payer")]
        public void TapHealthyNode_LosesALife()
        {
            var g = new DnaAnalysisGame(1, P());
            var h = g.Nodes.First(n => !n.Parasite);
            Assert.That(g.Tap(h.X, h.Y), Is.EqualTo(DnaTap.Healthy));
            Assert.That(g.Lives, Is.EqualTo(2));
            Assert.That(g.Score, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un toucher loin de tout nœud ne fait rien et ne coûte rien — parce qu'un doigt qui glisse à côté ne doit pas être puni")]
        public void TapFarFromAnyNode_Misses()
        {
            var g = new DnaAnalysisGame(1, P());
            Assert.That(g.Tap(0.5f, 1.0f), Is.EqualTo(DnaTap.Miss));
            Assert.That(g.Lives, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que retirer tous les parasites donne la victoire, que le temps écoulé donne la défaite, et que perdre toutes les vies donne la défaite — parce que ce sont les trois issues")]
        public void WinTimeoutNoLives()
        {
            var win = new DnaAnalysisGame(1, P());
            foreach (var p in win.Nodes.Where(n => n.Parasite).ToList()) win.Tap(p.X, p.Y);
            Assert.That(win.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(win.Result().Success, Is.True);

            var timeout = new DnaAnalysisGame(1, P(time: 15));
            for (int i = 0; i < 2000 && timeout.Status == GameStatus.Running; i++) timeout.Step(0.01f);
            Assert.That(timeout.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(timeout.TimeLeft, Is.LessThanOrEqualTo(0f));

            var hurt = new DnaAnalysisGame(1, P(lives: 2));
            foreach (var h in hurt.Nodes.Where(n => !n.Parasite).Take(2).ToList()) hurt.Tap(h.X, h.Y);
            Assert.That(hurt.Status, Is.EqualTo(GameStatus.Lost));
        }

        [Test, Description("Vérifie que sur 200 graines et aux deux extrêmes de réglage le niveau se gagne toujours en touchant les parasites — parce qu'un niveau insoluble est un blocage")]
        public void EveryLevel_IsWinnable()
        {
            for (ulong seed = 1; seed <= 200; seed++)
                foreach (int n in new[] { 3, 16 })
                {
                    var g = new DnaAnalysisGame(seed, P(parasites: n, lives: 1));
                    foreach (var p in g.Nodes.Where(x => x.Parasite).ToList())
                        Assert.That(g.Tap(p.X, p.Y), Is.EqualTo(DnaTap.Removed), $"graine {seed}, {n} parasites : le nœud {p.Id} doit être atteignable");
                    Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
                }
        }

        [Test, Description("Vérifie que l'abandon perd la partie en cours — parce qu'on doit toujours pouvoir sortir")]
        public void Abandon_Loses()
        {
            var g = new DnaAnalysisGame(1, P());
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }
    }
}
