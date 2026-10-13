#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Synergies formées par une escouade : une entrée par paire de membres dont les familles forment une synergie.</summary>
    public static class SquadSynergies
    {
        public static IReadOnlyList<(SynergyDef Synergy, MemberDef A, MemberDef B)> Of(IReadOnlyList<MemberDef> squad, IReadOnlyList<SynergyDef> synergies)
        {
            var families = new List<Family>();
            foreach (var m in squad) families.Add(m.Family);
            var list = new List<(SynergyDef, MemberDef, MemberDef)>();
            foreach (var a in SynergyRules.Active(families, synergies)) list.Add((a.Synergy, squad[a.A], squad[a.B]));
            return list;
        }

        /// <summary>Synergies auxquelles une famille peut participer (fiche membre).</summary>
        public static IReadOnlyList<SynergyDef> For(Family family, IReadOnlyList<SynergyDef> synergies)
        {
            var list = new List<SynergyDef>();
            foreach (var s in synergies) if (s.A == family || s.B == family) list.Add(s);
            return list;
        }
    }
}
