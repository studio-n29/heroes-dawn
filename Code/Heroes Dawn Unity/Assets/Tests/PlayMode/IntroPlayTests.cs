#nullable enable
using System.Linq;
using System.Collections;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Première connexion avec les vrais prefabs : cinématique, pseudo, campagne, puis le QG.</summary>
    public sealed class IntroPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Timeout(300000), Description("Vérifie qu'un nouveau joueur passe par la cinématique (passée d'un clic), ne peut pas choisir une campagne en cours de création, choisit New York puis arrive au QG avec son pseudo — parce que c'est le parcours de première connexion")]
        public IEnumerator FirstConnection_GoesThroughIntro_ThenTheHq()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway(), withIntro: true);
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<IntroPopupView>() != null);
            var intro = Object.FindAnyObjectByType<IntroPopupView>()!;
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Cinematic);
            intro.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign);

            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign);

            Assert.That(intro.Cards.Count, Is.EqualTo(2), "Agent D et Agent M");
            Assert.That(intro.CampaignConfirmButton.interactable, Is.False, "aucun personnage choisi");
            intro.SelectCampaign("agent_m");
            Assert.That(intro.CampaignConfirmButton.interactable, Is.False, "le parcours d Agent M est en cours de création");
            intro.SelectCampaign("agent_d");
            Assert.That(intro.CampaignConfirmButton.interactable, Is.True);
            intro.CampaignConfirmButton.onClick.Invoke();

            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            var session = GameBootstrap.Instance!.Scope.Container.Resolve<GameSession>();
            Assert.That(session.Account!.DisplayName, Is.EqualTo("Agent D"));
            Assert.That(session.Meta.onboarding.campaign, Is.EqualTo("new_york"));
            Assert.That(session.Meta.onboarding.hero, Is.EqualTo("agent_d"));
            Assert.That(session.Roster.Select(m => m.Id.Value), Is.EqualTo(new[] { "darius_brooks" }));
            Assert.That(Object.FindAnyObjectByType<IntroPopupView>(), Is.Null, "la popup est fermée");
        }

        [Test, Description("Vérifie que le compte local accepte l'inscription puis la connexion, refuse un mot de passe faux, un e-mail déjà pris et des saisies invalides — parce qu'il joue le rôle de Supabase Auth en attendant le vrai serveur")]
        public void LocalAccountGateway_SignUpAndSignIn()
        {
            var gw = new LocalAccountGateway(0);
            var created = gw.SignUpAsync("maya@example.com", "motdepasse1", "Maya", default).GetAwaiter().GetResult();
            Assert.That(created.IsSuccess, Is.True);
            Assert.That(created.Value.DisplayName, Is.EqualTo("Maya"));
            Assert.That(gw.SignUpAsync(" MAYA@example.com ", "motdepasse1", "Autre", default).GetAwaiter().GetResult().IsFailure, Is.True, "e-mail déjà pris");
            Assert.That(gw.SignUpAsync("pasunmail", "motdepasse1", "X", default).GetAwaiter().GetResult().IsFailure, Is.True);
            Assert.That(gw.SignUpAsync("b@example.com", "court", "X", default).GetAwaiter().GetResult().IsFailure, Is.True);
            var ok = gw.SignInAsync("MAYA@example.com", "motdepasse1", default).GetAwaiter().GetResult();
            Assert.That(ok.IsSuccess, Is.True);
            Assert.That(ok.Value.PlayerId, Is.EqualTo(created.Value.PlayerId));
            Assert.That(gw.SignInAsync("maya@example.com", "faux", default).GetAwaiter().GetResult().IsFailure, Is.True);
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures de la cinématique, du choix du personnage joueur — parce que ces rendus se relisent à l'œil")]
        public IEnumerator CaptureTheFirstConnection()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway(), withIntro: true);
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<IntroPopupView>() != null);
            var intro = Object.FindAnyObjectByType<IntroPopupView>()!;
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Cinematic);
            yield return new WaitForSecondsRealtime(4.5f);
            yield return ScreenCaptureTests.Capture("30-intro-ecran-1");
            for (int slide = 2; slide <= 4; slide++)
            {
                intro.TapButton.onClick.Invoke(); // texte entier
                intro.TapButton.onClick.Invoke(); // écran suivant
                yield return new WaitForSecondsRealtime(slide == 4 ? 5.5f : 3.5f);
            }
            yield return ScreenCaptureTests.Capture("31-intro-ecran-4");
            intro.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return PlayModeHarness.WaitFor(() => intro.Current == IntroPopupView.Stage.Campaign);
            intro.SelectCampaign("agent_d");
            yield return new WaitForSecondsRealtime(0.6f);
            yield return ScreenCaptureTests.Capture("33-campagne");
        }
    }
}
