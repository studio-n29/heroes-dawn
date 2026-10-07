using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Board;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class BoardPresenterTests
    {
        sealed class PendingRoll : IRollDiceUseCase
        {
            public int Calls;
            public readonly UniTaskCompletionSource<Result<IReadOnlyList<BoardEvent>>> Done = new UniTaskCompletionSource<Result<IReadOnlyList<BoardEvent>>>();
            public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(CancellationToken ct = default) { Calls++; return Done.Task; }
        }

        TestApp _app = null!;
        FakeBoardView _view = null!;
        FakeModalHost _host = null!;
        ModalService _modals = null!;
        BackButtonRouter _router = null!;
        FakeToasts _toasts = null!;
        BoardPresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeBoardView();
            _host = new FakeModalHost();
            _host.Register<IRestPopupView>(() => new FakeRestPopup());
            _host.Register<IZoneCompletePopupView>(() => new FakeZoneCompletePopup());
            _modals = new ModalService(_host);
            _router = new BackButtonRouter(new FakeBackInput(), _modals);
            _toasts = new FakeToasts();
        }

        /// <summary>Partie de test : pion en n02 avec 1 PM (destinations n01, n03 = combat Facile, n06).</summary>
        void ChoosingAtN02()
        {
            var zone = _app.Session.Content.Zone(PrototypeContent.Zone11);
            _app.Session.Run = ZoneRun.ForTests(_app.Session.Content, zone, 3UL, 1, true, "n02");
            _app.Session.RunZone = zone;
        }

        void Make(IRollDiceUseCase roll = null)
        {
            var loc = new LocLocalization();
            var cases = new CasePopupPresenter(_modals, _app.Zone.Rest, _app.Zone.Narrative, _app.Zone.PlanActivity, _app.Zone.Activity, _app.Session, _toasts, loc);
            _p = new BoardPresenter(_view, _app.Session, roll ?? _app.Zone.Roll, _app.Zone.Bonus, _app.Zone.Move, _app.Zone.End,
                                    _app.Combat.Begin, _app.Zone.Leave, new FakeGameFlow(), cases,
                                    _app.Subscriber<ZoneValidated>(), _app.Subscriber<ReturnedToBoard>(), _app.Subscriber<RosterChanged>(),
                                    _app.Subscriber<WalletChanged>(), _app.Subscriber<SaveStateChanged>(), _app.Publisher<NavigateRequested>(),
                                    _modals, _toasts, loc, _router);
            _view.Show();
            _p.Start();
        }

        static CombatReport Report(params BoardEvent[] events)
            => new CombatReport(true, 0, CombatRank.Easy, false, Array.Empty<FatigueChange>(), events, false);

        IEnumerable<GameState> Navigations => _app.Received<NavigateRequested>().Select(n => n.Target);

        [Test, Description("Vérifie qu'un plateau affiché sans partie en cours renvoie au QG — parce qu'il n'y a rien à jouer (reprise de l'ancien écran)")]
        public void Shown_NoRun_NavigatesToHub()
        {
            _app.Session.Run = null;
            Make();
            Assert.That(Navigations, Is.EqualTo(new[] { GameState.Hub }));
        }

        [Test, Description("Vérifie que Lancer lance le dé une fois, anime la face obtenue et passe au choix de destination — parce que c'est le premier geste de chaque tour (GDD, plateau)")]
        public void Roll_RollsOnceAndAnimatesDie()
        {
            _app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            Make();
            _view.ClickRoll();
            Assert.That(_view.DieRolls.Count, Is.EqualTo(1));
            Assert.That(_view.DieRolls[0], Is.InRange(1, 6));
            Assert.That(_app.Session.Run!.Phase, Is.EqualTo(ZoneRunPhase.ChoosingDestination));
        }

        [Test, Description("Vérifie qu'un second Lancer pendant le premier est ignoré — parce qu'un double appui (clic, Espace) ne doit pas lancer deux dés (spec §6)")]
        public void Roll_WhileBusy_Ignored()
        {
            _app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            var roll = new PendingRoll();
            Make(roll);
            _view.ClickRoll();
            _view.ClickRoll();
            Assert.That(roll.Calls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que Lancer est ignoré quand une modale est ouverte — parce qu'Espace derrière une confirmation ne doit pas jouer (Review Focus 5)")]
        public void Roll_WhileModalOpen_Ignored()
        {
            _app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            var roll = new PendingRoll();
            Make(roll);
            _modals.OpenAsync<FakePopup>(CancellationToken.None);
            _view.ClickRoll();
            Assert.That(roll.Calls, Is.Zero);
        }

        [Test, Description("Vérifie qu'un toucher sur une case hors de portée ne déplace pas le pion — parce que seules les destinations calculées par le domaine sont jouables")]
        public void TapNode_NotDestination_Ignored()
        {
            ChoosingAtN02();
            Make();
            _view.MapView.Tap("n10");
            Assert.That(_view.MapView.Moves, Is.Empty);
            Assert.That(_app.Session.Run!.Position, Is.EqualTo(new CaseId("n02")));
        }

        [Test, Description("Vérifie qu'un toucher sur une destination déplace le pion le long du chemin et rafraîchit la carte — parce que la vue ne calcule jamais un chemin (doc 07)")]
        public void TapNode_Destination_MovesAndRefreshes()
        {
            ChoosingAtN02();
            Make();
            int refreshes = _view.MapView.Refreshes;
            _view.MapView.Tap("n06");
            Assert.That(_view.MapView.Moves.Single().Last(), Is.EqualTo(new CaseId("n06")));
            Assert.That(_view.MapView.Refreshes, Is.GreaterThan(refreshes));
        }

        [Test, Description("Vérifie qu'arriver sur une case Combat engage le combat puis demande la scène de combat — parce que le combat se joue dans sa scène, par-dessus le plateau (doc 07)")]
        public void CombatCase_BeginsCombatAndNavigatesToCombat()
        {
            ChoosingAtN02();
            Make();
            _view.MapView.Tap("n03");
            Assert.That(_app.Combat.Active.Simulation, Is.Not.Null);
            Assert.That(Navigations, Is.EqualTo(new[] { GameState.Combat }));
        }

        [Test, Description("Vérifie qu'une case Repos déclenchée ouvre la popup de repos via le présentateur des cases — parce que les popups de case sont des prefabs pilotés par CasePopupPresenter (spec §3)")]
        public void RestCase_OpensRestPopupThroughCasePresenter()
        {
            ChoosingAtN02();
            Make();
            var rest = new CaseOutcome(new CaseId("n10"), CaseType.Rest, CombatRank.Easy, null);
            _app.Publisher<ReturnedToBoard>().Publish(new ReturnedToBoard(Report(new CaseTriggeredEvent(rest))));
            Assert.That(_host.Spawned.OfType<IRestPopupView>().Count(), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que Retour sur le plateau demande confirmation, puis quitte la zone et demande le QG — parce que « Retour QG » referme le brouillard (maquettes §1, spec §7)")]
        public void Back_AsksConfirmation_ThenLeavesZoneAndNavigatesHub()
        {
            ChoosingAtN02();
            Make();
            Assert.That(_router.Dispatch(), Is.True);
            var confirm = _host.Spawned.OfType<FakeConfirmPopup>().Single();
            confirm.Answer(true);
            Assert.That(_app.Session.Run, Is.Null);
            Assert.That(Navigations, Is.EqualTo(new[] { GameState.Hub }));
        }

        [Test, Description("Vérifie que Retour pendant le démarrage d'un combat (plateau encore affiché) n'ouvre pas « Retour au QG ? » — parce que confirmer abandonnerait un combat engagé sans retraite ni fatigue (relecture finale, Important 1)")]
        public void Back_WhileCombatStarting_NoHqConfirm()
        {
            ChoosingAtN02();
            Make();
            _view.MapView.Tap("n03");
            Assume.That(_app.Session.Run!.Phase, Is.EqualTo(ZoneRunPhase.InCombat));
            _router.Dispatch();
            _view.ClickBack();
            Assert.That(_host.Spawned.OfType<FakeConfirmPopup>(), Is.Empty);
            Assert.That(_app.Session.Run, Is.Not.Null);
        }

        [Test, Description("Vérifie qu'un plateau masqué (combat par-dessus) ne répond plus à Retour — parce qu'Échap en combat ne doit jamais proposer « Retour au QG » (Review Focus 2)")]
        public void Back_WhenHidden_NotHandled()
        {
            ChoosingAtN02();
            Make();
            _view.Hide();
            Assert.That(_router.Dispatch(), Is.False);
            Assert.That(_host.Spawned, Is.Empty);
        }

        [Test, Description("Vérifie que le rapport de combat est appliqué même si le plateau était masqué à sa publication — parce que le retour du combat arrive pendant ou juste après le masquage (Review Focus 2)")]
        public void ReturnedToBoard_WhileHidden_StillApplied()
        {
            ChoosingAtN02();
            Make();
            _view.Hide();
            _app.Publisher<ReturnedToBoard>().Publish(new ReturnedToBoard(Report(new SteppedBackEvent(new CaseId("n01")))));
            Assert.That(_p.ReportsApplied, Is.EqualTo(1));
            Assert.That(_view.MapView.Moves.Single(), Is.EqualTo(new[] { new CaseId("n01") }));
        }

        [Test, Description("Vérifie qu'une zone validée affiche les récompenses puis ramène au QG — parce que la validation termine la partie (GDD §5)")]
        public void ZoneValidated_ShowsZoneCompleteThenLeaves()
        {
            ChoosingAtN02();
            Make();
            _app.Publisher<ZoneValidated>().Publish(new ZoneValidated(new ZoneRewards(1500, 40, true)));
            _app.Publisher<ReturnedToBoard>().Publish(new ReturnedToBoard(Report(new ZoneValidatedEvent())));
            Assert.That(_host.Spawned.OfType<FakeZoneCompletePopup>().Single().Data!.Value.Rewards, Does.Contain("1"));
            Assert.That(_p.LastZoneRewards!.Pr, Is.EqualTo(40));
            Assert.That(Navigations, Is.EqualTo(new[] { GameState.Hub }));
        }
    }
}
