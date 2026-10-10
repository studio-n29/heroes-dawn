using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;

namespace N29.HeroesDawn.Domain.Board
{
    public abstract class BoardEvent { }

    public sealed class DiceRolledEvent : BoardEvent
    {
        public int Value { get; }
        public bool Bonus { get; }
        public DiceRolledEvent(int value, bool bonus) { Value = value; Bonus = bonus; }
    }

    public sealed class PawnMovedEvent : BoardEvent
    {
        public IReadOnlyList<CaseId> Path { get; }
        public PawnMovedEvent(IReadOnlyList<CaseId> path) { Path = path; }
    }

    public sealed class FogRevealedEvent : BoardEvent
    {
        public IReadOnlyList<CaseId> Cases { get; }
        public FogRevealedEvent(IReadOnlyList<CaseId> cases) { Cases = cases; }
    }

    public sealed class ResourceCollectedEvent : BoardEvent
    {
        public CaseId Case { get; }
        public int Credits { get; }
        public int Pr { get; }
        public ResourceCollectedEvent(CaseId c, int credits, int pr) { Case = c; Credits = credits; Pr = pr; }
    }

    public sealed class CaseTriggeredEvent : BoardEvent
    {
        public CaseOutcome Outcome { get; }
        public CaseTriggeredEvent(CaseOutcome outcome) { Outcome = outcome; }
    }

    public sealed class ObjectiveCompletedEvent : BoardEvent
    {
        public string ObjectiveId { get; }
        public ObjectiveCompletedEvent(string id) { ObjectiveId = id; }
    }

    public sealed class ZoneValidatedEvent : BoardEvent { }

    /// <summary>Dialogue de narration à jouer (avant l'action de la case ou après sa résolution).</summary>
    public sealed class NarrationEvent : BoardEvent
    {
        public CaseId Case { get; }
        public NarrationPhase Phase { get; }
        public System.Collections.Generic.IReadOnlyList<NarrationLineDef> Lines { get; }
        public NarrationEvent(CaseId at, NarrationPhase phase, System.Collections.Generic.IReadOnlyList<NarrationLineDef> lines) { Case = at; Phase = phase; Lines = lines; }
    }

    public sealed class PmBankedEvent : BoardEvent
    {
        public int Pm { get; }
        public PmBankedEvent(int pm) { Pm = pm; }
    }

    public sealed class SteppedBackEvent : BoardEvent
    {
        public CaseId To { get; }
        public SteppedBackEvent(CaseId to) { To = to; }
    }

    /// <summary>Une activité jouée est résolue : gains, malus éventuel, suite éventuelle (combat ou autre activité).</summary>
    public sealed class ActivityResolvedEvent : BoardEvent
    {
        public ActivityOutcome Outcome { get; }
        public ActivityResolvedEvent(ActivityOutcome outcome) { Outcome = outcome; }
    }
}
