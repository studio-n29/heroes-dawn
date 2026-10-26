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
            if (!u.IsReady || slotIndex < 0 || slotIndex >= ActiveSkillCount(u) || slotIndex > 3) return false;
            if (u.Has(StatusType.Bind) && slotIndex > 0) return false;
            return u.Energy >= u.Skills[slotIndex].Cost;
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
                case CommandType.Pass:
                    if (cmd.Actor < 0 || cmd.Actor >= _units.Count || !_units[cmd.Actor].IsReady || _units[cmd.Actor].Side != Side.Ally) return false;
                    output.Add(new TurnSkippedEvent(Tick, cmd.Actor));
                    EndOfTurn(_units[cmd.Actor], 0);
                    return true;
            }
            return false;
        }

        /// <summary>Début de tour : régénération de PV, durées des statuts ; le Choc fait perdre le tour.</summary>
        void BeginTurn(CombatUnit u, List<CombatEvent> output)
        {

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
                u.Point = 0;
            }
        }

        /// <summary>Fin d action : l unité est replacée sur la frise selon le poids de sa compétence (voir <see cref="ResetPointAfter"/>).</summary>
        void EndOfTurn(CombatUnit u, int slotIndex)
        {
            u.Point = ResetPointAfter(slotIndex);
            u.KnockbackGuard = false;
            u.PlannedSlot = -1;
            u.PlannedTarget = -1;
        }

        void PerformSkill(CombatUnit actor, SkillDef skill, int slotIndex, int preferredTarget, List<CombatEvent> output)
        {
            var targets = ResolveTargets(actor, skill.Target, preferredTarget);
            output.Add(new SkillUsedEvent(Tick, actor.Id, skill.Id, targets.ToArray()));
            actor.Energy = System.Math.Max(0, actor.Energy - skill.Cost);
            ApplyEffects(actor, skill.Effects, targets, output);
            EndOfTurn(actor, slotIndex);
        }

        void ApplyEffects(CombatUnit actor, IReadOnlyList<EffectDef> effects, List<int> targets, List<CombatEvent> output)
        {
            // Exécution : bonus lu avant la boucle, pour qu'il compte où qu'il soit placé dans la liste.
            int executeBonus = 0;
            foreach (var fx in effects) if (fx.Type == EffectType.Execute) executeBonus += fx.Value;
            foreach (var tid in targets)
            {
                var t = _units[tid];
                bool missed = false;
                int dealt = 0; // dégâts infligés à cette cible par la compétence (après bouclier), pour le vol de vie
                foreach (var fx in effects)
                {
                    if (missed) break;
                    // Une cible vaincue ne reçoit plus d'effet ; le vol de vie et le rebond comptent encore le coup qui l'a vaincue.
                    if (!t.IsAlive && fx.Type != EffectType.Lifesteal && fx.Type != EffectType.Bounce) break;
                    if (!_rng.Chance(fx.ChancePercent)) continue;
                    switch (fx.Type)
                    {
                        case EffectType.Damage:
                            dealt += HitWithDamage(actor, t, fx.Value, executeBonus, output, out missed);
                            break;
                        case EffectType.Bounce:
                            Bounce(actor, effects, targets, fx.Value, output);
                            break;
                        case EffectType.Purge:
                            Purge(actor, t, fx.Value, output);
                            break;
                        case EffectType.Lifesteal:
                            if (dealt <= 0 || !actor.IsAlive) break;
                            int stolen = Math.Min(dealt * fx.Value / 100, actor.MaxHp - actor.Hp);
                            actor.Hp += stolen;
                            output.Add(new HealEvent(Tick, actor.Id, stolen));
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
                            int reach = t.Point + PointsOf(fx.Value);
                            bool arrives = reach >= t.ActionPoint;
                            t.Point = arrives ? t.ActionPoint : reach;
                            output.Add(new AdvancedEvent(Tick, t.Id, fx.Value, arrives));
                            if (arrives)
                            {
                                output.Add(new GaugeFullEvent(Tick, t.Id));
                                BeginTurn(t, output);
                            }
                            break;
                        case EffectType.GaugeDelta:
                            if (t.IsReady && fx.Value > 0) break; // deja prete : rien a ajouter
                            int delta = (Math.Abs(fx.Value) * _b.TimelinePoints + 500) / 1000 * Math.Sign(fx.Value); // points sur 1000 -> points de frise
                            t.Point = Math.Max(0, Math.Min(t.ActionPoint - 1, t.Point + delta)); // ne declenche pas de tour hors de l avance normale
                            if (t.PlannedSlot >= 0 && t.Point < PlanStartPoint(t)) CancelPlan(t, InterruptReason.PushBack, output);

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Un coup de dégâts de puissance <paramref name="power"/> (% ATQ) : raté possible (Aveuglement, Esquive), critique, Marque, Corrosion,
        /// Vulnérable, bonus d'exécution sous le seuil de PV. Rend les dégâts retirés aux PV.
        /// </summary>
        int HitWithDamage(CombatUnit actor, CombatUnit t, int power, int executeBonus, List<CombatEvent> output, out bool missed)
        {
            missed = false;
            if (t.Side != actor.Side && (actor.Has(StatusType.Blind) && !_rng.Chance(50) || t.Has(StatusType.Evade)))
            {
                RemoveStatus(t, StatusType.Evade, output);
                output.Add(new MissEvent(Tick, t.Id));
                missed = true;
                return 0;
            }
            bool marked = t.Has(StatusType.Mark);
            bool crit = marked || _rng.Chance(_b.CritBasePercent + actor.CritBonusPercent);
            int def = t.Has(StatusType.Corrosion) ? t.Def * 70 / 100 : t.Def;
            int variance = _rng.Range(-_b.VariancePercent, _b.VariancePercent + 1);
            int dmg = DamageFormula.Compute(actor.Atk, power, def, _b.DefK, crit, t.Has(StatusType.Vulnerable), variance, _b);
            if (marked) RemoveStatus(t, StatusType.Mark, output);
            if (executeBonus > 0 && t.Hp * 100 < t.MaxHp * _b.ExecuteThresholdPercent)
            {
                dmg = dmg * (100 + executeBonus) / 100;
                output.Add(new ExecutedEvent(Tick, t.Id));
            }
            return DealDamage(t, dmg, crit, output);
        }

        /// <summary>Rebond : un autre ennemi vivant (hors des cibles) tiré au sort subit le premier effet de dégâts à <paramref name="percent"/> %. Sans candidat ni dégâts : aucun tirage.</summary>
        void Bounce(CombatUnit actor, IReadOnlyList<EffectDef> effects, List<int> targets, int percent, List<CombatEvent> output)
        {
            EffectDef? damage = null;
            foreach (var e in effects) if (e.Type == EffectType.Damage) { damage = e; break; }
            if (damage == null) return;
            var candidates = new List<CombatUnit>();
            foreach (var u in _units) if (u.Side != actor.Side && u.IsAlive && !targets.Contains(u.Id)) candidates.Add(u);
            if (candidates.Count == 0) return;
            var to = candidates[_rng.Range(0, candidates.Count)];
            output.Add(new BounceEvent(Tick, targets[0], to.Id));
            HitWithDamage(actor, to, damage.Value * percent / 100, 0, output, out _);
        }

        static readonly StatusType[] Negative = { StatusType.Burn, StatusType.Freeze, StatusType.Shock, StatusType.Corrosion, StatusType.Blind, StatusType.Vulnerable, StatusType.Bind, StatusType.Mark };
        static readonly StatusType[] Positive = { StatusType.Shield, StatusType.Regen, StatusType.Haste, StatusType.Evade, StatusType.Anchor, StatusType.Taunt };

        /// <summary>Purge : retire <paramref name="count"/> statuts (0 = tous), les plus récents d'abord — négatifs sur un allié du lanceur, positifs sur un ennemi.</summary>
        void Purge(CombatUnit actor, CombatUnit t, int count, List<CombatEvent> output)
        {
            var kinds = t.Side == actor.Side ? Negative : Positive;
            int removed = 0;
            for (int i = t.Statuses.Count - 1; i >= 0 && (count <= 0 || removed < count); i--)
            {
                var type = t.Statuses[i].Type;
                if (System.Array.IndexOf(kinds, type) < 0) continue;
                RemoveStatus(t, type, output);
                removed++;
            }
            if (removed > 0) output.Add(new PurgedEvent(Tick, t.Id, removed));
        }

        /// <summary>Inflige les dégâts (le bouclier absorbe d'abord) ; rend les dégâts retirés aux PV (0 si tout est absorbé).</summary>
        int DealDamage(CombatUnit t, int dmg, bool crit, List<CombatEvent> output)
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
            if (dmg <= 0) return 0;
            t.Hp = Math.Max(0, t.Hp - dmg);
            output.Add(new DamageEvent(Tick, t.Id, dmg, crit));
            if (t.Hp == 0)
            {
                t.Point = 0;
                t.Statuses.Clear();
                output.Add(new UnitDefeatedEvent(Tick, t.Id));
            }
            else if (crit) InterruptOnCrit(t, output);
            return dmg;
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
            foreach (var u in _units) if (u.Side == Side.Enemy) { u.Hp = 0; u.Point = 0; u.Statuses.Clear(); }
        }
    }
}
