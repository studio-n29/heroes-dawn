#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Liens entre agents : victoires remportées ensemble, par paire, enregistrées dans le profil.</summary>
    public static class TeamLinks
    {
        static (string, string) Key(string x, string y) => string.CompareOrdinal(x, y) <= 0 ? (x, y) : (y, x);

        public static int Wins(MetaSave meta, string x, string y)
        {
            var (a, b) = Key(x, y);
            foreach (var l in meta.links) if (l.a == a && l.b == b) return l.wins;
            return 0;
        }

        public static int Level(MetaSave meta, string x, string y) => LinkRules.Level(Wins(meta, x, y));

        /// <summary>Une victoire de plus pour chaque paire d agents engagés ensemble.</summary>
        public static void RecordVictory(MetaSave meta, IReadOnlyList<string> engaged)
        {
            for (int i = 0; i < engaged.Count; i++)
                for (int j = i + 1; j < engaged.Count; j++)
                {
                    var (a, b) = Key(engaged[i], engaged[j]);
                    LinkSave? found = null;
                    foreach (var l in meta.links) if (l.a == a && l.b == b) { found = l; break; }
                    if (found == null) { found = new LinkSave { a = a, b = b }; meta.links.Add(found); }
                    found.wins++;
                }
        }

        /// <summary>Niveaux de lien de toutes les paires d une escouade (pour <see cref="Domain.Combat.CombatSetup.WithLinks"/>).</summary>
        public static IReadOnlyList<LinkEntry> Entries(MetaSave meta, IReadOnlyList<string> squad)
        {
            var list = new List<LinkEntry>();
            for (int i = 0; i < squad.Count; i++)
                for (int j = i + 1; j < squad.Count; j++)
                {
                    int level = Level(meta, squad[i], squad[j]);
                    if (level > 1) list.Add(new LinkEntry(new MemberId(squad[i]), new MemberId(squad[j]), level));
                }
            return list;
        }
    }
}
