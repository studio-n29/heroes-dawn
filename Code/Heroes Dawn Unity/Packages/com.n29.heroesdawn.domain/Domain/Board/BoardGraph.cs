using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>Graphe d'une zone : voisins triés (ordre stable).</summary>
    public sealed class BoardGraph
    {
        readonly Dictionary<CaseId, NodeDef> _nodes = new Dictionary<CaseId, NodeDef>();
        readonly Dictionary<CaseId, List<CaseId>> _adj = new Dictionary<CaseId, List<CaseId>>();

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
            }
            foreach (var list in _adj.Values) list.Sort();
            ids.Sort();
            AllIds = ids;
        }

        public NodeDef Node(CaseId id) => _nodes[id];
        public IReadOnlyList<CaseId> Neighbors(CaseId id) => _adj[id];

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
