using System.Collections;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Composition;
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
    }
}
