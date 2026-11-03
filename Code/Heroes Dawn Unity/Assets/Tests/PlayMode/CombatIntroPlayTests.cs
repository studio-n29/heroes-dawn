#nullable enable
using System.Collections;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Entrée en combat avec les vrais prefabs : bandeaux des combattants, passage d'un toucher, introductions répétées abrégées (lot L7 de la 0.15.0).</summary>
    public sealed class CombatIntroPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static CombatIntroData Data(bool repeat) => new CombatIntroData
        {
            AllyTitle = "TON ÉQUIPE",
            EnemyTitle = "ENNEMIS",
            Allies = new[]
            {
                new CombatIntroFighter("GIULIA ROSSI", "Soutien", new PortraitData("Giulia", Family.Matter)),
                new CombatIntroFighter("LUCAS MENDES", "Tank", new PortraitData("Lucas", Family.PhysicalBio)),
            },
            Enemies = new[] { new CombatIntroFighter("BRICK", "BOSS · Tank", new PortraitData("Brick", Family.Elemental, true)) },
            Repeat = repeat,
        };

        sealed class Timing { public float Seconds = -1f; }

        static async UniTask PlayIntro(bool repeat, Timing timing)
        {
            var modals = ActivityGamesPlayTests.Container.Resolve<IModalService>();
            var view = await modals.OpenAsync<ICombatIntroView>(default);
            float start = Time.realtimeSinceStartup;
            await view.PlayAsync(Data(repeat), default);
            timing.Seconds = Time.realtimeSinceStartup - start;
            modals.Close(view);
        }

        static IEnumerator ToQg()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<N29.HeroesDawn.Presentation.Hub.QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que le vrai combat de la campagne ouvre l'entrée en combat avec un bandeau par combattant, que « Passer » la ferme et laisse le combat — parce que le joueur voit qui s'affronte puis peut passer l'introduction")]
        public IEnumerator Campaign_Combat_OpensTheIntro_AndSkipClosesIt()
        {
            yield return ToQg();
            var c = ActivityGamesPlayTests.Container;
            c.Resolve<CombatIntros>().Enabled = true;
            var session = c.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            c.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            PlayModeHarness.TapNode(new CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<CombatIntroPopupView>() != null, 900);

            var intro = Object.FindAnyObjectByType<CombatIntroPopupView>()!;
            Assert.That(intro.Banners.Count, Is.GreaterThanOrEqualTo(2), "au moins un allié et un ennemi");
            Assert.That(intro.AllyTitle, Is.Not.Empty);
            yield return new WaitForSecondsRealtime(1.3f);
            yield return ScreenCaptureTests.Capture("16-entree-combat");
            intro.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<CombatIntroPopupView>() == null, 300);
            Assert.That(SceneManager.GetSceneByName(SceneNames.Combat).isLoaded, Is.True, "le combat reste ouvert derrière");
        }

        [UnityTest, Timeout(300000), Description("Vérifie qu'une introduction jouée en entier dure plus de deux secondes, qu'un toucher la ferme tout de suite et qu'une introduction déjà vue se ferme seule en moins d'une seconde — parce que les introductions répétées ne doivent pas faire attendre")]
        public IEnumerator Intro_Timings_FirstRepeatAndTap()
        {
            yield return ToQg();
            var full = new Timing();
            PlayIntro(false, full).Forget();
            yield return PlayModeHarness.WaitFor(() => full.Seconds >= 0f, 900);
            Assert.That(full.Seconds, Is.GreaterThan(2f), "première fois : glissement des trois bandeaux puis lecture");

            var repeat = new Timing();
            PlayIntro(true, repeat).Forget();
            yield return PlayModeHarness.WaitFor(() => repeat.Seconds >= 0f, 900);
            Assert.That(repeat.Seconds, Is.LessThan(1f), "répétition : abrégée");

            var tapped = new Timing();
            PlayIntro(false, tapped).Forget();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<CombatIntroPopupView>() != null, 300);
            yield return new WaitForSecondsRealtime(0.3f);
            Object.FindAnyObjectByType<CombatIntroPopupView>()!.TapButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => tapped.Seconds >= 0f, 300);
            Assert.That(tapped.Seconds, Is.LessThan(1f), "un toucher ferme tout de suite");
        }
    }
}
