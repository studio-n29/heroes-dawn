using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public abstract class CombatEvent
    {
        public int Tick { get; }
        protected CombatEvent(int tick) { Tick = tick; }
    }

    /// <summary>Un pas de frise : toutes les unites ont avance d un point (ou franchi leurs points propres).</summary>
    public sealed class TimelineAdvancedEvent : CombatEvent { public TimelineAdvancedEvent(int t) : base(t) { } }

    /// <summary>Le point d action est vide a ce pas : fenetre d interruption (pause en millisecondes).</summary>
    public sealed class EmptyPointPauseEvent : CombatEvent { public int PauseMs { get; } public EmptyPointPauseEvent(int t, int pauseMs) : base(t) { PauseMs = pauseMs; } }

    /// <summary>Deux unites sur le meme point : combo (deux allies) ou parade (allie et ennemi).</summary>
    public sealed class EncounterEvent : CombatEvent
    {
        public int A { get; }
        public int B { get; }
        public TickEncounterKind Kind { get; }
        public EncounterEvent(int t, int a, int b, TickEncounterKind kind) : base(t) { A = a; B = b; Kind = kind; }
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

    /// <summary>Une unité entre en zone de préparation : son action est annoncée sur la frise.</summary>
    public sealed class ActionPlannedEvent : CombatEvent
    {
        public int Unit { get; }
        public int Slot { get; }
        public ActionPlannedEvent(int t, int unit, int slot) : base(t) { Unit = unit; Slot = slot; }
    }

    public enum InterruptReason { Critical, PushBack, Parade }

    /// <summary>Action annoncée annulée (critique en zone de préparation, recul, dépassement).</summary>
    public sealed class ActionInterruptedEvent : CombatEvent
    {
        public int Unit { get; }
        public InterruptReason Reason { get; }
        public ActionInterruptedEvent(int t, int unit, InterruptReason reason) : base(t) { Unit = unit; Reason = reason; }
    }

    /// <summary>Une unité est repoussée en arrière sur la frise (contrôle : Entrave, Choc, Gel).</summary>
    /// <summary>Une unité est propulsée vers le point d'action ; <c>Ready</c> : elle l'atteint et agit aussitôt.</summary>
    public sealed class AdvancedEvent : CombatEvent
    {
        public int Unit { get; }
        public int Percent { get; }
        public bool Ready { get; }
        public AdvancedEvent(int t, int unit, int percent, bool ready) : base(t) { Unit = unit; Percent = percent; Ready = ready; }
    }

    public sealed class PushedBackEvent : CombatEvent
    {
        public int Unit { get; }
        public int Percent { get; }
        public PushedBackEvent(int t, int unit, int percent) : base(t) { Unit = unit; Percent = percent; }
    }

    public sealed class WaveStartedEvent : CombatEvent
    {
        public int Wave { get; }
        public int[] Enemies { get; }
        public WaveStartedEvent(int t, int wave, int[] enemies) : base(t) { Wave = wave; Enemies = enemies; }
    }

    public sealed class AvatarSkillUsedEvent : CombatEvent
    {
        public SkillId Skill { get; }
        public SkillCategory Category { get; }
        public int[] Targets { get; }
        public AvatarSkillUsedEvent(int t, SkillId skill, SkillCategory category, int[] targets) : base(t) { Skill = skill; Category = category; Targets = targets; }
    }

    public sealed class CombatEndedEvent : CombatEvent { public bool Victory { get; } public CombatEndedEvent(int t, bool victory) : base(t) { Victory = victory; } }
}
