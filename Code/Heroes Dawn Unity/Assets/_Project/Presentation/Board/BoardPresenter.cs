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
using N29.HeroesDawn.Domain.Maps;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Feedback;
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
        /// <summary>Case visée par la prévisualisation (tapée), sa destination réelle et le chemin illuminé ; <c>null</c> : pas de fiche ouverte.</summary>
        CaseId? _previewTapped;
        MovePlan? _previewPlan;
        FogReachPlan? _previewFog;
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
            View.MoveConfirmClicked += ConfirmMove;
            View.MoveCancelClicked += CancelPreview;
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
            View.MoveConfirmClicked -= ConfirmMove;
            View.MoveCancelClicked -= CancelPreview;
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
                ClosePreview();
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
                Links = run.Graph.Zone.Links.Select(l => new BoardMapLink
                {
                    A = l.A, B = l.B, Cost = l.Cost,
                    Curve = LinkCurves.Resolve(run.Graph.Node(l.A), run.Graph.Node(l.B), l),
                }).ToArray(),
                Pawn = new PortraitData(_loc.MemberName(leader), leader.Family),
                PawnAt = run.Position,
                BackgroundKey = run.Graph.Zone.Background?.Key ?? "",
                BgMinX = run.Graph.Zone.Background?.MinX ?? 0, BgMinY = run.Graph.Zone.Background?.MinY ?? 0,
                BgMaxX = run.Graph.Zone.Background?.MaxX ?? 0, BgMaxY = run.Graph.Zone.Background?.MaxY ?? 0,
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
            ValidatePreview(run);
            var doneIds = new HashSet<string>(run.CompletedObjectives);
            var doneTargets = new HashSet<CaseId>(run.Graph.Zone.Objectives.Where(o => doneIds.Contains(o.Id)).Select(o => o.Target));
            bool choosing = run.Phase == ZoneRunPhase.ChoosingDestination;
            var pathNodes = _previewPlan != null ? new HashSet<CaseId>(_previewPlan.Path) : new HashSet<CaseId>();
            var stopPairs = _previewPlan != null ? Pairs(run.Position, _previewPlan.Path) : new HashSet<(CaseId, CaseId)>();
            var beyondPairs = _previewFog != null ? Pairs(run.Position, _previewFog.FullPath) : new HashSet<(CaseId, CaseId)>();
            var nodes = run.Graph.AllIds.Select(id =>
            {
                var def = run.Graph.Node(id);
                bool spent = run.IsSpent(id);
                var mark = NodeMark.None;
                int cost = -1;
                if (choosing)
                {
                    if (_previewTapped == id)
                    {
                        mark = NodeMark.Target;
                        cost = _previewFog != null ? _previewFog.Cost : _previewPlan!.Cost;
                    }
                    else if (pathNodes.Contains(id)) mark = NodeMark.Path;
                    else if (run.Destinations.TryGetValue(id, out var d)) { mark = NodeMark.Destination; cost = d.Cost; }
                    else if (run.FogReach.TryGetValue(id, out var f)) { mark = NodeMark.FogDestination; cost = f.Cost; }
                }
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
                    Mark = mark,
                    DestinationCost = cost,
                };
            }).ToArray();
            var links = run.Graph.Zone.Links.Select(l =>
            {
                bool a = run.Fog.IsRevealed(l.A), b = run.Fog.IsRevealed(l.B);
                var lm = stopPairs.Contains((l.A, l.B)) ? LinkMark.Path : beyondPairs.Contains((l.A, l.B)) ? LinkMark.Beyond : LinkMark.None;
                return new BoardLinkState { Visible = a || b || lm != LinkMark.None, Dim = !(a && b), Mark = lm };
            }).ToArray();
            View.Map.Refresh(nodes, links, run.Position, movePawn);
        }

        /// <summary>Paires de cases consécutives (dans les deux sens) du chemin parti de <paramref name="start"/>.</summary>
        static HashSet<(CaseId, CaseId)> Pairs(CaseId start, IReadOnlyList<CaseId> path)
        {
            var set = new HashSet<(CaseId, CaseId)>();
            var prev = start;
            foreach (var id in path)
            {
                set.Add((prev, id));
                set.Add((id, prev));
                prev = id;
            }
            return set;
        }

        // ------------------------------------------------------------------ Fiche de case (« S'y rendre »)

        /// <summary>Ouvre la fiche de la case tapée : décrit la case, illumine le chemin et dézoome dessus.</summary>
        void OpenPreview(CaseId tapped, MovePlan plan, FogReachPlan? fog)
        {
            var run = _run!;
            _previewTapped = tapped;
            _previewPlan = plan;
            _previewFog = fog;
            View.ShowMovePreview(BuildPreview(run, tapped, plan, fog));
            RefreshMap(run, false);
            var ids = new List<CaseId> { run.Position };
            ids.AddRange(fog != null ? fog.FullPath : plan.Path);
            View.Map.FitTo(ids, true);
        }

        MovePreviewData BuildPreview(ZoneRun run, CaseId tapped, MovePlan plan, FogReachPlan? fog)
        {
            if (!run.Fog.IsRevealed(tapped) && !run.IsBlocking(tapped))
            {
                return new MovePreviewData
                {
                    Title = _loc.T("board.preview.fog_title"),
                    Body = _loc.T("board.preview.fog_hidden"),
                    Detail = _loc.F("board.preview.fog_cost", plan.Cost, run.PmInHand - plan.Cost),
                    Tone = Tone.Info,
                    ConfirmLabel = _loc.T("board.preview.go"),
                    CancelLabel = _loc.T("board.preview.cancel"),
                };
            }
            var def = run.Graph.Node(tapped);
            var body = _loc.T("board.case." + def.Type + ".body");
            if (run.Graph.Zone.Objectives.Any(o => o.Target == tapped)) body += "\n" + _loc.T("board.case.objective");
            var detail = plan.Cost >= run.PmInHand ? _loc.F("board.preview.cost_all", plan.Cost) : _loc.F("board.preview.cost", plan.Cost, run.PmInHand - plan.Cost);
            if (run.IsBlocking(tapped)) detail += "\n" + _loc.T("board.preview.stop_combat");
            if (run.IsSpent(tapped) && def.Type != CaseType.Entry) detail += "\n" + _loc.T("board.preview.spent");
            return new MovePreviewData
            {
                Title = CaseTitle(def),
                Body = body,
                Detail = detail,
                Tone = def.Type == CaseType.Combat ? Tone.Danger : Tone.Primary,
                ConfirmLabel = _loc.T("board.preview.go"),
                CancelLabel = _loc.T("board.preview.cancel"),
            };
        }

        string CaseTitle(NodeDef def)
            => def.Type == CaseType.Combat ? _loc.F("board.case.Combat.title", _loc.RankName(def.Rank)) : _loc.T("board.case." + def.Type + ".title");

        void ClosePreview()
        {
            if (_previewTapped == null && _previewPlan == null) return;
            _previewTapped = null;
            _previewPlan = null;
            _previewFog = null;
            View.ShowMovePreview(null);
        }

        /// <summary>La fiche n'a de sens que pendant le choix de destination, tant que la destination existe encore.</summary>
        void ValidatePreview(ZoneRun run)
        {
            if (_previewPlan == null) return;
            if (run.Phase != ZoneRunPhase.ChoosingDestination || !run.Destinations.ContainsKey(_previewPlan.Target)) ClosePreview();
        }

        void CancelPreview()
        {
            if (_run == null || Disposed || _previewPlan == null) return;
            ClosePreview();
            RefreshAll(false);
            View.Map.CenterOn(_run.Position, true);
        }

        void ConfirmMove()
        {
            if (!CanAct || _run!.Phase != ZoneRunPhase.ChoosingDestination || _previewPlan == null) return;
            var target = _previewPlan.Target;
            ClosePreview();
            RunBoardAsync(ct => _moveTo.ExecuteAsync(target, ct), ProcessEventsAsync).Forget();
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
            ClosePreview(); // les PM changent : la fiche serait périmée
            RunBoardAsync(ct => _bonus.ExecuteAsync(ct), AnimateDieAsync).Forget();
        }

        void EndMove()
        {
            if (!CanAct || _run!.Phase != ZoneRunPhase.ChoosingDestination) return;
            ClosePreview();
            RunBoardAsync(ct => _endMove.ExecuteAsync(ct), ProcessEventsAsync).Forget();
        }

        /// <summary>
        /// Toucher une case en surbrillance n'engage plus le déplacement : sa fiche s'ouvre, le chemin s'illumine et la caméra dézoome ;
        /// « S'y rendre » valide. Une case cachée à portée mène à son arrêt forcé (premier arrêt du chemin).
        /// </summary>
        void TapNode(CaseId id)
        {
            if (!CanAct || _run!.Phase != ZoneRunPhase.ChoosingDestination) return;
            if (_run.Destinations.TryGetValue(id, out var plan)) OpenPreview(id, plan, null);
            else if (_run.FogReach.TryGetValue(id, out var fog) && _run.Destinations.TryGetValue(fog.StopAt, out var stop)) OpenPreview(id, stop, fog);
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
                if (_run != null) View.Map.CenterOn(_run.Position, true); // la caméra suit le pion
            }
            if (path != null && !events.Any(x => x is SteppedBackEvent)) _tutorial?.Signal(TutorialSignal.Moved);
            if (Disposed) return;
            await AnnounceAndTriggerAsync(events);
        }

        async UniTask AnnounceAndTriggerAsync(IReadOnlyList<BoardEvent> events)
        {
            CaseOutcome? triggered = null;
            bool validated = false;
            bool steppedBack = false;
            var narrations = new List<NarrationEvent>();
            foreach (var e in events)
            {
                switch (e)
                {
                    case ResourceCollectedEvent rc: _toasts.Show(_loc.F("board.resource", rc.Credits, rc.Pr), ToastKind.Reward); break;
                    case FogRevealedEvent _: _toasts.Show(_loc.T("board.fog"), ToastKind.Info); break;
                    case ObjectiveCompletedEvent oc: _toasts.Show(_loc.F("board.objective_done", ObjectiveName(oc.ObjectiveId)), ToastKind.Reward); break;
                    case PmBankedEvent pb: _toasts.Show(_loc.F("board.banked", pb.Pm), ToastKind.Info); break;
                    case SteppedBackEvent _: steppedBack = true; _toasts.Show(_loc.T("board.stepped_back"), ToastKind.Warning); _tutorial?.Hint("defeat"); break;
                    case NarrationEvent ne: narrations.Add(ne); break;
                    case ItemFoundEvent itf: _toasts.Show(_loc.F("board.item_found", _loc.T(_session.Content.Collection.Item(itf.Item).NameKey)), ToastKind.Reward); break;
                    case BoosterEarnedEvent _: _toasts.Show(_loc.T("board.booster_earned"), ToastKind.Reward); break;
                    case CaseTriggeredEvent ct: triggered = ct.Outcome; break;
                    case ZoneValidatedEvent _: validated = true; break;
                }
            }
            RefreshAll();
            if (steppedBack) await PlayCueAsync(ComicCue.Ouch);
            foreach (var n in narrations)
            {
                if (Disposed) return;
                await _cases.PlayNarrationAsync(n, Lifetime);
            }
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

        /// <summary>Onomatopée de bande dessinée avant l'action de la case (aucune pour les cases sans action).</summary>
        async UniTask PlayCueForAsync(CaseType type)
        {
            var cue = ComicCues.ForCase(type);
            if (cue == null) return;
            await PlayCueAsync(cue.Value);
        }

        async UniTask PlayCueAsync(ComicCue cue)
        {
            if (Disposed) return;
            await View.PlayCueAsync(new ComicCueData(_loc.T(ComicCues.TextKey(cue)), ComicCues.ColorOf(cue), ComicCues.TiltOf(cue)), Lifetime);
        }

        async UniTask OpenCaseAsync(CaseOutcome outcome)
        {
            if (_run == null || Disposed || _flow.Current != GameState.Board) return;
            _audio.Play(outcome.Type == CaseType.Combat ? SfxId.CombatAlert : SfxId.CaseEvent);
            await PlayCueForAsync(outcome.Type);
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
            if (_previewPlan != null) // Échap referme d'abord la fiche de case
            {
                CancelPreview();
                return true;
            }
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
