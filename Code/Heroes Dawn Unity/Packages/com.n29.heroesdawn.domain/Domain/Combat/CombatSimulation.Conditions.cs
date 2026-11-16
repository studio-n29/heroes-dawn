#nullable enable
using System;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        /// <summary>
        /// Évalue une condition d'effet. « Allié » = toute unité vivante du camp du lanceur (lanceur compris) ; « ennemis » = unités vivantes de l'autre camp ;
        /// les seuils de PV sont en % des PV max (strictement sous / strictement au-dessus).
        /// </summary>
        internal bool Holds(Condition c, CombatUnit actor, CombatUnit target)
        {
            switch (c.Kind)
            {
                case ConditionKind.TargetHasStatus: return target.Has(c.Status);
                case ConditionKind.TargetLacksStatus: return !target.Has(c.Status);
                case ConditionKind.TargetHpBelow: return target.Hp * 100 < target.MaxHp * c.Value;
                case ConditionKind.TargetHpAbove: return target.Hp * 100 > target.MaxHp * c.Value;
                case ConditionKind.CasterHasStatus: return actor.Has(c.Status);
                case ConditionKind.CasterHpBelow: return actor.Hp * 100 < actor.MaxHp * c.Value;
                case ConditionKind.AnyAllyHasStatus:
                    foreach (var u in _units) if (u.IsAlive && u.Side == actor.Side && u.Has(c.Status)) return true;
                    return false;
                case ConditionKind.TargetStatusCountAtLeast: return target.Statuses.Count >= c.Value;
                case ConditionKind.TargetPlannedAction: return target.PlannedSlot >= 0;
                case ConditionKind.EnemiesAliveAtLeast: return CountAlive(actor, enemies: true) >= c.Value;
                case ConditionKind.EnemiesAliveAtMost: return CountAlive(actor, enemies: true) <= c.Value;
                default: return false;
            }
        }

        /// <summary>Somme des bonus des modificateurs de l'effet : fixes si leur condition tient, à échelle = Bonus × min(compte, MaxCount).</summary>
        internal int ModBonus(EffectDef fx, CombatUnit actor, CombatUnit target)
        {
            int total = 0;
            foreach (var m in fx.Mods)
            {
                if (m.When != null && !Holds(m.When, actor, target)) continue;
                total += m.Per.HasValue ? m.Bonus * Math.Min(CountOf(m.Per.Value, actor, target), m.MaxCount) : m.Bonus;
            }
            return total;
        }

        int CountOf(ScaleKind kind, CombatUnit actor, CombatUnit target)
        {
            switch (kind)
            {
                case ScaleKind.TargetStatusCount: return target.Statuses.Count;
                case ScaleKind.EnemiesAlive: return CountAlive(actor, enemies: true);
                case ScaleKind.AlliesAlive: return CountAlive(actor, enemies: false);
                case ScaleKind.TargetMissingHpTenths: return (target.MaxHp - target.Hp) * 10 / target.MaxHp;
                case ScaleKind.CasterMissingHpTenths: return (actor.MaxHp - actor.Hp) * 10 / actor.MaxHp;
                default: return 0;
            }
        }

        int CountAlive(CombatUnit actor, bool enemies)
        {
            int n = 0;
            foreach (var u in _units) if (u.IsAlive && (u.Side != actor.Side) == enemies) n++;
            return n;
        }

        /// <summary>Valeur finale d'un effet : base + bonus (jamais négative) ; une jauge négative garde son signe (le bonus s'applique à sa valeur absolue). Sans modificateur : la valeur de base.</summary>
        int ValueOf(EffectDef fx, CombatUnit actor, CombatUnit target)
        {
            if (fx.Mods.Count == 0) return fx.Value;
            int bonus = ModBonus(fx, actor, target);
            if (fx.Type == EffectType.GaugeDelta && fx.Value < 0) return -Math.Max(0, -fx.Value + bonus);
            return Math.Max(0, fx.Value + bonus);
        }
    }
}
