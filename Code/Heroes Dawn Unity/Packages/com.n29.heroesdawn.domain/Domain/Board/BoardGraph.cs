using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>Graphe d'une zone : voisins triés (ordre stable), coût en PM de chaque chemin.</summary>
    public sealed class BoardGraph
    {
        readonly Dictionary<CaseId, NodeDef> _nodes = new Dictionary<CaseId, NodeDef>();
        readonly Dictionary<CaseId, List<CaseId>> _adj = new Dictionary<CaseId, List<CaseId>>();
        readonly Dictionary<(CaseId, CaseId), LinkDef> _links = new Dictionary<(CaseId, CaseId), LinkDef>();

        public ZoneDef Zone { get; }
        public IReadOnlyList<CaseId> AllIds { get; }

        public BoardGraph(ZoneDef zone)
        {
            Zone = zone;
            var ids = new List<CaseId>();
            foreach (var n in zone.Nodes)
            {
                _nodes.Add(n.Id, n);
                _adj.Add(n.Id, new List<CaseId>());
                ids.Add(n.Id);
            }
            foreach (var l in zone.Links)
            {
                _adj[l.A].Add(l.B);
                _adj[l.B].Add(l.A);
                _links[(l.A, l.B)] = l;
                _links[(l.B, l.A)] = l;
            }
            foreach (var list in _adj.Values) list.Sort();
            ids.Sort();
            AllIds = ids;
        }

        public NodeDef Node(CaseId id) => _nodes[id];
        public IReadOnlyList<CaseId> Neighbors(CaseId id) => _adj[id];

        /// <summary>Chemin reliant deux cases voisines.</summary>
        public LinkDef Link(CaseId a, CaseId b) => _links[(a, b)];

        /// <summary>PM dépensés pour passer de <paramref name="a"/> à sa voisine <paramref name="b"/>.</summary>
        public int CostBetween(CaseId a, CaseId b) => _links[(a, b)].Cost;

        /// <summary>Coût total d'un trajet : <paramref name="from"/> puis les cases de <paramref name="path"/>.</summary>
        public int PathCost(CaseId from, IReadOnlyList<CaseId> path)
        {
            int sum = 0;
            var at = from;
            foreach (var step in path) { sum += CostBetween(at, step); at = step; }
            return sum;
        }

        public IReadOnlyList<CaseId> WithinRadius(CaseId center, int radius)
        {
            var seen = new HashSet<CaseId> { center };
            var frontier = new List<CaseId> { center };
            for (int d = 0; d < radius; d++)
            {
                var next = new List<CaseId>();
                foreach (var c in frontier)
                    foreach (var n in _adj[c])
                        if (seen.Add(n)) next.Add(n);
                frontier = next;
            }
            var result = new List<CaseId>(seen);
            result.Sort();
            return result;
        }
    }
}
