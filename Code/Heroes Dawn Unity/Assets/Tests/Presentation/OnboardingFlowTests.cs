using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Première connexion : cinématique, choix du personnage joueur (sans pseudo).</summary>
    public sealed class OnboardingFlowTests
    {
        TestApp _app = null!;
        FakeModalHost _host = null!;
        OnboardingGuide _service = null!;
        OnboardingFlow _flow = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _host = new FakeModalHost();
            _host.Register<IIntroPopupView>(() => new FakeIntroPopup());
            _service = new OnboardingGuide(_app.MetaCtx);
            _flow = new OnboardingFlow(_service, new ModalService(_host), new LocLocalization());
        }

        FakeIntroPopup Popup => (FakeIntroPopup)_host.Spawned.Single();

        [Test, Description("Vérifie que la cinématique montre les 12 plans de la version courte, dans l ordre, et qu elle coupe le thème principal du jeu — parce que le flux doit jouer le document de cinématique de bout en bout")]
        public void Cinematic_PlaysTheCondensedCut()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Assert.That(Popup.Shots!.Count, Is.EqualTo(12));
            Assert.That(Popup.Shots[0].Id, Is.EqualTo("S01"));
            Assert.That(Popup.Shots[11].Id, Is.EqualTo("S46"));
            Assert.That(Popup.MainTheme, Is.Not.Null, "le thème principal est piloté par la cinématique");
        }

        [Test, Description("Vérifie que les deux personnages sont proposés, Agent D disponible et Agent M en création — parce que les zones de Moscou ne sont pas encore créées")]
        public void Campaigns_OnlyAgentDIsAvailable()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishCinematic();
            var cards = Popup.Campaigns!.Value.Cards;
            Assert.That(cards.Count, Is.EqualTo(2));
            Assert.That(cards.Single(c => c.Available).Id, Is.EqualTo("agent_d"));
            Assert.That(cards.Where(c => !c.Available).Select(c => c.Badge).Distinct().Single(), Is.EqualTo("EN CRÉATION"));
        }

        [Test, Description("Vérifie que le parcours complet enregistre le personnage, la campagne et l état « terminé », renomme le compte et ferme la popup — parce que la première connexion ne se joue qu'une fois")]
        public void FullRun_SavesTheHeroAndRenamesTheAccount()
        {
            var run = _flow.RunIfNeededAsync(default);
            var popup = Popup;
            popup.FinishCinematic();
            popup.Choose("agent_d");
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_app.Session.Meta.onboarding.done, Is.True);
            Assert.That(_app.Session.Meta.onboarding.pseudo, Is.EqualTo("Agent D"));
            Assert.That(_app.Session.Meta.onboarding.campaign, Is.EqualTo("new_york"));
            Assert.That(_app.Session.Meta.onboarding.hero, Is.EqualTo("agent_d"));
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Agent D"));
            Assert.That(_host.Despawned, Does.Contain(popup));
            Assert.That(_flow.Needed, Is.False);
        }

        [Test, Description("Vérifie qu'un profil qui a déjà validé une zone ne revoit pas la première connexion — parce que les joueurs existants ne repassent pas par l'introduction")]
        public void ExistingPlayer_SkipsTheIntro()
        {
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            var run = _flow.RunIfNeededAsync(default);
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_host.Spawned, Is.Empty);
        }

        [Test, Description("Vérifie que chaque texte de la première connexion existe — parce qu'une clé manquante s'afficherait telle quelle à l'écran")]
        public void EveryOnboardingTextExists()
        {
            foreach (var id in Loc.CinematicVoiceIds) Assert.That(Loc.Has("cinematic.vo." + id), Is.True, "cinematic.vo." + id);
            foreach (var key in new[] { "intro.skip",
                                        "onboarding.campaign.title", "onboarding.campaign.confirm", "onboarding.campaign.open", "onboarding.campaign.wip", "onboarding.campaign.unavailable" })
                Assert.That(Loc.Has(key), Is.True, key);
            foreach (var c in Campaigns.All) { Assert.That(Loc.Has(c.NameKey), Is.True, c.NameKey); Assert.That(Loc.Has(c.DescKey), Is.True, c.DescKey); }
        }
    }
}
