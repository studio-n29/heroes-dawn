#nullable enable
using System.Collections;
using System.IO;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour le hub (coque et QG, jeu complet démarré) : quatre formats, cibles de manette, chemin de focus complet.</summary>
    public sealed class HubScreenUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-hub"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OnHub()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>().IsTransitioning);
            Assert.That(PlayModeHarness.Visible<QgPageView>(), Is.True, "connexion → QG");
            for (int i = 0; i < 30; i++) yield return null;
        }

        [UnityTest, Timeout(300000), Description("Vérifie que le hub (coque et QG) garde tous ses boutons visibles, dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce qu'un écran migré ne doit pas perdre sa disposition sur un format (point de vigilance 1 du plan)")]
        public IEnumerator Hub_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OnHub();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "hub", _ => true, 6);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons visibles du hub sont atteignables à la manette depuis le premier — parce qu'aucun écran ne doit avoir de bouton inaccessible à la manette (spec §3)")]
        public IEnumerator Hub_Gamepad_EveryButtonIsReachable()
        {
            yield return OnHub();
            yield return UiToolkitAcceptance.CheckGamepadReach("hub", _ => true, 6);
        }
    }
}
