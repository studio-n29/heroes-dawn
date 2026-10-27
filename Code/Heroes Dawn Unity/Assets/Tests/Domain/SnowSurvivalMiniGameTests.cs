using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Survie en neige habillée en mini-jeu 2D : mêmes règles, entrées et abandon à la forme commune.</summary>
    public sealed class SnowSurvivalMiniGameTests
    {
        static (ActivityLevel Level, ActivityParameters Params) Level(int obstacles = 0, int distance = 14)
        {
            var def = new ActivityDef(new ActivityId("t"), ActivityKinds.SnowSurvival, "n", "d",
                new ActivityParameters(new System.Collections.Generic.Dictionary<string, int> { ["obstacle_count"] = obstacles, ["target_distance_m"] = distance }));
            var p = ActivityRegistry.Default.Resolve(def);
            var level = ActivityRegistry.Default.Get(ActivityKinds.SnowSurvival).Generate(new ActivityContext(def, p, new ZoneEnvironment(Biome.Snow, Weather.Snow), 5UL));
            return (level, p);
        }

        [Test, Description("Vérifie qu'un joueur qui file droit sur l'objet gagne avec le score 1 sur 1, sans vies — parce que trouver l'objet est le but")]
        public void WalkingToTheTarget_Wins()
        {
            var (level, p) = Level();
            var g = new SnowSurvivalMiniGame(level, p);
            for (int i = 0; i < 4000 && g.Status == GameStatus.Running; i++)
            {
                g.SetMove((g.Inner.Target - g.Inner.Position).Normalized);
                g.Step(0.02f);
            }
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Result().Success, Is.True);
            Assert.That((g.Score, g.Target, g.Lives), Is.EqualTo((1, 1, -1)));
        }

        [Test, Description("Vérifie que rester immobile jusqu'au bout de la jauge ou du temps perd la partie, et que le temps restant est celui du jeu — parce que la survie est une course")]
        public void Idle_Loses_AndExposesTheTimeLeft()
        {
            var (level, p) = Level(distance: 60);
            var g = new SnowSurvivalMiniGame(level, p);
            float before = g.TimeLeft;
            g.Step(1f);
            Assert.That(g.TimeLeft, Is.EqualTo(before - 1f).Within(1e-3f));
            for (int i = 0; i < 20000 && g.Status == GameStatus.Running; i++) g.Step(0.05f);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(g.Score, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que l'abandon perd la partie en cours, qu'un second abandon ne change rien et que le jeu n'avance plus ensuite — parce qu'on doit toujours pouvoir sortir")]
        public void Abandon_Loses_AndFreezesTheGame()
        {
            var (level, p) = Level();
            var g = new SnowSurvivalMiniGame(level, p);
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            float time = g.TimeLeft;
            g.SetMove(new Vec2f(1f, 0f));
            g.Step(1f);
            Assert.That(g.TimeLeft, Is.EqualTo(time));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }

        [Test, Description("Vérifie qu'une direction plus longue que 1 est ramenée à 1 (même distance parcourue qu'une direction unitaire) — parce qu'un joystick ne doit pas donner de bonus de vitesse")]
        public void Move_IsClampedToUnit()
        {
            var (level, p) = Level();
            var a = new SnowSurvivalMiniGame(level, p); var b = new SnowSurvivalMiniGame(level, p);
            a.SetMove(new Vec2f(5f, 0f)); b.SetMove(new Vec2f(1f, 0f));
            a.Step(0.2f); b.Step(0.2f);
            Assert.That(a.Inner.Position.X, Is.EqualTo(b.Inner.Position.X).Within(1e-4f));
        }
    }
}
