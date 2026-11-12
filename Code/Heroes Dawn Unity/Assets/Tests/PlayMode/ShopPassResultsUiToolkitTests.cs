#nullable enable
using System.Collections;
using System.IO;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Critères de sortie de la phase 2A pour la boutique, le pass et la fenêtre de résultats du combat (jeu démarré) : quatre formats, cibles de manette, chemin de focus.</summary>
    public sealed class ShopPassResultsUiToolkitTests
    {
        static string Folder => Path.GetFullPath(Path.Combine(ScreenCaptureTests.Folder, "../uitk-shop"));

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IEnumerator OpenTab(HubPage tab)
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !GameBootstrap.Instance!.Scope.Container.Resolve<IGameFlow>().IsTransitioning);
            for (int i = 0; i < 20; i++) yield return null;
            Object.FindAnyObjectByType<HubShellView>()!.TabButton(tab).Button.onClick.Invoke();
            yield return null; yield return null;
            for (int i = 0; i < 30; i++) yield return null;
        }

        static IEnumerator OpenResults()
        {
            yield return CombatScreenUiToolkitTests.InCombat();
            var hud = Object.FindAnyObjectByType<CombatView>()!.Hud;
            hud.DebugButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<CombatResultsPopupView>(), 1200);
            Assert.That(PlayModeHarness.Visible<CombatResultsPopupView>(), Is.True, "la fenêtre de résultats s'affiche");
            yield return new WaitForSeconds(1.5f); // apparition et décompte des récompenses
        }

        static System.Func<UnityEngine.UIElements.Button, bool> In<T>() where T : Component
            => b => b.panel != null && Object.FindAnyObjectByType<T>() is T v && v.GetComponent<UIDocument>().rootVisualElement.Contains(b);

        /// <summary>La boutique s'ouvre sur l'onglet Héros (recrutement) ; l'onglet Boosters affiche la page Boutique.</summary>
        static IEnumerator OpenShop(int shopTab)
        {
            yield return OpenTab(HubPage.Shop);
            Assert.That(PlayModeHarness.Visible<RecruitPageView>(), Is.True, "la boutique s'ouvre sur l'onglet Héros");
            if (shopTab > 0)
            {
                Object.FindAnyObjectByType<RecruitPageView>()!.ShopTabs.Tab(shopTab).Button.onClick.Invoke();
                yield return null; yield return null;
                Assert.That(PlayModeHarness.Visible<BoutiquePageView>(), Is.True, "l'onglet Boosters affiche la page Boutique");
                for (int i = 0; i < 30; i++) yield return null;
            }
        }

        [UnityTest, Timeout(300000), Description("Vérifie que l'onglet Héros de la boutique (recrutement) garde ses boutons dans le panneau et à taille de manette sur 16:9, 20:9, 4:3 et 1280×720 — parce que sa grille d'offres est la page la plus chargée du hub")]
        public IEnumerator ShopHeroes_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenShop(0);
            yield return UiToolkitAcceptance.CheckFormats(Folder, "shop-heroes", In<RecruitPageView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons de l'onglet Héros de la boutique sont atteignables à la manette — parce qu'aucune offre ne doit être hors de portée de la croix directionnelle")]
        public IEnumerator ShopHeroes_Gamepad_EveryButtonIsReachable()
        {
            yield return OpenShop(0);
            yield return UiToolkitAcceptance.CheckGamepadReach("shop-heroes", In<RecruitPageView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que l'onglet Boosters de la boutique garde ses boutons dans le panneau et à taille de manette sur les quatre formats — parce que les packs et accélérateurs forment une grille dense")]
        public IEnumerator ShopBoosters_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenShop(1);
            yield return UiToolkitAcceptance.CheckFormats(Folder, "shop-boosters", In<BoutiquePageView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons de l'onglet Boosters sont atteignables à la manette — parce que l'achat doit se faire à la croix directionnelle")]
        public IEnumerator ShopBoosters_Gamepad_EveryButtonIsReachable()
        {
            yield return OpenShop(1);
            yield return UiToolkitAcceptance.CheckGamepadReach("shop-boosters", In<BoutiquePageView>(), 2);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que le pass saisonnier garde ses boutons dans le panneau et à taille de manette sur les quatre formats — parce que sa liste de paliers déborde en premier sur les formats courts")]
        public IEnumerator Pass_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenTab(HubPage.Pass);
            Assert.That(PlayModeHarness.Visible<PassPageView>(), Is.True);
            yield return UiToolkitAcceptance.CheckFormats(Folder, "pass", In<PassPageView>(), 1);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que tous les boutons du pass sont atteignables à la manette — parce que la réclamation des paliers doit se faire à la croix directionnelle")]
        public IEnumerator Pass_Gamepad_EveryButtonIsReachable()
        {
            yield return OpenTab(HubPage.Pass);
            yield return UiToolkitAcceptance.CheckGamepadReach("pass", In<PassPageView>(), 1);
        }

        [UnityTest, Timeout(400000), Description("Vérifie que la fenêtre de résultats du combat garde son bouton Continuer dans le panneau et à taille de manette sur les quatre formats — parce qu'elle ne doit jamais bloquer le joueur")]
        public IEnumerator Results_ButtonsStayInsideAndTappable_OnTheFourFormats()
        {
            yield return OpenResults();
            yield return UiToolkitAcceptance.CheckFormats(Folder, "results", In<CombatResultsPopupView>(), 1);
        }

        [UnityTest, Timeout(400000), Description("Vérifie que les boutons de la fenêtre de résultats sont atteignables à la manette — parce que le joueur doit pouvoir continuer sans souris")]
        public IEnumerator Results_Gamepad_EveryButtonIsReachable()
        {
            yield return OpenResults();
            yield return UiToolkitAcceptance.CheckGamepadReach("results", In<CombatResultsPopupView>(), 1);
        }
    }
}
