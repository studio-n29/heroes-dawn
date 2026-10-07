#nullable enable
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Popups;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Activités jouées depuis le plateau, avec les vrais prefabs (fenêtre d'activité, combat enchaîné).</summary>
    public sealed class ActivityFlowTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();
        static bool Loaded(string scene) => SceneManager.GetSceneByName(scene).isLoaded;

        /// <summary>Plateau affiché, pion à côté de la case <paramref name="target"/> avec 1 PM : un toucher l'y amène.</summary>
        static IEnumerator OnBoardNext(string target)
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            var from = new BoardGraph(zone).Neighbors(new CaseId(target)).First();
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, from.Value);
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
        }

        [UnityTest, Timeout(180000), Description("Vérifie qu'une case Mini-jeu assignée à la pêche ouvre la fenêtre d'activité, que la réussite simulée résout la case et verse base + pièces par poisson — parce que le framework relie la case, l'activité et le portefeuille de bout en bout")]
        public IEnumerator Fishing_OpensTheActivityWindow_AndPaysOnSuccess()
        {
            yield return OnBoardNext("n15");
            var session = Container.Resolve<GameSession>();
            int credits = session.Wallet.Credits;
            PlayModeHarness.TapNode(new CaseId("n15"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ActivityPopupView>() != null);
            var popup = Object.FindAnyObjectByType<ActivityPopupView>()!;
            Assert.That(session.Run!.CurrentActivity!.Def.Id.Value, Is.EqualTo("fishing_docks"));
            popup.WinButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ActivityPopupView>() == null && session.Run.Phase == ZoneRunPhase.AwaitingRoll);
            Assert.That(session.Run.IsSpent(new CaseId("n15")), Is.True);
            Assert.That(session.Wallet.Credits, Is.EqualTo(credits + 100 + 40 * 4));
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'un dialogue raté lance le combat Facile enchaîné (scène de combat chargée) et que la case de dialogue reste à rejouer après la victoire — parce que l'échec d'une activité peut en lancer une autre")]
        public IEnumerator Dialogue_Failure_LaunchesACombat_AndLeavesTheCaseReplayable()
        {
            yield return OnBoardNext("n21");
            var session = Container.Resolve<GameSession>();
            PlayModeHarness.TapNode(new CaseId("n21"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<ActivityPopupView>() != null);
            Object.FindAnyObjectByType<ActivityPopupView>()!.FailButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Combat) && Flow.Current == GameState.Combat && !Flow.IsTransitioning, 900);
            Assert.That(session.Run!.PendingCase!.Rank, Is.EqualTo(N29.HeroesDawn.Definitions.CombatRank.Easy));

            Container.Resolve<IForceCombatWinUseCase>().ExecuteAsync().Forget();
            yield return PlayModeHarness.WaitFor(PlayModeHarness.ContinueResults, 600);
            yield return PlayModeHarness.WaitFor(() => !Loaded(SceneNames.Combat) && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board));
            Assert.That(session.Run!.IsSpent(new CaseId("n21")), Is.False, "le dialogue reste à rejouer");
        }
    }
}
