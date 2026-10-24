using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Activités du jour : des objectifs quotidiens sur les zones déjà terminées.</summary>
    public sealed class DailyActivitiesTests
    {
        TestApp _app = null!;
        QuestLog Q => _app.Quests;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [Test, Description("Vérifie qu'aucune activité n'apparaît tant qu'aucune zone n'est terminée — parce que les activités se tirent parmi les zones terminées")]
        public void NoZoneDone_NoActivity()
        {
            Assert.That(Q.Activities(), Is.Empty);
        }

        [Test, Description("Vérifie qu'une zone terminée donne une activité du jour sur cette zone, et que le nombre d'activités suit le nombre de zones jusqu'à trois — parce que le design veut quelques activités par jour")]
        public void ActivitiesFollowCompletedZones()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var one = Q.Activities();
            Assert.That(one.Count, Is.EqualTo(1));
            Assert.That(one[0].ZoneId, Is.EqualTo("zone_ny_1_1"));
            Assert.That(QuestLog.ActivityCount(2), Is.EqualTo(2));
            Assert.That(QuestLog.ActivityCount(9), Is.EqualTo(3));
        }

        [Test, Description("Vérifie que la progression suit les combats gagnés et validations de la bonne zone, puis qu'on réclame la récompense une seule fois — parce que c'est la boucle quotidienne")]
        public void ProgressThenClaimOnce()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var a = Q.Activities()[0];
            for (int i = 0; i < a.Target; i++) { Q.RecordCombatWon("zone_ny_1_1"); Q.RecordZoneValidated("zone_ny_1_1"); }
            Q.RecordCombatWon("zone_ny_1_2");
            var done = Q.Activities()[0];
            Assert.That(done.Complete, Is.True);
            Assert.That(done.Progress, Is.EqualTo(done.Target), "la progression s'arrête à l'objectif");
            int credits = _app.Session.Wallet.Credits;
            Assert.That(Q.ClaimActivity(0).IsSuccess);
            Assert.That(_app.Session.Wallet.Credits, Is.GreaterThan(credits));
            Assert.That(Q.ClaimActivity(0).IsFailure, "déjà réclamée");
            Assert.That(Q.ClaimActivity(5).IsFailure);
        }

        [Test, Description("Vérifie qu'on ne réclame pas une activité inachevée — parce que la récompense se mérite")]
        public void CannotClaimBeforeTheGoal()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Q.Activities();
            Assert.That(Q.ClaimActivity(0).IsFailure);
        }

        [Test, Description("Vérifie qu'un nouveau jour renouvelle les activités — parce qu'elles sont quotidiennes")]
        public void NewDay_RenewsActivities()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Q.Activities();
            Q.RecordCombatWon("zone_ny_1_1");
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(1);
            _app.Time.UtcNow = _app.Time.UtcNow.AddDays(1);
            Assert.That(Q.Activities()[0].Progress, Is.EqualTo(0));
        }
    }
}
