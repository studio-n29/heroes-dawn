#nullable enable
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Narration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Onomatopées, boîte de dialogue de narration et brouillard visuel avec les vrais prefabs (plan 2026-10-10, sous-projets H, I, O).</summary>
    public sealed class ComicFogPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        /// <summary>Plateau 1.1 affiché sous le brouillard, pion à l'entrée.</summary>
        static IEnumerator FoggedBoard()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var session = ActivityGamesPlayTests.Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, false);
            session.RunZone = zone;
            ActivityGamesPlayTests.Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
        }

        [UnityTest, Timeout(240000), Description("Vérifie que le brouillard visuel couvre exactement les cases encore cachées — deux nuages par case, dissipés sur les cases révélées — parce que le joueur doit voir ce qu'il ne connaît pas encore")]
        public IEnumerator Fog_CoversExactlyTheHiddenCases()
        {
            yield return FoggedBoard();
            var run = ActivityGamesPlayTests.Container.Resolve<GameSession>().Run!;
            yield return null;
            var clouds = Object.FindObjectsByType<FogCloudView>(FindObjectsSortMode.None);
            int nodes = run.Graph.AllIds.Count;
            int revealed = run.Fog.Revealed.Count;
            Assert.That(clouds.Length, Is.EqualTo(nodes * 2), "deux nuages par case");
            Assert.That(clouds.Count(c => c.Fogged), Is.EqualTo((nodes - revealed) * 2), "les nuages couvrent les cases cachées");
            Assert.That(revealed, Is.GreaterThan(0).And.LessThan(nodes));
        }

        [UnityTest, Timeout(240000), Description("Vérifie que l'étoile « COMBAT ! » surgit puis disparaît en rendant la main — parce que l'animation d'entrée précède l'action sans la bloquer")]
        public IEnumerator Cue_PopsThenEnds()
        {
            yield return FoggedBoard();
            var board = Object.FindAnyObjectByType<BoardView>()!;
            var burst = Object.FindAnyObjectByType<ComicBurstView>()!;
            bool done = false;
            UniTask.Create(async () =>
            {
                await board.PlayCueAsync(new ComicCueData("COMBAT !", Color.red, -7f), default);
                done = true;
            }).Forget();
            yield return PlayModeHarness.WaitFor(() => burst.Playing, 120);
            Assert.That(burst.Text, Is.EqualTo("COMBAT !"));
            yield return PlayModeHarness.WaitFor(() => done, 600);
            Assert.That(burst.Playing, Is.False);
        }

        [UnityTest, Timeout(240000), Description("Vérifie que la boîte de dialogue joue les répliques avant le combat de Brick, que le premier toucher achève l'écriture et le suivant passe à la réplique suivante — parce que la narration se lit à son rythme")]
        public IEnumerator DialogueBox_PlaysTheBrickLines()
        {
            yield return FoggedBoard();
            var session = ActivityGamesPlayTests.Container.Resolve<GameSession>();
            var node = session.Content.Zone(PrototypeContent.Zone11).Nodes.First(n => n.Id.Value == "n23");
            Assume.That(node.Narration, Is.Not.Null);
            var evt = new NarrationEvent(node.Id, NarrationPhase.Before, node.Narration!.Before);
            bool done = false;
            UniTask.Create(async () =>
            {
                await ActivityGamesPlayTests.CasePresenter().PlayNarrationAsync(evt, default);
                done = true;
            }).Forget();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() != null);
            var box = Object.FindAnyObjectByType<DialogueBoxView>()!;
            yield return PlayModeHarness.WaitFor(() => box.Speaker == "Brick", 300);
            box.TapButton.onClick.Invoke();                       // achève l'écriture
            yield return PlayModeHarness.WaitFor(() => !box.Typing, 120);
            Assert.That(box.Body, Does.Contain("Vous avez traversé"));
            box.TapButton.onClick.Invoke();                       // réplique suivante
            yield return PlayModeHarness.WaitFor(() => box.Speaker != "Brick", 300);
            Assert.That(done, Is.False, "il reste une réplique");
            box.SkipButton.onClick.Invoke();                      // passe le reste
            yield return PlayModeHarness.WaitFor(() => done, 300);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() == null, 300);
        }
    }
}
