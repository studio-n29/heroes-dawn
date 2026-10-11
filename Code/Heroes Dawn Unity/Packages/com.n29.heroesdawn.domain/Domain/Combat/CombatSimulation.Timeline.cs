#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// Frise chronologique par points : chaque unité occupe un point de 0 (départ) à ActionPoint (point d'action, c'est son tour).
    /// Tout le monde avance d'un point par pas. Une compétence replace l'unité selon son poids (léger 3, intermédiaire 2, lourd 0,
    /// combo -2 en canalisation), un contrôle la repousse en arrière, la zone de préparation (points 8 à 10) annonce l'action ennemie
    /// (visible) et un critique la fait avorter. Deux alliés sur un point : combo possible ; un allié et un ennemi : parade.
    /// Tout est entier et déterministe.
    /// </summary>
    public sealed partial class CombatSimulation
    {
        /// <summary>Premier point où l'action d'une unité devient visible : la Perception alliée la révèle plus tôt (précognition).</summary>
        public int PlanStartPoint(CombatUnit u)
        {
            if (u.Side == Side.Enemy)
                foreach (var a in _units)
                    if (a.Side == Side.Ally && a.IsAlive && a.Family == Family.Perception) return _b.TimelinePrecognitionPoint;
            return _b.TimelinePrepZonePoint;
        }

        public bool InPrepZone(CombatUnit u) => u.IsAlive && !u.IsReady && u.Point >= _b.TimelinePrepZonePoint;

        /// <summary>Convertit un pourcentage de la frise en nombre de points (arrondi au plus proche, au moins 1).</summary>
        int PointsOf(int percent) => Math.Max(1, (percent * _b.TimelinePoints + 50) / 100);

        /// <summary>Point de reprise après l'action : selon l'emplacement de la compétence (-1 = combo).</summary>
        int ResetPointAfter(int slotIndex)
            => slotIndex == 0 ? _b.TimelineResetLightPoint
             : slotIndex == 1 ? _b.TimelineResetMediumPoint
             : slotIndex == 2 || slotIndex == 3 ? _b.TimelineResetHeavyPoint
             : _b.TimelineResetComboPoint;

        /// <summary>Annonce l'action d'une unité ennemie qui entre en zone de préparation.</summary>
        void PlanIfNeeded(CombatUnit u, List<CombatEvent> output)
        {
            if (u.Side != Side.Enemy || u.PlannedSlot >= 0 || !u.IsAlive || u.IsReady) return;
            if (u.Point < PlanStartPoint(u)) return;
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
        bool PushBack(CombatUnit u, int percent, InterruptReason reason, List<CombatEvent> output)
        {
            if (percent <= 0 || !u.IsAlive || u.IsReady || u.Has(StatusType.Anchor) || u.Point <= 0) return false;
            int before = u.Point;
            u.Point = Math.Max(0, before - PointsOf(percent));
            if (u.Point == before) return false;
            output.Add(new PushedBackEvent(Tick, u.Id, percent));
            if (u.PlannedSlot >= 0 && u.Point < PlanStartPoint(u)) CancelPlan(u, reason, output);
            return true;
        }

        static int PushBackFor(BalanceConfig b, StatusType s)
            => s == StatusType.Bind ? b.PushBackBindPercent : s == StatusType.Shock ? b.PushBackShockPercent : s == StatusType.Freeze ? b.PushBackFreezePercent : 0;

        /// <summary>Critique en zone de préparation : l'action avorte et l'unité est renvoyée en arrière (l'Ancrage protège).</summary>
        void InterruptOnCrit(CombatUnit t, List<CombatEvent> output)
        {
            if (!InPrepZone(t) || t.Has(StatusType.Anchor)) return;
            if (t.Point > _b.TimelineInterruptResetPoint) t.Point = _b.TimelineInterruptResetPoint;
            t.PlannedSlot = -1;
            t.PlannedTarget = -1;
            output.Add(new ActionInterruptedEvent(Tick, t.Id, InterruptReason.Critical));
        }

        /// <summary>Brûlure : dégâts proportionnels au point franchi (BurnPercentPerTimeline % des PV max par frise complète), avec report du reste.</summary>
        void BurnStep(CombatUnit u, List<CombatEvent> output)
        {
            if (!u.Has(StatusType.Burn) || _b.BurnPercentPerTimeline <= 0) return;
            u.BurnRemainder += (long)u.MaxHp * _b.BurnPercentPerTimeline;
            long perPoint = Math.Max(1, _b.TimelinePoints) * 100L;
            int dmg = (int)(u.BurnRemainder / perPoint);
            u.BurnRemainder %= perPoint;
            if (dmg > 0) DealDamage(u, dmg, false, output);
        }

        readonly HashSet<long> _comboEncounters = new HashSet<long>();

        /// <summary>
        /// Rencontres du pas. Parade : un allie et un ennemi sur un point annulent l'action annoncee de l'ennemi, qui repart au point leger
        /// (sans action annoncee il n'y a rien a parer). Combo : deux allies d'une paire valide qui se retrouvent ensemble le declarent
        /// une fois ; <see cref="AvailableSyncCombos"/> les propose tant qu'ils restent ensemble.
        /// </summary>
        void ResolveEncounters(List<CombatEvent> output)
        {
            var together = new HashSet<long>();
            foreach (var e in TickEncounters.Detect(_units, _b))
            {
                if (e.Kind == TickEncounterKind.Combo)
                {
                    var a = _units[e.A];
                    var b = _units[e.B];
                    if (a.ActionsSinceCombo < _b.ComboCooldownActions || b.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                    if (_c.FindCombo(a.Family, b.Family) == null) continue;
                    long key = ((long)e.A << 32) | (uint)e.B;
                    together.Add(key);
                    if (_comboEncounters.Add(key)) output.Add(new EncounterEvent(Tick, e.A, e.B, e.Kind));
                    continue;
                }
                var enemy = _units[e.A].Side == Side.Enemy ? _units[e.A] : _units[e.B];
                if (enemy.PlannedSlot < 0) continue;
                output.Add(new EncounterEvent(Tick, e.A, e.B, e.Kind));
                CancelPlan(enemy, InterruptReason.Parade, output);
                if (enemy.Point > _b.TimelineResetLightPoint) enemy.Point = _b.TimelineResetLightPoint;
            }
            _comboEncounters.IntersectWith(together);
        }

        /// <summary>
        /// Combos par proximité : deux alliés sur le même point (hors canalisation), ou un allié prêt et un partenaire proche du point d'action.
        /// Un combo n'a lieu qu'avec une paire de familles valide et si les deux alliés ne sont pas en repos de combo.
        /// </summary>
        public IReadOnlyList<ComboOption> AvailableSyncCombos()
        {
            var list = new List<ComboOption>();
            foreach (var a in _units)
            {
                if (a.Side != Side.Ally || !a.IsAlive || a.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                foreach (var b in _units)
                {
                    if (b.Side != Side.Ally || b.Id == a.Id || !b.IsAlive || b.IsReady) continue;
                    if (b.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                    bool together = a.Id < b.Id && !a.IsReady && !a.IsChannelling && !b.IsChannelling && a.Point == b.Point;
                    bool near = a.IsReady && b.Point >= _b.ComboSyncWindowPoint;
                    if (!together && !near) continue;
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
                foreach (var u in new[] { _units[a], _units[b] })
                {
                    if (u.IsReady) continue;
                    u.Point = u.ActionPoint;
                    u.PlannedSlot = -1;
                    output.Add(new GaugeFullEvent(Tick, u.Id));
                    BeginTurn(u, output);
                }
                if (!_units[a].IsReady || !_units[b].IsReady) return true; // un Choc lui a fait perdre son tour : pas de combo
                return TryPerformCombo(a, b, target, output);
            }
            return false;
        }
    }
}
