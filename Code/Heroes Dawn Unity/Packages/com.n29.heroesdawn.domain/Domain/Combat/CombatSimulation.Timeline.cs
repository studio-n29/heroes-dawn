#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// Frise chronologique : chaque unité avance de 0 % (départ) à 100 % (point d'action). Une compétence la replace selon son poids
    /// (légère 30 %, intermédiaire 15 %, lourde 0 %, combo en canalisation à moins), un contrôle la repousse en arrière,
    /// la zone 80-100 % annonce l'action (visible) et un critique la fait avorter. Réserve temporelle d'équipe (Time Bank) :
    /// combos synchronisés par proximité (gratuits). Tout est entier et déterministe.
    /// </summary>
    public sealed partial class CombatSimulation
    {
        /// <summary>Position (%) où l'action d'une unité devient visible : la Perception alliée la révèle plus tôt (précognition).</summary>
        public int PlanStartPercent(CombatUnit u)
        {
            if (u.Side == Side.Enemy)
                foreach (var a in _units)
                    if (a.Side == Side.Ally && a.IsAlive && a.Family == Family.Perception) return _b.TimelinePrecognitionPercent;
            return _b.TimelinePrepZonePercent;
        }

        public bool InPrepZone(CombatUnit u) => u.IsAlive && !u.IsReady && u.TimelinePercent >= _b.TimelinePrepZonePercent;

        /// <summary>Position après l'action : selon l'emplacement de la compétence (-1 = combo).</summary>
        long GaugeAfter(CombatUnit u, int slotIndex)
        {
            int pct = slotIndex == 0 ? _b.TimelineLightResetPercent
                    : slotIndex == 1 ? _b.TimelineMediumResetPercent
                    : slotIndex == 2 ? _b.TimelineHeavyResetPercent
                    : _b.TimelineComboResetPercent;
            return u.GaugeMax * pct / 100;
        }

        /// <summary>Annonce l'action d'une unité ennemie qui entre en zone de préparation.</summary>
        void PlanIfNeeded(CombatUnit u, List<CombatEvent> output)
        {
            if (u.Side != Side.Enemy || u.PlannedSlot >= 0 || !u.IsAlive || u.IsReady) return;
            if (u.TimelinePercent < PlanStartPercent(u)) return;
            var (slot, target) = ChooseAction(u);
            u.PlannedSlot = slot;
            u.PlannedTarget = target;
            output.Add(new ActionPlannedEvent(Tick, u.Id, slot));
        }

        void CancelPlan(CombatUnit u, InterruptReason reason, List<CombatEvent> output)
        {
            if (u.PlannedSlot < 0) return;
            u.PlannedSlot = -1;
            u.PlannedTarget = -1;
            output.Add(new ActionInterruptedEvent(Tick, u.Id, reason));
        }

        /// <summary>Repousse une unité en arrière sur la frise (sauf Ancrage ou unité déjà prête) ; annule son action annoncée si elle sort de la zone.</summary>
        void PushBack(CombatUnit u, int percent, InterruptReason reason, List<CombatEvent> output)
        {
            if (percent <= 0 || !u.IsAlive || u.IsReady || u.Has(StatusType.Anchor) || u.Gauge <= 0) return;
            long before = u.Gauge;
            u.Gauge = Math.Max(0, before - u.GaugeMax * percent / 100);
            if (u.Gauge == before) return;
            output.Add(new PushedBackEvent(Tick, u.Id, percent));
            if (u.PlannedSlot >= 0 && u.TimelinePercent < PlanStartPercent(u)) CancelPlan(u, reason, output);
        }

        static int PushBackFor(BalanceConfig b, StatusType s)
            => s == StatusType.Bind ? b.PushBackBindPercent : s == StatusType.Shock ? b.PushBackShockPercent : s == StatusType.Freeze ? b.PushBackFreezePercent : 0;

        /// <summary>Critique en zone de préparation : l'action avorte et l'unité est renvoyée en arrière (l'Ancrage protège).</summary>
        void InterruptOnCrit(CombatUnit t, List<CombatEvent> output)
        {
            if (!InPrepZone(t) || t.Has(StatusType.Anchor)) return;
            long to = t.GaugeMax * _b.TimelineInterruptResetPercent / 100;
            if (t.Gauge > to) t.Gauge = to;
            t.PlannedSlot = -1;
            t.PlannedTarget = -1;
            output.Add(new ActionInterruptedEvent(Tick, t.Id, InterruptReason.Critical));
        }

        /// <summary>Brûlure : dégâts proportionnels au chemin parcouru sur la frise (à chaque pas), avec report du reste.</summary>
        void BurnStep(CombatUnit u, long fill, List<CombatEvent> output)
        {
            if (!u.Has(StatusType.Burn) || _b.BurnPercentPerTimeline <= 0) return;
            long unit = u.GaugeMax * 100;
            u.BurnRemainder += (long)u.MaxHp * _b.BurnPercentPerTimeline * fill;
            int dmg = (int)(u.BurnRemainder / unit);
            u.BurnRemainder %= unit;
            if (dmg > 0) DealDamage(u, dmg, false, output);
        }

        /// <summary>Combos synchronisés : un allié prêt, un partenaire proche de la fin de la frise ; le Temps d'équipe rattrape l'écart.</summary>
        public IReadOnlyList<ComboOption> AvailableSyncCombos()
        {
            var list = new List<ComboOption>();
            foreach (var a in _units)
            {
                if (a.Side != Side.Ally || !a.IsReady || a.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                foreach (var b in _units)
                {
                    if (b.Side != Side.Ally || b.Id == a.Id || !b.IsAlive || b.IsReady) continue;
                    if (b.ActionsSinceCombo < _b.ComboCooldownActions || b.TimelinePercent < _b.ComboSyncWindowPercent) continue;
                    var combo = _c.FindCombo(a.Family, b.Family);
                    if (combo != null) list.Add(new ComboOption(a.Id, b.Id, combo));
                }
            }
            return list;
        }

        bool TrySyncCombo(int a, int b, int target, List<CombatEvent> output)
        {
            foreach (var o in AvailableSyncCombos())
            {
                if (o.A != a || o.B != b) continue;
                var partner = _units[b];
                partner.Gauge = partner.GaugeMax;
                partner.PlannedSlot = -1;
                output.Add(new GaugeFullEvent(Tick, partner.Id));
                BeginTurn(partner, output);
                if (!partner.IsReady) return true; // un Choc lui a fait perdre son tour : pas de combo
                return TryPerformCombo(a, b, target, output);
            }
            return false;
        }
    }
}
