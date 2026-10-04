#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Combos formés par une escouade : une entrée par paire de membres dont les familles forment un combo.</summary>
    public static class SquadCombos
    {
        public static IReadOnlyList<(ComboDef Combo, MemberDef A, MemberDef B)> Of(IReadOnlyList<MemberDef> squad, IReadOnlyList<ComboDef> combos)
        {
            var list = new List<(ComboDef, MemberDef, MemberDef)>();
            for (int i = 0; i < squad.Count; i++)
                for (int j = i + 1; j < squad.Count; j++)
                    foreach (var c in combos)
                        if (c.Matches(squad[i].Family, squad[j].Family)) { list.Add((c, squad[i], squad[j])); break; }
            return list;
        }

        /// <summary>Combos auxquels une famille peut participer (fiche membre).</summary>
        public static IReadOnlyList<ComboDef> For(Family family, IReadOnlyList<ComboDef> combos)
        {
            var list = new List<ComboDef>();
            foreach (var c in combos) if (c.A == family || c.B == family) list.Add(c);
            return list;
        }
    }
}
