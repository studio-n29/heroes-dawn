#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        void CheckEnd(List<CombatEvent> output)
        {
            if (IsOver) return;
            bool alliesAlive = false, enemiesAlive = false;
            foreach (var u in _units)
            {
                if (!u.IsAlive) continue;
                if (u.Side == Side.Ally) alliesAlive = true; else enemiesAlive = true;
            }
            if (!enemiesAlive) { Victory = true; output.Add(new CombatEndedEvent(Tick, true)); return; }
            if (!alliesAlive) { Victory = false; output.Add(new CombatEndedEvent(Tick, false)); return; }
            AnnounceCombos(output);
        }

        /// <summary>Empreinte de l'état (comparée lors du rejeu).</summary>
        public ulong StateHash()
        {
            var h = new Fnv1a64();
            h.Add(Tick);
            h.Add(TimeBank);
            h.Add(_bankTicks);
            foreach (var u in _units)
            {
                h.Add(u.Id);
                h.Add(u.Hp);
                h.Add(u.Gauge);
                h.Add(u.PlannedSlot);
                h.Add(u.BurnRemainder);
                foreach (var s in u.Statuses) { h.Add((int)s.Type); h.Add(s.TurnsLeft); h.Add(s.Value); }
            }
            h.Add(Victory == true ? 1 : Victory == false ? 2 : 0);
            return h.Value;
        }

        public CombatResult Result()
        {
            var hp = new List<(int UnitId, int Hp)>();
            foreach (var u in _units) if (u.Side == Side.Ally) hp.Add((u.Id, u.Hp));
            return new CombatResult(Victory == true, Tick, hp, StateHash());
        }
    }
}
