using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Infiltration » : vue de dessus, déplacement à 360°, gardes à champ de vision, atteindre la sortie sans être repéré.</summary>
    public sealed class InfiltrationGameTests
    {
        static ActivityParameters P(int time = 60, int guards = 3, int lives = 3, int detect = 600)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.Infiltration, "n", "d",
                new ActivityParameters(new Dictionary<string, int> { ["time_limit_s"] = time, ["guards"] = guards, ["lives"] = lives, ["detect_ms"] = detect })));

        static bool Inside(InfiltrationGame.Wall w, Vec2f p, float pad)
            => p.X >= w.X - pad && p.X <= w.X + w.W + pad && p.Y >= w.Y - pad && p.Y <= w.Y + w.H + pad;

        /// <summary>Vérificateur indépendant : parcours en largeur sur une grille 32×20 en gonflant les murs du rayon du joueur.</summary>
        static bool Reachable(InfiltrationGame g)
        {
            const int cols = 32, rows = 20;
            Vec2f Center(int c, int r) => new Vec2f((c + 0.5f) * InfiltrationGame.Width / cols, (r + 0.5f) / rows);
            bool Blocked(int c, int r) => g.Walls.Any(w => Inside(w, Center(c, r), InfiltrationGame.PlayerRadius));
            (int, int) CellOf(Vec2f p) => ((int)(p.X / (InfiltrationGame.Width / cols)), (int)(p.Y * rows));
            var (sc, sr) = CellOf(g.Start); var (ec, er) = CellOf(g.Exit);
            var seen = new HashSet<(int, int)> { (sc, sr) }; var todo = new Queue<(int, int)>(); todo.Enqueue((sc, sr));
            while (todo.Count > 0)
            {
                var (c, r) = todo.Dequeue();
                if (c == ec && r == er) return true;
                foreach (var (dc, dr) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) })
                {
                    int nc = c + dc, nr = r + dr;
                    if (nc < 0 || nc >= cols || nr < 0 || nr >= rows || Blocked(nc, nr) || !seen.Add((nc, nr))) continue;
                    todo.Enqueue((nc, nr));
                }
            }
            return false;
        }

        [Test, Description("Vérifie que la même graine donne le même niveau (murs, gardes, sortie) — parce que le niveau doit être rejouable à l'identique")]
        public void SameSeed_SameLevel()
        {
            var a = new InfiltrationGame(5, P()); var b = new InfiltrationGame(5, P());
            Assert.That(a.Walls.Select(w => (w.X, w.Y, w.W, w.H)), Is.EqualTo(b.Walls.Select(w => (w.X, w.Y, w.W, w.H))));
            Assert.That(a.Guards.Select(g => (g.Pos.X, g.Pos.Y)), Is.EqualTo(b.Guards.Select(g => (g.Pos.X, g.Pos.Y))));
            Assert.That((a.Exit.X, a.Exit.Y), Is.EqualTo((b.Exit.X, b.Exit.Y)));
        }

        [Test, Description("Vérifie que sur 200 graines, avec le minimum et le maximum de gardes, la sortie reste accessible depuis le départ en contournant les murs — parce qu'un niveau infranchissable est un blocage")]
        public void Exit_IsReachable_ForManySeeds()
        {
            for (ulong seed = 1; seed <= 200; seed++)
                foreach (int guards in new[] { 1, 8 })
                    Assert.That(Reachable(new InfiltrationGame(seed, P(guards: guards))), Is.True, $"graine {seed}, {guards} gardes");
        }

        [Test, Description("Vérifie que le départ et la sortie ne sont jamais dans un mur et que les gardes sont loin du départ — parce qu'on ne doit pas être repéré dès la première image")]
        public void StartAndExit_AreFree_AndGuardsAreFarFromTheStart()
        {
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var g = new InfiltrationGame(seed, P(guards: 8));
                Assert.That(g.Walls.Any(w => Inside(w, g.Start, InfiltrationGame.PlayerRadius)), Is.False, $"départ, graine {seed}");
                Assert.That(g.Walls.Any(w => Inside(w, g.Exit, InfiltrationGame.PlayerRadius)), Is.False, $"sortie, graine {seed}");
                Assert.That(g.Guards.All(x => Vec2f.Distance(x.Pos, g.Start) > InfiltrationGame.Range + 0.05f), Is.True, $"gardes, graine {seed}");
                Assert.That(g.Guards.Count, Is.EqualTo(8));
            }
        }

        [Test, Description("Vérifie que le joueur ne traverse pas un mur et reste dans le monde — parce que les murs bloquent le déplacement")]
        public void Player_CannotWalkThroughWalls_OrOutOfTheWorld()
        {
            for (ulong seed = 1; seed <= 40; seed++)
            {
                var g = new InfiltrationGame(seed, P(time: 120, lives: 9));
                for (int dir = 0; dir < 4; dir++)
                {
                    g.SetMove(dir == 0 ? new Vec2f(1, 0) : dir == 1 ? new Vec2f(-1, 0) : dir == 2 ? new Vec2f(0, 1) : new Vec2f(0, -1));
                    for (int i = 0; i < 300 && g.Status == GameStatus.Running; i++)
                    {
                        g.Step(0.02f);
                        Assert.That(g.Player.X, Is.InRange(0f, InfiltrationGame.Width));
                        Assert.That(g.Player.Y, Is.InRange(0f, 1f));
                        Assert.That(g.Walls.Any(w => Inside(w, g.Player, InfiltrationGame.PlayerRadius * 0.7f)), Is.False, $"graine {seed}");
                    }
                }
            }
        }

        [Test, Description("Vérifie qu'une direction plus longue que 1 ne fait pas aller plus vite que la vitesse maximale — parce qu'un joystick diagonal ne doit pas donner un bonus de vitesse")]
        public void Move_IsClampedToUnitSpeed()
        {
            var g = new InfiltrationGame(5, P(guards: 1));
            var from = g.Player;
            g.SetMove(new Vec2f(5f, 5f));
            g.Step(0.1f);
            Assert.That(Vec2f.Distance(from, g.Player), Is.LessThanOrEqualTo(InfiltrationGame.Speed * 0.1f + 1e-4f));
        }

        static string WallsText(InfiltrationGame g) => string.Join(" ; ", g.Walls.Select(w => w.X.ToString("0.000") + "," + w.Y.ToString("0.000") + "," + w.W.ToString("0.000") + "," + w.H.ToString("0.000")));

        static Vec2f Dir(float angle) => new Vec2f((float)System.Math.Cos(angle), (float)System.Math.Sin(angle));

        /// <summary>Vrai si un mur coupe le segment (échantillonnage fin) : vérificateur indépendant de la ligne de vue.</summary>
        static bool Blocked(InfiltrationGame g, Vec2f from, Vec2f to)
        {
            for (int i = 1; i < 200; i++)
            {
                var p = from + (to - from) * (i / 200f);
                if (g.Walls.Any(w => Inside(w, p, 0f))) return true;
            }
            return false;
        }

        /// <summary>Un niveau et un garde pour lesquels un point à 0,1 devant le garde est bien vu.</summary>
        static (InfiltrationGame Game, InfiltrationGame.Guard Guard) OpenLevel(ActivityParameters p)
        {
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var g = new InfiltrationGame(seed, p);
                foreach (var guard in g.Guards)
                    if (g.Sees(guard, guard.Pos + Dir(guard.Facing) * 0.1f)) return (g, guard);
            }
            Assert.Fail("aucun niveau où un garde voit devant lui");
            throw new System.InvalidOperationException();
        }

        [Test, Description("Vérifie que, dans le cône et à portée, un garde voit exactement ce qu'aucun mur ne cache (comparé à un échantillonnage indépendant), et qu'il ne voit ni hors portée ni dans son dos — parce que ce sont les règles du champ de vision")]
        public void Guard_Sees_MatchesLineOfSight_AndNotOutsideTheCone()
        {
            int seen = 0, hidden = 0;
            for (ulong seed = 1; seed <= 60; seed++)
            {
                var g = new InfiltrationGame(seed, P(guards: 3));
                foreach (var guard in g.Guards)
                {
                    foreach (float a in new[] { -0.5f, -0.25f, 0f, 0.25f, 0.5f })
                        foreach (float d in new[] { 0.1f, 0.2f, 0.3f })
                        {
                            var p = guard.Pos + Dir(guard.Facing + a) * d;
                            if (g.Walls.Any(w => Inside(w, p, 0.01f))) continue; // point collé à un mur : le résultat dépend de la précision d'échantillonnage
                            bool expected = !Blocked(g, guard.Pos, p);
                            Assert.That(g.Sees(guard, p), Is.EqualTo(expected), $"graine {seed}, angle {a}, distance {d}, garde {guard.Pos} face {guard.Facing}, p {p}, murs {WallsText(g)}");
                            if (expected) seen++; else hidden++;
                        }
                    Assert.That(g.Sees(guard, guard.Pos + Dir(guard.Facing) * (InfiltrationGame.Range * 1.6f)), Is.False, "hors portée");
                    Assert.That(g.Sees(guard, guard.Pos + Dir(guard.Facing + 3.14159f) * 0.1f), Is.False, "dans le dos");
                }
            }
            Assert.That(seen, Is.GreaterThan(0));
            Assert.That(hidden, Is.GreaterThan(0), "au moins un cas masqué par un mur est exercé");
        }

        [Test, Description("Vérifie qu'être vu cumule l'exposition jusqu'au seuil, coûte alors une vie, ramène au départ et remet l'exposition à zéro — parce que c'est la sanction de l'infiltration")]
        public void Exposure_AccumulatesThenCostsALife_AndResets()
        {
            var (g, guard) = OpenLevel(P(guards: 1, detect: 200, lives: 3));
            float peak = 0f;
            for (int i = 0; i < 60 && g.Lives == 3; i++)
            {
                g.PlaceForTests(guard.Pos + Dir(guard.Facing) * 0.1f);
                g.Step(0.02f);
                peak = System.Math.Max(peak, g.Exposure);
            }
            Assert.That(peak, Is.GreaterThan(0f), "l'exposition a cumulé");
            Assert.That(g.Lives, Is.EqualTo(2));
            Assert.That(g.Exposure, Is.EqualTo(0f));
            Assert.That(Vec2f.Distance(g.Player, g.Start), Is.LessThan(0.01f), "retour au départ");
        }

        [Test, Description("Vérifie que l'exposition décroît hors de vue — parce qu'on peut se cacher pour récupérer")]
        public void Exposure_DecaysOutOfSight()
        {
            var g = new InfiltrationGame(5, P(guards: 1, detect: 2000));
            g.PlaceForTests(g.Start);
            g.SetExposureForTests(500f);
            g.Step(0.1f);
            Assert.That(g.Exposure, Is.LessThan(500f));
        }

        [Test, Description("Vérifie qu'arriver sur la sortie donne la victoire, que le temps écoulé donne la défaite et que perdre toutes les vies donne la défaite — parce que ce sont les trois issues")]
        public void WinTimeoutNoLives()
        {
            var win = new InfiltrationGame(5, P(guards: 1));
            win.PlaceForTests(win.Exit);
            win.Step(0.02f);
            Assert.That(win.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(win.Result().Success, Is.True);

            var timeout = new InfiltrationGame(5, P(time: 20, guards: 1));
            for (int i = 0; i < 1200 && timeout.Status == GameStatus.Running; i++) timeout.Step(0.02f);
            Assert.That(timeout.Status, Is.EqualTo(GameStatus.Lost));

            var (caught, gd) = OpenLevel(P(guards: 1, lives: 1, detect: 200));
            for (int i = 0; i < 400 && caught.Status == GameStatus.Running; i++)
            {
                caught.PlaceForTests(gd.Pos + Dir(gd.Facing) * 0.1f);
                caught.Step(0.05f);
            }
            Assert.That(caught.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(caught.Lives, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que l'abandon perd la partie en cours — parce qu'on doit toujours pouvoir sortir")]
        public void Abandon_Loses()
        {
            var g = new InfiltrationGame(5, P());
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }
    }
}
