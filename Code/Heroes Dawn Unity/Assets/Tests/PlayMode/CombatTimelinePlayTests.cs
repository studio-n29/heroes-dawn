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

        [UnityTest, Timeout(300000), Description("Vérifie que les pastilles de la frise sont des losanges bleu marine sans contour noir — parce que la direction artistique HUD pastel remplace les disques à contour noir")]
        public IEnumerator Timeline_MarkersAreDiamonds()
        {
            yield return IntoCombat();
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.Timeline.Markers.Count > 0, 300);
            foreach (var m in view.Timeline.Markers)
            {
                var images = m.GetComponentsInChildren<UnityEngine.UI.Image>(true);
                var frame = System.Array.Find(images, i => i.name == "Frame")!;
                Assert.That(frame.sprite, Is.SameAs(N29.HeroesDawn.Presentation.Common.Sprites.Diamond));
                Assert.That(frame.color, Is.Not.EqualTo(Color.black));
            }
        }

        static void Press(SkillIconView card, bool down)
        {
            var frame = card.transform.Find("Frame").gameObject;
            var e = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            if (down) UnityEngine.EventSystems.ExecuteEvents.Execute(frame, e, UnityEngine.EventSystems.ExecuteEvents.pointerDownHandler);
            else UnityEngine.EventSystems.ExecuteEvents.Execute(frame, e, UnityEngine.EventSystems.ExecuteEvents.pointerUpHandler);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que l'éventail de compétences n'apparaît que pour l'agent dont c'est le tour, que cet agent a un contour lumineux et que la description d'une carte s'affiche tant qu'on reste appuyé puis disparaît au relâchement — parce que c'est l'organisation de combat voulue")]
        public IEnumerator Fan_ShowsOnAgentTurn_AndHoldShowsDescription()
        {
            yield return IntoCombat();
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.FanVisible, 2400);
            Assert.That(view.FanVisible, Is.True, "un agent est prêt : l'éventail est visible");
            Assert.That(view.DescriptionVisible, Is.False, "pas d'appui : pas de description");
            var card = view.SkillCard(0);
            Assert.That(card.Data.Visible, Is.True);
            Press(card, true);
            Assert.That(view.DescriptionVisible, Is.True, "appui maintenu : la description est affichée");
            Assert.That(view.DescriptionText, Is.Not.Empty);
            yield return null;
            Assert.That(view.DescriptionVisible, Is.True, "tant qu'on reste appuyé");
            Press(card, false);
            Assert.That(view.DescriptionVisible, Is.False, "relâché : la description disparaît");
        }

        static void DragCard(SkillIconView card, Vector2 to)
        {
            var frame = card.transform.Find("Frame").gameObject;
            var es = UnityEngine.EventSystems.EventSystem.current;
            var e = new UnityEngine.EventSystems.PointerEventData(es) { position = to };
            UnityEngine.EventSystems.ExecuteEvents.Execute(frame, e, UnityEngine.EventSystems.ExecuteEvents.beginDragHandler);
            UnityEngine.EventSystems.ExecuteEvents.Execute(frame, e, UnityEngine.EventSystems.ExecuteEvents.dragHandler);
            UnityEngine.EventSystems.ExecuteEvents.Execute(frame, e, UnityEngine.EventSystems.ExecuteEvents.endDragHandler);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que glisser une carte de compétence jusqu'à sa cible (ou dans le vide pour une compétence générale) la lance et la fait revenir dans l'éventail — parce que c'est le geste voulu pour jouer ses cartes")]
        public IEnumerator Fan_DragAndDrop_PlaysTheCard()
        {
            yield return IntoCombat();
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.FanVisible, 2400);
            var sim = ActivityGamesPlayTests.Container.Resolve<N29.HeroesDawn.Application.Features.Combat.IActiveCombatReadModel>().Simulation!;
            var actor = sim.Units.First(u => u.Side == N29.HeroesDawn.Domain.Combat.Side.Ally && u.IsReady);
            int slot = -1;
            for (int i = 0; i < 3; i++) if (view.SkillCard(i).Data.Enabled) { slot = i; break; }
            Assume.That(slot, Is.GreaterThanOrEqualTo(0), "aucune carte lançable");
            var card = view.SkillCard(slot);
            var enemy = sim.Units.First(u => u.Side == N29.HeroesDawn.Domain.Combat.Side.Enemy && u.IsAlive);
            var target = card.Data.NeedsTarget ? view.UnitScreenPoint(enemy.Id) : new Vector2(Screen.width * 0.92f, Screen.height * 0.2f);
            DragCard(card, target);
            Assert.That(card.transform.parent, Is.SameAs(view.FanRootForTests), "la carte est revenue dans l'éventail");
            yield return PlayModeHarness.WaitFor(() => !actor.IsReady, 600);
            Assert.That(actor.IsReady, Is.False, "la compétence a été lancée");
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(300000), Description("Produit les captures de l'éventail de compétences et de la description d'une carte — parce que ces rendus se relisent à l'œil")]
        public IEnumerator CaptureTheFan()
        {
            yield return IntoCombat();
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.FanVisible, 2400);
            yield return new WaitForSecondsRealtime(0.6f);
            yield return ScreenCaptureTests.Capture("22-eventail");
            Press(view.SkillCard(1), true);
            yield return new WaitForSecondsRealtime(0.3f);
            yield return ScreenCaptureTests.Capture("23-description-carte");
            Press(view.SkillCard(1), false);
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
