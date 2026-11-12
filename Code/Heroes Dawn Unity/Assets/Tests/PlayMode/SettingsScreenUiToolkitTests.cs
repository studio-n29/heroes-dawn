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
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour les réglages (7 catégories, jeu démarré) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class SettingsScreenUiToolkitTests
    {
        const int Categories = 7;
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-settings"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OpenSettings()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>().IsTransitioning);
            for (int i = 0; i < 20; i++) yield return null;
            Object.FindAnyObjectByType<HubLifetimeScope>()!.Container.Resolve<HubNavigator>().Open(HubPage.Settings);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<SettingsPageView>());
            for (int i = 0; i < 30; i++) yield return null;
        }

        static IEnumerator SelectCategory(int index)
        {
            Object.FindAnyObjectByType<SettingsPageView>()!.CategoryButton(index).onClick.Invoke();
            for (int i = 0; i < 15; i++) yield return null;
        }

        static System.Func<UnityEngine.UIElements.Button, bool> InPage()
            => b => b.panel != null && Object.FindAnyObjectByType<SettingsPageView>() is SettingsPageView v && v.GetComponent<UIDocument>().rootVisualElement.Contains(b);

        [UnityTest, Timeout(600000), Description("Vérifie que chacune des 7 catégories de réglages garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce que la catégorie Compte est la plus haute et déborde en premier sur les formats courts")]
        public IEnumerator Settings_EveryCategory_StaysInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenSettings();
            for (int c = 0; c < Categories; c++)
            {
                yield return SelectCategory(c);
                yield return UiToolkitAcceptance.CheckFormats(Folder, "settings-" + c, InPage(), 8);
            }
        }

        [UnityTest, Timeout(600000), Description("Vérifie que dans chacune des 7 catégories de réglages tous les boutons sont atteignables à la manette — parce que les curseurs et interrupteurs doivent se régler à la croix directionnelle")]
        public IEnumerator Settings_EveryCategory_GamepadReachesEveryButton()
        {
            yield return OpenSettings();
            for (int c = 0; c < Categories; c++)
            {
                yield return SelectCategory(c);
                yield return UiToolkitAcceptance.CheckGamepadReach("settings-" + c, InPage(), 8);
            }
        }
    }
}
