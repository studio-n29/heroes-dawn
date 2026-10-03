#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class ShopFlowTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();

        static RecruitCardView[] Cards() => Object.FindObjectsByType<RecruitCardView>(FindObjectsSortMode.None)
            .Where(c => c.gameObject.activeInHierarchy).ToArray();

        [UnityTest, Description("Vérifie qu'un clic réel sur Renouveler (gratuit) passe au tirage suivant et réaffiche les cartes de la boutique — parce que c'est le geste quotidien de la boutique éphémère (spec §5)")]
        public IEnumerator RerollButton_ShowsNewOffers()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<HubView>() && !Flow.IsTransitioning);
            var hub = Object.FindAnyObjectByType<HubView>()!;
            hub.RecruitTabButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Cards().Length > 0);
            var session = Container.Resolve<GameSession>();
            Assert.That(session.Shop.Draw, Is.EqualTo(0));
            Assert.That(Cards().Length, Is.EqualTo(session.Shop.Offers.Count));
            var before = Cards();

            hub.RerollButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => session.Shop.Draw == 1 && Cards().Length > 0 && before.All(c => c == null));
            Assert.That(session.Shop.Draw, Is.EqualTo(1));
            Assert.That(Cards().Length, Is.EqualTo(session.Shop.Offers.Count));
            Assert.That(before.All(c => c == null), Is.True, "les cartes ont été reconstruites");
        }
    }
}
