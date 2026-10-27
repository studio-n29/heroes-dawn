using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Tir franc » : maintenir pour faire aller et venir la jauge de puissance, relâcher pour lancer vers la cible.</summary>
    public sealed class FreeThrowGameTests
    {
        static ActivityParameters P(int throws = 5, int hits = 3, int speed = 100)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.FreeThrow, "n", "d",
                new ActivityParameters(new Dictionary<string, int> { ["throws"] = throws, ["hits"] = hits, ["speed_percent"] = speed })));

        /// <summary>Charge puis relâche quand la jauge est sur le centre de la cible ; rend vrai si le lancer touche.</summary>
        static bool ThrowAtTheCenter(FreeThrowGame g)
        {
            g.BeginCharge();
            for (int i = 0; i < 4000 && System.Math.Abs(g.Gauge - g.TargetCenter) > 0.01f; i++) g.Step(0.005f);
            return g.Release();
        }

        [Test, Description("Vérifie que la même graine donne la même suite de cibles — parce que le niveau doit être rejouable à l'identique en test")]
        public void SameSeed_SameTargets()
        {
            var a = new FreeThrowGame(5, P()); var b = new FreeThrowGame(5, P());
            Assert.That(a.TargetCenter, Is.EqualTo(b.TargetCenter));
            ThrowAtTheCenter(a); ThrowAtTheCenter(b);
            Assert.That(a.TargetCenter, Is.EqualTo(b.TargetCenter));
        }

        [Test, Description("Vérifie que sur 200 graines la cible est toujours atteignable par la jauge (zone entière dans [0, 1]) — parce qu'un niveau injouable est un blocage")]
        public void Target_IsAlwaysReachable()
        {
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var g = new FreeThrowGame(seed, P(throws: 12, hits: 8));
                for (int i = 0; i < 12 && g.Status == GameStatus.Running; i++)
                {
                    Assert.That(g.TargetCenter - g.TargetHalfWidth, Is.GreaterThanOrEqualTo(0f));
                    Assert.That(g.TargetCenter + g.TargetHalfWidth, Is.LessThanOrEqualTo(1f));
                    ThrowAtTheCenter(g);
                }
            }
        }

        [Test, Description("Vérifie qu'un relâchement quand la jauge est sur le centre de la cible touche, marque un point et consomme un lancer — parce que c'est le geste attendu")]
        public void PerfectPower_Hits()
        {
            var g = new FreeThrowGame(2, P());
            Assert.That(ThrowAtTheCenter(g), Is.True);
            Assert.That(g.Score, Is.EqualTo(1));
            Assert.That(g.ThrowsLeft, Is.EqualTo(4));
            Assert.That(g.LastHit, Is.True);
        }

        [Test, Description("Vérifie qu'un lancer à mauvaise puissance rate sans marquer mais consomme quand même un lancer — parce que chaque essai compte")]
        public void WrongPower_Misses_AndConsumesAThrow()
        {
            var g = new FreeThrowGame(2, P());
            g.BeginCharge();                       // jauge à 0 : trop faible pour une cible à 0,3 ou plus
            Assert.That(g.Release(), Is.False);
            Assert.That(g.Score, Is.EqualTo(0));
            Assert.That(g.ThrowsLeft, Is.EqualTo(4));
        }

        [Test, Description("Vérifie que toucher le nombre de cibles demandé donne la victoire avec le bon score — parce que c'est l'objectif")]
        public void EnoughHits_Wins()
        {
            var g = new FreeThrowGame(9, P(throws: 5, hits: 3));
            while (g.Status == GameStatus.Running) ThrowAtTheCenter(g);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Score, Is.EqualTo(3));
            Assert.That(g.Result().Success, Is.True);
        }

        [Test, Description("Vérifie que rater tous les lancers donne la défaite — parce que les essais sont limités")]
        public void ThrowsExhausted_Loses()
        {
            var g = new FreeThrowGame(9, P(throws: 3, hits: 2));
            for (int i = 0; i < 3; i++) { g.BeginCharge(); g.Release(); }
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(g.ThrowsLeft, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un relâchement sans charge ne fait rien et que la jauge repart de zéro à chaque charge — parce qu'un appui fantôme ne doit pas gâcher un lancer")]
        public void Release_WithoutCharge_DoesNothing_AndGaugeRestartsAtZero()
        {
            var g = new FreeThrowGame(2, P());
            Assert.That(g.Release(), Is.False);
            Assert.That(g.ThrowsLeft, Is.EqualTo(5));
            g.BeginCharge();
            for (int i = 0; i < 50; i++) g.Step(0.01f);
            Assert.That(g.Gauge, Is.GreaterThan(0.3f));
            g.Release();
            g.BeginCharge();
            Assert.That(g.Gauge, Is.EqualTo(0f));
        }

        [Test, Description("Vérifie que la jauge reste dans [0, 1] et fait l'aller-retour — parce que la puissance ne peut pas sortir de l'échelle")]
        public void Gauge_StaysInRange_AndBounces()
        {
            var g = new FreeThrowGame(2, P(speed: 220));
            g.BeginCharge();
            float max = 0f; bool wentDown = false; float prev = 0f;
            for (int i = 0; i < 600; i++)
            {
                g.Step(0.01f);
                Assert.That(g.Gauge, Is.InRange(0f, 1f));
                if (g.Gauge < prev - 1e-4f) wentDown = true;
                max = System.Math.Max(max, g.Gauge); prev = g.Gauge;
            }
            Assert.That(max, Is.GreaterThan(0.95f));
            Assert.That(wentDown, Is.True);
        }

        [Test, Description("Vérifie que le nombre de touches demandé est plafonné au nombre de lancers (8 touches pour 2 lancers = 2) — parce qu'une cible hors d'atteinte rendrait la partie impossible")]
        public void HitsParam_IsClampedToThrows()
        {
            var g = new FreeThrowGame(2, P(throws: 2, hits: 8));
            Assert.That(g.Target, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que l'abandon perd la partie en cours et qu'il n'y a pas de minuteur — parce que le tir se joue au geste")]
        public void Abandon_Loses_NoTimer()
        {
            var g = new FreeThrowGame(2, P());
            Assert.That(g.TimeLeft, Is.EqualTo(-1f));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }
    }
}
