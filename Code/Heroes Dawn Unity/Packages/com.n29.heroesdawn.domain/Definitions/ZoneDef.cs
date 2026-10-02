using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class ZoneDef
    {
        public ZoneId Id { get; }
        public string NameKey { get; }
        public CaseId Entry { get; }
        public IReadOnlyList<NodeDef> Nodes { get; }
        public IReadOnlyList<LinkDef> Links { get; }
        public IReadOnlyList<ObjectiveDef> Objectives { get; }
        public IReadOnlyList<EncounterDef> Encounters { get; }

        public ZoneDef(ZoneId id, string nameKey, CaseId entry, IReadOnlyList<NodeDef> nodes, IReadOnlyList<LinkDef> links,
                       IReadOnlyList<ObjectiveDef> objectives, IReadOnlyList<EncounterDef> encounters)
        {
            Id = id; NameKey = nameKey; Entry = entry; Nodes = nodes; Links = links; Objectives = objectives; Encounters = encounters;
        }
    }
}
