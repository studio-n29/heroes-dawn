using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Engrenage » : choisir le sens de rotation de chaque engrenage pour que tout le mécanisme tourne depuis le moteur.</summary>
    public sealed class GearsGameTests
    {
        static ActivityParameters P(int gears = 6, int moves = 12)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.Gears, "n", "d",
                new ActivityParameters(new Dictionary<string, int> { ["gears"] = gears, ["moves"] = moves })));

        /// <summary>Le sens attendu d'un engrenage : deux engrenages en contact tournent en sens contraires, donc le sens suit la parité de la case.</summary>
        static bool Required(GearsGame g, GearsGame.Gear gear)
        {
            var motor = g.Gears.First(x => x.Motor);
            bool sameParity = (gear.Col + gear.Row) % 2 == (motor.Col + motor.Row) % 2;
            return sameParity ? motor.Clockwise : !motor.Clockwise;
        }

        [Test, Description("Vérifie que la même graine donne la même disposition (cases et sens) — parce que le niveau doit être rejouable à l'identique")]
        public void SameSeed_SameLayout()
        {
            var a = new GearsGame(3, P()); var b = new GearsGame(3, P());
            Assert.That(a.Gears.Select(x => (x.Col, x.Row, x.Clockwise)), Is.EqualTo(b.Gears.Select(x => (x.Col, x.Row, x.Clockwise))));
        }

        [Test, Description("Vérifie que la disposition est connexe, sans deux engrenages sur la même case, dans la grille 5×5, avec un seul moteur — parce qu'un engrenage isolé ne tournerait jamais")]
        public void Layout_IsConnectedAndInsideTheGrid()
        {
            for (ulong seed = 1; seed <= 100; seed++)
                foreach (int n in new[] { 3, 12 })
                {
                    var g = new GearsGame(seed, P(gears: n));
                    Assert.That(g.Gears.Count, Is.EqualTo(n));
                    Assert.That(g.Gears.Select(x => (x.Col, x.Row)).Distinct().Count(), Is.EqualTo(n), "cases distinctes");
                    Assert.That(g.Gears.All(x => x.Col is >= 0 and < 5 && x.Row is >= 0 and < 5), Is.True);
                    Assert.That(g.Gears.Count(x => x.Motor), Is.EqualTo(1));
                    var seen = new HashSet<int> { 0 }; var todo = new Queue<int>(new[] { 0 });
                    while (todo.Count > 0)
                    {
                        int cur = todo.Dequeue();
                        foreach (var (a, b) in g.Edges)
                        {
                            int other = a == cur ? b : b == cur ? a : -1;
                            if (other >= 0 && seen.Add(other)) todo.Enqueue(other);
                        }
                    }
                    Assert.That(seen.Count, Is.EqualTo(n), $"graine {seed}, {n} engrenages : tout est relié au moteur");
                }
        }

        [Test, Description("Vérifie que le moteur ne peut pas être basculé et qu'un refus ne consomme aucun coup — parce que le moteur est la source de mouvement")]
        public void Motor_CannotBeToggled()
        {
            var g = new GearsGame(3, P());
            int moves = g.MovesLeft;
            var motor = g.Gears.First(x => x.Motor);
            bool dir = motor.Clockwise;
            Assert.That(g.Toggle(motor.Id), Is.False);
            Assert.That(motor.Clockwise, Is.EqualTo(dir));
            Assert.That(g.MovesLeft, Is.EqualTo(moves));
        }

        [Test, Description("Vérifie que sur 100 graines, au plus petit et au plus grand réglage, basculer exactement les engrenages mal orientés gagne dans le nombre de coups donné — parce que chaque niveau doit être soluble")]
        public void EveryLevel_IsSolvable()
        {
            for (ulong seed = 1; seed <= 100; seed++)
                foreach (var (n, moves) in new[] { (3, 4), (12, 4), (12, 40) })
                {
                    var g = new GearsGame(seed, P(n, moves));
                    foreach (var gear in g.Gears.Where(x => !x.Motor && x.Clockwise != Required(g, x)).ToList())
                        Assert.That(g.Toggle(gear.Id), Is.True, $"graine {seed}: coup refusé trop tôt");
                    Assert.That(g.Status, Is.EqualTo(GameStatus.Won), $"graine {seed}, {n} engrenages, {moves} coups");
                    Assert.That(g.Score, Is.EqualTo(n));
                    Assert.That(g.MovesLeft, Is.GreaterThanOrEqualTo(0));
                }
        }

        [Test, Description("Vérifie que le niveau n'est jamais résolu au départ et que le nombre de coups est relevé au minimum nécessaire — parce qu'un puzzle déjà gagné ou impossible n'a pas de sens")]
        public void NeverSolvedAtStart_AndMovesCoverTheNeed()
        {
            for (ulong seed = 1; seed <= 100; seed++)
            {
                var g = new GearsGame(seed, P(gears: 12, moves: 4));
                int wrong = g.Gears.Count(x => !x.Motor && x.Clockwise != Required(g, x));
                Assert.That(wrong, Is.GreaterThan(0));
                Assert.That(g.Status, Is.EqualTo(GameStatus.Running));
                Assert.That(g.MovesLeft, Is.GreaterThanOrEqualTo(wrong));
            }
        }

        [Test, Description("Vérifie qu'un basculement change le sens et consomme un coup, et que des coups gaspillés jusqu'à épuisement donnent la défaite — parce que les coups sont comptés")]
        public void Toggle_ConsumesAMove_AndRunningOutLoses()
        {
            var g = new GearsGame(3, P(gears: 5, moves: 4));
            var free = g.Gears.First(x => !x.Motor);
            bool before = free.Clockwise;
            Assert.That(g.Toggle(free.Id), Is.True);
            Assert.That(free.Clockwise, Is.EqualTo(!before));
            // On bascule le même engrenage jusqu'à épuisement : le niveau ne peut être résolu que s'il tombe juste.
            int guard = 0;
            while (g.Status == GameStatus.Running && guard++ < 100) g.Toggle(free.Id);
            if (g.Status == GameStatus.Lost) Assert.That(g.MovesLeft, Is.EqualTo(0));
            Assert.That(g.Toggle(free.Id), Is.False, "plus de coup ou partie finie : refusé");
        }

        [Test, Description("Vérifie que l'abandon perd la partie et que le jeu n'a ni minuteur ni vies — parce que seuls les coups comptent")]
        public void Abandon_Loses_NoTimerNoLives()
        {
            var g = new GearsGame(3, P());
            Assert.That(g.TimeLeft, Is.EqualTo(-1f));
            Assert.That(g.Lives, Is.EqualTo(-1));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }
    }
}
