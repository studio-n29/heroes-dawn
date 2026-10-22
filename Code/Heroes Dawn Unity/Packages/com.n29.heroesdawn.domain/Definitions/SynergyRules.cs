using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Bonus de statistiques en % que l'ensemble des synergies actives donne à chaque allié.</summary>
    public readonly struct SynergyBonus
    {
        public int Hp { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Spd { get; }
        public SynergyBonus(int hp, int atk, int def, int spd) { Hp = hp; Atk = atk; Def = def; Spd = spd; }
        public bool IsNone => Hp == 0 && Atk == 0 && Def == 0 && Spd == 0;
    }

    /// <summary>Synergies d'une escouade : une par paire de membres dont les familles forment une synergie (la première de la liste).</summary>
    public static class SynergyRules
    {
        /// <summary>Plafond du cumul par statistique, en %.</summary>
        public const int CapPercent = 30;

        public static IReadOnlyList<(SynergyDef Synergy, int A, int B)> Active(IReadOnlyList<Family> squad, IReadOnlyList<SynergyDef> synergies)
        {
            var list = new List<(SynergyDef, int, int)>();
            for (int i = 0; i < squad.Count; i++)
                for (int j = i + 1; j < squad.Count; j++)
                    foreach (var s in synergies)
                        if (s.Matches(squad[i], squad[j])) { list.Add((s, i, j)); break; }
            return list;
        }

        /// <summary>Même total avec le niveau de lien de chaque paire : l effet passif de la synergie est multiplié (voir <see cref="LinkRules"/>).</summary>
        public static SynergyBonus Total(IEnumerable<(SynergyDef Synergy, int LinkLevel)> active)
        {
            var scaled = new List<SynergyDef>();
            foreach (var (s, level) in active)
                scaled.Add(new SynergyDef(s.Id, s.NameKey, s.A, s.B, s.Stat, s.Percent * LinkRules.MultiplierPercent(level) / 100));
            return Total(scaled);
        }

        public static SynergyBonus Total(IEnumerable<SynergyDef> active)
        {
            int hp = 0, atk = 0, def = 0, spd = 0;
            foreach (var s in active)
                switch (s.Stat)
                {
                    case SynergyStat.Hp: hp += s.Percent; break;
                    case SynergyStat.Atk: atk += s.Percent; break;
                    case SynergyStat.Def: def += s.Percent; break;
                    case SynergyStat.Spd: spd += s.Percent; break;
                }
            int Cap(int v) => v > CapPercent ? CapPercent : v;
            return new SynergyBonus(Cap(hp), Cap(atk), Cap(def), Cap(spd));
        }
    }
}
