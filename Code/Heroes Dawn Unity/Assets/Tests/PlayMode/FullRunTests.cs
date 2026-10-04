#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class FullRunTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();
        static bool Loaded(string scene) => SceneManager.GetSceneByName(scene).isLoaded;

        static readonly CaseId[] PathToCombat = { new CaseId("n03"), new CaseId("n02"), new CaseId("n01") };

        [UnityTest, Timeout(300000), Description("Vérifie une partie complète au clic, sans méthode de test : connexion, recrutement, déploiement, dé et déplacements jusqu'au combat, combat en auto, résultats, retour au plateau puis au QG — parce que c'est la boucle du prototype M1 que le joueur parcourt (spec §9)")]
        public IEnumerator LoginRecruitDeployCombatResultsBoardHq()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub), "connexion → QG");

            // Onglet Recruter par la barre de navigation, première recrue abordable, confirmation d'achat.
            var session = Container.Resolve<GameSession>();
            int roster = session.Roster.Count;
            var shell = Object.FindAnyObjectByType<HubShellView>()!;
            shell.TabButton(HubPage.Recruit).Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<RecruitPageView>() && session.Shop.Offers.Count > 0);
            yield return PlayModeHarness.RecruitAffordable();
            Assert.That(session.Roster.Count, Is.EqualTo(roster + 1), "recrue ajoutée à l'équipe");

            // Retour au QG, MISSION, puis dé et déplacements jusqu'au combat n03.
            shell.TabButton(HubPage.Qg).Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            Object.FindAnyObjectByType<QgPageView>()!.MissionButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board), "QG → plateau");
            var board = Object.FindAnyObjectByType<BoardView>()!;
            for (int i = 0; i < 400 && !Loaded(SceneNames.Combat); i++)
            {
                var run = session.Run!;
                if (run.Phase == ZoneRunPhase.AwaitingRoll) board.RollButton.onClick.Invoke();
                else if (run.Phase == ZoneRunPhase.ChoosingDestination)
                {
                    var next = PathToCombat.Where(id => run.Destinations.ContainsKey(id)).Select(id => (CaseId?)id).FirstOrDefault();
                    if (next != null) PlayModeHarness.TapNode(next.Value);
                    else board.EndMoveButton.onClick.Invoke();
                }
                for (int f = 0; f < 15; f++) yield return null;
            }
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<CombatView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Combat), "plateau → combat");

            // Combat en automatique, vitesse ×3.
            var combat = Object.FindAnyObjectByType<CombatView>()!;
            combat.AutoButton.onClick.Invoke();
            combat.SpeedButton.onClick.Invoke();
            combat.SpeedButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<CombatResultsPopupView>() != null, 20000);
            Assert.That(Object.FindAnyObjectByType<CombatResultsPopupView>(), Is.Not.Null, "les résultats s'affichent");
            for (int f = 0; f < 30; f++) yield return null;
            Assert.That(PlayModeHarness.ContinueResults(), Is.True);
            yield return PlayModeHarness.WaitFor(() => !Loaded(SceneNames.Combat) && PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board), "résultats → plateau");

            // Retour QG confirmé.
            yield return PlayModeHarness.BackToHqConfirmed();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning, 600);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub), "plateau → QG");
            Assert.That(session.Run, Is.Null);
            Assert.That(session.Roster.Count, Is.EqualTo(roster + 1), "la recrue est toujours dans l'équipe");
        }
    }
}
