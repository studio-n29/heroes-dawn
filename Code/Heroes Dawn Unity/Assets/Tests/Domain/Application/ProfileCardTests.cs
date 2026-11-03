using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Carte de profil : rang d'après le pass, défilement de l'avatar et du fond, sauvegarde du choix.</summary>
    public sealed class ProfileCardTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [TestCase(1, "profile.rank.recruit")]
        [TestCase(4, "profile.rank.recruit")]
        [TestCase(5, "profile.rank.agent")]
        [TestCase(10, "profile.rank.veteran")]
        [TestCase(20, "profile.rank.chief")]
        [Test, Description("Vérifie que le rang change aux niveaux de pass 5, 10 et 20 — parce que la carte affiche la progression du joueur")]
        public void Rank_ChangesAtEachPassThreshold(int level, string key)
        {
            Assert.That(ProfileRank.KeyFor(level), Is.EqualTo(key));
        }

        [Test, Description("Vérifie que le fond passe au suivant puis revient au premier, et qu'un identifiant inconnu retombe sur le premier — parce qu'une sauvegarde ancienne ne doit pas casser la carte")]
        public void Background_CyclesAndFallsBack()
        {
            Assert.That(ProfileBackgrounds.Resolve(""), Is.EqualTo(ProfileBackgrounds.Night));
            Assert.That(ProfileBackgrounds.Resolve("inconnu"), Is.EqualTo(ProfileBackgrounds.Night));
            Assert.That(ProfileBackgrounds.Next(ProfileBackgrounds.Night), Is.EqualTo(ProfileBackgrounds.Dawn));
            Assert.That(ProfileBackgrounds.Next(ProfileBackgrounds.Steel), Is.EqualTo(ProfileBackgrounds.Night));
        }

        [Test, Description("Vérifie que le choix d'avatar et de fond est gardé dans la sauvegarde et que l'avatar revient au premier après le dernier — parce que le joueur retrouve sa carte à la reconnexion")]
        public void CycleAvatarAndBackground_AreSaved()
        {
            var profile = _app.Profile;
            profile.CycleBackground();
            Assert.That(_app.Session.Meta.profileBackground, Is.EqualTo(ProfileBackgrounds.Dawn));
            Assert.That(profile.Background, Is.EqualTo(ProfileBackgrounds.Dawn));

            string first = profile.Avatar;
            for (int i = 0; i < PlayerAvatars.Gallery.Count; i++) profile.CycleAvatar();
            Assert.That(profile.Avatar, Is.EqualTo(first), "après un tour complet de la galerie");
            profile.CycleAvatar();
            Assert.That(profile.Avatar, Is.Not.EqualTo(first));
            Assert.That(_app.Session.Meta.onboarding.avatar, Is.EqualTo(profile.Avatar));
        }
    }
}
