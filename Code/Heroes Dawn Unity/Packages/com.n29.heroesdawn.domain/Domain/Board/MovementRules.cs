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

        /// <summary>
        /// Cases encore sous le brouillard et à portée (1..PM pas, sans retour immédiat), arrêts forcés ignorés :
        /// elles sont surlignées pour montrer la portée complète. Chacune renvoie vers son premier arrêt forcé
        /// (une destination de <paramref name="destinations"/>) : c'est là que le déplacement s'arrête réellement.
        /// </summary>
        public static IReadOnlyDictionary<CaseId, FogReachPlan> FindFogReach(
            BoardGraph g, FogOfWar fog, CaseId from, int pm, Func<CaseId, bool> isBlocking,
            IReadOnlyDictionary<CaseId, MovePlan> destinations)
        {
            var result = new Dictionary<CaseId, FogReachPlan>();
            var frontier = new List<Walk> { new Walk { At = from, HasPrev = false, Path = new List<CaseId>() } };

            for (int step = 1; step <= pm; step++)
            {
                var next = new List<Walk>();
                foreach (var w in frontier)
                {
                    foreach (var n in g.Neighbors(w.At))
                    {
                        if (w.HasPrev && n == w.Prev) continue;
                        if (n == from) continue;
                        var path = new List<CaseId>(w.Path) { n };
                        if (!fog.IsRevealed(n) && !result.ContainsKey(n))
                        {
                            foreach (var p in path)
                            {
                                if (fog.IsRevealed(p) && !isBlocking(p) && !fog.WouldReveal(p)) continue;
                                if (destinations.TryGetValue(p, out var stop)) result.Add(n, new FogReachPlan(n, path, p, stop.Path));
                                break;
                            }
                        }
                        next.Add(new Walk { At = n, Prev = w.At, HasPrev = true, Path = path });
                    }
                }
                frontier = next;
            }
            return result;
        }
    }
}
