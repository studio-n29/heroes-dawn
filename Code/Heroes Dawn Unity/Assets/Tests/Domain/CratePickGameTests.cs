using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Caisse au choix » : trois caisses scellées, un curseur en va-et-vient ; on charge puis on relâche pour frapper la caisse sous le curseur.</summary>
    public sealed class CratePickGameTests
    {
        static ActivityParameters P(int rounds = 3, int lives = 3, int speed = 100)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.CratePick, "n", "d",
                new ActivityParameters(new Dictionary<string, int> { ["rounds"] = rounds, ["lives"] = lives, ["speed_percent"] = speed })));

        /// <summary>Charge, attend que le curseur soit au-dessus de la caisse <paramref name="crate"/> puis relâche ; rend vrai si c'est la bonne caisse.</summary>
        static bool Strike(CratePickGame g, int crate)
        {
            g.BeginCharge();
            for (int i = 0; i < 40; i++) g.Step(0.01f);
            for (int i = 0; i < 2000 && CratePickGame.CrateAt(g.Cursor) != crate; i++) g.Step(0.005f);
            return g.Release();
        }

        [Test, Description("Vérifie que le même numéro de graine donne la même suite de caisses récompense — parce que le niveau doit être rejouable à l'identique")]
        public void SameSeed_SameRewards()
        {
            var a = new CratePickGame(4, P(rounds: 6)); var b = new CratePickGame(4, P(rounds: 6));
            var ra = new List<int>(); var rb = new List<int>();
            for (int i = 0; i < 5; i++) { ra.Add(a.Reward); rb.Add(b.Reward); Strike(a, a.Reward); Strike(b, b.Reward); }
            Assert.That(ra, Is.EqualTo(rb));
        }

        [Test, Description("Vérifie que la caisse récompense est montrée au début de chaque manche puis cachée — parce que le joueur doit la retenir")]
        public void Reward_IsRevealedThenHidden()
        {
            var g = new CratePickGame(4, P());
            Assert.That(g.Revealed, Is.True);
            for (int i = 0; i < 150; i++) g.Step(0.01f);
            Assert.That(g.Revealed, Is.False);
            Strike(g, g.Reward);
            Assert.That(g.Revealed, Is.True, "nouvelle manche : la nouvelle récompense est montrée");
        }

        [Test, Description("Vérifie que le curseur reste entre 0 et 1 et fait des allers-retours à vitesse constante — parce que c'est le motif ping-pong du PDF")]
        public void Cursor_StaysInRange_AndBounces()
        {
            var g = new CratePickGame(4, P());
            float min = 1f, max = 0f; float prev = g.Cursor; int turns = 0; float dir = 0f;
            for (int i = 0; i < 1000; i++)
            {
                g.Step(0.01f);
                Assert.That(g.Cursor, Is.InRange(0f, 1f));
                min = System.Math.Min(min, g.Cursor); max = System.Math.Max(max, g.Cursor);
                float d = System.Math.Sign(g.Cursor - prev);
                if (d != 0 && dir != 0 && d != dir) turns++;
                if (d != 0) dir = d;
                prev = g.Cursor;
            }
            Assert.That(min, Is.LessThan(0.02f));
            Assert.That(max, Is.GreaterThan(0.98f));
            Assert.That(turns, Is.GreaterThanOrEqualTo(3));
        }

        [Test, Description("Vérifie qu'un relâchement après une charge trop courte ne frappe rien — parce qu'un simple effleurement ne doit pas coûter une vie")]
        public void Release_BelowMinCharge_DoesNothing()
        {
            var g = new CratePickGame(4, P());
            g.BeginCharge();
            g.Step(0.05f);
            Assert.That(g.Release(), Is.False);
            Assert.That(g.Lives, Is.EqualTo(3));
            Assert.That(g.LastStruck, Is.EqualTo(-1));
            Assert.That(g.Charging, Is.False);
        }

        [Test, Description("Vérifie que la caisse frappée est celle sous le curseur au relâchement (tiers gauche, milieu, droit) — parce que la visée est le cœur du jeu")]
        public void Strike_HitsTheCrateUnderTheCursor()
        {
            Assert.That(CratePickGame.CrateAt(0.1f), Is.EqualTo(0));
            Assert.That(CratePickGame.CrateAt(0.5f), Is.EqualTo(1));
            Assert.That(CratePickGame.CrateAt(0.9f), Is.EqualTo(2));
            var g = new CratePickGame(4, P());
            for (int crate = 0; crate < 3 && g.Status == GameStatus.Running; crate++)
            {
                g.BeginCharge();
                for (int i = 0; i < 40; i++) g.Step(0.01f);
                for (int i = 0; i < 2000 && CratePickGame.CrateAt(g.Cursor) != crate; i++) g.Step(0.005f);
                int under = CratePickGame.CrateAt(g.Cursor);
                g.Release();
                Assert.That(g.LastStruck, Is.EqualTo(under));
            }
        }

        [Test, Description("Vérifie que frapper la caisse récompense marque un point et lance la manche suivante, et que frapper une autre caisse coûte une vie — parce que c'est la règle du jeu")]
        public void RewardScores_OtherCostsALife()
        {
            var g = new CratePickGame(4, P(rounds: 5));
            Assert.That(Strike(g, g.Reward), Is.True);
            Assert.That(g.Score, Is.EqualTo(1));
            Assert.That(g.Round, Is.EqualTo(1));
            int other = (g.Reward + 1) % 3;
            Assert.That(Strike(g, other), Is.False);
            Assert.That(g.Lives, Is.EqualTo(2));
            Assert.That(g.Score, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que réussir toutes les manches donne la victoire et que perdre toutes les vies donne la défaite — parce que ce sont les deux issues")]
        public void AllRoundsWon_Wins_NoLivesLeft_Loses()
        {
            var win = new CratePickGame(4, P(rounds: 3));
            while (win.Status == GameStatus.Running) Strike(win, win.Reward);
            Assert.That(win.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(win.Result().Success, Is.True);
            var lose = new CratePickGame(4, P(rounds: 3, lives: 2));
            while (lose.Status == GameStatus.Running) Strike(lose, (lose.Reward + 1) % 3);
            Assert.That(lose.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(lose.Lives, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que l'abandon perd la partie et qu'il n'y a pas de minuteur — parce que le jeu se joue au geste")]
        public void Abandon_Loses_NoTimer()
        {
            var g = new CratePickGame(4, P());
            Assert.That(g.TimeLeft, Is.EqualTo(-1f));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }
    }
}
