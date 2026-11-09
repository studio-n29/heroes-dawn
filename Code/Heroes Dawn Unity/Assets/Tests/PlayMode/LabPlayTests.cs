#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Infrastructure.Simulated;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Atelier de fabrication (L4) : on y va depuis le QG, on fabrique avec des scraps, l'objet arrive dans l'inventaire.</summary>
    public sealed class LabPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Timeout(300000), Description("Vérifie que l'atelier s'ouvre depuis le QG, que Fabriquer est grisé sans matériaux, et qu'avec des scraps l'objet entre dans l'inventaire — parce que c'est la version jouable du lot L4")]
        public IEnumerator Craft_FromWorkshop_AddsItemToInventory()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !ActivityGamesPlayTests.Flow.IsTransitioning);
            var scope = Object.FindAnyObjectByType<HubLifetimeScope>()!;
            var nav = scope.Container.Resolve<HubNavigator>();
            var session = scope.Container.Resolve<N29.HeroesDawn.Application.Session.GameSession>();
            nav.Open(HubPage.Lab);
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<LabPageView>() != null);
            yield return new WaitForSeconds(0.5f);
            var page = Object.FindAnyObjectByType<LabPageView>()!;
            var recipes = scope.Container.Resolve<N29.HeroesDawn.Application.Features.Meta.Workshop>().Recipes();
            Assert.That(recipes.Count, Is.GreaterThan(0), "au moins une recette connue");

            // Sans matériaux : le bouton de fabrication du panneau est grisé.
            page.SelectRowForTest(0);
            yield return null;
            var craft = page.SideRows.Last();
            Assert.That(craft.Buttons[0].Interactable, Is.False, "aucun matériau : pas de fabrication");

            // Avec des scraps : fabriquer ajoute l'objet à l'inventaire.
            var type = recipes[0].Recipe.Type;
            int before = session.OwnedItemCount;
            CraftingOps.AddScraps(session, N29.HeroesDawn.Definitions.ScrapKinds.For(type), 1000);
            CraftingOps.AddScraps(session, N29.HeroesDawn.Definitions.ScrapKind.Core, 1000);
            page.SelectRowForTest(0);
            yield return null;
            page.SideRows.Last().Buttons[0].Button.onClick.Invoke();
            yield return null;
            Assert.That(session.OwnedItemCount, Is.GreaterThan(before), "l'objet fabriqué est dans l'inventaire");
            yield return ScreenCaptureTests.Capture("70-atelier");
        }
    }
}
