#nullable enable
using System.Collections;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Narration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Combat de boss de la campagne avec les vrais prefabs : barre élargie, phases aux seuils prévus (lot L8 de la 0.15.0).</summary>
    public sealed class BossCombatPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Timeout(300000), Description("Vérifie que le combat du boss Brick affiche sa barre élargie avec son nom et sa phase 1 sur 3, puis qu'en passant sous 66 % la phase 2 s'ouvre à l'écran (nom, bonus, annonce) — parce que le joueur doit voir le boss changer de phase")]
        public IEnumerator BossFight_ShowsTheBar_AndOpensPhaseTwoAtTheThreshold()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var c = ActivityGamesPlayTests.Container;
            var session = c.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 3, true, "n22");
            session.RunZone = zone;
            c.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            PlayModeHarness.TapNode(new CaseId("n23"));
            // Le dialogue de Brick précède le combat : on le passe.
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() != null || SceneManager.GetSceneByName(SceneNames.Combat).isLoaded, 900);
            var box = Object.FindAnyObjectByType<DialogueBoxView>();
            if (box != null) box.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !ActivityGamesPlayTests.Flow.IsTransitioning, 900);

            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.BossBar.gameObject.activeInHierarchy, 300);
            Assert.That(view.BossBar.Name, Is.EqualTo("BRICK"));
            Assert.That(view.BossBar.Phase, Does.Contain("1/3"));
            Assert.That(view.BossBar.MarkCount, Is.EqualTo(2), "un repère par seuil");

            var sim = c.Resolve<IActiveCombatReadModel>().Simulation!;
            var boss = sim.Units.First(u => u.Side == Side.Enemy && u.IsBoss);
            boss.Hp = boss.MaxHp * 66 / 100 + 1;
            view.AutoButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => boss.PhaseReached > 0 || !boss.IsAlive, 3000);
            Assert.That(boss.PhaseReached, Is.EqualTo(1), "la phase 2 s'est ouverte");
            yield return new WaitForSecondsRealtime(0.6f);
            Assert.That(view.BossBar.Phase, Does.Contain("2/3"));
            yield return ScreenCaptureTests.Capture("17-combat-boss-phase");
        }
    }
}
