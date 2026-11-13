#nullable enable
using System.Collections;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Account;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class ConsentPromptPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Timeout(240000), Description("Vérifie qu'à la première connexion la vraie fenêtre de consentement s'affiche avant le QG, que « Accepter » mémorise le oui et que le jeu continue vers le QG — parce que la mesure ne peut démarrer qu'après ce choix explicite")]
        public IEnumerator FirstLogin_ShowsTheConsentWindow_AcceptStoresAndContinues()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            var c = GameBootstrap.Instance!.Scope.Container;
            c.Resolve<ConsentPrompt>().Enabled = true;
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ConfirmPopupView>() != null, 600);
            var popup = Object.FindAnyObjectByType<ConfirmPopupView>();
            Assert.That(popup, Is.Not.Null, "la fenêtre de consentement s'affiche");
            Assert.That(PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>(), Is.False, "avant le QG");
            Assert.That(c.Resolve<PlayerSettings>().Current.consentAnswered, Is.False, "rien n'est mémorisé avant la réponse");
            popup!.OkButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !c.Resolve<IGameFlow>().IsTransitioning, 600);
            Assert.That(PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>(), Is.True, "puis le QG");
            Assert.That(c.Resolve<PlayerSettings>().Current.consentStats, Is.True);
            Assert.That(c.Resolve<PlayerSettings>().Current.consentAnswered, Is.True);
        }
    }
}
