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
    /// <summary>Critères de sortie de la phase 2A pour la carte de la ville et les chapitres (jeu démarré) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class CityMapScreenUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-map"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator Open(HubPage page)
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>().IsTransitioning);
            for (int i = 0; i < 20; i++) yield return null;
            var nav = Object.FindAnyObjectByType<HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            nav.Open(HubPage.CityMap);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<CityMapPageView>());
            if (page == HubPage.Chapters)
            {
                Object.FindAnyObjectByType<CityMapPageView>()!.ChaptersButton.onClick.Invoke();
                yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<ChaptersPageView>());
            }
            for (int i = 0; i < 20; i++) yield return null;
        }

        static System.Func<UnityEngine.UIElements.Button, bool> In<T>() where T : Component
            => b => b.panel != null && Object.FindAnyObjectByType<T>() is T v && v.GetComponent<UIDocument>().rootVisualElement.Contains(b);

        [UnityTest, Timeout(300000), Description("Vérifie que la carte de la ville garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce que la carte liste les zones et le bouton de départ en mission")]
        public IEnumerator Map_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return Open(HubPage.CityMap);
            yield return UiToolkitAcceptance.CheckFormats(Folder, "map", In<CityMapPageView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons de la carte de la ville sont atteignables à la manette — parce que le choix de zone et le départ doivent se faire à la croix directionnelle")]
        public IEnumerator Map_Gamepad_EveryButtonIsReachable()
        {
            yield return Open(HubPage.CityMap);
            yield return UiToolkitAcceptance.CheckGamepadReach("map", In<CityMapPageView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que la page des chapitres garde ses boutons dans le panneau et à taille de manette sur les quatre formats — parce que la liste des chapitres peut dépasser la hauteur sur les formats courts")]
        public IEnumerator Chapters_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return Open(HubPage.Chapters);
            yield return UiToolkitAcceptance.CheckFormats(Folder, "chapters", In<ChaptersPageView>(), 1);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons de la page des chapitres sont atteignables à la manette — parce qu'aucun chapitre ne doit être hors de portée de la croix directionnelle")]
        public IEnumerator Chapters_Gamepad_EveryButtonIsReachable()
        {
            yield return Open(HubPage.Chapters);
            yield return UiToolkitAcceptance.CheckGamepadReach("chapters", In<ChaptersPageView>(), 1);
        }
    }
}
