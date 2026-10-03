#nullable enable
using System.Collections;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class BackNavigationTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();
        static BackButtonRouter Router => Container.Resolve<BackButtonRouter>();
        static bool ConfirmOpen => Object.FindAnyObjectByType<ConfirmPopupView>() != null;

        /// <summary>Connecté, pion en n02 avec 1 PM (n03 = combat Facile), plateau affiché.</summary>
        static IEnumerator OnBoard()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<HubView>() && !Flow.IsTransitioning);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub), "connexion → QG");
            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board), "QG → plateau");
        }

        [UnityTest, Description("Vérifie qu'Échap sur le plateau ouvre « Retour au QG ? » et qu'un second Échap la referme sans quitter la zone — parce que Retour ferme d'abord la modale du dessus, puis seulement l'écran (spec §7)")]
        public IEnumerator EscapeOnBoard_OpensConfirm_SecondEscapeCloses()
        {
            yield return OnBoard();
            Assert.That(Router.Dispatch(), Is.True, "Retour pris en charge par le plateau");
            yield return PlayModeHarness.WaitFor(() => ConfirmOpen);
            Assert.That(ConfirmOpen, Is.True, "la confirmation de retour au QG est ouverte");

            Assert.That(Router.Dispatch(), Is.True, "Retour pris en charge par la modale");
            yield return PlayModeHarness.WaitFor(() => !ConfirmOpen);
            Assert.That(ConfirmOpen, Is.False);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board));
            Assert.That(PlayModeHarness.Visible<BoardView>(), Is.True);
            Assert.That(Container.Resolve<GameSession>().Run, Is.Not.Null, "la zone n'est pas quittée");
        }

        [UnityTest, Description("Vérifie qu'Échap pendant un combat n'ouvre jamais « Retour au QG ? » — parce que le plateau masqué retire son gestionnaire Retour pendant le combat (Review Focus 2)")]
        public IEnumerator EscapeInCombat_DoesNotOpenHqConfirm()
        {
            yield return OnBoard();
            PlayModeHarness.TapNode(new CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && Flow.Current == GameState.Combat && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Combat));

            Router.Dispatch();
            for (int i = 0; i < 10; i++) yield return null;
            Assert.That(ConfirmOpen, Is.False, "aucune confirmation de retour au QG pendant le combat");
            Assert.That(Flow.Current, Is.EqualTo(GameState.Combat));
            Assert.That(Container.Resolve<GameSession>().Run, Is.Not.Null);
        }
    }
}
