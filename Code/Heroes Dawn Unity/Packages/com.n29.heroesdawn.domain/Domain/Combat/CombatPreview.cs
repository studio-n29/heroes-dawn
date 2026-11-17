#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>
    /// Prévisualisation d'une carte jouée par le joueur (dry-run) : le combat réel est rejoué à l'identique (graine + journal) dans une copie,
    /// puis la commande hypothétique y est soumise et un pas est joué. Toute la logique (ciblage, conditions, échelles, réactions, morts, phases)
    /// est celle du moteur ; rien n'est dupliqué et le combat réel n'est jamais touché.
    /// Le premier échantillon suit le flux d'aléa réel (il est donc le vrai résultat) ; les autres tirent d'autres flux, ce qui donne les plages.
    /// Aucun aperçu (null) si le combat n'attend pas une compétence du joueur, ou si le rejeu ne retombe pas sur l'état réel (mutation hors journal).
    /// </summary>
    public static class CombatPreview
    {
        public const int DefaultSamples = 16;

        sealed class Snap
        {
            public int Hp, Shield, Point, Energy, Planned;
            public bool Alive;
            public readonly HashSet<StatusType> Statuses = new HashSet<StatusType>();
        }

        sealed class Acc
        {
            public int HpMin = int.MaxValue, HpMax = int.MinValue, HpSum;
            public int PtMin = int.MaxValue, PtMax = int.MinValue, PtSum;
            public int ShMin = int.MaxValue, ShMax = int.MinValue, ShSum;
            public int EnSum, Defeated, PlanCancelled;
            public readonly Dictionary<StatusType, int> Added = new Dictionary<StatusType, int>();
            public readonly Dictionary<StatusType, int> Removed = new Dictionary<StatusType, int>();
        }

        public static CombatPreviewResult? Compute(ContentCatalog catalog, CombatSimulation live, CombatCommand command, int samples = DefaultSamples)
        {
            if (samples < 1) samples = 1;
            if (command.Type != CommandType.Skill || live.IsOver || live.AwaitingSide != Side.Ally) return null;
            if (command.Actor < 0 || command.Actor >= live.Units.Count || live.Units[command.Actor].Side != Side.Ally || !live.CanUse(command.Actor, command.SlotIndex)) return null;

            int count = live.Units.Count;
            var before = new Snap[count];
            for (int i = 0; i < count; i++) before[i] = Take(live.Units[i]);
            var acc = new Acc[count];
            for (int i = 0; i < count; i++) acc[i] = new Acc();
            int victory = 0, defeat = 0, nextWave = 0;

            for (int s = 0; s < samples; s++)
            {
                var sim = Replay(catalog, live, out var rng);
                if (sim == null) return null;
                if (s == 0 && !Matches(sim, live)) return null;
                if (s > 0) rng!.Switch(new Pcg32(unchecked((ulong)live.Tick * 7919UL + (ulong)s * 104729UL + 1UL)).Fork(3));
                sim.Submit(command);
                sim.Step(1);
                if (sim.IsOver && sim.Victory == true) victory++;
                else if (sim.IsOver && sim.Victory == false) defeat++;
                else if (sim.Units.Count > count) nextWave++;
                for (int i = 0; i < count; i++) Add(acc[i], before[i], Take(sim.Units[i]));
            }

            var units = new List<UnitPreview>(count);
            for (int i = 0; i < count; i++) units.Add(Build(i, live.Units[i], before[i], acc[i], samples));
            return new CombatPreviewResult(units, samples, Percent(victory, samples), Percent(defeat, samples), Percent(nextWave, samples));
        }

        static Snap Take(CombatUnit u)
        {
            var s = new Snap { Hp = u.Hp, Point = u.Point, Energy = u.Energy, Planned = u.PlannedSlot, Alive = u.IsAlive };
            foreach (var st in u.Statuses)
            {
                s.Statuses.Add(st.Type);
                if (st.Type == StatusType.Shield) s.Shield += st.Value;
            }
            return s;
        }

        static void Add(Acc a, Snap before, Snap after)
        {
            a.HpMin = Math.Min(a.HpMin, after.Hp); a.HpMax = Math.Max(a.HpMax, after.Hp); a.HpSum += after.Hp;
            a.PtMin = Math.Min(a.PtMin, after.Point); a.PtMax = Math.Max(a.PtMax, after.Point); a.PtSum += after.Point;
            a.ShMin = Math.Min(a.ShMin, after.Shield); a.ShMax = Math.Max(a.ShMax, after.Shield); a.ShSum += after.Shield;
            a.EnSum += after.Energy;
            if (before.Alive && !after.Alive) a.Defeated++;
            if (before.Planned >= 0 && after.Planned < 0 && after.Alive) a.PlanCancelled++;
            foreach (var t in after.Statuses) if (!before.Statuses.Contains(t)) Bump(a.Added, t);
            foreach (var t in before.Statuses) if (!after.Statuses.Contains(t)) Bump(a.Removed, t);
        }

        static void Bump(Dictionary<StatusType, int> d, StatusType t) { d.TryGetValue(t, out int n); d[t] = n + 1; }

        static UnitPreview Build(int id, CombatUnit u, Snap b, Acc a, int n)
        {
            return new UnitPreview
            {
                Id = id,
                HpBefore = b.Hp, MaxHp = u.MaxHp, HpAfter = Mean(a.HpSum, n), HpMin = a.HpMin, HpMax = a.HpMax,
                ShieldBefore = b.Shield, ShieldAfter = Mean(a.ShSum, n), ShieldVariable = a.ShMin != a.ShMax,
                EnergyBefore = b.Energy, EnergyAfter = Mean(a.EnSum, n),
                PointBefore = b.Point, PointAfter = Mean(a.PtSum, n), PointMin = a.PtMin, PointMax = a.PtMax,
                DefeatPercent = Percent(a.Defeated, n),
                PlannedBefore = b.Planned, PlanCancelledPercent = Percent(a.PlanCancelled, n),
                Added = Changes(a.Added, n), Removed = Changes(a.Removed, n),
            };
        }

        static List<StatusChange> Changes(Dictionary<StatusType, int> d, int n)
        {
            var list = new List<StatusChange>(d.Count);
            foreach (var kv in d) list.Add(new StatusChange(kv.Key, Percent(kv.Value, n)));
            list.Sort((x, y) => x.Status.CompareTo(y.Status));
            return list;
        }

        static int Mean(int sum, int n) => (int)Math.Floor(sum / (double)n + 0.5);

        /// <summary>Pourcentage entier ; jamais 0 ni 100 tant que l'événement n'est pas absent ou général.</summary>
        static int Percent(int hits, int n)
        {
            if (hits <= 0) return 0;
            if (hits >= n) return 100;
            return Math.Max(1, Math.Min(99, (int)Math.Round(hits * 100.0 / n)));
        }

        /// <summary>Rejoue le combat réel jusqu'au tick courant : mêmes graine, mêmes commandes aux mêmes ticks.</summary>
        static CombatSimulation? Replay(ContentCatalog catalog, CombatSimulation live, out SwitchableRandom? rng)
        {
            SwitchableRandom? holder = null;
            var sim = new CombatSimulation(catalog, live.Setup, r => holder = new SwitchableRandom(r));
            rng = holder;
            var inputs = live.InputLog;
            int next = 0;
            while (sim.Tick < live.Tick && !sim.IsOver)
            {
                while (next < inputs.Count && inputs[next].Tick == sim.Tick + 1) sim.Submit(inputs[next++].Command);
                sim.Step(1);
            }
            return sim;
        }

        /// <summary>Le rejeu retombe exactement sur l'état réel (tick, unités, PV, frise, énergie, statuts, réactions).</summary>
        static bool Matches(CombatSimulation replay, CombatSimulation live)
        {
            if (replay.Tick != live.Tick || replay.IsOver != live.IsOver || replay.Units.Count != live.Units.Count) return false;
            for (int i = 0; i < live.Units.Count; i++)
            {
                var a = replay.Units[i]; var b = live.Units[i];
                if (a.Hp != b.Hp || a.Point != b.Point || a.Energy != b.Energy || a.PlannedSlot != b.PlannedSlot
                    || a.Statuses.Count != b.Statuses.Count || a.Reactions.Count != b.Reactions.Count) return false;
                for (int k = 0; k < a.Statuses.Count; k++)
                    if (a.Statuses[k].Type != b.Statuses[k].Type || a.Statuses[k].TurnsLeft != b.Statuses[k].TurnsLeft || a.Statuses[k].Value != b.Statuses[k].Value) return false;
            }
            return true;
        }

        /// <summary>Générateur du combat enveloppé : suit le flux réel pendant le rejeu, puis passe à un autre flux pour l'échantillon.</summary>
        sealed class SwitchableRandom : IRandom
        {
            IRandom _current;
            public SwitchableRandom(IRandom inner) { _current = inner; }
            public void Switch(IRandom other) { _current = other; }
            public uint NextUInt() => _current.NextUInt();
            public int Range(int minInclusive, int maxExclusive) => _current.Range(minInclusive, maxExclusive);
            public bool Chance(int percent) => _current.Chance(percent);
            public IRandom Fork(ulong stream) => _current.Fork(stream);
            public ulong State => _current.State;
        }
    }
}
