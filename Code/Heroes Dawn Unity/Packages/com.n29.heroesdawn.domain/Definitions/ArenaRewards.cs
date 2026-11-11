#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    public readonly struct ComponentGrant
    {
        public readonly ComponentKind Kind;
        public readonly ComponentGrade Grade;
        public readonly int Count;
        public ComponentGrant(ComponentKind kind, ComponentGrade grade, int count) { Kind = kind; Grade = grade; Count = count; }
    }

    public readonly struct CatalystGrant
    {
        public readonly CatalystKind Kind;
        public readonly int Count;
        public CatalystGrant(CatalystKind kind, int count) { Kind = kind; Count = count; }
    }

    /// <summary>Un lot de l'arène : Crédits, Composants, catalyseurs, recette et cosmétiques (<c>cosmetic:&lt;id&gt;</c>, où {r} devient l'identifiant des récompenses du tournoi).</summary>
    public sealed class ArenaRewardDef
    {
        public int Credits { get; }
        public IReadOnlyList<ComponentGrant> Components { get; }
        public IReadOnlyList<CatalystGrant> Catalysts { get; }
        public string? RecipeId { get; }
        public IReadOnlyList<string> Cosmetics { get; }

        public ArenaRewardDef(int credits, IReadOnlyList<ComponentGrant> components, IReadOnlyList<CatalystGrant> catalysts, string? recipeId, IReadOnlyList<string> cosmetics)
        {
            Credits = credits; Components = components; Catalysts = catalysts; RecipeId = recipeId; Cosmetics = cosmetics;
        }

        public static readonly ArenaRewardDef Empty = new ArenaRewardDef(0, new ComponentGrant[0], new CatalystGrant[0], null, new string[0]);
    }

    /// <summary>Les lots de l'arène : un par palier de points, le lot du top 10 % et celui du top 3.</summary>
    public sealed class ArenaRewardTable
    {
        public const int TierTop10 = 6, TierTop3 = 7;

        public IReadOnlyList<ArenaRewardDef> Tiers { get; }
        public ArenaRewardDef Top10 { get; }
        public ArenaRewardDef Top3 { get; }

        public ArenaRewardTable(IReadOnlyList<ArenaRewardDef> tiers, ArenaRewardDef top10, ArenaRewardDef top3)
        {
            Tiers = tiers; Top10 = top10; Top3 = top3;
        }

        public static ArenaRewardTable Empty => new ArenaRewardTable(new ArenaRewardDef[0], ArenaRewardDef.Empty, ArenaRewardDef.Empty);

        /// <summary>Lot d'un numéro de palier : 1 à 5 (points), 6 (top 10 %), 7 (top 3) ; nul si inconnu.</summary>
        public ArenaRewardDef? ForTier(int tier)
        {
            if (tier == TierTop10) return Top10;
            if (tier == TierTop3) return Top3;
            return tier >= 1 && tier <= Tiers.Count ? Tiers[tier - 1] : null;
        }
    }
}
