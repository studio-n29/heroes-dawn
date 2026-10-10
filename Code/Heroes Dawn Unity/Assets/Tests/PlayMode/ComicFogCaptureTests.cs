#nullable enable
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Content.Prototype;
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
    /// <summary>Captures du brouillard, de l'étoile de bande dessinée et de la boîte de dialogue pour relecture visuelle (explicites).</summary>
    public sealed class ComicFogCaptureTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures du plateau sous le brouillard, de l'étoile « COMBAT ! » et de la boîte de dialogue de Brick — parce que ces rendus se relisent à l'œil")]
        public IEnumerator CaptureComicFogAndDialogue()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var session = ActivityGamesPlayTests.Container.Resolve<N29.HeroesDawn.Application.Session.GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 4, false);
            session.RunZone = zone;
            ActivityGamesPlayTests.Container.Resolve<MessagePipe.IPublisher<N29.HeroesDawn.Application.Messages.NavigateRequested>>()
                .Publish(new N29.HeroesDawn.Application.Messages.NavigateRequested(N29.HeroesDawn.Application.Flow.GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            yield return new WaitForSeconds(1.0f);
            yield return ScreenCaptureTests.Capture("12-brouillard");

            var board = Object.FindAnyObjectByType<BoardView>()!;
            UniTask.Create(async () => await board.PlayCueAsync(new ComicCueData("COMBAT !", ComicCues.ColorOf(ComicCue.Combat), ComicCues.TiltOf(ComicCue.Combat)), default)).Forget();
            yield return new WaitForSeconds(0.45f);
            yield return ScreenCaptureTests.Capture("13-bd-combat");
            yield return new WaitForSeconds(1.0f);

            var node = zone.Nodes.First(n => n.Id.Value == "n23");
            var evt = new NarrationEvent(node.Id, NarrationPhase.Before, node.Narration!.Before);
            UniTask.Create(async () => await ActivityGamesPlayTests.CasePresenter().PlayNarrationAsync(evt, default)).Forget();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() != null);
            yield return new WaitForSeconds(1.2f);
            yield return ScreenCaptureTests.Capture("14-dialogue-brick");
            var box = Object.FindAnyObjectByType<DialogueBoxView>()!;
            box.TapButton.onClick.Invoke();
            yield return new WaitForSeconds(0.2f);
            box.TapButton.onClick.Invoke();
            yield return new WaitForSeconds(1.8f);
            yield return ScreenCaptureTests.Capture("15-dialogue-agent");
            box.SkipButton.onClick.Invoke();
            yield return new WaitForSeconds(0.3f);
        }
    }
}
