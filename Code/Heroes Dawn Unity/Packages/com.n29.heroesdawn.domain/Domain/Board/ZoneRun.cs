#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>
    /// Une partie de zone (GDD §5) : dé, déplacements, brouillard, cases, objectifs.
    /// Toute action est journalisée (Log) pour le rejeu serveur (M2).
    /// Les actions interdites par la phase lèvent InvalidOperationException (traduite en Result par l'Application).
    /// </summary>
    public sealed class ZoneRun
    {
        readonly ContentCatalog _c;
        readonly IRandom _diceRng;
        readonly IRandom _caseRng;
        readonly HashSet<CaseId> _collected = new HashSet<CaseId>();
        readonly HashSet<CaseId> _defeated = new HashSet<CaseId>();
        readonly HashSet<CaseId> _resolvedCases = new HashSet<CaseId>();
        readonly HashSet<string> _objectives = new HashSet<string>();
        readonly List<BoardAction> _log = new List<BoardAction>();
        CaseId _previous;
        IReadOnlyDictionary<CaseId, MovePlan> _destinations = new Dictionary<CaseId, MovePlan>();

        public BoardGraph Graph { get; }
        public FogOfWar Fog { get; }
        public DiceBank Dice { get; }
        public CaseId Position { get; private set; }
        public int PmInHand { get; private set; }
        public ZoneRunPhase Phase { get; private set; }
        public CaseOutcome? PendingCase { get; private set; }
        public IReadOnlyDictionary<CaseId, MovePlan> Destinations => _destinations;
        public IReadOnlyCollection<string> CompletedObjectives => _objectives;
        public IReadOnlyCollection<CaseId> CollectedResources => _collected;
        public IReadOnlyCollection<CaseId> DefeatedCombats => _defeated;
        public IReadOnlyList<BoardAction> Log => _log;
        public ulong Seed { get; }

        public ZoneRun(ContentCatalog c, ZoneDef zone, ulong seed)
        {
            _c = c;
            Seed = seed;
            var root = new Pcg32(seed);
            _diceRng = root.Fork(1);
            _caseRng = root.Fork(2);
            Graph = new BoardGraph(zone);
            Fog = new FogOfWar(Graph, c.Balance.FogRadius);
            Dice = new DiceBank(c.Balance);
            Position = zone.Entry;
            _previous = zone.Entry;
            Fog.RevealAround(Position);
            Phase = ZoneRunPhase.AwaitingRoll;
        }

        /// <summary>Case de combat non encore gagnée.</summary>
        public bool IsBlocking(CaseId id) => Graph.Node(id).Type == CaseType.Combat && !_defeated.Contains(id);

        /// <summary>Case déjà « consommée » (ressource ramassée, combat gagné, mini-jeu/événement/repos résolu).</summary>
        public bool IsSpent(CaseId id) => _collected.Contains(id) || _defeated.Contains(id) || _resolvedCases.Contains(id);

        public IReadOnlyList<BoardEvent> Roll()
        {
            Require(ZoneRunPhase.AwaitingRoll);
            int v = _diceRng.Range(1, _c.Balance.DieFaces + 1);
            _log.Add(new BoardAction(BoardActionType.Roll, "", v));
            PmInHand += v;
            RefreshDestinations();
            Phase = ZoneRunPhase.ChoosingDestination;
            return new BoardEvent[] { new DiceRolledEvent(v, false) };
        }

        public IReadOnlyList<BoardEvent> UseBonusDie()
        {
            if (Phase != ZoneRunPhase.AwaitingRoll && Phase != ZoneRunPhase.ChoosingDestination)
                throw new InvalidOperationException("Dé bonus impossible en phase " + Phase);
            if (!Dice.TryConsumeBonusDie()) throw new InvalidOperationException("Aucun dé bonus");
            int v = _diceRng.Range(1, _c.Balance.DieFaces + 1);
            _log.Add(new BoardAction(BoardActionType.BonusDie, "", v));
            PmInHand += v;
            RefreshDestinations();
            Phase = ZoneRunPhase.ChoosingDestination;
            return new BoardEvent[] { new DiceRolledEvent(v, true) };
        }

        public IReadOnlyList<BoardEvent> MoveTo(CaseId target)
        {
            Require(ZoneRunPhase.ChoosingDestination);
            if (!_destinations.TryGetValue(target, out var plan)) throw new InvalidOperationException("Destination invalide : " + target);
            _log.Add(new BoardAction(BoardActionType.Move, target.Value, 0));
            var events = new List<BoardEvent> { new PawnMovedEvent(plan.Path) };

            CaseId before = Position;
            foreach (var step in plan.Path)
            {
                _previous = before;
                before = step;
                var node = Graph.Node(step);
                if (node.Type == CaseType.Resource && _collected.Add(step))
                {
                    int cr = _caseRng.Range(_c.Balance.ResourceCreditsMin, _c.Balance.ResourceCreditsMax + 1);
                    int pr = _caseRng.Range(_c.Balance.ResourcePrMin, _c.Balance.ResourcePrMax + 1);
                    events.Add(new ResourceCollectedEvent(step, cr, pr));
                }
            }
            Position = target;
            PmInHand -= plan.Cost;

            var revealed = Fog.RevealAround(Position);
            if (revealed.Count > 0) events.Add(new FogRevealedEvent(revealed));
            CompleteObjectives(ObjectiveType.ReachCase, Position, events);

            var node0 = Graph.Node(Position);
            var type = node0.Type;
            bool interactive = (type == CaseType.MiniGame || type == CaseType.Narrative || type == CaseType.Rest) && !_resolvedCases.Contains(Position);

            if (type == CaseType.Combat && IsBlocking(Position))
            {
                BankRemaining(events);
                PendingCase = new CaseOutcome(Position, type, node0.Rank, PickEncounter(node0.Rank));
                Phase = ZoneRunPhase.InCombat;
                events.Add(new CaseTriggeredEvent(PendingCase));
            }
            else if (interactive)
            {
                BankRemaining(events);
                PendingCase = new CaseOutcome(Position, type, node0.Rank, null);
                Phase = ZoneRunPhase.InCaseInteraction;
                events.Add(new CaseTriggeredEvent(PendingCase));
            }
            else if (PmInHand > 0 && revealed.Count > 0)
            {
                // Arrêt de brouillard : le joueur choisit la suite avec les PM restants.
                RefreshDestinations();
                if (_destinations.Count > 0) Phase = ZoneRunPhase.ChoosingDestination;
                else { BankRemaining(events); Phase = ZoneRunPhase.AwaitingRoll; }
            }
            else
            {
                BankRemaining(events);
                Phase = ZoneRunPhase.AwaitingRoll;
            }
            CheckValidated(events);
            return events;
        }

        public IReadOnlyList<BoardEvent> EndMove()
        {
            Require(ZoneRunPhase.ChoosingDestination);
            _log.Add(new BoardAction(BoardActionType.EndMove, "", 0));
            var events = new List<BoardEvent>();
            BankRemaining(events);
            Phase = ZoneRunPhase.AwaitingRoll;
            return events;
        }

        public IReadOnlyList<BoardEvent> ResolveCombat(bool victory)
        {
            Require(ZoneRunPhase.InCombat);
            _log.Add(new BoardAction(BoardActionType.CombatResult, Position.Value, victory ? 1 : 0));
            var events = new List<BoardEvent>();
            if (victory)
            {
                _defeated.Add(Position);
                CompleteObjectives(ObjectiveType.DefeatCombat, Position, events);
            }
            else
            {
                Position = _previous;
                events.Add(new SteppedBackEvent(Position));
            }
            PendingCase = null;
            Phase = ZoneRunPhase.AwaitingRoll;
            CheckValidated(events);
            return events;
        }

        public IReadOnlyList<BoardEvent> ResolveCase(bool success)
        {
            Require(ZoneRunPhase.InCaseInteraction);
            _log.Add(new BoardAction(BoardActionType.CaseResolved, Position.Value, success ? 1 : 0));
            var events = new List<BoardEvent>();
            var type = Graph.Node(Position).Type;
            if (success)
            {
                _resolvedCases.Add(Position);
                if (type == CaseType.MiniGame) CompleteObjectives(ObjectiveType.CompleteMiniGame, Position, events);
                if (type == CaseType.Narrative) CompleteObjectives(ObjectiveType.ResolveNarrative, Position, events);
            }
            PendingCase = null;
            Phase = ZoneRunPhase.AwaitingRoll;
            CheckValidated(events);
            return events;
        }

        void BankRemaining(List<BoardEvent> events)
        {
            _destinations = new Dictionary<CaseId, MovePlan>();
            if (PmInHand <= 0) { PmInHand = 0; return; }
            Dice.AddUnusedPm(PmInHand);
            events.Add(new PmBankedEvent(PmInHand));
            PmInHand = 0;
        }

        void RefreshDestinations() => _destinations = MovementRules.FindDestinations(Graph, Fog, Position, PmInHand, IsBlocking);

        EncounterDef PickEncounter(CombatRank rank)
        {
            var list = new List<EncounterDef>();
            foreach (var e in Graph.Zone.Encounters) if (e.Rank == rank) list.Add(e);
            if (list.Count == 0) foreach (var e in Graph.Zone.Encounters) list.Add(e);
            return list[_caseRng.Range(0, list.Count)];
        }

        void CompleteObjectives(ObjectiveType type, CaseId at, List<BoardEvent> events)
        {
            foreach (var o in Graph.Zone.Objectives)
                if (o.Type == type && o.Target == at && _objectives.Add(o.Id)) events.Add(new ObjectiveCompletedEvent(o.Id));
        }

        void CheckValidated(List<BoardEvent> events)
        {
            if (Phase == ZoneRunPhase.Completed || Graph.Zone.Objectives.Count == 0) return;
            if (Phase == ZoneRunPhase.InCombat || Phase == ZoneRunPhase.InCaseInteraction) return;
            if (_objectives.Count == Graph.Zone.Objectives.Count)
            {
                BankRemaining(events);
                Phase = ZoneRunPhase.Completed;
                events.Add(new ZoneValidatedEvent());
            }
        }

        void Require(ZoneRunPhase p)
        {
            if (Phase != p) throw new InvalidOperationException("Phase attendue " + p + ", actuelle " + Phase);
        }

        // --- Aides de test et de debug ---

        public static ZoneRun ForTests(ContentCatalog c, ZoneDef zone, ulong seed, int pmInHand, bool revealAll,
                                       string? position = null, string[]? collected = null)
        {
            var run = new ZoneRun(c, zone, seed);
            if (revealAll) run.Fog.Restore(run.Graph.AllIds);
            if (position != null)
            {
                run.Position = new CaseId(position);
                run._previous = run.Position;
            }
            if (collected != null) foreach (var id in collected) run._collected.Add(new CaseId(id));
            run.SetPmForTests(pmInHand);
            return run;
        }

        public void SetPmForTests(int pm)
        {
            PmInHand = pm;
            if (pm > 0)
            {
                RefreshDestinations();
                Phase = ZoneRunPhase.ChoosingDestination;
            }
        }
    }
}
