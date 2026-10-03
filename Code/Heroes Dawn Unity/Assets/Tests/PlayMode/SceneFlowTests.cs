#nullable enable
using System.Collections;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Composition;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Screens;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using VContainer;

namespace N29.HeroesDawn.Tests.PlayMode
{
    public sealed class SceneFlowTests
    {
        MemoryProfileGateway _profiles = null!;
        InstantAuthGateway _auth = null!;

        [SetUp]
        public void SetUp()
        {
            _profiles = new MemoryProfileGateway();
            _auth = new InstantAuthGateway();
        }

        [UnityTearDown]
        public IEnumerator TearDown() => PlayModeHarness.TearDown();

        static IObjectResolver Container => GameBootstrap.Instance!.Scope.Container;
        static bool Loaded(string scene) => SceneManager.GetSceneByName(scene).isLoaded;
        static IGameFlow Flow => Container.Resolve<IGameFlow>();

        IEnumerator SignedIn()
        {
            yield return PlayModeHarness.Boot(_profiles, _auth);
            PlayModeHarness.Visible<LoginScreen>();
            UnityEngine.Object.FindAnyObjectByType<LoginScreen>()!.Play();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<HubScreen>() && !Flow.IsTransitioning);
        }

        /// <summary>Depuis le QG : pion en n02 avec 1 PM (n03 = combat Facile), puis plateau affiché.</summary>
        IEnumerator OnBoardBeforeEasyCombat()
        {
            yield return SignedIn();
            var session = Container.Resolve<GameSession>();
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardScreen>() && !Flow.IsTransitioning);
        }

        [UnityTest, Description("Vérifie que la connexion charge la scène du QG et masque l'écran de titre — parce que chaque écran vit désormais dans sa scène, chargée par la machine d'états (spec §6.1)")]
        public IEnumerator SignIn_LoadsHubScene_HidesLogin()
        {
            yield return SignedIn();
            Assert.That(Loaded(SceneNames.Hub), Is.True);
            Assert.That(PlayModeHarness.Visible<LoginScreen>(), Is.False);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub));
        }

        [UnityTest, Description("Vérifie que « Déployer » décharge la scène du QG et charge celle du plateau — parce que le QG et le plateau ne sont jamais chargés ensemble (spec §6.1)")]
        public IEnumerator Deploy_UnloadsHub_LoadsBoard()
        {
            yield return SignedIn();
            UnityEngine.Object.FindAnyObjectByType<HubScreen>()!.Deploy();
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Board) && !Flow.IsTransitioning);
            Assert.That(Loaded(SceneNames.Board), Is.True);
            Assert.That(Loaded(SceneNames.Hub), Is.False);
            Assert.That(PlayModeHarness.Visible<BoardScreen>(), Is.True);
        }

        [UnityTest, Description("Vérifie le cycle plateau → combat → plateau : le combat se charge par-dessus le plateau masqué, puis se décharge et le plateau réapparaît avec le rapport appliqué — parce que c'est le cœur de la boucle de jeu (doc 07)")]
        public IEnumerator BoardCombatBoard_CombatSceneLoadedOverBoard_ThenUnloaded_BoardShownWithReport()
        {
            yield return OnBoardBeforeEasyCombat();
            UnityEngine.Object.FindAnyObjectByType<BoardScreen>()!.TapNode(new CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Combat) && Flow.Current == GameState.Combat && !Flow.IsTransitioning, 600);
            Assert.That(Loaded(SceneNames.Combat), Is.True);
            Assert.That(Loaded(SceneNames.Board), Is.True, "le plateau reste chargé pendant le combat");
            Assert.That(PlayModeHarness.Visible<BoardScreen>(), Is.False, "le plateau est masqué");

            Container.Resolve<IForceCombatWinUseCase>().ExecuteAsync().Forget();
            var combat = UnityEngine.Object.FindAnyObjectByType<CombatScreen>()!;
            yield return PlayModeHarness.WaitFor(() => combat.ContinueForTests(), 600);
            yield return PlayModeHarness.WaitFor(() => !Loaded(SceneNames.Combat) && !Flow.IsTransitioning, 600);

            Assert.That(Loaded(SceneNames.Combat), Is.False);
            Assert.That(PlayModeHarness.Visible<BoardScreen>(), Is.True);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Board));
            Assert.That(Container.Resolve<GameSession>().Run!.IsSpent(new CaseId("n03")), Is.True, "victoire appliquée au plateau");
        }
    }
}
