#nullable enable
using System.Collections;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Activities;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Captures des activités jouables pour relecture visuelle (Code/docs/test-reports/captures, ignoré par git). Explicites : à la demande.</summary>
    public sealed class ActivityCaptureTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        [UnityTest, Explicit("Captures à la demande"), Timeout(240000), Description("Produit la capture de la potion magique avec deux ingrédients déjà ajoutés — parce que la mise en page du livre et des ingrédients se relit à l'œil")]
        public IEnumerator CapturePotion()
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeZones.Zone13, "n06");
            var session = ActivityGamesPlayTests.Container.Resolve<GameSession>();
            PlayModeHarness.TapNode(new CaseId("n06"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<PotionPopupView>() != null);
            var potion = ActivityGamesPlayTests.Find<PotionPopupView>();
            var recipe = session.Run!.CurrentActivity!.Level.PayloadAs<PotionRecipe>()!;
            potion.IngredientButton(recipe.Steps[0].Ingredient).onClick.Invoke();
            potion.QuantityPlus.onClick.Invoke();
            potion.IngredientButton(recipe.Steps[1].Ingredient).onClick.Invoke();
            yield return ScreenCaptureTests.Capture("07-potion");
            // Un essai raté pour voir le retour sur les étapes.
            potion.QuantityMinus.onClick.Invoke();
            potion.IngredientButton(recipe.Steps[2 % recipe.Steps.Count].Ingredient).onClick.Invoke();
            potion.BrewButton.onClick.Invoke();
            yield return ScreenCaptureTests.Capture("07b-potion-retour");
            Assert.That(System.IO.File.Exists(System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../docs/test-reports/captures/07b-potion-retour.png"))));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(240000), Description("Produit la capture du dialogue des quais (réplique et trois ou quatre réponses) — parce que la mise en page des textes se relit à l'œil")]
        public IEnumerator CaptureDialogue()
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeContent.Zone11, "n21");
            PlayModeHarness.TapNode(new CaseId("n21"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialoguePopupView>() != null);
            var dialogue = ActivityGamesPlayTests.Find<DialoguePopupView>();
            yield return PlayModeHarness.WaitFor(() => dialogue.ChoiceButtons > 0);
            yield return ScreenCaptureTests.Capture("08-dialogue");
            Assert.That(System.IO.File.Exists(System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../docs/test-reports/captures/08-dialogue.png"))));
        }

        [UnityTest, Explicit("Captures à la demande"), Timeout(400000), Description("Produit les captures de la survie en neige, de la pêche et de l'arène en cours de partie (3D) — parce que le rendu des primitives plates et le HUD se relisent à l'œil")]
        public IEnumerator CaptureTheStages()
        {
            yield return ActivityGamesPlayTests.OnBoardNext(PrototypeContent.Zone11, "n15");
            foreach (var (id, env, name, frames) in new[]
            {
                ("survival_snow", new ZoneEnvironment(Biome.Snow, Weather.Snow), "09-survie", 140),
                ("fishing_docks", new ZoneEnvironment(Biome.Harbor, Weather.Night), "10-peche", 80),
                ("arena_training", new ZoneEnvironment(Biome.City, Weather.Day), "11-arene", 260),
            })
            {
                var plan = ActivityGamesPlayTests.PlanOf(id, env);
                var request = ActivityGamesPlayTests.CasePresenter().StageRequestFor(plan);
                ArenaGame? arena = null;
                if (request.ArenaFactory != null) { var inner = request.ArenaFactory; request.ArenaFactory = i => arena = inner(i); }
                ActivityResult? result = null;
                var run = ActivityGamesPlayTests.OpenStage(request, v =>
                {
                    if (request.Kind == StageKind.Arena) v.AgentButtonAt(0).onClick.Invoke(); else v.StartButton.onClick.Invoke();
                    v.InputOverride = () =>
                    {
                        if (request.Kind == StageKind.Survival) { var g = request.Survival!; var d = (g.Target - g.Position).Normalized; return new StageInput { Move = new Vector2(d.X, d.Y) }; }
                        if (request.Kind == StageKind.Fishing) return new StageInput { APressed = request.Fishing!.State == FishingState.Idle, Move = new Vector2(0.3f, 0.2f) };
                        var a = arena;
                        if (a == null) return default;
                        var near = a.Enemies.Where(e => e.Alive).OrderBy(e => Vec2f.Distance(e.Position, a.Position)).FirstOrDefault();
                        var dir = near == null ? Vec2f.Zero : (near.Position - a.Position).Normalized;
                        return new StageInput { Move = new Vector2(dir.X, dir.Y), A = true };
                    };
                }, r => result = r);
                run.MoveNext();
                yield return run.Current;
                for (int i = 0; i < frames && result == null; i++) { run.MoveNext(); yield return null; }
                yield return ScreenCaptureTests.Capture(name);
                ActivityGamesPlayTests.Find<StageGameView>().Driver?.Abandon();
                while (result == null && run.MoveNext()) yield return run.Current;
            }
            Assert.That(System.IO.File.Exists(System.IO.Path.GetFullPath(System.IO.Path.Combine(UnityEngine.Application.dataPath, "../../docs/test-reports/captures/11-arene.png"))));
        }
    }
}
