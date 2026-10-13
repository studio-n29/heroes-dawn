#nullable enable
using N29.HeroesDawn.Application.Features.Auth;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Règles du pseudo et service de première connexion (version 0.9.0).</summary>
    public sealed class OnboardingTests
    {
        TestApp _app = null!;
        OnboardingGuide _service = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _service = new OnboardingGuide(_app.MetaCtx);
        }

        [TestCase("Maya", null)]
        [TestCase("  Maya   Dorée  ", null)]
        [TestCase("Agent-47_x", null)]
        [TestCase("ab", "onboarding.pseudo.short")]
        [TestCase("", "onboarding.pseudo.short")]
        [TestCase("Un pseudo vraiment trop long", "onboarding.pseudo.long")]
        [TestCase("Maya<3", "onboarding.pseudo.chars")]
        [Description("Vérifie les règles du pseudo : 3 à 16 caractères, lettres, chiffres, espace, tiret et tiret bas, espaces nettoyés — parce que le pseudo s'affiche partout et doit rester lisible")]
        public void Pseudo_FollowsTheRules(string raw, string? error)
        {
            Assert.That(PseudoRules.Validate(raw, out _), Is.EqualTo(error));
        }

        [Test, Description("Vérifie que les espaces répétés du pseudo sont réduits à un seul — parce que « Maya    D » et « Maya D » sont le même nom")]
        public void Pseudo_IsCleaned()
        {
            PseudoRules.Validate("  Maya    Dorée ", out var clean);
            Assert.That(clean, Is.EqualTo("Maya Dorée"));
        }

        [Test, Description("Vérifie qu'un profil neuf a besoin de la première connexion et qu'un profil qui a validé une zone n'en a pas besoin — parce que les joueurs existants gardent leur partie")]
        public void Needed_OnlyForAFreshProfile()
        {
            Assert.That(_service.Needed, Is.True);
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Assert.That(_service.Needed, Is.False);
        }

        [Test, Description("Vérifie que terminer la première connexion enregistre pseudo et campagne, renomme le compte et plus rien n'est demandé — parce que le pseudo choisi remplace le nom d'invité")]
        public void Complete_SavesAndRenamesTheAccount()
        {
            Assert.That(_service.Complete("Maya", Campaigns.NewYork), Is.Null);
            Assert.That(_service.Needed, Is.False);
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Maya"));
            Assert.That(_app.Session.Account.PlayerId, Is.EqualTo("p1"), "l'identifiant ne change pas");
            Assert.That(_service.Campaign, Is.EqualTo("new_york"));
        }

        [Test, Description("Vérifie qu'un pseudo invalide n'enregistre rien — parce qu'on ne valide pas la première connexion avec un nom refusé")]
        public void Complete_WithAnInvalidPseudo_SavesNothing()
        {
            Assert.That(_service.Complete("x", Campaigns.NewYork), Is.EqualTo("onboarding.pseudo.short"));
            Assert.That(_service.Needed, Is.True);
            Assert.That(_app.Session.Meta.onboarding.pseudo, Is.Empty);
        }

        [Test, Description("Vérifie que seule New York est disponible parmi les campagnes — parce que les autres sont en cours de création")]
        public void OnlyNewYorkIsAvailable()
        {
            Assert.That(Campaigns.All.Count, Is.EqualTo(4));
            Assert.That(System.Linq.Enumerable.Single(Campaigns.All, c => c.Available).Id, Is.EqualTo(Campaigns.NewYork));
        }
    }
}
