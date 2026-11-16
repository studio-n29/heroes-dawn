#nullable enable
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Les parcours de test valident les boutons par un événement direct : ils ne passent jamais par le pointeur, donc un élément
    /// transparent plein écran qui capte les touches (cas réel : la zone sûre du tutoriel) leur échappait, alors qu'un doigt ne pouvait
    /// plus rien toucher. Ces tests font un vrai tir de rayon (<c>Panel.Pick</c>, ce que fait le système d'événements) au centre des boutons
    /// et exigent que ce soit le bouton, ou un de ses enfants, qui reçoive la touche.
    /// </summary>
    public sealed class PointerHitDiagnosticTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest]
        public IEnumerator TitleScreen_PlayAndAccountButtons_ReceiveThePointer()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            for (int i = 0; i < 30; i++) yield return null;
            var view = Object.FindAnyObjectByType<LoginView>()!;
            AssertReceivesPointer("titre : Jouer", view.PlayButton.Element);
            AssertReceivesPointer("titre : Se connecter", view.AccountButton.Element);
        }

        [UnityTest]
        public IEnumerator Hq_Buttons_ReceiveThePointer()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>());
            // Le tutoriel de la première mission grise volontairement tout sauf sa cible : on le passe pour tester le pointeur « libre ».
            N29.HeroesDawn.Composition.GameBootstrap.Instance!.Scope.Container.Resolve<N29.HeroesDawn.Application.Features.Tutorial.TutorialGuide>().Skip();
            yield return new WaitForSecondsRealtime(0.3f);
            for (int i = 0; i < 30; i++) yield return null;

            var shell = Object.FindAnyObjectByType<HubShellView>()!;
            AssertReceivesPointer("QG : réglages", shell.SettingsButton.Element);
            foreach (var tab in new[] { HubPage.Qg, HubPage.Team, HubPage.Shop })
                AssertReceivesPointer("QG : onglet " + tab, shell.TabButton(tab).Element);
            var qg = Object.FindAnyObjectByType<QgPageView>()!;
            AssertReceivesPointer("QG : mission", qg.MissionButton.Element);
        }

        /// <summary>Le centre du bouton, vu du panneau qui le porte puis de tous les panneaux actifs : le premier élément touché doit être le bouton.</summary>
        static void AssertReceivesPointer(string label, VisualElement button)
        {
            var center = button.worldBound.center;
            // Les panneaux sont superposés : celui du dessus (ordre de tri le plus haut) reçoit la touche en premier.
            var docs = Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None)
                .Where(d => d.isActiveAndEnabled && d.rootVisualElement?.panel != null)
                .OrderByDescending(d => d.sortingOrder).ToList();
            foreach (var panel in docs.Select(d => d.rootVisualElement.panel).Distinct())
            {
                var hit = panel.Pick(center);
                if (hit == null) continue;
                Assert.That(IsSameOrChildOf(hit, button), Is.True, $"{label} : la touche au centre du bouton est captée par « {Describe(hit)} » au lieu du bouton");
                return;
            }
            Assert.Fail($"{label} : aucun élément touchable au centre du bouton");
        }

        static bool IsSameOrChildOf(VisualElement? e, VisualElement ancestor)
        {
            for (; e != null; e = e.parent) if (e == ancestor) return true;
            return false;
        }

        static string Describe(VisualElement e)
        {
            var path = new List<string>();
            for (var x = e; x != null && path.Count < 6; x = x.parent)
                path.Add(string.IsNullOrEmpty(x.name) ? x.GetType().Name + "." + string.Join(".", x.GetClasses().Take(2)) : x.name);
            return string.Join(" < ", path);
        }
    }
}
