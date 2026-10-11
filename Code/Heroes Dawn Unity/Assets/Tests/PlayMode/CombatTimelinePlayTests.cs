#nullable enable
using System.Collections;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Frise de combat par points avec les vrais prefabs (plan 2026-10-11, sous-projet 1).</summary>
    public sealed class CombatTimelinePlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator IntoCombat()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            c.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            PlayModeHarness.TapNode(new CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !ActivityGamesPlayTests.Flow.IsTransitioning, 600);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que la frise du vrai combat a 11 segments (points 0 à 10), que chaque unité vivante occupe un segment et qu'une transition dure 0,5 s — parce que la frise doit se lire point par point")]
        public IEnumerator Timeline_ShowsOneSegmentPerPoint()
        {
            yield return IntoCombat();
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.Timeline.Segments.Count > 0, 300);
            var timeline = view.Timeline;
            Assert.That(timeline.Segments.Count, Is.EqualTo(11));
            Assert.That(timeline.StepDuration, Is.EqualTo(0.5f).Within(0.001f));
            int alive = timeline.Segments.Sum(s => s.Occupants.Count);
            Assert.That(alive, Is.EqualTo(timeline.Markers.Count), "chaque pastille est sur un segment");
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit la capture du combat avec la frise par points, une pause sur point vide et l'éventail de compétences — parce que ces rendus se relisent à l'œil")]
        public IEnumerator CaptureTheTickTimeline()
        {
            yield return IntoCombat();
            yield return new WaitForSecondsRealtime(3f);
            yield return ScreenCaptureTests.Capture("21-frise-par-points");
        }
    }
}
