#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Tests.Domain.Application;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Flow
{
    public sealed class GameFlowMachineTests
    {
        List<string> _calls = null!;
        FakeSceneLoader _scenes = null!;
        IServiceProvider _bus = null!;
        GameFlowMachine _flow = null!;
        readonly List<GameStateChanged> _changed = new List<GameStateChanged>();
        readonly List<ReturnedToBoard> _returned = new List<ReturnedToBoard>();
        readonly List<ToastRequested> _toasts = new List<ToastRequested>();

        [SetUp]
        public void SetUp()
        {
            _calls = new List<string>();
            _scenes = new FakeSceneLoader(_calls);
            var b = new BuiltinContainerBuilder();
            b.AddMessagePipe();
            b.AddMessageBroker<NavigateRequested>();
            b.AddMessageBroker<CombatResultsClosed>();
            b.AddMessageBroker<GameStateChanged>();
            b.AddMessageBroker<ReturnedToBoard>();
            b.AddMessageBroker<ToastRequested>();
            _bus = b.BuildServiceProvider();
            _changed.Clear(); _returned.Clear(); _toasts.Clear();
            Sub<GameStateChanged>().Subscribe(m => _changed.Add(m));
            Sub<ReturnedToBoard>().Subscribe(m => { _calls.Add("msg:returned"); _returned.Add(m); });
            Sub<ToastRequested>().Subscribe(m => _toasts.Add(m));
            _flow = new GameFlowMachine(_scenes, new FakeScreens(_calls), Sub<NavigateRequested>(), Sub<CombatResultsClosed>(),
                                        Pub<GameStateChanged>(), Pub<ReturnedToBoard>(), Pub<ToastRequested>(), new TestApp.Log());
        }

        [TearDown]
        public void TearDown() => _flow.Dispose();

        ISubscriber<T> Sub<T>() => _bus.GetRequiredService<ISubscriber<T>>();
        IPublisher<T> Pub<T>() => _bus.GetRequiredService<IPublisher<T>>();

        bool Go(GameState s) => _flow.RequestAsync(s).GetAwaiter().GetResult();

        /// <summary>Amène la machine dans l'état voulu par le chemin normal, puis vide le journal.</summary>
        void Reach(GameState s)
        {
            _flow.Start();
            if (s != GameState.Login) Go(GameState.Hub);
            if (s == GameState.Board || s == GameState.Combat) Go(GameState.Board);
            if (s == GameState.Combat) Go(GameState.Combat);
            Assert.That(_flow.Current, Is.EqualTo(s));
            _calls.Clear(); _changed.Clear();
        }

        static CombatReport Report(bool victory, bool toHq)
            => new CombatReport(victory, victory ? 200 : 0, CombatRank.Easy, false, Array.Empty<FatigueChange>(), Array.Empty<BoardEvent>(), toHq);

        [Test, Description("Vérifie qu'au démarrage la machine affiche l'écran de connexion — parce que le jeu commence toujours par le titre (screen flow F01, W01)")]
        public void Start_ShowsLogin()
        {
            _flow.Start();
            Assert.That(_calls, Is.EqualTo(new[] { "login:on" }));
            Assert.That(_flow.Current, Is.EqualTo(GameState.Login));
        }

        [Test, Description("Vérifie que la connexion masque le titre, charge la scène du QG et annonce le changement d'état — parce que Login → Hub est la transition d'entrée du jeu (spec §6.1)")]
        public void LoginToHub_HidesLogin_LoadsHub_PublishesStateChanged()
        {
            Reach(GameState.Login);
            Assert.That(Go(GameState.Hub), Is.True);
            Assert.That(_calls, Is.EqualTo(new[] { "login:off", "load:Hub" }));
            Assert.That(_changed.Single().From, Is.EqualTo(GameState.Login));
            Assert.That(_changed.Single().To, Is.EqualTo(GameState.Hub));
        }

        [Test, Description("Vérifie que la déconnexion décharge le QG puis réaffiche le titre — parce que Hub → Login doit libérer la scène du QG (spec §6.1)")]
        public void HubToLogin_UnloadsHub_ShowsLogin()
        {
            Reach(GameState.Hub);
            Assert.That(Go(GameState.Login), Is.True);
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Hub", "login:on" }));
        }

        [Test, Description("Vérifie que le déploiement décharge le QG puis charge le plateau — parce que Hub → Board remplace une scène par l'autre (spec §6.1)")]
        public void HubToBoard_UnloadsHub_LoadsBoard()
        {
            Reach(GameState.Hub);
            Assert.That(Go(GameState.Board), Is.True);
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Hub", "load:Board" }));
        }

        [Test, Description("Vérifie que le retour au QG décharge le plateau puis charge le QG — parce que « Retour QG » referme la zone (spec §6.1)")]
        public void BoardToHub_UnloadsBoard_LoadsHub()
        {
            Reach(GameState.Board);
            Assert.That(Go(GameState.Hub), Is.True);
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Board", "load:Hub" }));
        }

        [Test, Description("Vérifie que le combat se charge par-dessus le plateau, qui est ensuite masqué sans être déchargé — parce que le plateau reste en mémoire et en pause pendant le combat (doc 07)")]
        public void BoardToCombat_LoadsCombat_ThenHidesBoard()
        {
            Reach(GameState.Board);
            Assert.That(Go(GameState.Combat), Is.True);
            Assert.That(_calls, Is.EqualTo(new[] { "load:Combat", "hide:Board" }));
        }

        [Test, Description("Vérifie qu'après une victoire, fermer les résultats décharge le combat, réaffiche le plateau puis lui transmet le rapport — parce que le plateau doit appliquer le rapport (cases, objectifs) une fois visible")]
        public void CombatResultsClosed_Victory_UnloadsCombat_ShowsBoard_ThenPublishesReturnedToBoard()
        {
            Reach(GameState.Combat);
            var report = Report(victory: true, toHq: false);
            Pub<CombatResultsClosed>().Publish(new CombatResultsClosed(report));
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Combat", "show:Board", "msg:returned" }));
            Assert.That(_returned.Single().Report, Is.SameAs(report));
            Assert.That(_flow.Current, Is.EqualTo(GameState.Board));
        }

        [Test, Description("Vérifie qu'une défaite qui renvoie au QG décharge le combat puis le plateau et charge le QG — parce que l'escouade épuisée rentre au QG (GDD §6)")]
        public void CombatResultsClosed_ReturnedToHq_UnloadsCombatThenBoard_LoadsHub()
        {
            Reach(GameState.Combat);
            Pub<CombatResultsClosed>().Publish(new CombatResultsClosed(Report(victory: false, toHq: true)));
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Combat", "unload:Board", "load:Hub" }));
            Assert.That(_returned, Is.Empty);
            Assert.That(_flow.Current, Is.EqualTo(GameState.Hub));
        }

        [Test, Description("Vérifie qu'une transition non prévue (QG → Combat) est refusée sans rien charger — parce qu'un combat ne peut venir que du plateau (spec §6.1)")]
        public void HubToCombat_IsRefused_StateUnchanged()
        {
            Reach(GameState.Hub);
            Assert.That(Go(GameState.Combat), Is.False);
            Assert.That(_calls, Is.Empty);
            Assert.That(_flow.Current, Is.EqualTo(GameState.Hub));
        }

        [Test, Description("Vérifie qu'une demande reçue pendant une transition est ignorée et qu'aucune scène n'est chargée deux fois — parce qu'un double clic sur Déployer ne doit pas dupliquer le plateau (Review Focus 2)")]
        public void RequestWhileTransitioning_IsIgnored_SingleLoad()
        {
            Reach(GameState.Hub);
            var gate = new UniTaskCompletionSource();
            _scenes.Gate = gate;
            var first = _flow.RequestAsync(GameState.Board);
            Assert.That(_flow.IsTransitioning, Is.True);
            Assert.That(Go(GameState.Board), Is.False);
            gate.TrySetResult();
            Assert.That(first.GetAwaiter().GetResult(), Is.True);
            Assert.That(_calls.Count(c => c == "load:Board"), Is.EqualTo(1));
            Assert.That(_flow.IsTransitioning, Is.False);
        }

        [Test, Description("Vérifie qu'une demande vers l'état courant est ignorée sans effet — parce qu'un message de navigation répété ne doit rien recharger")]
        public void RequestToCurrentState_IsIgnored()
        {
            Reach(GameState.Hub);
            Assert.That(Go(GameState.Hub), Is.False);
            Assert.That(_calls, Is.Empty);
            Assert.That(_changed, Is.Empty);
        }

        [Test, Description("Vérifie qu'un échec de chargement annule ce qui a été fait, garde l'état de départ et prévient le joueur — parce qu'on ne doit jamais rester bloqué entre deux scènes (spec §8)")]
        public void LoadFailure_RollsBack_StaysInState_ToastsError()
        {
            Reach(GameState.Hub);
            _scenes.FailOnLoad = SceneNames.Board;
            Assert.That(Go(GameState.Board), Is.False);
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Hub", "load:Board", "load:Hub" }));
            Assert.That(_flow.Current, Is.EqualTo(GameState.Hub));
            Assert.That(_toasts.Single().Text, Is.EqualTo("errors.navigation"));
            Assert.That(_toasts.Single().Kind, Is.EqualTo(ToastKind.Error));
            Assert.That(_changed, Is.Empty);
        }

        [Test, Description("Vérifie qu'un message NavigateRequested déclenche la transition — parce que les écrans naviguent uniquement par message, sans se référencer entre eux")]
        public void NavigateRequestedMessage_TriggersTransition()
        {
            Reach(GameState.Hub);
            Pub<NavigateRequested>().Publish(new NavigateRequested(GameState.Board));
            Assert.That(_flow.Current, Is.EqualTo(GameState.Board));
            Assert.That(_calls, Is.EqualTo(new[] { "unload:Hub", "load:Board" }));
        }
    }
}
