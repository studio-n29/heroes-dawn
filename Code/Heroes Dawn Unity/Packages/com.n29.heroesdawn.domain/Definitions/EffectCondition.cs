#nullable enable
using System;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Nature d'une condition d'effet (données seules : l'évaluation vient avec le moteur).</summary>
    public enum ConditionKind
    {
        TargetHasStatus, TargetLacksStatus, TargetHpBelow, TargetHpAbove, CasterHasStatus, CasterHpBelow,
        AnyAllyHasStatus, TargetStatusCountAtLeast, TargetPlannedAction, EnemiesAliveAtLeast, EnemiesAliveAtMost,
    }

    /// <summary>Condition portée par un effet ou un modificateur : une nature, une altération et un seuil selon la nature.</summary>
    public sealed class Condition
    {
        public ConditionKind Kind { get; }
        public StatusType Status { get; }
        public int Value { get; }

        public Condition(ConditionKind kind, StatusType status = StatusType.None, int value = 0)
        {
            Kind = kind; Status = status; Value = value;
        }
    }

    /// <summary>Grandeur comptée par un modificateur à échelle.</summary>
    public enum ScaleKind { TargetStatusCount, EnemiesAlive, AlliesAlive, TargetMissingHpTenths, CasterMissingHpTenths }

    /// <summary>
    /// Modificateur de valeur d'un effet. Sans <see cref="Per"/> : bonus fixe quand <see cref="When"/> est vraie.
    /// Avec <see cref="Per"/> : <see cref="Bonus"/> × min(compte, <see cref="MaxCount"/>), <see cref="MaxCount"/> ≥ 1 exigé.
    /// </summary>
    public sealed class ValueMod
    {
        public Condition? When { get; }
        public int Bonus { get; }
        public ScaleKind? Per { get; }
        public int MaxCount { get; }

        public ValueMod(int bonus, Condition? when = null, ScaleKind? per = null, int maxCount = 0)
        {
            if (per.HasValue && maxCount < 1) throw new ArgumentException("MaxCount doit être au moins 1 pour un modificateur à échelle.", nameof(maxCount));
            Bonus = bonus; When = when; Per = per; MaxCount = maxCount;
        }
    }
}
