using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public abstract class CombatEvent
    {
        public int Tick { get; }
        protected CombatEvent(int tick) { Tick = tick; }
    }

    public sealed class GaugeFullEvent : CombatEvent { public int Unit { get; } public GaugeFullEvent(int t, int u) : base(t) { Unit = u; } }
    public sealed class TurnSkippedEvent : CombatEvent { public int Unit { get; } public TurnSkippedEvent(int t, int u) : base(t) { Unit = u; } }

    public sealed class SkillUsedEvent : CombatEvent
    {
        public int Actor { get; }
        public SkillId Skill { get; }
        public int[] Targets { get; }
        public SkillUsedEvent(int t, int actor, SkillId skill, int[] targets) : base(t) { Actor = actor; Skill = skill; Targets = targets; }
    }

    public sealed class DamageEvent : CombatEvent
    {
        public int Target { get; }
        public int Amount { get; }
        public bool Crit { get; }
        public DamageEvent(int t, int target, int amount, bool crit) : base(t) { Target = target; Amount = amount; Crit = crit; }
    }

    public sealed class MissEvent : CombatEvent { public int Target { get; } public MissEvent(int t, int target) : base(t) { Target = target; } }
    public sealed class HealEvent : CombatEvent { public int Target { get; } public int Amount { get; } public HealEvent(int t, int target, int amount) : base(t) { Target = target; Amount = amount; } }
    public sealed class ShieldAbsorbedEvent : CombatEvent { public int Target { get; } public int Amount { get; } public ShieldAbsorbedEvent(int t, int target, int amount) : base(t) { Target = target; Amount = amount; } }

    public sealed class StatusAppliedEvent : CombatEvent
    {
        public int Target { get; }
        public StatusType Status { get; }
        public int Turns { get; }
        public StatusAppliedEvent(int t, int target, StatusType status, int turns) : base(t) { Target = target; Status = status; Turns = turns; }
    }

    public sealed class StatusExpiredEvent : CombatEvent
    {
        public int Target { get; }
        public StatusType Status { get; }
        public StatusExpiredEvent(int t, int target, StatusType status) : base(t) { Target = target; Status = status; }
    }

    public sealed class UnitDefeatedEvent : CombatEvent { public int Unit { get; } public UnitDefeatedEvent(int t, int u) : base(t) { Unit = u; } }

    public sealed class ComboAvailableEvent : CombatEvent
    {
        public int A { get; }
        public int B { get; }
        public string ComboId { get; }
        public ComboAvailableEvent(int t, int a, int b, string comboId) : base(t) { A = a; B = b; ComboId = comboId; }
    }

    public sealed class ComboUsedEvent : CombatEvent
    {
        public int A { get; }
        public int B { get; }
        public string ComboId { get; }
        public ComboUsedEvent(int t, int a, int b, string comboId) : base(t) { A = a; B = b; ComboId = comboId; }
    }

    public sealed class CombatEndedEvent : CombatEvent { public bool Victory { get; } public CombatEndedEvent(int t, bool victory) : base(t) { Victory = victory; } }
}
