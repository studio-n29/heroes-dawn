#nullable enable
using System;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Objets fabriqués au labo : identifiant (<c>crafted_N</c>), clé de nom portant la recette et la qualité, définition d'objet.</summary>
    public static class CraftedItems
    {
        public const string IdPrefix = "crafted_";
        const string KeyPrefix = "craft.item.";

        /// <summary><c>craft.item.&lt;recette&gt;.&lt;qualité en minuscules&gt;</c> : la présentation en tire le nom affiché.</summary>
        public static string NameKey(string recipeId, CraftQuality quality) => KeyPrefix + recipeId + "." + quality.ToString().ToLowerInvariant();

        public static bool TryParseNameKey(string nameKey, out string recipeId, out CraftQuality quality)
        {
            recipeId = ""; quality = CraftQuality.Standard;
            if (!nameKey.StartsWith(KeyPrefix, StringComparison.Ordinal)) return false;
            string rest = nameKey.Substring(KeyPrefix.Length);
            int dot = rest.LastIndexOf('.');
            if (dot <= 0) return false;
            if (!Enum.TryParse(rest.Substring(dot + 1), true, out quality)) return false;
            recipeId = rest.Substring(0, dot);
            return true;
        }

        /// <summary>Définition d'un exemplaire : type, rareté et effet spécial de la recette, statistiques tirées, jamais en vente.</summary>
        public static ItemDef ToDef(string id, RecipeDef recipe, CraftQuality quality, CraftedStats stats)
            => new ItemDef(new ItemId(id), NameKey(recipe.Id, quality), recipe.Rarity, stats.Hp, stats.Atk, stats.Def, stats.Spd,
                           recipe.Perk, recipe.PerkValue, 0, recipe.Type);
    }
}
