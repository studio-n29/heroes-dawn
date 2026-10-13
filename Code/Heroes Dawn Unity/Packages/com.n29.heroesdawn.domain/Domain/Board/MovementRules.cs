using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>
    /// GDD §5 — destinations dont le trajet coûte au plus PM (chaque chemin a un coût en PM) ; un combat non résolu en travers du chemin arrête le pion, qui repart ensuite vers sa destination
    /// ; le brouillard ne bloque pas (toute case à portée est une destination, il se lève au passage).
    /// Chemin retenu : le moins cher, puis le plus court en cases, puis l'ordre des identifiants. Le pion suit ce chemin sans s'arrêter
    /// aux bifurcations ni aux cases traversées.
    /// </summary>
    public static class MovementRules
    {
        sealed class Label
        {
            public int Cost;
            public List<CaseId> Path = new List<CaseId>();
        }

        static bool Better(Label candidate, Label current)
        {
            if (candidate.Cost != current.Cost) return candidate.Cost < current.Cost;
            if (candidate.Path.Count != current.Path.Count) return candidate.Path.Count < current.Path.Count;
            for (int i = 0; i < candidate.Path.Count; i++)
            {
                int c = candidate.Path[i].CompareTo(current.Path[i]);
                if (c != 0) return c < 0;
            }
            return false;
        }

        /// <summary>
        /// Plus courts chemins depuis <paramref name="from"/> dans la limite de <paramref name="budget"/> PM.
        /// <paramref name="canEnter"/> : la case peut être atteinte ; <paramref name="canExpand"/> : on peut continuer au-delà.
        /// </summary>
        static Dictionary<CaseId, Label> Shortest(BoardGraph g, CaseId from, int budget, Func<CaseId, bool> canEnter, Func<CaseId, bool> canExpand)
        {
            var best = new Dictionary<CaseId, Label>();
            var settled = new HashSet<CaseId> { from };
            var open = new List<CaseId>();
            void Relax(CaseId at, Label atLabel)
            {
                foreach (var n in g.Neighbors(at))
                {
                    if (n == from || settled.Contains(n) || !canEnter(n)) continue;
                    int cost = atLabel.Cost + g.CostBetween(at, n);
                    if (cost > budget) continue;
                    var cand = new Label { Cost = cost, Path = new List<CaseId>(atLabel.Path) { n } };
                    if (!best.TryGetValue(n, out var cur)) { best[n] = cand; open.Add(n); }
                    else if (Better(cand, cur)) best[n] = cand;
                }
            }
            Relax(from, new Label());
            while (open.Count > 0)
            {
                int pick = 0;
                for (int i = 1; i < open.Count; i++) if (Better(best[open[i]], best[open[pick]])) pick = i;
                var id = open[pick];
                open.RemoveAt(pick);
                settled.Add(id);
                if (canExpand(id)) Relax(id, best[id]);
            }
            return best;
        }

        public static IReadOnlyDictionary<CaseId, MovePlan> FindDestinations(
            BoardGraph g, FogOfWar fog, CaseId from, int pm, Func<CaseId, bool> isBlocking)
        {
            var labels = Shortest(g, from, pm, _ => true, _ => true);
            var result = new Dictionary<CaseId, MovePlan>();
            foreach (var kv in labels) result.Add(kv.Key, new MovePlan(kv.Key, kv.Value.Path, kv.Value.Cost));
            return result;
        }

        /// <summary>
        /// Portée « cachée » : supprimée (spec 2026-10-12) — le brouillard ne bloque plus, toute case à portée est une destination
        /// (visible ou non) et le brouillard se lève au passage du pion. Renvoie toujours un dictionnaire vide.
        /// </summary>
        public static IReadOnlyDictionary<CaseId, FogReachPlan> FindFogReach(
            BoardGraph g, FogOfWar fog, CaseId from, int pm, Func<CaseId, bool> isBlocking,
            IReadOnlyDictionary<CaseId, MovePlan> destinations)
            => new Dictionary<CaseId, FogReachPlan>();
    }
}
