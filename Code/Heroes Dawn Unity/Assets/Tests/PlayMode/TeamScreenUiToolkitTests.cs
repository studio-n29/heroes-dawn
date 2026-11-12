#nullable enable
using System.Collections;
using System.IO;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Session;
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
    /// <summary>Critères de sortie de la phase 2A pour l'équipe et la fiche d'agent (jeu démarré) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class TeamScreenUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-team"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OnHub()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>().IsTransitioning);
            for (int i = 0; i < 20; i++) yield return null;
        }

        static IEnumerator OpenTeam()
        {
            yield return OnHub();
            Object.FindAnyObjectByType<HubShellView>()!.TabButton(HubPage.Team).Button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<TeamPageView>());
            for (int i = 0; i < 20; i++) yield return null;
        }

        static IEnumerator OpenSheet()
        {
            yield return OnHub();
            var c = GameBootstrap.Instance!.Scope.Container;
            var nav = Object.FindAnyObjectByType<HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            nav.Open(HubPage.MemberSheet, c.Resolve<GameSession>().Roster[0].Id);
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<MemberSheetView>());
            for (int i = 0; i < 20; i++) yield return null;
        }

        static System.Func<UnityEngine.UIElements.Button, bool> In<T>() where T : Component
            => b => b.panel != null && Object.FindAnyObjectByType<T>() is T v && v.GetComponent<UIDocument>().rootVisualElement.Contains(b);

        [UnityTest, Timeout(300000), Description("Vérifie que la page Équipe garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce que la grille des agents est la page la plus dense du hub")]
        public IEnumerator Team_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenTeam();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "team", In<TeamPageView>(), 3);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons de la page Équipe sont atteignables à la manette — parce qu'aucun agent ou action ne doit être hors de portée de la croix directionnelle")]
        public IEnumerator Team_Gamepad_EveryButtonIsReachable()
        {
            yield return OpenTeam();
            yield return UiToolkitAcceptance.CheckGamepadReach("team", In<TeamPageView>(), 3);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que la fiche d'agent garde ses boutons dans le panneau et à taille de manette sur les quatre formats — parce que la fiche empile portrait, statistiques, compétences et équipement")]
        public IEnumerator Sheet_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenSheet();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "sheet", In<MemberSheetView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons de la fiche d'agent sont atteignables à la manette — parce que l'équipement et les compétences s'y consultent à la croix directionnelle")]
        public IEnumerator Sheet_Gamepad_EveryButtonIsReachable()
        {
            yield return OpenSheet();
            yield return UiToolkitAcceptance.CheckGamepadReach("sheet", In<MemberSheetView>(), 2);
        }
    }
}
