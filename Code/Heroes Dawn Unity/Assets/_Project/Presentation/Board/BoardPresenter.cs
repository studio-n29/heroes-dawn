#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>
    /// Plateau : traduit la partie en données de carte et de HUD, exécute les gestes (lancer, dé bonus, case, fin),
    /// joue les événements du domaine dans l'ordre (déplacement animé, messages, case déclenchée), ouvre le combat,
    /// les popups de case et la zone sécurisée. Les abonnements « à vie » survivent au masquage pendant le combat.
    /// </summary>
    public sealed class BoardPresenter : PresenterBase<IBoardView>, IBackHandler
    {
        readonly ISessionReadModel _session;
        readonly IRollDiceUseCase _roll;
        readonly IUseBonusDieUseCase _bonus;
        readonly TutorialGuide? _tutorial;
        readonly IMoveToUseCase _moveTo;
        readonly IEndMoveUseCase _endMove;
        readonly IBeginCombatUseCase _beginCombat;
        readonly ILeaveZoneUseCase _leave;
        readonly IGameFlow _flow;
        readonly CasePopupPresenter _cases;
        readonly ISubscriber<ZoneValidated> _zoneValidated;
        readonly ISubscriber<ReturnedToBoard> _returned;
        readonly ISubscriber<RosterChanged> _roster;
        readonly ISubscriber<WalletChanged> _wallet;
        readonly ISubscriber<SaveStateChanged> _saving;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly IAudioService _audio;

        readonly BusyGate _busy = new BusyGate();
        ZoneRun? _run;
        ZoneRun? _built;
        IDisposable? _lifetimeSubs;
        IDisposable? _rosterSub;
        WalletBinder? _walletBinder;

        /// <summary>Diagnostic (tests PlayMode) : rapports de combat appliqués, dernières récompenses de validation reçues.</summary>
        public int ReportsApplied { get; private set; }
        public ZoneRewards? LastZoneRewards { get; private set; }

        public BoardPresenter(IBoardView view, ISessionReadModel session, IRollDiceUseCase roll, IUseBonusDieUseCase bonus, IMoveToUseCase moveTo,
                              IEndMoveUseCase endMove, IBeginCombatUseCase beginCombat, ILeaveZoneUseCase leave, IGameFlow flow, CasePopupPresenter cases,
                              ISubscriber<ZoneValidated> zoneValidated, ISubscriber<ReturnedToBoard> returned, ISubscriber<RosterChanged> roster,
                              ISubscriber<WalletChanged> wallet, ISubscriber<SaveStateChanged> saving, IPublisher<NavigateRequested> navigate,
                              IModalService modals, IToastService toasts, ILocalization loc, BackButtonRouter router,
                              IAudioService? audio = null, TutorialGuide? tutorial = null)
            : base(view, router)
        {
            _session = session; _roll = roll; _bonus = bonus; _moveTo = moveTo; _endMove = endMove; _beginCombat = beginCombat;
            _leave = leave; _flow = flow; _cases = cases; _zoneValidated = zoneValidated; _returned = returned; _roster = roster;
            _wallet = wallet; _saving = saving; _navigate = navigate; _modals = modals; _toasts = toasts; _loc = loc;
            _audio = audio ?? NullAudioService.Instance;
            _tutorial = tutorial;
        }

        protected override IBackHandler BackHandler => this;

        // ------------------------------------------------------------------ Cycle de vie

        protected override void OnStart()
        {
            // À vie (pas au masquage) : la validation de zone comme le retour du combat arrivent pendant ou juste après le masquage.
            var bag = DisposableBag.CreateBuilder();
            _zoneValidated.Subscribe(m => LastZoneRewards = m.Rewards).AddTo(bag);
            _returned.Subscribe(OnReturnedToBoard).AddTo(bag);
            _lifetimeSubs = bag.Build();
            _walletBinder = new WalletBinder(View.Wallet, _session, _wallet, _saving);
            View.RollClicked += Roll;
            View.BonusClicked += Bonus;
            View.EndMoveClicked += EndMove;
            View.BackClicked += RequestBackToHq;
            View.Map.NodeTapped += TapNode;
        }

        protected override void OnDispose()
        {
            _lifetimeSubs?.Dispose();
            _rosterSub?.Dispose();
            _walletBinder?.Dispose();
            View.RollClicked -= Roll;
            View.BonusClicked -= Bonus;
            View.EndMoveClicked -= EndMove;
            View.BackClicked -= RequestBackToHq;
            View.Map.NodeTapped -= TapNode;
        }

        protected override void OnShown()
        {
            var run = _session.Run;
            if (run == null)
            {
                _navigate.Publish(new NavigateRequested(GameState.Hub));
                return;
            }
            if (_run != run)
            {
                _run = run;
                _busy.Exit();
                View.ResetDie();
            }
            if (_built != run)
            {
                View.Map.Build(MapData(run));
                _built = run;
            }
            var zone = _session.RunZone;
            View.SetZoneTitle(zone != null ? _loc.T("zones.ny_1_1.code") + " · " + _loc.T(zone.NameKey) : "");
            _rosterSub?.Dispose();
            _rosterSub = _roster.Subscribe(_ => RefreshSquad());
            RefreshAll();
            // Reprise d'une case interactive laissée ouverte, ou d'un combat engagé.
            if (run.Phase == ZoneRunPhase.InCaseInteraction && run.PendingCase != null) OpenCaseAsync(run.PendingCase).Forget();
            else if (run.Phase == ZoneRunPhase.InCombat) StartCombatAfterAsync(0.3f).Forget();
        }

        protected override void OnHidden()
        {
            _rosterSub?.Dispose();
            _rosterSub = null;
        }

        // ------------------------------------------------------------------ Données

        BoardMapData MapData(ZoneRun run)
        {
            var squad = _session.Squad;
            var leader = squad.Count > 0 ? squad[0].Def : _session.Content.Members[0];
            var objectiveTargets = new HashSet<CaseId>(run.Graph.Zone.Objectives.Select(o => o.Target));
            return new BoardMapData
            {
                Nodes = run.Graph.AllIds.Select(id =>
                {
                    var def = run.Graph.Node(id);
                    return new BoardMapNode { Id = id, X = def.X, Y = def.Y, IsObjective = objectiveTargets.Contains(id) };
                }).ToArray(),
                Links = run.Graph.Zone.Links.Select(l => new BoardMapLink { A = l.A, B = l.B }).ToArray(),
                Pawn = new PortraitData(_loc.MemberName(leader), leader.Family),
                PawnAt = run.Position,
            };
        }

        void RefreshAll(bool movePawn = true)
        {
            var run = _run;
            if (run == null || Disposed) return;
            RefreshMap(run, movePawn);
            RefreshObjectives(run);
            RefreshSquad();
            RefreshHud();
        }

        void RefreshMap(ZoneRun run, bool movePawn)
        {
            var doneIds = new HashSet<string>(run.CompletedObjectives);
            var doneTargets = new HashSet<CaseId>(run.Graph.Zone.Objectives.Where(o => doneIds.Contains(o.Id)).Select(o => o.Target));
            var nodes = run.Graph.AllIds.Select(id =>
            {
                var def = run.Graph.Node(id);
                bool spent = run.IsSpent(id);
                bool dest = run.Phase == ZoneRunPhase.ChoosingDestination && run.Destinations.ContainsKey(id);
                return new BoardNodeState
                {
                    Id = id,
                    Revealed = run.Fog.IsRevealed(id),
                    Glyph = Glyph(def.Type, spent),
                    Sub = def.Type == CaseType.Combat && !spent ? _loc.RankName(def.Rank) : "",
                    SubElite = def.Rank == CombatRank.Elite,
                    Type = def.Type,
                    Rank = def.Rank,
                    Spent = spent,
                    ObjectiveDone = doneTargets.Contains(id),
                    DestinationCost = dest ? run.Destinations[id].Cost : -1,
                };
            }).ToArray();
            var links = run.Graph.Zone.Links.Select(l =>
            {
                bool a = run.Fog.IsRevealed(l.A), b = run.Fog.IsRevealed(l.B);
                return new BoardLinkState { Visible = a || b, Dim = !(a && b) };
            }).ToArray();
            View.Map.Refresh(nodes, links, run.Position, movePawn);
        }

        static string Glyph(CaseType t, bool spent)
        {
            switch (t)
            {
                case CaseType.Entry: return "QG";
                case CaseType.Combat: return spent ? "OK" : "VS";
                case CaseType.MiniGame: return spent ? "OK" : "MJ";
                case CaseType.Narrative: return spent ? "OK" : "!";
                case CaseType.Resource: return "$";
                case CaseType.Rest: return spent ? "OK" : "Zz";
                default: return "";
            }
        }

        void RefreshObjectives(ZoneRun run)
        {
            var done = new HashSet<string>(run.CompletedObjectives);
            View.ShowObjectives(run.Graph.Zone.Objectives.Select(o =>
            {
                bool ok = done.Contains(o.Id);
                return new ObjectiveLineData { Text = (ok ? "[X]  " : "[  ]  ") + _loc.T(o.NameKey), Done = ok };
            }).ToArray());
        }

        void RefreshSquad()
        {
            if (Disposed) return;
            View.ShowSquad(_session.Squad.Select(m =>
            {
                var name = _loc.MemberName(m.Def);
                return new SquadSlotData { Name = name, Portrait = new PortraitData(name, m.Def.Family), Fatigue01 = m.Fatigue / 100f, Fatigue = m.Fatigue, Exhausted = m.Exhausted };
            }).ToArray());
        }

        void RefreshHud()
        {
            var run = _run;
            if (run == null || Disposed) return;
            var b = _session.Content.Balance;
            bool awaiting = run.Phase == ZoneRunPhase.AwaitingRoll;
            bool choosing = run.Phase == ZoneRunPhase.ChoosingDestination;
            bool busy = _busy.IsBusy;
            View.ShowHud(new BoardHudData
            {
                Hint = run.Phase == ZoneRunPhase.Completed ? _loc.T("board.hint.done")
                     : choosing ? _loc.T("board.hint.choose")
                     : awaiting ? _loc.T("board.hint.roll") : "",
                PmText = run.PmInHand > 0 ? _loc.F("board.pm", run.PmInHand) : "",
                Reserve01 = run.Dice.BonusDice >= b.MaxBonusDice ? 1f : run.Dice.StoredPm / (float)b.PmPerBonusDie,
                ReserveLabel = _loc.F("board.reserve", run.Dice.StoredPm, b.PmPerBonusDie),
                RollEnabled = awaiting && !busy,
                BonusLabel = _loc.F("board.bonus", run.Dice.BonusDice),
                BonusEnabled = (awaiting || choosing) && run.Dice.BonusDice > 0 && !busy,
                EndEnabled = choosing && !busy,
            });
        }

        // ------------------------------------------------------------------ Gestes

        bool CanAct => _run != null && !_busy.IsBusy && !_modals.AnyOpen && !Disposed;

        void Roll()
        {
            if (!CanAct || _run!.Phase != ZoneRunPhase.AwaitingRoll) return;
            RunBoardAsync(ct => _roll.ExecuteAsync(ct), AnimateDieAsync).Forget();
        }

        void Bonus()
        {
            if (!CanAct || _run!.Dice.BonusDice == 0) return;
            RunBoardAsync(ct => _bonus.ExecuteAsync(ct), AnimateDieAsync).Forget();
        }

        void EndMove()
        {
            if (!CanAct || _run!.Phase != ZoneRunPhase.ChoosingDestination) return;
            RunBoardAsync(ct => _endMove.ExecuteAsync(ct), ProcessEventsAsync).Forget();
        }

        void TapNode(CaseId id)
        {
            if (!CanAct || _run!.Phase != ZoneRunPhase.ChoosingDestination || !_run.Destinations.ContainsKey(id)) return;
            RunBoardAsync(ct => _moveTo.ExecuteAsync(id, ct), ProcessEventsAsync).Forget();
        }

        /// <summary>Action de plateau : entrées bloquées pendant l'appel, erreur affichée, puis suite.</summary>
        async UniTaskVoid RunBoardAsync(Func<CancellationToken, UniTask<Result<IReadOnlyList<BoardEvent>>>> action,
                                        Func<IReadOnlyList<BoardEvent>, UniTask> then)
        {
            if (!_busy.TryEnter()) return;
            Result<IReadOnlyList<BoardEvent>> r;
            try { r = await action(Lifetime); }
            catch (OperationCanceledException) { return; }
            finally { _busy.Exit(); }
            if (Disposed) return;
            if (r.IsFailure)
            {
                _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
                RefreshAll();
                return;
            }
            try { await then(r.Value); }
            catch (OperationCanceledException) { }
        }

        async UniTask AnimateDieAsync(IReadOnlyList<BoardEvent> events)
        {
            int value = 0;
            foreach (var e in events) if (e is DiceRolledEvent d) value = d.Value;
            _busy.TryEnter();
            RefreshHud();
            _audio.Play(SfxId.DiceThrow);
            try { await View.RollDieAsync(value, _session.Content.Balance.DieFaces, Lifetime); }
            finally { _busy.Exit(); }
            RefreshAll();
            _tutorial?.Signal(TutorialSignal.Rolled);
            if (_run != null && _run.Dice.BonusDice > 0) _tutorial?.Hint("bonus_die");
            if (_run != null && _run.Phase == ZoneRunPhase.ChoosingDestination && _run.PmInHand > 0) _tutorial?.Hint("end_move");
        }

        /// <summary>Joue les événements du domaine dans l'ordre : déplacement animé, puis messages, puis case déclenchée.</summary>
        async UniTask ProcessEventsAsync(IReadOnlyList<BoardEvent> events)
        {
            if (_run == null || Disposed) return;
            IReadOnlyList<CaseId>? path = null;
            foreach (var e in events)
            {
                if (e is PawnMovedEvent pm) path = pm.Path;
                if (e is SteppedBackEvent sb) path = new[] { sb.To };
            }
            if (path != null)
            {
                _busy.TryEnter();
                RefreshHud();
                _audio.Play(SfxId.PawnMove);
                try { await View.Map.MovePawnAsync(path, Lifetime); }
                finally { _busy.Exit(); }
            }
            if (path != null && !events.Any(x => x is SteppedBackEvent)) _tutorial?.Signal(TutorialSignal.Moved);
            if (Disposed) return;
            await AnnounceAndTriggerAsync(events);
        }

        async UniTask AnnounceAndTriggerAsync(IReadOnlyList<BoardEvent> events)
        {
            CaseOutcome? triggered = null;
            bool validated = false;
            foreach (var e in events)
            {
                switch (e)
                {
                    case ResourceCollectedEvent rc: _toasts.Show(_loc.F("board.resource", rc.Credits, rc.Pr), ToastKind.Reward); break;
                    case FogRevealedEvent _: _toasts.Show(_loc.T("board.fog"), ToastKind.Info); break;
                    case ObjectiveCompletedEvent oc: _toasts.Show(_loc.F("board.objective_done", ObjectiveName(oc.ObjectiveId)), ToastKind.Reward); break;
                    case PmBankedEvent pb: _toasts.Show(_loc.F("board.banked", pb.Pm), ToastKind.Info); break;
                    case SteppedBackEvent _: _toasts.Show(_loc.T("board.stepped_back"), ToastKind.Warning); _tutorial?.Hint("defeat"); break;
                    case CaseTriggeredEvent ct: triggered = ct.Outcome; break;
                    case ZoneValidatedEvent _: validated = true; break;
                }
            }
            RefreshAll();
            if (validated)
            {
                await View.PauseAsync(0.5f, Lifetime);
                _audio.Play(SfxId.ZoneComplete);
                await ShowZoneCompleteAsync();
                return;
            }
            if (triggered != null)
            {
                if (triggered.Type == CaseType.Narrative) _tutorial?.Hint("event");
                else if (triggered.Type == CaseType.MiniGame) _tutorial?.Hint("minigame");
                else if (triggered.Type == CaseType.Rest) _tutorial?.Hint("rest");
                await View.PauseAsync(0.35f, Lifetime);
                await OpenCaseAsync(triggered);
            }
        }

        string ObjectiveName(string id)
        {
            var o = _run?.Graph.Zone.Objectives.FirstOrDefault(x => x.Id == id);
            return o != null ? _loc.T(o.NameKey) : id;
        }

        async UniTask OpenCaseAsync(CaseOutcome outcome)
        {
            if (_run == null || Disposed || _flow.Current != GameState.Board) return;
            _audio.Play(outcome.Type == CaseType.Combat ? SfxId.CombatAlert : SfxId.CaseEvent);
            if (outcome.Type == CaseType.Combat)
            {
                await StartCombatAsync();
                return;
            }
            var events = await _cases.RunAsync(outcome, Lifetime);
            if (!Disposed) await ProcessEventsAsync(events);
        }

        async UniTaskVoid StartCombatAfterAsync(float delay)
        {
            try
            {
                await View.PauseAsync(delay, Lifetime);
                await StartCombatAsync();
            }
            catch (OperationCanceledException) { }
        }

        async UniTask StartCombatAsync()
        {
            if (_run == null || _run.Phase != ZoneRunPhase.InCombat || _flow.Current != GameState.Board) return;
            if (!_busy.TryEnter()) return;
            Result<Domain.Combat.CombatSimulation> r;
            try { r = await _beginCombat.ExecuteAsync(Lifetime); }
            finally { _busy.Exit(); }
            if (Disposed) return;
            if (r.IsFailure)
            {
                _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
                return;
            }
            _navigate.Publish(new NavigateRequested(GameState.Combat));
        }

        /// <summary>
        /// Retour du combat (<see cref="ReturnedToBoard"/>, publié une fois le plateau réaffiché) : joue les événements du plateau
        /// (recul, objectifs, validation). Le retour au QG est géré par la machine d'états.
        /// </summary>
        void OnReturnedToBoard(ReturnedToBoard m)
        {
            ReportsApplied++;
            ProcessAsync(m.Report.BoardEvents).Forget();
        }

        async UniTaskVoid ProcessAsync(IReadOnlyList<BoardEvent> events)
        {
            try { await ProcessEventsAsync(events); }
            catch (OperationCanceledException) { }
        }

        async UniTask ShowZoneCompleteAsync()
        {
            await _cases.ShowZoneCompleteAsync(LastZoneRewards, Lifetime);
            _tutorial?.Hint("zone_done");
            await LeaveToHqAsync();
        }

        async UniTask LeaveToHqAsync()
        {
            var r = await _leave.ExecuteAsync(Lifetime);
            if (Disposed) return;
            if (r.IsFailure) { _toasts.Show(_loc.Error(r.Error!), ToastKind.Error); return; }
            _run = null;
            _navigate.Publish(new NavigateRequested(GameState.Hub));
        }

        // ------------------------------------------------------------------ Retour au QG

        public bool HandleBack()
        {
            RequestBackToHq();
            return true;
        }

        void RequestBackToHq()
        {
            if (_busy.IsBusy || Disposed) return;
            // Combat ou case engagés (pause avant la scène de combat, transition en cours) : quitter abandonnerait la partie en cours.
            if (_run != null && (_run.Phase == ZoneRunPhase.InCombat || _run.Phase == ZoneRunPhase.InCaseInteraction)) return;
            if (_flow.IsTransitioning || _flow.Current != GameState.Board) return;
            BackToHqAsync().Forget();
        }

        async UniTaskVoid BackToHqAsync()
        {
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("board.back_hq.title"), _loc.T("board.back_hq.body"),
                                                                     _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Danger), Lifetime);
                if (ok && !Disposed) await LeaveToHqAsync();
            }
            catch (OperationCanceledException) { }
        }
    }
}
