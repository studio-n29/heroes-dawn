#nullable enable
using System.Collections;
using System.IO;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Narration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour le HUD du combat (UI Toolkit sur l'arène uGUI, combat de boss démarré) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class CombatScreenUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-combat"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator InCombat()
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
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialogueBoxView>() != null || SceneManager.GetSceneByName(SceneNames.Combat).isLoaded, 900);
            var box = Object.FindAnyObjectByType<DialogueBoxView>();
            if (box != null) box.SkipButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && !ActivityGamesPlayTests.Flow.IsTransitioning, 900);
            var view = Object.FindAnyObjectByType<CombatView>()!;
            yield return PlayModeHarness.WaitFor(() => view.Hud.BossVisible, 300);
            for (int i = 0; i < 30; i++) yield return null;
        }

        /// <summary>Boutons du HUD du combat (le document du HUD).</summary>
        static System.Func<UnityEngine.UIElements.Button, bool> InHud()
            => b => b.panel != null && Object.FindAnyObjectByType<CombatHudView>() is CombatHudView v && v.GetComponent<UIDocument>().rootVisualElement.Contains(b);

        [UnityTest, Timeout(400000), Description("Vérifie que le HUD du combat (retraite, AUTO, vitesse, pause) garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce qu'il se superpose à l'arène sur tous les formats")]
        public IEnumerator CombatHud_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return InCombat();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "combat", InHud(), 2);
        }

        [UnityTest, Timeout(400000), Description("Vérifie que tous les boutons du HUD du combat sont atteignables à la manette — parce que la retraite, AUTO et la vitesse doivent se jouer à la croix directionnelle")]
        public IEnumerator CombatHud_Gamepad_EveryButtonIsReachable()
        {
            yield return InCombat();
            yield return UiToolkitAcceptance.CheckGamepadReach("combat", InHud(), 2);
        }
    }
}
