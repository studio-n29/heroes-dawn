#nullable enable
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

        /// <summary>Code court affiché sur la carte (« 1.2 »).</summary>
        public string Code { get; }
        /// <summary>Acte de l'histoire (1 = première nuit).</summary>
        public int Act { get; }
        /// <summary>Quartier de la carte de la ville (« brooklyn »).</summary>
        public string Borough { get; }
        /// <summary>Niveau d'escouade conseillé.</summary>
        public int RecommendedLevel { get; }
        /// <summary>Zones à valider avant d'ouvrir celle-ci (vide : disponible d'emblée).</summary>
        public IReadOnlyList<ZoneId> Requires { get; }

        public ZoneDef(ZoneId id, string nameKey, CaseId entry, IReadOnlyList<NodeDef> nodes, IReadOnlyList<LinkDef> links,
                       IReadOnlyList<ObjectiveDef> objectives, IReadOnlyList<EncounterDef> encounters,
                       string code = "", int act = 1, string borough = "brooklyn", int recommendedLevel = 1, IReadOnlyList<ZoneId>? requires = null)
        {
            Id = id; NameKey = nameKey; Entry = entry; Nodes = nodes; Links = links; Objectives = objectives; Encounters = encounters;
            Code = code; Act = act; Borough = borough; RecommendedLevel = recommendedLevel;
            Requires = requires ?? System.Array.Empty<ZoneId>();
        }
    }
}
