#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>Combos (03_Metahumains) : deux alliés prêts d'une paire valide agissent ensemble et consomment leur tour.</summary>
    public sealed partial class CombatSimulation
    {
        readonly HashSet<long> _announcedCombos = new HashSet<long>();

        public readonly struct ComboOption
        {
            public readonly int A;
            public readonly int B;
            public readonly ComboDef Combo;
            public ComboOption(int a, int b, ComboDef combo) { A = a; B = b; Combo = combo; }
        }

        public IReadOnlyList<ComboOption> AvailableCombos()
        {
            var list = new List<ComboOption>();
            for (int i = 0; i < _units.Count; i++)
            {
                var a = _units[i];
                if (a.Side != Side.Ally || !a.IsReady || a.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                for (int j = i + 1; j < _units.Count; j++)
                {
                    var b = _units[j];
                    if (b.Side != Side.Ally || !b.IsReady || b.ActionsSinceCombo < _b.ComboCooldownActions) continue;
                    var combo = _c.FindCombo(a.Family, b.Family);
                    if (combo != null) list.Add(new ComboOption(a.Id, b.Id, combo));
                }
            }
            return list;
        }

        void AnnounceCombos(List<CombatEvent> output)
        {
            var current = new HashSet<long>();
            foreach (var o in AvailableCombos())
            {
                long key = ((long)o.A << 32) | (uint)o.B;
                current.Add(key);
                if (_announcedCombos.Add(key)) output.Add(new ComboAvailableEvent(Tick, o.A, o.B, o.Combo.Id));
            }
            _announcedCombos.IntersectWith(current);
        }

        bool TryPerformCombo(int a, int b, int target, List<CombatEvent> output)
        {
            ComboDef? combo = null;
            foreach (var o in AvailableCombos())
                if ((o.A == a && o.B == b) || (o.A == b && o.B == a)) { combo = o.Combo; break; }
            if (combo == null) return false;

            var ua = _units[a];
            var ub = _units[b];
            var caster = ua.Atk >= ub.Atk ? ua : ub;
            var other = caster == ua ? ub : ua;
            output.Add(new ComboUsedEvent(Tick, a, b, combo.Id));
            PerformSkill(caster, combo.Effect, -1, target, output);
            EndOfTurn(other, -1);
            ua.ActionsSinceCombo = 0;
            ub.ActionsSinceCombo = 0;
            return true;
        }
    }
}
