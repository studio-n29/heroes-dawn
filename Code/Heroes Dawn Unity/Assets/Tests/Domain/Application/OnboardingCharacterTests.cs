#nullable enable
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class OnboardingCharacterTests
    {
        TestApp _app = null!;
        OnboardingGuide _guide = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _guide = new OnboardingGuide(_app.MetaCtx);
        }

        [Test, Description("Vérifie que le joueur crée son personnage : son nom et son avatar sont gardés, et la première connexion est terminée — parce que le joueur incarne un agent qu'il crée lui-même")]
        public void CreateCharacter_KeepsNameAndAvatar_AndCompletesOnboarding()
        {
            Assert.That(_guide.CreateCharacter("Maya Rivera", "avatar_3"), Is.Null);
            Assert.That(_guide.Pseudo, Is.EqualTo("Maya Rivera"));
            Assert.That(_app.Session.Meta.onboarding.avatar, Is.EqualTo("avatar_3"));
            Assert.That(_app.Session.Meta.onboarding.done, Is.True);
        }

        [Test, Description("Vérifie qu'un nom vide ou trop long est refusé sans rien enregistrer — parce qu'un personnage doit avoir un nom affichable")]
        public void CreateCharacter_RefusesEmptyOrTooLongName()
        {
            Assert.That(_guide.CreateCharacter("   ", "avatar_1"), Is.EqualTo("onboarding.name.invalid"));
            Assert.That(_guide.CreateCharacter(new string('x', PlayerTag.MaxNameLength + 1), "avatar_1"), Is.EqualTo("onboarding.name.invalid"));
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
        }
    }
}
