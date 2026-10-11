#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Tutorial;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class TutorialFlowTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();
        static TutorialGuide Guide => Container.Resolve<TutorialGuide>();
        static TutorialOverlayView Overlay => Object.FindAnyObjectByType<TutorialOverlayView>()!;
        static bool Loaded(string scene) => SceneManager.GetSceneByName(scene).isLoaded;

        static IEnumerator WaitStep(TutorialStep step, int frames = 600, float seconds = 0f)
        {
            float until = Time.realtimeSinceStartup + seconds;
            yield return PlayModeHarness.WaitFor(() => Guide.Current == step && Overlay.StepVisible, frames);
            while (!(Guide.Current == step && Overlay.StepVisible) && Time.realtimeSinceStartup < until) yield return null; // le combat avance en temps réel
            Assert.That(Guide.Current, Is.EqualTo(step));
            Assert.That(Overlay.StepVisible, Is.True, "la bulle de l'étape " + step + " est affichée");
        }

        static readonly CaseId[] PathToCombat = { new CaseId("n03"), new CaseId("n02"), new CaseId("n01") };

        [UnityTest, Timeout(400000), Description("Vérifie le parcours guidé de bout en bout sur la vraie interface : Bienvenue → Équipe → Escouade → MISSION → Déployer → Objectifs → Dé → Déplacement → Combat (figé) → Compétence → Résultats → fin — parce que c'est la première partie de tout nouveau joueur (spec §6)")]
        public IEnumerator GuidedFirstMission_WalksEveryStep()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            var session = Container.Resolve<GameSession>();
            var shell = Object.FindAnyObjectByType<HubShellView>()!;

            yield return WaitStep(TutorialStep.Welcome);
            Overlay.OkButton.Button.onClick.Invoke();
            yield return WaitStep(TutorialStep.OpenTeam);
            shell.TabButton(HubPage.Team).Button.onClick.Invoke();
            yield return WaitStep(TutorialStep.Squad);
            Overlay.OkButton.Button.onClick.Invoke();
            yield return WaitStep(TutorialStep.StartMission);
            Object.FindAnyObjectByType<TeamPageView>()!.MissionButton.onClick.Invoke();
            yield return WaitStep(TutorialStep.Deploy);
            PlayModeHarness.DeployNow();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning, 600);
            yield return WaitStep(TutorialStep.Objectives);
            Overlay.OkButton.Button.onClick.Invoke();
            yield return WaitStep(TutorialStep.Roll);
            var board = Object.FindAnyObjectByType<BoardView>()!;
            board.RollButton.onClick.Invoke();
            yield return WaitStep(TutorialStep.Move);
            var run = session.Run!;
            var next = PathToCombat.Where(id => run.Destinations.ContainsKey(id)).Select(id => (CaseId?)id).FirstOrDefault();
            PlayModeHarness.TapNode(next ?? run.Destinations.Keys.First());

            // Dé et déplacements (la bulle ne bloque pas les appels directs) jusqu'au combat.
            for (int i = 0; i < 400 && !Loaded(SceneNames.Combat); i++)
            {
                var r = session.Run!;
                if (r.Phase == ZoneRunPhase.AwaitingRoll) board.RollButton.onClick.Invoke();
                else if (r.Phase == ZoneRunPhase.ChoosingDestination)
                {
                    var n = PathToCombat.Where(id => r.Destinations.ContainsKey(id)).Select(id => (CaseId?)id).FirstOrDefault();
                    if (n != null) PlayModeHarness.TapNode(n.Value);
                    else board.EndMoveButton.onClick.Invoke();
                }
                for (int f = 0; f < 15; f++) yield return null;
            }
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<CombatView>() && !Flow.IsTransitioning, 600);

            yield return WaitStep(TutorialStep.CombatAtb);
            Assert.That(Guide.PausesCombat, Is.True, "le combat est figé pendant la bulle");
            Overlay.OkButton.Button.onClick.Invoke();
            var fast = Object.FindAnyObjectByType<CombatView>()!;
            fast.SpeedButton.onClick.Invoke();
            fast.SpeedButton.onClick.Invoke(); // x3 : le premier allié prêt arrive vite
            yield return WaitStep(TutorialStep.CombatSkill, 600, 60f);
            Assert.That(Guide.PausesCombat, Is.True, "figé jusqu'à ce qu'une compétence soit choisie");
            var combat = Object.FindAnyObjectByType<CombatView>()!;
            combat.SkillButton(0).onClick.Invoke();
            combat.EnemyCards().First().Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Guide.Current == TutorialStep.Results, 1200);
            Assert.That(Guide.Current, Is.EqualTo(TutorialStep.Results), "la compétence lancée fait avancer le tutoriel");

            combat.AutoButton.onClick.Invoke();
            combat.SpeedButton.onClick.Invoke();
            combat.SpeedButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<CombatResultsPopupView>() != null, 20000);
            yield return WaitStep(TutorialStep.Results);
            Assert.That(PlayModeHarness.ContinueResults(), Is.True);
            yield return PlayModeHarness.WaitFor(() => !Guide.Active, 120);
            Assert.That(Guide.Active, Is.False, "tutoriel terminé");
            for (int f = 0; f < 5; f++) yield return null;
            Assert.That(Overlay.StepVisible, Is.False, "plus de bulle");
            Assert.That(session.Meta.tutorial.done, Is.True, "enregistré dans le profil");
        }

        [UnityTest, Description("Vérifie que « Passer le tutoriel » demande une confirmation puis ferme le tutoriel — parce que le joueur expérimenté doit pouvoir l'ignorer sans le subir (spec §6)")]
        public IEnumerator Skip_AsksConfirmation_ThenEndsTutorial()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            yield return WaitStep(TutorialStep.Welcome);
            Overlay.SkipButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ConfirmPopupView>() != null);
            Assert.That(Guide.Active, Is.True, "rien n'est passé avant la confirmation");
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ConfirmPopupView>()!.OkButton.Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => !Guide.Active, 120);
            Assert.That(Guide.Active, Is.False);
            Assert.That(Guide.Skipped, Is.True);
        }

        [UnityTest, Description("Vérifie qu'une astuce contextuelle s'affiche une seule fois après le tutoriel, avec son bouton OK — parce que les astuces sont non bloquantes et ne se répètent pas")]
        public IEnumerator Hint_ShowsOnce_AfterTutorial()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
            Guide.Skip();
            Assert.That(Guide.Hint("combo"), Is.True);
            yield return null;
            Assert.That(Overlay.HintVisible, Is.True);
            Overlay.HintOkButton.Button.onClick.Invoke();
            yield return null;
            Assert.That(Overlay.HintVisible, Is.False);
            Assert.That(Guide.Hint("combo"), Is.False, "une seule fois");
        }
    }
}
