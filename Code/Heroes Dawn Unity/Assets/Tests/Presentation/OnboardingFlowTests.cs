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
    /// <summary>Première connexion : cinématique, pseudo narratif, choix de la campagne (version 0.9.0).</summary>
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

        [Test, Description("Vérifie que la cinématique montre les neuf écrans de l'univers, chacun avec son texte — parce que l'introduction raconte la bataille de Paris jusqu'à la Guilde")]
        public void Cinematic_ShowsTheNineScreens()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Assert.That(Popup.Slides!.Count, Is.EqualTo(9));
            Assert.That(Popup.Slides.All(s => !string.IsNullOrWhiteSpace(s.Text)), Is.True);
            Assert.That(Popup.Slides[0].Text, Does.Contain("Méphisto"));
            Assert.That(Popup.Slides[8].Text, Does.Contain("trouver sa place"));
        }

        [Test, Description("Vérifie que le pseudo invalide est refusé avec une explication et que le flux n'avance pas — parce que le joueur doit corriger avant de continuer")]
        public void Pseudo_Invalid_IsRefusedWithAnExplanation()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishCinematic();
            Assert.That(Popup.Prompt, Is.Not.Null);
            Popup.SubmitPseudo("ab");
            Assert.That(Popup.LastError, Is.Not.Null.And.Contain("3"));
            Popup.SubmitPseudo("Agent <3>");
            Assert.That(Popup.LastError, Is.Not.Null);
            Assert.That(Popup.Campaigns, Is.Null, "la campagne n'est pas encore proposée");
        }

        [Test, Description("Vérifie que New York est proposée comme disponible et les autres campagnes comme en cours de création — parce que seule New York est jouable en 0.9.0")]
        public void Campaigns_OnlyNewYorkIsAvailable()
        {
            _flow.RunIfNeededAsync(default).Forget();
            Popup.FinishCinematic();
            Popup.SubmitPseudo("Maya");
            var cards = Popup.Campaigns!.Value.Cards;
            Assert.That(cards.Count, Is.EqualTo(4));
            Assert.That(cards.Single(c => c.Available).Id, Is.EqualTo("new_york"));
            Assert.That(cards.Where(c => !c.Available).Select(c => c.Badge).Distinct().Single(), Is.EqualTo("EN CRÉATION"));
        }

        [Test, Description("Vérifie que le parcours complet enregistre le pseudo, la campagne et l'état « terminé », renomme le compte et ferme la popup — parce que la première connexion ne se joue qu'une fois")]
        public void FullRun_SavesThePseudoAndTheCampaign()
        {
            var run = _flow.RunIfNeededAsync(default);
            var popup = Popup;
            popup.FinishCinematic();
            popup.SubmitPseudo("  Maya  Dorée ");
            popup.Choose("new_york");
            Assert.That(run.Status, Is.EqualTo(UniTaskStatus.Succeeded));
            Assert.That(_app.Session.Meta.onboarding.done, Is.True);
            Assert.That(_app.Session.Meta.onboarding.pseudo, Is.EqualTo("Maya Dorée"));
            Assert.That(_app.Session.Meta.onboarding.campaign, Is.EqualTo("new_york"));
            Assert.That(_app.Session.Account!.DisplayName, Is.EqualTo("Maya Dorée"));
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
            for (int i = 1; i <= 9; i++) Assert.That(Loc.Has("intro.slide." + i), Is.True, "intro.slide." + i);
            foreach (var key in new[] { "intro.skip", "intro.tap", "onboarding.speaker", "onboarding.pseudo.line", "onboarding.pseudo.placeholder",
                                        "onboarding.pseudo.confirm", "onboarding.pseudo.short", "onboarding.pseudo.long", "onboarding.pseudo.chars",
                                        "onboarding.campaign.title", "onboarding.campaign.confirm", "onboarding.campaign.open", "onboarding.campaign.wip" })
                Assert.That(Loc.Has(key), Is.True, key);
            foreach (var c in Campaigns.All) { Assert.That(Loc.Has(c.NameKey), Is.True, c.NameKey); Assert.That(Loc.Has(c.DescKey), Is.True, c.DescKey); }
        }
    }
}
