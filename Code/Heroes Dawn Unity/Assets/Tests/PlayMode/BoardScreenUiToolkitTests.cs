#nullable enable
using System.Collections;
using System.IO;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour le plateau (HUD UI Toolkit au-dessus de la surface de jeu, partie démarrée) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class BoardScreenUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-board"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OnBoard()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            var flow = GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !flow.IsTransitioning);
            for (int i = 0; i < 20; i++) yield return null;
            yield return PlayModeHarness.Deploy(Object.FindAnyObjectByType<QgPageView>()!.MissionButton);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !flow.IsTransitioning, 600);
            Assert.That(flow.Current, Is.EqualTo(GameState.Board), "QG → plateau");
            for (int i = 0; i < 40; i++) yield return null;
        }

        /// <summary>Boutons du HUD : tout ce qui n'est pas dans le viewport de la carte (dont les cases défilent hors écran par construction).</summary>
        static System.Func<UnityEngine.UIElements.Button, bool> InHud()
            => b => b.panel != null && Object.FindAnyObjectByType<BoardHudView>() is BoardHudView v
                    && v.GetComponent<UIDocument>().rootVisualElement.Contains(b) && !IsInViewport(b);

        static bool IsInViewport(VisualElement e)
        {
            for (var p = e; p != null; p = p.parent) if (p.name == "viewport") return true;
            return false;
        }

        [UnityTest, Timeout(300000), Description("Vérifie que le HUD du plateau garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce que le HUD (dé, fin de déplacement, retour) se superpose à la carte sur tous les formats")]
        public IEnumerator BoardHud_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OnBoard();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "board", InHud(), 1);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons du HUD du plateau sont atteignables à la manette — parce que le dé et le retour doivent se jouer à la croix directionnelle")]
        public IEnumerator BoardHud_Gamepad_EveryButtonIsReachable()
        {
            yield return OnBoard();
            yield return UiToolkitAcceptance.CheckGamepadReach("board", InHud(), 1);
        }
    }
}
