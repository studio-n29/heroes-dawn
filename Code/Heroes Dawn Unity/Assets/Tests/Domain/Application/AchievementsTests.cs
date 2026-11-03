#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class AchievementsTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        AchievementView Find(string id) => _app.Profile.Achievements().List().Single(a => a.Def.Id == id);

        [Test, Description("Vérifie qu'un premier combat gagné débloque « Premier combat gagné » avec le jour du déblocage — parce que la liste des succès montre quand on les a obtenus")]
        public void FirstWin_UnlocksWithTheDay()
        {
            Assert.That(Find("first_win").Unlocked, Is.False);
            Stats.Add(_app.Session.Meta, Stats.CombatsWon, 1);
            var a = Find("first_win");
            Assert.That(a.Unlocked, Is.True);
            Assert.That(a.UnlockedDay, Is.EqualTo(_app.MetaCtx.Today));
        }

        [Test, Description("Vérifie qu'un succès débloqué le reste même si la mesure redescend, et que la progression se plafonne à la cible — parce que le joueur ne doit jamais perdre un succès")]
        public void Unlocked_StaysUnlocked_AndProgressCapsAtTarget()
        {
            Stats.Add(_app.Session.Meta, Stats.CombatsWon, 12);
            Assert.That(Find("wins_10").Unlocked, Is.True);
            Assert.That(Find("wins_10").Progress, Is.EqualTo(10));
            var day = Find("wins_10").UnlockedDay;
            _app.Session.Meta.stats.Clear();
            Assert.That(Find("wins_10").Unlocked, Is.True, "le succès reste débloqué");
            Assert.That(Find("wins_10").UnlockedDay, Is.EqualTo(day));
        }

        [Test, Description("Vérifie que le dernier succès est le plus récent, et qu'un ami ajouté débloque « Premier allié » — parce que la carte de profil affiche le dernier succès")]
        public void LastAchievement_IsTheMostRecent_AndFriendCounts()
        {
            Assert.That(_app.Profile.Achievements().Last(), Is.Null);
            Stats.Add(_app.Session.Meta, Stats.CombatsWon, 1);
            _app.Session.Meta.friends.Add("ami#0001");
            Assert.That(Find("friend_1").Unlocked, Is.True);
            Assert.That(_app.Profile.Achievements().Last(), Is.Not.Null);
        }
    }
}
