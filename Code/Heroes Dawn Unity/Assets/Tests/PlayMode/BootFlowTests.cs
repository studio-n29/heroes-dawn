#nullable enable
using System.Collections;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Screens;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;
using VContainer.Unity;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class BootFlowTests
    {
        MemoryProfileGateway _profiles = null!;
        InstantAuthGateway _auth = null!;

        [SetUp]
        public void SetUp()
        {
            // L'instance démarrée automatiquement à l'entrée en Play Mode est remplacée par une instance avec doubles.
            GameBootstrap.DestroyForTests();
            _profiles = new MemoryProfileGateway();
            _auth = new InstantAuthGateway();
        }

        [TearDown]
        public void TearDown() => GameBootstrap.DestroyForTests();

        IEnumerator Boot()
        {
            using (LifetimeScope.Enqueue(b =>
                   {
                       b.RegisterInstance<IProfileGateway>(_profiles);
                       b.RegisterInstance<IAuthGateway>(_auth);
                   }))
            {
                GameBootstrap.StartForTests();
            }
            for (int i = 0; i < 300 && !LoginVisible(); i++) yield return null;
        }

        static bool LoginVisible()
        {
            var l = Object.FindAnyObjectByType<LoginScreen>();
            return l != null && l.gameObject.activeInHierarchy;
        }

        [UnityTest]
        public IEnumerator Boot_ShowsLoginScreen()
        {
            yield return Boot();
            Assert.That(GameBootstrap.Instance, Is.Not.Null);
            Assert.That(GameBootstrap.Instance!.Scope.Container, Is.Not.Null);
            // Review Focus 2 : les doubles remplacent bien les vrais ports (la vraie sauvegarde n'est jamais touchée).
            Assert.That(GameBootstrap.Instance.Scope.Container.Resolve<IProfileGateway>(), Is.SameAs(_profiles));
            Assert.That(GameBootstrap.Instance.Scope.Container.Resolve<IAuthGateway>(), Is.SameAs(_auth));
            Assert.That(LoginVisible(), Is.True);
        }

        static T? Visible<T>() where T : Component
        {
            var s = Object.FindAnyObjectByType<T>();
            return s != null && s.gameObject.activeInHierarchy ? s : null;
        }

        static IEnumerator WaitFor(System.Func<bool> condition, int frames = 300)
        {
            for (int i = 0; i < frames && !condition(); i++) yield return null;
        }

        [UnityTest]
        public IEnumerator GuestSignIn_ShowsHub()
        {
            yield return Boot();
            Visible<LoginScreen>()!.Play();
            yield return WaitFor(() => Visible<HubScreen>() != null);
            Assert.That(Visible<HubScreen>(), Is.Not.Null);
            Assert.That(_profiles.Saves, Is.GreaterThanOrEqualTo(1));
            Assert.That(_profiles.Store.ContainsKey(InstantAuthGateway.PlayerId));
        }

        [UnityTest] // Review Focus 1 : double clic sur « Jouer » pendant la connexion
        public IEnumerator DoublePlay_SignsInOnce()
        {
            _auth.DelayFrames = 5;
            yield return Boot();
            var login = Visible<LoginScreen>()!;
            login.Play();
            login.Play();
            yield return WaitFor(() => Visible<HubScreen>() != null);
            Assert.That(Visible<HubScreen>(), Is.Not.Null);
            Assert.That(_auth.Calls, Is.EqualTo(1));
        }

        [UnityTest] // Review Focus 3 : ré-afficher le QG déjà courant ne double pas ses abonnements
        public IEnumerator ShowHubTwice_WalletChangedRefreshesHubOnce()
        {
            yield return Boot();
            Visible<LoginScreen>()!.Play();
            yield return WaitFor(() => Visible<HubScreen>() != null);
            var ctx = GameBootstrap.Instance!.Ui.Context;
            ctx.Nav.Show(ctx.Hub);          // écran déjà courant : OnShow rappelé sans OnHide
            yield return null;
            var hub = Visible<HubScreen>()!;
            int before = hub.RefreshCount;
            GameBootstrap.Instance.Scope.Container.Resolve<IPublisher<WalletChanged>>().Publish(new WalletChanged(1, 2));
            Assert.That(hub.RefreshCount - before, Is.EqualTo(1));
        }

        [UnityTest] // Review Focus 3 : pas d'abonnement en double après déconnexion / reconnexion
        public IEnumerator SignOutThenSignIn_WalletChangedRefreshesHubOnce()
        {
            yield return Boot();
            Visible<LoginScreen>()!.Play();
            yield return WaitFor(() => Visible<HubScreen>() != null);
            var container = GameBootstrap.Instance!.Scope.Container;
            container.Resolve<ISignOutUseCase>().ExecuteAsync().GetAwaiter().GetResult();
            GameBootstrap.Instance.Ui.Context.Nav.Show(GameBootstrap.Instance.Ui.Context.Login);
            yield return null;
            Visible<LoginScreen>()!.Play();
            yield return WaitFor(() => Visible<HubScreen>() != null);
            var hub = Visible<HubScreen>()!;
            int before = hub.RefreshCount;
            container.Resolve<IPublisher<WalletChanged>>().Publish(new WalletChanged(1, 2));
            Assert.That(hub.RefreshCount - before, Is.EqualTo(1));
        }
    }
}
