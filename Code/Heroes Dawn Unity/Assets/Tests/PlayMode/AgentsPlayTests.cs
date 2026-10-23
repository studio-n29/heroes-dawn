#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Registre des 30 agents avec les vrais prefabs (plan 2026-10-10, sous-projet N).</summary>
    public sealed class AgentsPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Timeout(240000), Description("Vérifie que le bouton Registre de la page Recruter ouvre la vraie page du registre qui liste les 30 agents — parce que le joueur doit voir toute la collection et comment l'obtenir")]
        public IEnumerator Registry_OpensFromRecruit_AndListsTheRegistry()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Login.LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var nav = Object.FindAnyObjectByType<N29.HeroesDawn.Composition.HubLifetimeScope>()!.Container.Resolve<HubNavigator>();
            nav.SelectTab(HubPage.Recruit);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<RecruitPageView>() != null);
            yield return null;
            var recruit = Object.FindAnyObjectByType<RecruitPageView>()!;
            var button = recruit.GetComponentsInChildren<UnityEngine.UI.Button>(true).First(b => b.name == "Registry");
            button.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<AgentsPageView>() != null);
            yield return null;
            var page = Object.FindAnyObjectByType<AgentsPageView>()!;
            int rows = page.GetComponentsInChildren<InfoRowView>(true).Count(r => r.gameObject.activeInHierarchy);
            Assert.That(rows, Is.EqualTo(30), "30 agents : le personnage joueur est un avatar hors registre");
            yield return new WaitForSeconds(0.5f);
            yield return ScreenCaptureTests.Capture("20-registre-agents");
        }
    }
}
