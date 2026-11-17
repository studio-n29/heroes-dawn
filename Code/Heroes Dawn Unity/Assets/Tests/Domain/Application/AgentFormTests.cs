#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Fiche d'identification de The Dawn (première connexion) : nom, prénom, âge, avatar, pouvoir, et ville de départ.</summary>
    public sealed class AgentFormTests
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

        static AgentForm Valid(string power = "electrogenese") => new AgentForm("Rivera", "Maya", 27, "avatar_3", power);

        [Test, Description("Vérifie que la fiche remplie est enregistrée dans le profil (nom, prénom, âge, avatar, pouvoir) et que la première connexion est terminée — parce que The Dawn garde la fiche du joueur")]
        public void CreateAgent_SavesTheWholeFormInTheProfile()
        {
            Assert.That(_guide.CreateAgent(Valid(), StoryRoutes.AgentD), Is.Null);
            var s = _app.Session.Meta.onboarding;
            Assert.That(s.lastName, Is.EqualTo("Rivera"));
            Assert.That(s.firstName, Is.EqualTo("Maya"));
            Assert.That(s.age, Is.EqualTo(27));
            Assert.That(s.avatar, Is.EqualTo("avatar_3"));
            Assert.That(s.power, Is.EqualTo("electrogenese"));
            Assert.That(s.done, Is.True);
        }

        [Test, Description("Vérifie que le prénom devient le nom du compte — parce qu'il faut un nom court pour le tag et l'affichage")]
        public void CreateAgent_UsesTheFirstNameAsAccountName()
        {
            _guide.CreateAgent(Valid(), StoryRoutes.AgentD);
            Assert.That(_guide.Pseudo, Is.EqualTo("Maya"));
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Maya"));
        }

        [Test, Description("Vérifie que choisir l'agent D démarre par New York — parce que le joueur choisit sa ville de départ")]
        public void CreateAgent_AgentD_StartsInNewYork()
        {
            _guide.CreateAgent(Valid(), StoryRoutes.AgentD);
            Assert.That(_app.Session.Meta.onboarding.campaign, Is.EqualTo(StoryRoutes.NewYork));
            Assert.That(_app.Session.Meta.onboarding.hero, Is.EqualTo(StoryRoutes.AgentD));
        }

        [Test, Description("Vérifie que l'agent M n'est accepté que si son parcours est ouvert, et alors démarre par Moscou — parce que Moscou n'est jouable qu'une fois ses zones livrées")]
        public void CreateAgent_AgentM_FollowsTheRouteAvailability()
        {
            var error = _guide.CreateAgent(Valid(), StoryRoutes.AgentM);
            if (StoryRoutes.M.Available)
            {
                Assert.That(error, Is.Null);
                Assert.That(_app.Session.Meta.onboarding.campaign, Is.EqualTo(StoryRoutes.Moscow));
                Assert.That(_app.Session.Meta.onboarding.hero, Is.EqualTo(StoryRoutes.AgentM));
            }
            else
            {
                Assert.That(error, Is.EqualTo("onboarding.campaign.unavailable"));
                Assert.That(_app.Session.Meta.onboarding.done, Is.False);
            }
        }

        [Test, Description("Vérifie qu'un parcours inconnu est refusé sans rien enregistrer — parce qu'on ne démarre pas dans une ville qui n'existe pas")]
        public void CreateAgent_UnknownRoute_IsRefused()
        {
            Assert.That(_guide.CreateAgent(Valid(), "atlantis"), Is.EqualTo("onboarding.campaign.unavailable"));
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
        }

        [TestCase("", "Maya"), TestCase("Rivera", ""), TestCase("   ", "Maya"), TestCase("Rivera", "   ")]
        [Description("Vérifie qu'un nom ou un prénom vide est refusé — parce que la fiche doit être complète")]
        public void CreateAgent_EmptyNames_AreRefused(string last, string first)
        {
            var form = new AgentForm(last, first, 27, "avatar_1", "electrogenese");
            Assert.That(_guide.CreateAgent(form, StoryRoutes.AgentD), Is.EqualTo("onboarding.name.invalid"));
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
        }

        [Test, Description("Vérifie qu'un nom ou un prénom trop long est refusé — parce que la longueur maximale protège l'affichage")]
        public void CreateAgent_TooLongNames_AreRefused()
        {
            string tooLong = new string('x', PlayerTag.MaxNameLength + 1);
            Assert.That(_guide.CreateAgent(new AgentForm(tooLong, "Maya", 27, "avatar_1", "electrogenese"), StoryRoutes.AgentD), Is.EqualTo("onboarding.name.invalid"));
            Assert.That(_guide.CreateAgent(new AgentForm("Rivera", tooLong, 27, "avatar_1", "electrogenese"), StoryRoutes.AgentD), Is.EqualTo("onboarding.name.invalid"));
        }

        [Test, Description("Vérifie que l'âge hors bornes est refusé et que les bornes sont acceptées — parce que la fiche doit rester plausible")]
        public void CreateAgent_AgeMustBeInRange()
        {
            Assert.That(_guide.CreateAgent(new AgentForm("R", "M", AgentForm.MinAge - 1, "avatar_1", "electrogenese"), StoryRoutes.AgentD), Is.EqualTo("onboarding.age.invalid"));
            Assert.That(_guide.CreateAgent(new AgentForm("R", "M", AgentForm.MaxAge + 1, "avatar_1", "electrogenese"), StoryRoutes.AgentD), Is.EqualTo("onboarding.age.invalid"));
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
            Assert.That(_guide.CreateAgent(new AgentForm("R", "M", AgentForm.MinAge, "avatar_1", "electrogenese"), StoryRoutes.AgentD), Is.Null);
        }

        [Test, Description("Vérifie qu'un pouvoir qui n'est pas au catalogue est refusé — parce que chaque pouvoir doit avoir son nom traduit")]
        public void CreateAgent_UnknownPower_IsRefused()
        {
            Assert.That(_guide.CreateAgent(Valid("telekinesie_galactique"), StoryRoutes.AgentD), Is.EqualTo("onboarding.power.invalid"));
            Assert.That(_app.Session.Meta.onboarding.done, Is.False);
        }

        [Test, Description("Vérifie que les espaces autour des noms sont rognés — parce que « Maya » et « Maya  » doivent donner le même nom")]
        public void CreateAgent_TrimsNames()
        {
            _guide.CreateAgent(new AgentForm("  Rivera ", " Maya  ", 27, "avatar_1", "electrogenese"), StoryRoutes.AgentD);
            Assert.That(_app.Session.Meta.onboarding.lastName, Is.EqualTo("Rivera"));
            Assert.That(_app.Session.Meta.onboarding.firstName, Is.EqualTo("Maya"));
        }

        [Test, Description("Vérifie qu'un avatar inconnu retombe sur le premier de la galerie — parce que l'avatar est toujours affichable")]
        public void CreateAgent_UnknownAvatar_FallsBackToTheFirst()
        {
            _guide.CreateAgent(new AgentForm("R", "M", 20, "nope", "electrogenese"), StoryRoutes.AgentD);
            Assert.That(_app.Session.Meta.onboarding.avatar, Is.EqualTo(PlayerAvatars.Gallery[0]));
        }

        [Test, Description("Vérifie que le catalogue propose les quatre familles du lore et beaucoup de pouvoirs — parce que le joueur doit avoir un vrai choix")]
        public void Catalogue_HasFourFamiliesAndManyPowers()
        {
            Assert.That(HeroPowers.Families.Select(f => f.Id), Is.EquivalentTo(new[] { "matiere", "elementaire", "physique_bio", "perception" }));
            Assert.That(HeroPowers.All.Count, Is.GreaterThanOrEqualTo(24));
            foreach (var f in HeroPowers.Families) Assert.That(f.Powers.Count, Is.GreaterThanOrEqualTo(5), f.Id);
        }

        [Test, Description("Vérifie que les identifiants de pouvoir sont uniques et retrouvables — parce que la sauvegarde ne garde que l'identifiant")]
        public void Catalogue_IdsAreUniqueAndFindable()
        {
            Assert.That(HeroPowers.All.Select(p => p.Id).Distinct().Count(), Is.EqualTo(HeroPowers.All.Count));
            foreach (var p in HeroPowers.All) Assert.That(HeroPowers.Find(p.Id), Is.SameAs(p));
            Assert.That(HeroPowers.Find("inconnu"), Is.Null);
        }
    }
}
