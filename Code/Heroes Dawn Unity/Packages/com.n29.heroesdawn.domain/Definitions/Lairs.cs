#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Récompense d'un boss de repaire (bloc C de la 1.0.0) : Composants, chance de catalyseur et recette.
    /// Complète une fois par jour et par repaire, réduite de moitié ensuite (la recette n'est donnée qu'une fois).
    /// </summary>
    public sealed class LairRewardDef
    {
        /// <summary>Type des Composants ; nul : tiré au hasard à chaque récompense.</summary>
        public ComponentKind? Kind { get; }
        public ComponentGrade Grade { get; }
        public int Count { get; }
        /// <summary>Chance, en %, de recevoir un catalyseur.</summary>
        public int CatalystPercent { get; }
        /// <summary>Recette donnée à la première victoire ; nul : aucune.</summary>
        public string? RecipeId { get; }
        /// <summary>Seconde ligne de Composants du même type (ex. un Exceptionnel en plus) ; <see cref="BonusCount"/> à 0 : aucune.</summary>
        public ComponentGrade BonusGrade { get; }
        public int BonusCount { get; }

        public LairRewardDef(ComponentKind? kind, ComponentGrade grade, int count, int catalystPercent, string? recipeId = null,
                             ComponentGrade bonusGrade = ComponentGrade.Exceptional, int bonusCount = 0)
        {
            Kind = kind; Grade = grade; Count = count; CatalystPercent = catalystPercent; RecipeId = recipeId; BonusGrade = bonusGrade; BonusCount = bonusCount;
        }
    }

    /// <summary>Un boss d'un repaire : la case du plateau où il se trouve, son ennemi et sa récompense.</summary>
    public sealed class LairBossDef
    {
        public string Case { get; }
        public string Enemy { get; }
        /// <summary>Récompense de Composants et de recette ; nulle : aucune récompense propre.</summary>
        public LairRewardDef? Reward { get; }
        public LairBossDef(string caseId, string enemy, LairRewardDef? reward = null) { Case = caseId; Enemy = enemy; Reward = reward; }
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
                new LairBossDef("n04", "brick", new LairRewardDef(null, ComponentGrade.Refined, 6, 15, "plated_coat")),
                new LairBossDef("n09", "crocodile", new LairRewardDef(null, ComponentGrade.Refined, 6, 20, "grapple_rig")),
                new LairBossDef("n14", "bridge_titan", new LairRewardDef(null, ComponentGrade.Refined, 4, 35, "aegis_plate", ComponentGrade.Exceptional, 1))),
        };

        public static LairDef? Find(string id) => All.FirstOrDefault(l => l.Id == id);

        public static LairDef? OfZone(string zoneId) => All.FirstOrDefault(l => l.Zone == zoneId);
    }
}
