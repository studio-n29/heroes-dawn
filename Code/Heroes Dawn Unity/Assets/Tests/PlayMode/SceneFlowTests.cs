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
using N29.HeroesDawn.Presentation.Login;
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
            PlayModeHarness.Visible<LoginView>();
            UnityEngine.Object.FindAnyObjectByType<LoginView>()!.PlayButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<HubView>() && !Flow.IsTransitioning);
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
            Assert.That(PlayModeHarness.Visible<LoginView>(), Is.False);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub));
        }

        [UnityTest, Description("Vérifie que « Déployer » décharge la scène du QG et charge celle du plateau — parce que le QG et le plateau ne sont jamais chargés ensemble (spec §6.1)")]
        public IEnumerator Deploy_UnloadsHub_LoadsBoard()
        {
            yield return SignedIn();
            UnityEngine.Object.FindAnyObjectByType<HubView>()!.DeployButton.onClick.Invoke();
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
            Assert.That(UnityEngine.Object.FindAnyObjectByType<BoardScreen>()!.ReportsApplied, Is.EqualTo(1), "le plateau a reçu le rapport du combat");
        }

        IEnumerator InEasyCombat()
        {
            yield return OnBoardBeforeEasyCombat();
            UnityEngine.Object.FindAnyObjectByType<BoardScreen>()!.TapNode(new CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Combat) && Flow.Current == GameState.Combat && !Flow.IsTransitioning, 600);
        }

        [UnityTest, Description("Vérifie que « Retour QG » depuis le plateau décharge le plateau et recharge le QG — parce que quitter la zone referme le brouillard et ramène au hub (screen flow F01)")]
        public IEnumerator LeaveZone_UnloadsBoard_LoadsHub()
        {
            yield return OnBoardBeforeEasyCombat();
            UnityEngine.Object.FindAnyObjectByType<BoardScreen>()!.LeaveToHq();
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Hub) && !Flow.IsTransitioning);
            Assert.That(Loaded(SceneNames.Board), Is.False);
            Assert.That(Loaded(SceneNames.Hub), Is.True);
            Assert.That(Container.Resolve<GameSession>().Run, Is.Null);
        }

        [UnityTest, Description("Vérifie que deux demandes de plateau rapprochées ne chargent qu'une seule scène Board — parce qu'un double clic sur Déployer ne doit pas dupliquer le plateau (Review Focus 2)")]
        public IEnumerator DoubleDeploy_SingleBoardScene()
        {
            yield return SignedIn();
            var nav = Container.Resolve<IPublisher<NavigateRequested>>();
            Container.Resolve<Application.Features.Zone.IStartZoneUseCase>().ExecuteAsync(PrototypeContent.Zone11).Forget();
            nav.Publish(new NavigateRequested(GameState.Board));
            nav.Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Board) && !Flow.IsTransitioning);
            int boards = 0;
            for (int i = 0; i < SceneManager.sceneCount; i++) if (SceneManager.GetSceneAt(i).name == SceneNames.Board) boards++;
            Assert.That(boards, Is.EqualTo(1));
        }

        [UnityTest, Description("Vérifie qu'un double clic sur le bouton « Déployer » du QG ne démarre la zone qu'une fois — parce qu'un second démarrage remplacerait la partie (nouvelle graine) pendant que le plateau se charge (relecture, Important 2)")]
        public IEnumerator DoubleDeployClick_StartsZoneOnce()
        {
            yield return SignedIn();
            var hub = UnityEngine.Object.FindAnyObjectByType<HubView>()!;
            var session = Container.Resolve<GameSession>();
            hub.DeployButton.onClick.Invoke();
            var firstRun = session.Run;
            hub.DeployButton.onClick.Invoke();
            Assert.That(firstRun, Is.Not.Null);
            Assert.That(session.Run, Is.SameAs(firstRun), "la partie n'est pas remplacée");
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Board) && !Flow.IsTransitioning);
            Assert.That(session.Run, Is.SameAs(firstRun));
        }

        [UnityTest, Description("Vérifie qu'une validation de zone annoncée pendant que le plateau est masqué (combat en cours) est conservée pour l'écran de fin de zone — parce que le plateau masqué ne doit perdre aucun abonnement à vie (Review Focus 1)")]
        public IEnumerator ZoneValidatedWhileBoardHidden_IsKeptForZoneComplete()
        {
            yield return InEasyCombat();
            Assert.That(PlayModeHarness.Visible<BoardScreen>(), Is.False);
            var rewards = new Application.Features.Zone.ZoneRewards(1500, 40, true);
            Container.Resolve<IPublisher<ZoneValidated>>().Publish(new ZoneValidated(rewards));
            var board = UnityEngine.Object.FindAnyObjectByType<BoardScreen>(UnityEngine.FindObjectsInactive.Include)!;
            Assert.That(board.LastZoneRewards, Is.SameAs(rewards));
        }

        [UnityTest, Description("Vérifie qu'après le déchargement du QG, un changement de portefeuille ne provoque aucune erreur — parce que les abonnements d'une scène déchargée doivent disparaître avec elle (Review Focus 3)")]
        public IEnumerator HubUnloaded_WalletChanged_NoErrorNoDestroyedRefresh()
        {
            yield return SignedIn();
            var hub = UnityEngine.Object.FindAnyObjectByType<HubView>()!;
            hub.DeployButton.onClick.Invoke();
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Board) && !Flow.IsTransitioning);
            Assert.That(hub == null, Is.True, "l'écran du QG est détruit avec sa scène");
            Container.Resolve<IPublisher<WalletChanged>>().Publish(new WalletChanged(1, 2));
            yield return null;
            // Une erreur ou exception journalisée (ex. MissingReferenceException sur l'écran détruit) fait échouer le test d'office.
        }

        [UnityTest, Description("Vérifie qu'une défaite qui épuise l'escouade ramène au QG en déchargeant combat et plateau — parce que l'équipe entière à 100 % de fatigue rentre au QG (GDD §6)")]
        public IEnumerator DefeatWithExhaustedSquad_ReturnsToHub_UnloadsBoardAndCombat()
        {
            yield return SignedIn();
            var session = Container.Resolve<GameSession>();
            var b = session.Content.Balance;
            foreach (var m in session.Roster) m.Fatigue = 100 - b.FatiguePerWin - b.FatiguePerLossExtra;
            var zone = session.Content.Zone(PrototypeContent.Zone11);
            session.Run = ZoneRun.ForTests(session.Content, zone, 3UL, 1, true, "n02");
            session.RunZone = zone;
            Container.Resolve<IPublisher<NavigateRequested>>().Publish(new NavigateRequested(GameState.Board));
            yield return PlayModeHarness.WaitFor(() => PlayModeHarness.Visible<BoardScreen>() && !Flow.IsTransitioning);
            UnityEngine.Object.FindAnyObjectByType<BoardScreen>()!.TapNode(new CaseId("n03"));
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Combat) && Flow.Current == GameState.Combat && !Flow.IsTransitioning, 600);

            Container.Resolve<IActiveCombatReadModel>().Simulation!.Submit(Domain.Combat.CombatCommand.Retreat());
            var combat = UnityEngine.Object.FindAnyObjectByType<CombatScreen>()!;
            yield return PlayModeHarness.WaitFor(() => combat.ContinueForTests(), 600);
            yield return PlayModeHarness.WaitFor(() => Loaded(SceneNames.Hub) && !Flow.IsTransitioning, 600);

            Assert.That(Loaded(SceneNames.Hub), Is.True);
            Assert.That(Loaded(SceneNames.Board), Is.False);
            Assert.That(Loaded(SceneNames.Combat), Is.False);
            Assert.That(Flow.Current, Is.EqualTo(GameState.Hub));
        }
    }
}
