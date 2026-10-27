using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>« Le rythme » : une séquence d'ennemis arrive sur des pistes, on touche la bonne piste au bon moment.</summary>
    public sealed class RhythmGameTests
    {
        static ActivityParameters P(int notes = 24, int lanes = 3, int lives = 5, int speed = 100)
            => ActivityRegistry.Default.Resolve(new ActivityDef(new ActivityId("t"), ActivityKinds.Rhythm, "n", "d",
                new ActivityParameters(new System.Collections.Generic.Dictionary<string, int> { ["notes"] = notes, ["lanes"] = lanes, ["lives"] = lives, ["speed_percent"] = speed })));

        static void PlayPerfectly(RhythmGame g)
        {
            foreach (var note in g.Notes.ToList())
            {
                while (g.Song < note.Time && g.Status == GameStatus.Running) g.Step(0.01f);
                g.Press(note.Lane);
            }
        }

        [Test, Description("Vérifie que la même graine donne les mêmes notes (pistes et instants) et qu'une autre graine en donne d'autres — parce que le niveau doit être rejouable à l'identique en test")]
        public void SameSeed_SameNotes()
        {
            var a = new RhythmGame(7, P()); var b = new RhythmGame(7, P()); var c = new RhythmGame(8, P());
            Assert.That(a.Notes.Select(n => (n.Lane, n.Time)), Is.EqualTo(b.Notes.Select(n => (n.Lane, n.Time))));
            Assert.That(a.Notes.Select(n => (n.Lane, n.Time)), Is.Not.EqualTo(c.Notes.Select(n => (n.Lane, n.Time))));
        }

        [Test, Description("Vérifie que les notes sont dans l'ordre, espacées d'au moins 0,35 s au rythme normal, en nombre demandé et sur des pistes valides — parce qu'une séquence illisible serait injouable")]
        public void Notes_AreSortedSpacedAndInRange()
        {
            for (ulong seed = 1; seed <= 50; seed++)
            {
                var g = new RhythmGame(seed, P(notes: 40, lanes: 4));
                Assert.That(g.Notes.Count, Is.EqualTo(40));
                for (int i = 0; i < g.Notes.Count; i++)
                {
                    Assert.That(g.Notes[i].Lane, Is.InRange(0, 3));
                    if (i > 0) Assert.That(g.Notes[i].Time - g.Notes[i - 1].Time, Is.GreaterThanOrEqualTo(0.35f - 1e-4f));
                }
            }
        }

        [Test, Description("Vérifie qu'appuyer au bon instant sur la bonne piste pour chaque note donne la victoire avec toutes les notes marquées et toutes les vies — parce que c'est la partie parfaite")]
        public void PerfectPlay_Wins()
        {
            var g = new RhythmGame(3, P(notes: 20, lives: 4));
            PlayPerfectly(g);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(g.Score, Is.EqualTo(20));
            Assert.That(g.Perfects, Is.GreaterThan(15));
            Assert.That(g.Lives, Is.EqualTo(4));
            Assert.That(g.Result().Success, Is.True);
        }

        [Test, Description("Vérifie que ne rien toucher fait rater toutes les notes jusqu'à la perte des vies — parce qu'une partie passive est perdue")]
        public void NoPress_Loses()
        {
            var g = new RhythmGame(3, P(notes: 20, lives: 3));
            for (int i = 0; i < 6000 && g.Status == GameStatus.Running; i++) g.Step(0.01f);
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            Assert.That(g.Lives, Is.EqualTo(0));
            Assert.That(g.Result().Success, Is.False);
        }

        [Test, Description("Vérifie qu'un appui hors de toute fenêtre ou sur une mauvaise piste ne rend rien, ne coûte rien et ne marque rien — parce que spammer ne doit ni punir ni récompenser")]
        public void PressWithoutANoteInTheWindow_DoesNothing()
        {
            var g = new RhythmGame(3, P());
            var first = g.Notes[0];
            Assert.That(g.Press(first.Lane), Is.EqualTo(RhythmHit.None), "bien trop tôt");
            while (g.Song < first.Time) g.Step(0.01f);
            int wrongLane = (first.Lane + 1) % g.Lanes;
            if (g.Notes.Any(n => n.Lane == wrongLane && System.Math.Abs(n.Time - first.Time) <= RhythmGame.GoodWindow)) Assert.Pass("une autre note occupe la piste voisine");
            Assert.That(g.Press(wrongLane), Is.EqualTo(RhythmHit.None));
            Assert.That(g.Score, Is.EqualTo(0));
            Assert.That(g.Lives, Is.EqualTo(5));
        }

        [Test, Description("Vérifie qu'un appui dans la fenêtre « Bon » mais hors de la fenêtre « Parfait » marque le point sans compter comme parfait — parce que la précision est récompensée")]
        public void GoodWindow_ScoresButIsNotPerfect()
        {
            var g = new RhythmGame(3, P());
            var first = g.Notes[0];
            while (g.Song < first.Time - 0.12f) g.Step(0.01f);
            Assert.That(g.Song, Is.LessThan(first.Time - RhythmGame.PerfectWindow));
            Assert.That(g.Press(first.Lane), Is.EqualTo(RhythmHit.Good));
            Assert.That(g.Score, Is.EqualTo(1));
            Assert.That(g.Perfects, Is.EqualTo(0));
        }

        [Test, Description("Vérifie que l'abandon perd la partie en cours et que la partie n'a ni minuteur ni piège de temps (TimeLeft = -1) — parce que le rythme se joue au tempo, pas au chrono")]
        public void Abandon_Loses_AndThereIsNoTimer()
        {
            var g = new RhythmGame(3, P());
            Assert.That(g.TimeLeft, Is.EqualTo(-1f));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
            g.Abandon();
            Assert.That(g.Status, Is.EqualTo(GameStatus.Lost));
        }

        [Test, Description("Vérifie que le type d'activité « rhythm » est enregistré avec ses quatre réglages bornés (notes 8-60, pistes 3-4, vies 1-9, vitesse 50-220) — parce que les bornes protègent contre un réglage cassé")]
        public void Kind_IsRegisteredWithBoundedParameters()
        {
            var kind = ActivityRegistry.Default.Get(ActivityKinds.Rhythm);
            var s = kind.Schema.ToDictionary(p => p.Key);
            Assert.That((s["notes"].Min, s["notes"].Max), Is.EqualTo((8, 60)));
            Assert.That((s["lanes"].Min, s["lanes"].Max), Is.EqualTo((3, 4)));
            Assert.That((s["lives"].Min, s["lives"].Max), Is.EqualTo((1, 9)));
            Assert.That((s["speed_percent"].Min, s["speed_percent"].Max), Is.EqualTo((50, 220)));
        }
    }
}
