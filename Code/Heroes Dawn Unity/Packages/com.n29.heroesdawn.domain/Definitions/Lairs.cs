#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Un boss d'un repaire : la case du plateau où il se trouve et son ennemi.</summary>
    public sealed class LairBossDef
    {
        public string Case { get; }
        public string Enemy { get; }
        public LairBossDef(string caseId, string enemy) { Case = caseId; Enemy = enemy; }
    }

    /// <summary>
    /// Repaire : donjon à plusieurs boss, joué comme une zone (<see cref="ZoneDef.IsLair"/>) depuis la page Repaire.
    /// Les boss se battent à la suite sur le plateau ; la zone est validée quand le dernier tombe.
    /// </summary>
    public sealed class LairDef
    {
        public string Id { get; }
        public string Zone { get; }
        /// <summary>Zone à valider pour ouvrir le repaire.</summary>
        public string RequiresZone { get; }
        public IReadOnlyList<LairBossDef> Bosses { get; }
        public string NameKey => "lair." + Id + ".name";
        public string DescKey => "lair." + Id + ".desc";

        public LairDef(string id, string zone, string requiresZone, params LairBossDef[] bosses)
        {
            Id = id; Zone = zone; RequiresZone = requiresZone; Bosses = bosses;
        }
    }

    public static class Lairs
    {
        public static readonly IReadOnlyList<LairDef> All = new[]
        {
            new LairDef("amif_vault", "zone_lair_amif_vault", "zone_ny_1_4",
                new LairBossDef("n04", "brick"), new LairBossDef("n09", "crocodile"), new LairBossDef("n14", "bridge_titan")),
        };

        public static LairDef? Find(string id) => All.FirstOrDefault(l => l.Id == id);

        public static LairDef? OfZone(string zoneId) => All.FirstOrDefault(l => l.Zone == zoneId);
    }
}
