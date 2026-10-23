#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Collection;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Service de première connexion : choix du personnage joueur, sans pseudo (retour de playtest 0.10.1).</summary>
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

        [Test, Description("Vérifie qu'un profil neuf a besoin de la première connexion et qu'un profil qui a validé une zone n'en a pas besoin — parce que les joueurs existants gardent leur partie")]
        public void Needed_OnlyForAFreshProfile()
        {
            Assert.That(_service.Needed, Is.True);
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Assert.That(_service.Needed, Is.False);
        }

        [Test, Description("Vérifie que terminer la première connexion enregistre le personnage et la campagne, et renomme le compte « Agent D » sans rien demander au joueur — parce que le joueur incarne l'agent, il ne choisit plus de pseudo")]
        public void Complete_SavesAndRenamesTheAccountAfterTheHero()
        {
            Assert.That(_service.Complete(Campaigns.AgentD), Is.Null);
            Assert.That(_service.Needed, Is.False);
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Agent D"));
            Assert.That(_app.Session.Account.PlayerId, Is.EqualTo("p1"), "l'identifiant ne change pas");
            Assert.That(_service.Campaign, Is.EqualTo("new_york"));
            Assert.That(_service.Hero, Is.EqualTo(Campaigns.AgentD));
        }

        [Test, Description("Vérifie que la première connexion offre un objet courant — parce que le tutoriel apprend à équiper la recrue et qu'il faut un objet à lui donner")]
        public void Complete_GrantsAStarterItem()
        {
            var before = _app.Session.Content.Collection.Items.Sum(i => CollectionOps.OwnedItems(_app.Session, i.Id));
            _service.Complete(Campaigns.AgentD);
            var after = _app.Session.Content.Collection.Items.Sum(i => CollectionOps.OwnedItems(_app.Session, i.Id));
            Assert.That(after, Is.EqualTo(before + 1));
        }

        [Test, Description("Vérifie qu Agent M est refusé tant que son parcours n est pas disponible, sans rien enregistrer — parce qu on ne démarre pas un parcours sans zones")]
        public void Complete_WithAnUnavailableHero_SavesNothing()
        {
            Assert.That(_service.Complete(Campaigns.AgentM), Is.EqualTo("onboarding.campaign.unavailable"));
            Assert.That(_service.Needed, Is.True);
            Assert.That(_app.Session.Meta.onboarding.hero, Is.Empty);
        }

        [Test, Description("Vérifie que seul Agent D est disponible parmi les deux personnages — parce que les zones de Moscou (parcours d Agent M) sont en cours de création")]
        public void OnlyAgentDIsAvailable()
        {
            Assert.That(Campaigns.All.Count, Is.EqualTo(2));
            Assert.That(System.Linq.Enumerable.Single(Campaigns.All, c => c.Available).Id, Is.EqualTo(Campaigns.AgentD));
        }
    }
}
