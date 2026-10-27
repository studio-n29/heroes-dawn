#nullable enable
using System.Collections;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Les cinq activités jouées avec les vrais prefabs : potion, dialogue, pêche, survie en neige, arène de combat.</summary>
    public sealed class ActivityGamesPlayTests
    {
        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        internal static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        internal static IGameFlow Flow => Container.Resolve<IGameFlow>();

        static IEnumerator SignedIn()
        {
            yield return PlayModeHarness.Boot(new MemoryProfileGateway(), new InstantAuthGateway());
            Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<QgPageView>() && !Flow.IsTransitioning);
        }

        /// <summary>Plateau affiché, pion à côté de la case <paramref name="target"/> (1 PM) dans la zone <paramref name="zoneId"/>.</summary>
        internal static IEnumerator OnBoardNext(ZoneId zoneId, string target)
        {
            yield return SignedIn();
            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(zoneId);
            var from = new BoardGraph(zone).Neighbors(new CaseId(target)).First();
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, from.Value);
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardView>() && !Flow.IsTransitioning);
        }

        internal static T Find<T>() where T : Object => Object.FindAnyObjectByType<T>()!;

        // ------------------------------------------------------------------ Potion

        [UnityTest, Timeout(240000), Description("Vérifie que la potion se joue de bout en bout sur le vrai écran : les ingrédients de la recette ajoutés dans l'ordre avec leur quantité, « Préparer » réussit, la case est résolue et la récompense versée — parce que la potion magique est une activité jouable")]
        public IEnumerator Potion_PlayedWithTheRealScreen_Succeeds()
        {
            yield return OnBoardNext(PrototypeZones.Zone13, "n06");
            var session = Container.Resolve<GameSession>();
            int credits = session.Wallet.Credits;
            PlayModeHarness.TapNode(new CaseId("n06"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<PotionPopupView>() != null);
            var popup = Find<PotionPopupView>();
            var recipe = session.Run!.CurrentActivity!.Level.PayloadAs<PotionRecipe>()!;
            Assert.That(popup.IngredientButtons, Is.EqualTo(recipe.Ingredients.Count));
            foreach (var step in recipe.Steps)
            {
                for (int i = 1; i < step.Quantity; i++) popup.QuantityPlus.onClick.Invoke();
                popup.IngredientButton(step.Ingredient).onClick.Invoke();
                for (int i = 1; i < step.Quantity; i++) popup.QuantityMinus.onClick.Invoke();
            }
            popup.BrewButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<PotionPopupView>() == null && session.Run.Phase == ZoneRunPhase.AwaitingRoll, 600);
            Assert.That(session.Run.IsSpent(new CaseId("n06")), Is.True);
            Assert.That(session.Wallet.Credits, Is.EqualTo(credits + 250));
            Assert.That(session.Run.Maluses, Is.Empty);
        }

        [UnityTest, Timeout(240000), Description("Vérifie qu'une potion ratée jusqu'au dernier essai (mauvaises quantités) inflige le malus de 15 % au patient et laisse la case à rejouer — parce que l'échec pénalise l'agent pour tout le plateau")]
        public IEnumerator Potion_FailedUntilTheLastAttempt_AppliesTheMalus()
        {
            yield return OnBoardNext(PrototypeZones.Zone13, "n06");
            var session = Container.Resolve<GameSession>();
            PlayModeHarness.TapNode(new CaseId("n06"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<PotionPopupView>() != null);
            var popup = Find<PotionPopupView>();
            var plan = session.Run!.CurrentActivity!;
            var recipe = plan.Level.PayloadAs<PotionRecipe>()!;
            for (int attempt = 0; attempt < recipe.Attempts; attempt++)
            {
                foreach (var step in recipe.Steps)
                    popup.IngredientButton((step.Ingredient + 1) % recipe.Ingredients.Count).onClick.Invoke();
                popup.BrewButton.onClick.Invoke();
            }
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<PotionPopupView>() == null && session.Run.Phase == ZoneRunPhase.AwaitingRoll, 600);
            Assert.That(session.Run.IsSpent(new CaseId("n06")), Is.False);
            Assert.That(session.Run.Maluses.Count, Is.EqualTo(1));
            Assert.That(session.Run.MalusPercent(plan.Patient!.Value), Is.EqualTo(-15));
        }

        // ------------------------------------------------------------------ Dialogue

        [UnityTest, Timeout(240000), Description("Vérifie que le dialogue se joue sur le vrai écran : les meilleures réponses à chaque réplique réussissent, la case est résolue, la récompense versée et aucun combat n'est lancé — parce que les bonnes réponses évitent le combat")]
        public IEnumerator Dialogue_BestAnswers_Succeed()
        {
            yield return OnBoardNext(PrototypeContent.Zone11, "n21");
            var session = Container.Resolve<GameSession>();
            int credits = session.Wallet.Credits;
            PlayModeHarness.TapNode(new CaseId("n21"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialoguePopupView>() != null);
            var popup = Find<DialoguePopupView>();
            var script = session.Run!.CurrentActivity!.Level.PayloadAs<DialogueScript>()!;
            foreach (var line in script.Lines)
            {
                yield return PlayModeHarness.WaitFor(() => popup.ChoiceButtons == line.Choices.Count);
                Assert.That(popup.Prompt, Is.Not.Empty);
                popup.ChoiceButton(line.Choices.First(c => c.Score == 2).Index).onClick.Invoke();
                yield return PlayModeHarness.WaitFor(() => popup.ChoiceButtons == 0, 120);
            }
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialoguePopupView>() == null && session.Run.Phase == ZoneRunPhase.AwaitingRoll, 900);
            Assert.That(session.Run.IsSpent(new CaseId("n21")), Is.True);
            Assert.That(session.Wallet.Credits, Is.EqualTo(credits + 200));
        }

        [UnityTest, Timeout(300000), Description("Vérifie que de mauvaises réponses au dialogue lancent le combat Facile enchaîné (scène de combat chargée) — parce que l'échec du dialogue tourne mal")]
        public IEnumerator Dialogue_BadAnswers_LaunchTheCombat()
        {
            yield return OnBoardNext(PrototypeContent.Zone11, "n21");
            var session = Container.Resolve<GameSession>();
            PlayModeHarness.TapNode(new CaseId("n21"));
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<DialoguePopupView>() != null);
            var popup = Find<DialoguePopupView>();
            var script = session.Run!.CurrentActivity!.Level.PayloadAs<DialogueScript>()!;
            foreach (var line in script.Lines)
            {
                yield return PlayModeHarness.WaitFor(() => popup.ChoiceButtons == line.Choices.Count);
                popup.ChoiceButton(line.Choices.First(c => c.Score == 0).Index).onClick.Invoke();
                yield return PlayModeHarness.WaitFor(() => popup.ChoiceButtons == 0, 120);
            }
            yield return PlayModeHarness.WaitFor(() => UnityEngine.SceneManagement.SceneManager.GetSceneByName(SceneNames.Combat).isLoaded && Flow.Current == GameState.Combat && !Flow.IsTransitioning, 1500);
            Assert.That(session.Run.PendingCase!.Type, Is.EqualTo(CaseType.Combat));
        }

        // ------------------------------------------------------------------ Jeux 3D

        internal static ActivityPlan PlanOf(string id, ZoneEnvironment env, params (string Key, int Value)[] overrides)
        {
            var session = Container.Resolve<GameSession>();
            var def = session.Content.Activity(new ActivityId(id));
            var p = def.Parameters;
            foreach (var (k, v) in overrides) p = p.With(k, v);
            return ActivityPlanner.Plan(ActivityRegistry.Default, def.WithParameters(p), env, 5UL, new CaseId("n01"), 1,
                                        session.Squad.Select(m => m.Id).ToList());
        }

        internal static CasePopupPresenter CasePresenter()
            => Object.FindAnyObjectByType<BoardLifetimeScope>()!.Container.Resolve<CasePopupPresenter>();

        internal static IEnumerator OpenStage(StageRequest request, System.Action<StageGameView> started, System.Action<ActivityResult> done)
        {
            var modals = Container.Resolve<IModalService>();
            var task = UniTask.Create(async () =>
            {
                var view = await modals.OpenAsync<IStageGameView>(default);
                var result = await view.PlayAsync(request, default);
                modals.Close(view);
                done(result);
            });
            yield return PlayModeHarness.WaitFor(() => Object.FindAnyObjectByType<StageGameView>() != null);
            started(Find<StageGameView>());
            while (!task.Status.IsCompleted()) yield return null;
        }

        [UnityTest, Timeout(240000), Description("Vérifie que la survie en neige se joue sur la vraie fenêtre 2D commune : le premier toucher lance la partie, puis un joueur qui pousse le joystick droit vers l'objet gagne — parce que trouver l'objet est le but")]
        public IEnumerator Survival_PlayedOnTheCommonWindow_ReachingTheTargetWins()
        {
            yield return OnBoardNext(PrototypeContent.Zone11, "n15");
            var plan = PlanOf("survival_snow", new ZoneEnvironment(Biome.Snow, Weather.Snow), ("obstacle_count", 0), ("target_distance_m", 14));
            var presenter = CasePresenter();
            var driver = MiniGameDrivers.Create(plan, new N29.HeroesDawn.Presentation.Mvp.LocLocalization())!;
            var request = presenter.MiniGame2DRequestFor(plan, driver);
            var host = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ModalLayer>()!;
            var view = host.Spawn<IMiniGame2DPopupView>();
            var task = view.PlayAsync(request, default);
            var popup = (MiniGame2DPopupView)view;
            var game = ((N29.HeroesDawn.Domain.Activities.Games.SnowSurvivalMiniGame)driver.Game).Inner;
            popup.Input.PressAt(new Vector2(0.5f, 0.5f));
            yield return null;
            Assert.That(popup.Started, Is.True, "le premier toucher lance la partie");
            popup.Input.ReleaseNow();
            yield return null;
            // Joystick : on pose le doigt au centre et on le pousse vers l'objet, au bout de la portée.
            var rect = popup.Field.rect;
            for (int i = 0; i < 3000 && !task.Status.IsCompleted(); i++)
            {
                var dir = (game.Target - game.Position).Normalized;
                if (!popup.Input.Held) popup.Input.PressAt(new Vector2(0.5f, 0.5f));
                float reach = MiniGameField.StickRadius;
                popup.Input.DragTo(new Vector2(0.5f + dir.X * reach, 0.5f + dir.Y * reach * rect.width / rect.height));
                yield return null;
            }
            Assert.That(task.Status.IsCompleted(), Is.True, "la partie se termine");
            Assert.That(task.GetAwaiter().GetResult().Success, Is.True);
            Assert.That(game.Status, Is.EqualTo(N29.HeroesDawn.Domain.Activities.Games.GameStatus.Won));
        }

        [UnityTest, Timeout(120000), Description("Vérifie que Rempart se joue sur la vraie fenêtre 2D : le premier toucher lance la partie, puis un joueur qui touche les projectiles gagne avant la fin du temps — parce que c'est le mini-jeu du quai de Red Hook")]
        public IEnumerator Deflect_PlayedOnTheRealWindow_Wins()
        {
            yield return OnBoardNext(PrototypeContent.Zone11, "n15");
            var plan = PlanOf("deflect_docks", new ZoneEnvironment(Biome.Harbor, Weather.Day), ("target", 3), ("time_limit_s", 60));
            var request = CasePresenter().HeroGameRequestFor(plan);
            var host = Object.FindAnyObjectByType<N29.HeroesDawn.Presentation.Common.ModalLayer>()!;
            var view = host.Spawn<N29.HeroesDawn.Presentation.Activities.IHeroGamePopupView>();
            var task = view.PlayAsync(request, default);
            var popup = (N29.HeroesDawn.Presentation.Activities.HeroGamePopupView)view;
            var tap = popup.Field.GetComponent<N29.HeroesDawn.Presentation.Activities.HeroFieldTap>();
            tap.OnPointerDown(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = Vector2.zero });
            Assert.That(popup.Started, Is.True, "le premier toucher lance la partie");
            var game = request.Deflect!;
            for (int i = 0; i < 60; i++) { game.Step(0.05f); yield return null; }
            yield return ScreenCaptureTests.Capture("18-rempart");
            for (int i = 0; i < 1800 && game.Status == GameStatus.Running; i++)
            {
                foreach (var it in game.Items.Where(x => !x.Civilian && x.Y > 0.25f).ToList()) game.Tap(it.X, it.Y);
                yield return null;
            }
            Assert.That(game.Status, Is.EqualTo(GameStatus.Won));
            for (int i = 0; i < 10 && task.Status == Cysharp.Threading.Tasks.UniTaskStatus.Pending; i++) yield return null;
            Assert.That(task.Status, Is.EqualTo(Cysharp.Threading.Tasks.UniTaskStatus.Succeeded));
            host.Despawn(view);
        }

        [UnityTest, Timeout(300000), Description("Vérifie que l'arène se joue sur le vrai écran 3D : choix d'un agent de l'escouade, puis un joueur qui fonce sur les ennemis en attaquant vainc la vague — parce que l'arène est un combat jouable")]
        public IEnumerator Arena_PlayedOnTheRealStage_ClearsTheWave()
        {
            yield return OnBoardNext(PrototypeContent.Zone11, "n15");
            var plan = PlanOf("arena_training", new ZoneEnvironment(Biome.City, Weather.Day), ("waves", 1), ("enemies_per_wave", 2));
            var request = CasePresenter().StageRequestFor(plan);
            ArenaGame? game = null;
            var inner = request.ArenaFactory!;
            request.ArenaFactory = i => game = inner(i);
            ActivityResult? result = null;
            StageGameView? view = null;
            var run = OpenStage(request, v =>
            {
                view = v;
                Assert.That(v.AgentButtons, Is.EqualTo(request.Agents.Count));
                v.AgentButtonAt(0).onClick.Invoke();
                v.InputOverride = () =>
                {
                    var g = game;
                    if (g == null) return default;
                    var near = g.Enemies.Where(e => e.Alive).OrderBy(e => Vec2f.Distance(e.Position, g.Position)).FirstOrDefault();
                    var d = near == null ? Vec2f.Zero : (near.Position - g.Position).Normalized;
                    return new StageInput { Move = new Vector2(d.X, d.Y), A = true, B = true, BPressed = true };
                };
            }, r => result = r);
            while (result == null && run.MoveNext()) yield return run.Current;
            Assert.That(game, Is.Not.Null);
            Assert.That(game!.Status, Is.EqualTo(GameStatus.Won));
            Assert.That(result!.Value.Success, Is.True);
            Assert.That(result.Value.Score, Is.EqualTo(2));
        }
    }
}
