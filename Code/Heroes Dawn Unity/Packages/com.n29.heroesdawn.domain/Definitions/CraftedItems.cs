#nullable enable
using System;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Objets fabriqués à l'Atelier : identifiant (<c>crafted_N</c>), clé de nom portant la recette et les étoiles, définition d'objet.</summary>
    public static class CraftedItems
    {
        public const string IdPrefix = "crafted_";
        const string KeyPrefix = "craft.item.";

        /// <summary><c>craft.item.&lt;recette&gt;.&lt;étoiles&gt;</c> : la présentation en tire le nom affiché.</summary>
        public static string NameKey(string recipeId, int stars) => KeyPrefix + recipeId + "." + stars;

        public static bool TryParseNameKey(string nameKey, out string recipeId, out int stars)
        {
            recipeId = ""; stars = 0;
            if (!nameKey.StartsWith(KeyPrefix, StringComparison.Ordinal)) return false;
            string rest = nameKey.Substring(KeyPrefix.Length);
            int dot = rest.LastIndexOf('.');
            if (dot <= 0) return false;
            if (!int.TryParse(rest.Substring(dot + 1), out stars) || stars < CraftStars.Min || stars > CraftStars.Max) return false;
            recipeId = rest.Substring(0, dot);
            return true;
        }

        /// <summary>Définition d'un exemplaire : type, rareté et effet spécial de la recette, statistiques tirées, jamais en vente.</summary>
        public static ItemDef ToDef(string id, RecipeDef recipe, int stars, CraftedStats stats)
            => new ItemDef(new ItemId(id), NameKey(recipe.Id, stars), recipe.Rarity, stats.Hp, stats.Atk, stats.Def, stats.Spd,
                           recipe.Perk, recipe.PerkValue, 0, recipe.Type);
    }
}
