#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        readonly Queue<CombatCommand> _queue = new Queue<CombatCommand>();
        readonly List<CombatInput> _inputLog = new List<CombatInput>();

        /// <summary>Commandes réellement appliquées, avec leur tick (rejouables).</summary>
        public IReadOnlyList<CombatInput> InputLog => _inputLog;

        /// <summary>Enregistre une commande ; elle est appliquée au prochain tick.</summary>
        public void Submit(CombatCommand cmd) => _queue.Enqueue(cmd);

        public static int ActiveSkillCount(CombatUnit u)
        {
            int n = 0;
            foreach (var s in u.Skills) if (s.Slot != SkillSlot.Passive) n++;
            return n;
        }

        public bool CanUse(int actorId, int slotIndex)
        {
            if (actorId < 0 || actorId >= _units.Count) return false;
            var u = _units[actorId];
            if (!u.IsReady || slotIndex < 0 || slotIndex >= ActiveSkillCount(u) || slotIndex > 2) return false;
            if (u.Has(StatusType.Bind) && slotIndex > 0) return false;
            return u.Cooldowns[slotIndex] == 0;
        }

        void ProcessCommands(List<CombatEvent> output)
        {
            while (_queue.Count > 0 && !IsOver)
            {
                var cmd = _queue.Dequeue();
                if (ApplyCommand(cmd, output)) _inputLog.Add(new CombatInput(Tick, cmd));
            }
        }

        void ActReadyUnits(List<CombatEvent> output)
        {
            for (int i = 0; i < _units.Count; i++)
            {
                if (IsOver) return;
                var u = _units[i];
                if (!u.IsReady) continue;
                if (u.Side == Side.Enemy || Auto) AiAct(u, output);
            }
        }

        bool ApplyCommand(CombatCommand cmd, List<CombatEvent> output)
        {
            switch (cmd.Type)
            {
                case CommandType.SetAuto:
                    Auto = cmd.Flag;
                    return true;
                case CommandType.Retreat:
                    Victory = false;
                    output.Add(new CombatEndedEvent(Tick, false));
                    return true;
                case CommandType.Skill:
                    if (!CanUse(cmd.Actor, cmd.SlotIndex)) return false;
                    if (_units[cmd.Actor].Side != Side.Ally) return false;
                    PerformSkill(_units[cmd.Actor], _units[cmd.Actor].Skills[cmd.SlotIndex], cmd.SlotIndex, cmd.Target, output);
                    return true;
                case CommandType.Combo:
                    return cmd.Flag ? TrySyncCombo(cmd.Actor, cmd.Partner, cmd.Target, output) : TryPerformCombo(cmd.Actor, cmd.Partner, cmd.Target, output);
                case CommandType.Avatar:
                    return TryAvatar(cmd.SlotIndex, cmd.Target, output);
            }
            return false;
        }

        /// <summary>Début de tour : brûlure, régénération, durées des statuts, recharges ; le Choc fait perdre le tour.</summary>
        void BeginTurn(CombatUnit u, List<CombatEvent> output)
        {
            for (int i = 0; i < 3; i++) if (u.Cooldowns[i] > 0) u.Cooldowns[i]--;

            if (u.Has(StatusType.Regen))
            {
                int h = Math.Min(u.MaxHp * 8 / 100, u.MaxHp - u.Hp);
                u.Hp += h;
                if (h > 0) output.Add(new HealEvent(Tick, u.Id, h));
            }

            bool shocked = u.Has(StatusType.Shock);
            for (int i = u.Statuses.Count - 1; i >= 0; i--)
            {
                var s = u.Statuses[i];
                if (s.Type == StatusType.Mark) continue; // retirée au prochain coup reçu
                s.TurnsLeft--;
                if (s.TurnsLeft <= 0)
                {
                    u.Statuses.RemoveAt(i);
                    output.Add(new StatusExpiredEvent(Tick, u.Id, s.Type));
                }
            }

            if (shocked)
            {
                RemoveStatus(u, StatusType.Shock, output);
                output.Add(new TurnSkippedEvent(Tick, u.Id));
                u.Gauge = 0;
            }
        }

        /// <summary>Fin d action : l unité est replacée sur la frise selon le poids de sa compétence (voir <see cref="GaugeAfter"/>).</summary>
        void EndOfTurn(CombatUnit u, int slotIndex)
        {
            u.Gauge = GaugeAfter(u, slotIndex);
            u.KnockbackGuard = false;
            u.PlannedSlot = -1;
            u.PlannedTarget = -1;
            u.ActionsSinceCombo++;
        }

        void PerformSkill(CombatUnit actor, SkillDef skill, int slotIndex, int preferredTarget, List<CombatEvent> output)
        {
            var targets = ResolveTargets(actor, skill.Target, preferredTarget);
            output.Add(new SkillUsedEvent(Tick, actor.Id, skill.Id, targets.ToArray()));
            ApplyEffects(actor, skill.Effects, targets, output);
            if (slotIndex >= 0 && slotIndex < 3) actor.Cooldowns[slotIndex] = skill.Cooldown + 1;
            EndOfTurn(actor, slotIndex);
        }

        void ApplyEffects(CombatUnit actor, IReadOnlyList<EffectDef> effects, List<int> targets, List<CombatEvent> output)
        {
            foreach (var tid in targets)
            {
                var t = _units[tid];
                bool missed = false;
                foreach (var fx in effects)
                {
                    if (!t.IsAlive || missed) break;
                    if (!_rng.Chance(fx.ChancePercent)) continue;
                    switch (fx.Type)
                    {
                        case EffectType.Damage:
                            if (t.Side != actor.Side && (actor.Has(StatusType.Blind) && !_rng.Chance(50) || t.Has(StatusType.Evade)))
                            {
                                RemoveStatus(t, StatusType.Evade, output);
                                output.Add(new MissEvent(Tick, t.Id));
                                missed = true;
                                break;
                            }
                            bool marked = t.Has(StatusType.Mark);
                            bool crit = marked || _rng.Chance(_b.CritBasePercent);
                            int def = t.Has(StatusType.Corrosion) ? t.Def * 70 / 100 : t.Def;
                            int variance = _rng.Range(-_b.VariancePercent, _b.VariancePercent + 1);
                            int dmg = DamageFormula.Compute(actor.Atk, fx.Value, def, _b.DefK, crit, t.Has(StatusType.Vulnerable), variance, _b);
                            if (marked) RemoveStatus(t, StatusType.Mark, output);
                            DealDamage(t, dmg, crit, output);
                            break;
                        case EffectType.Heal:
                            int heal = t.MaxHp * fx.Value / 100;
                            int applied = Math.Min(heal, t.MaxHp - t.Hp);
                            t.Hp += applied;
                            output.Add(new HealEvent(Tick, t.Id, applied));
                            break;
                        case EffectType.Shield:
                            ApplyStatus(t, StatusType.Shield, Math.Max(1, fx.Duration), t.MaxHp * fx.Value / 100, output);
                            break;
                        case EffectType.ApplyStatus:
                            ApplyStatus(t, fx.Status, fx.Duration, 0, output);
                            break;
                        case EffectType.PushBack:
                            if (t.KnockbackGuard || t.Side == actor.Side) break;
                            if (PushBack(t, fx.Value, InterruptReason.PushBack, output)) t.KnockbackGuard = true;
                            break;
                        case EffectType.Advance:
                            if (t == actor || t.IsReady) break;
                            long reach = t.Gauge + t.GaugeMax * fx.Value / 100;
                            bool arrives = reach >= t.GaugeMax;
                            t.Gauge = arrives ? t.GaugeMax : reach;
                            output.Add(new AdvancedEvent(Tick, t.Id, fx.Value, arrives));
                            if (arrives)
                            {
                                output.Add(new GaugeFullEvent(Tick, t.Id));
                                BeginTurn(t, output);
                            }
                            break;
                        case EffectType.GaugeDelta:
                            if (t.IsReady && fx.Value > 0) break; // déjà prête : rien à ajouter
                            long g = t.Gauge + (long)fx.Value * 1000;
                            t.Gauge = Math.Max(0, Math.Min(t.GaugeMax - 1, g)); // ne déclenche pas de tour hors du remplissage
                            if (t.PlannedSlot >= 0 && t.TimelinePercent < PlanStartPercent(t)) CancelPlan(t, InterruptReason.PushBack, output);
                            break;
                    }
                }
            }
        }

        void DealDamage(CombatUnit t, int dmg, bool crit, List<CombatEvent> output)
        {
            var shield = t.Find(StatusType.Shield);
            if (shield != null)
            {
                int absorbed = Math.Min(shield.Value, dmg);
                shield.Value -= absorbed;
                dmg -= absorbed;
                if (absorbed > 0) output.Add(new ShieldAbsorbedEvent(Tick, t.Id, absorbed));
                if (shield.Value <= 0) RemoveStatus(t, StatusType.Shield, output);
            }
            if (dmg <= 0) return;
            t.Hp = Math.Max(0, t.Hp - dmg);
            output.Add(new DamageEvent(Tick, t.Id, dmg, crit));
            if (t.Hp == 0)
            {
                t.Gauge = 0;
                t.Statuses.Clear();
                output.Add(new UnitDefeatedEvent(Tick, t.Id));
            }
            else if (crit) InterruptOnCrit(t, output);
        }

        void ApplyStatus(CombatUnit t, StatusType s, int turns, int value, List<CombatEvent> output)
        {
            if (s == StatusType.None || !t.IsAlive) return;
            var existing = t.Find(s);
            if (existing != null)
            {
                existing.TurnsLeft = Math.Max(existing.TurnsLeft, turns);
                existing.Value = Math.Max(existing.Value, value);
            }
            else t.Statuses.Add(new StatusInstance(s, turns, value));
            output.Add(new StatusAppliedEvent(Tick, t.Id, s, turns));
            PushBack(t, PushBackFor(_b, s), InterruptReason.PushBack, output); // un contrôle repousse sur la frise
        }

        void RemoveStatus(CombatUnit t, StatusType s, List<CombatEvent> output)
        {
            var st = t.Find(s);
            if (st == null) return;
            t.Statuses.Remove(st);
            output.Add(new StatusExpiredEvent(Tick, t.Id, s));
        }

        List<int> ResolveTargets(CombatUnit actor, TargetRule rule, int preferred)
        {
            var list = new List<int>();
            Side foe = actor.Side == Side.Ally ? Side.Enemy : Side.Ally;
            switch (rule)
            {
                case TargetRule.Self:
                    list.Add(actor.Id);
                    break;
                case TargetRule.SingleEnemy:
                {
                    int t = ResolveSingleTarget(actor, foe, preferred);
                    if (t >= 0) list.Add(t);
                    break;
                }
                case TargetRule.SingleAlly:
                    if (preferred >= 0 && preferred < _units.Count && _units[preferred].Side == actor.Side && _units[preferred].IsAlive) list.Add(preferred);
                    else
                    {
                        int low = LowestHpRatio(actor.Side);
                        if (low >= 0) list.Add(low);
                    }
                    break;
                case TargetRule.AllEnemies:
                    foreach (var u in _units) if (u.Side == foe && u.IsAlive) list.Add(u.Id);
                    break;
                case TargetRule.AllAllies:
                    foreach (var u in _units) if (u.Side == actor.Side && u.IsAlive) list.Add(u.Id);
                    break;
            }
            return list;
        }

        /// <summary>Cible unique : la Provocation prime, puis la cible voulue si valide, puis le premier vivant.</summary>
        public int ResolveSingleTarget(CombatUnit actor, Side foe, int preferred)
        {
            foreach (var u in _units) if (u.Side == foe && u.IsAlive && u.Has(StatusType.Taunt)) return u.Id;
            if (preferred >= 0 && preferred < _units.Count && _units[preferred].Side == foe && _units[preferred].IsAlive) return preferred;
            foreach (var u in _units) if (u.Side == foe && u.IsAlive) return u.Id;
            return -1;
        }

        int LowestHpRatio(Side side)
        {
            int best = -1;
            long bestRatio = long.MaxValue;
            foreach (var u in _units)
            {
                if (u.Side != side || !u.IsAlive) continue;
                long r = (long)u.Hp * 1000 / u.MaxHp;
                if (r < bestRatio) { bestRatio = r; best = u.Id; }
            }
            return best;
        }

        /// <summary>Aides de test et de debug.</summary>
        public IReadOnlyList<CombatEvent> ApplyDamageForTests(CombatUnit t, int dmg, bool crit = false)
        {
            var output = new List<CombatEvent>();
            DealDamage(t, dmg, crit, output);
            return output;
        }
        public void ForceEnemiesDefeatedForDebug()
        {
            _wave = WaveCount - 1; // saute les vagues restantes
            foreach (var u in _units) if (u.Side == Side.Enemy) { u.Hp = 0; u.Gauge = 0; u.Statuses.Clear(); }
        }
    }
}
