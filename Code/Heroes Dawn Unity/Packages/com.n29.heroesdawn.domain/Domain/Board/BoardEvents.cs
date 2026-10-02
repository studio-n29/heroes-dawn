using System.Collections.Generic;
using N29.HeroesDawn.Core;

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
}
