using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>
    /// GDD §5 — destinations en 1..PM pas ; pas de retour immédiat ; on ne traverse ni un combat non résolu
    /// ni une case qui ouvre du brouillard (elles peuvent être des destinations finales) ; cases visibles uniquement.
    /// Chemin retenu : le plus court, puis l'ordre des identifiants.
    /// </summary>
    public static class MovementRules
    {
        struct Walk
        {
            public CaseId At;
            public CaseId Prev;
            public bool HasPrev;
            public List<CaseId> Path;
        }

        public static IReadOnlyDictionary<CaseId, MovePlan> FindDestinations(
            BoardGraph g, FogOfWar fog, CaseId from, int pm, Func<CaseId, bool> isBlocking)
        {
            var result = new Dictionary<CaseId, MovePlan>();
            var frontier = new List<Walk> { new Walk { At = from, HasPrev = false, Path = new List<CaseId>() } };

            for (int step = 1; step <= pm; step++)
            {
                var next = new List<Walk>();
                foreach (var w in frontier)
                {
                    foreach (var n in g.Neighbors(w.At))
                    {
                        if (w.HasPrev && n == w.Prev) continue;
                        if (!fog.IsRevealed(n)) continue;
                        if (n == from) continue;
                        var path = new List<CaseId>(w.Path) { n };
                        if (!result.ContainsKey(n)) result.Add(n, new MovePlan(n, path));
                        bool stops = isBlocking(n) || fog.WouldReveal(n);
                        if (!stops) next.Add(new Walk { At = n, Prev = w.At, HasPrev = true, Path = path });
                    }
                }
                frontier = next;
            }
            return result;
        }
    }
}
