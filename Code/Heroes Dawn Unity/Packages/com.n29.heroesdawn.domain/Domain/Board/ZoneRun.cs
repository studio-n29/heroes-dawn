#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;

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
        readonly HashSet<(CaseId, NarrationPhase)> _narrated = new HashSet<(CaseId, NarrationPhase)>();
        readonly HashSet<CaseId> _resolvedCases = new HashSet<CaseId>();
        readonly HashSet<string> _objectives = new HashSet<string>();
        readonly List<BoardAction> _log = new List<BoardAction>();
        readonly List<AgentMalus> _maluses = new List<AgentMalus>();
        ActivityPlan? _activity;
        int _activityAttempts;
        ActivityId? _followUpActivity;
        bool _combatIsFollowUp;
        CaseId _previous;
        IReadOnlyDictionary<CaseId, MovePlan> _destinations = new Dictionary<CaseId, MovePlan>();
        IReadOnlyDictionary<CaseId, FogReachPlan> _fogReach = new Dictionary<CaseId, FogReachPlan>();

        public BoardGraph Graph { get; }
        public FogOfWar Fog { get; }
        public DiceBank Dice { get; }
        public CaseId Position { get; private set; }
        public int PmInHand { get; private set; }
        public ZoneRunPhase Phase { get; private set; }
        public CaseOutcome? PendingCase { get; private set; }
        public IReadOnlyDictionary<CaseId, MovePlan> Destinations => _destinations;
        /// <summary>Cases encore cachées mais à portée : surlignées, elles renvoient vers leur arrêt forcé (une destination).</summary>
        public IReadOnlyDictionary<CaseId, FogReachPlan> FogReach => _fogReach;
        public IReadOnlyCollection<string> CompletedObjectives => _objectives;
        /// <summary>Malus d'agents (ex. potion ratée), valables jusqu'à la fin du plateau.</summary>
        public IReadOnlyList<AgentMalus> Maluses => _maluses;
        /// <summary>Activité en cours sur la case Mini-jeu (null hors activité).</summary>
        public ActivityPlan? CurrentActivity => _activity;
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
            EmitNarration(Position, NarrationPhase.Before, events);
            if (!(type == CaseType.Combat && IsBlocking(Position)) && !interactive) EmitNarration(Position, NarrationPhase.After, events);

            if (type == CaseType.Combat && IsBlocking(Position))
            {
                BankRemaining(events);
                PendingCase = new CaseOutcome(Position, type, node0.Rank, PickEncounter(node0.Rank, node0));
                Phase = ZoneRunPhase.InCombat;
                events.Add(new CaseTriggeredEvent(PendingCase));
            }
            else if (interactive)
            {
                BankRemaining(events);
                PendingCase = new CaseOutcome(Position, type, node0.Rank, null);
                _activity = null;
                _followUpActivity = null;
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

        /// <summary>Aleatoire propre a une case (graine du plateau + case) : ne touche pas au flux des autres tirages.</summary>
        Pcg32 EventRng(CaseId at, string salt)
        {
            var h = new Fnv1a64();
            h.Add((long)Seed);
            h.Add(at.Value);
            h.Add(salt);
            return new Pcg32(h.Value);
        }

        /// <summary>Evenement resolu : avec une chance, un objet est trouve (tire parmi les objets du jeu).</summary>
        void RollItemFind(CaseId at, List<BoardEvent> events)
        {
            var rng = EventRng(at, "item");
            if (!rng.Chance(_c.Balance.EventItemChancePercent)) return;
            var item = CollectionRules.RollItem(_c.Collection, rng);
            if (item != null) events.Add(new ItemFoundEvent(at, item.Id));
        }

        /// <summary>Dialogue de la case joué une seule fois par plateau : l'événement porte les répliques.</summary>
        void EmitNarration(CaseId at, NarrationPhase phase, List<BoardEvent> events)
        {
            var lines = Graph.Node(at).Narration?.Of(phase);
            if (lines == null || lines.Count == 0 || !_narrated.Add((at, phase))) return;
            events.Add(new NarrationEvent(at, phase, lines));
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
                // Combat lancé par l'échec d'une activité : la case d'activité reste à rejouer (ni « vaincue » ni objectif de combat).
                if (!_combatIsFollowUp)
                {
                    _defeated.Add(Position);
                    CompleteObjectives(ObjectiveType.DefeatCombat, Position, events);
                    EmitNarration(Position, NarrationPhase.After, events);
                    if (Graph.Node(Position).Rank == CombatRank.Elite) events.Add(new BoosterEarnedEvent(CollectionRules.StandardBooster, BoosterSource.Exploit));
                }
            }
            else
            {
                Position = _previous;
                events.Add(new SteppedBackEvent(Position));
            }
            _combatIsFollowUp = false;
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
                if (type == CaseType.Narrative) RollItemFind(Position, events);
                EmitNarration(Position, NarrationPhase.After, events);
            }
            PendingCase = null;
            Phase = ZoneRunPhase.AwaitingRoll;
            CheckValidated(events);
            return events;
        }

        // ------------------------------------------------------------------ Activités

        /// <summary>Malus cumulé d'un agent (négatif, borné) : appliqué à ses statistiques en combat jusqu'à la fin du plateau.</summary>
        public void AddMalusForTests(AgentMalus malus) => _maluses.Add(malus);

        public int MalusPercent(MemberId member)
        {
            int sum = 0;
            foreach (var m in _maluses) if (m.Member == member) sum += m.StatPercent;
            return sum < -ActivityRules.MaxMalusPercent ? -ActivityRules.MaxMalusPercent : sum > 0 ? 0 : sum;
        }

        /// <summary>
        /// Prépare l'activité de la case Mini-jeu où l'on se trouve : activité de la case (ou celle qu'un échec vient d'enchaîner), environnement
        /// de la zone, graine dérivée de la partie. Idempotent tant que l'activité n'est pas résolue (reprise d'une partie).
        /// </summary>
        public ActivityPlan PlanActivity(IReadOnlyList<MemberId> squad)
        {
            Require(ZoneRunPhase.InCaseInteraction);
            if (Graph.Node(Position).Type != CaseType.MiniGame) throw new InvalidOperationException("Pas d'activité sur la case " + Position);
            if (_activity != null) return _activity;
            var id = _followUpActivity ?? Graph.Node(Position).Activity ?? ActivityIds.Reflex;
            _followUpActivity = null;
            _activity = ActivityPlanner.Plan(ActivityRegistry.Default, _c.Activity(id), Graph.Zone.Environment, Seed, Position, ++_activityAttempts, squad);
            return _activity;
        }

        /// <summary>
        /// Résout l'activité en cours : réussite = case résolue (objectif validé) ; échec = case à rejouer, malus d'agent éventuel,
        /// puis suite éventuelle (combat de rang donné, ou autre activité enchaînée sur la même case).
        /// </summary>
        public IReadOnlyList<BoardEvent> ResolveActivity(ActivityResult result)
        {
            Require(ZoneRunPhase.InCaseInteraction);
            var plan = _activity ?? PlanActivity(Array.Empty<MemberId>());
            var outcome = ActivityRules.Resolve(plan, result);
            _log.Add(new BoardAction(BoardActionType.ActivityResolved, plan.Def.Id.Value, (result.Score << 1) | (result.Success ? 1 : 0)));
            _activity = null;
            if (outcome.Malus != null) _maluses.Add(outcome.Malus);
            var events = new List<BoardEvent> { new ActivityResolvedEvent(outcome) };

            if (result.Success)
            {
                _resolvedCases.Add(Position);
                CompleteObjectives(ObjectiveType.CompleteMiniGame, Position, events);
                if (EventRng(Position, "booster").Chance(_c.Balance.ActivityBoosterChancePercent)) events.Add(new BoosterEarnedEvent(CollectionRules.StandardBooster, BoosterSource.MiniGame));
            }
            else if (outcome.FollowUp != null && outcome.FollowUp.Type == FollowUpType.Combat)
            {
                _combatIsFollowUp = true;
                PendingCase = new CaseOutcome(Position, CaseType.Combat, outcome.FollowUp.Rank, PickEncounter(outcome.FollowUp.Rank));
                Phase = ZoneRunPhase.InCombat;
                events.Add(new CaseTriggeredEvent(PendingCase));
                return events;
            }
            else if (outcome.FollowUp != null && outcome.FollowUp.Type == FollowUpType.Activity)
            {
                _followUpActivity = outcome.FollowUp.Activity; // la case reste interactive : l'activité liée est la suivante
                return events;
            }
            PendingCase = null;
            Phase = ZoneRunPhase.AwaitingRoll;
            CheckValidated(events);
            return events;
        }

        void BankRemaining(List<BoardEvent> events)
        {
            _destinations = new Dictionary<CaseId, MovePlan>();
            _fogReach = new Dictionary<CaseId, FogReachPlan>();
            if (PmInHand <= 0) { PmInHand = 0; return; }
            Dice.AddUnusedPm(PmInHand);
            events.Add(new PmBankedEvent(PmInHand));
            PmInHand = 0;
        }

        void RefreshDestinations()
        {
            _destinations = MovementRules.FindDestinations(Graph, Fog, Position, PmInHand, IsBlocking);
            _fogReach = MovementRules.FindFogReach(Graph, Fog, Position, PmInHand, IsBlocking, _destinations);
        }

        EncounterDef PickEncounter(CombatRank rank, NodeDef? node = null)
        {
            if (node != null && node.Combat != null && node.Combat.EnemyTable.Count > 0) return EncounterGenerator.Generate(node.Combat, rank, _caseRng);
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
