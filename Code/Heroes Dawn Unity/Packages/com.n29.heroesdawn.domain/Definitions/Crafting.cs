#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Matière première de la fabrication : un scrap par type d'objet, et le Noyau, rare et commun à tous.</summary>
    public enum ScrapKind { Weapon, Armor, Tool, Implant, Core }

    public static class ScrapKinds
    {
        public const int Count = 5;

        public static ScrapKind For(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return ScrapKind.Weapon;
                case ItemType.Armor: return ScrapKind.Armor;
                case ItemType.Tool: return ScrapKind.Tool;
                default: return ScrapKind.Implant;
            }
        }
    }

    /// <summary>Qualité d'un objet fabriqué : multiplie les statistiques tirées.</summary>
    public enum CraftQuality { Rough, Standard, Fine, Superior, Masterwork }

    public static class CraftQualities
    {
        /// <summary>Multiplicateur des statistiques, en % (80, 100, 115, 130, 150).</summary>
        public static int Percent(CraftQuality quality)
        {
            switch (quality)
            {
                case CraftQuality.Rough: return 80;
                case CraftQuality.Fine: return 115;
                case CraftQuality.Superior: return 130;
                case CraftQuality.Masterwork: return 150;
                default: return 100;
            }
        }
    }

    /// <summary>Statistiques tirées pour un objet fabriqué, en % (0 : la recette n'en donne pas).</summary>
    public readonly struct CraftedStats
    {
        public int Hp { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Spd { get; }
        public CraftedStats(int hp, int atk, int def, int spd) { Hp = hp; Atk = atk; Def = def; Spd = spd; }
    }

    public enum RecipeStatKind { Hp, Atk, Def, Spd }

    /// <summary>Une statistique d'une recette et la fourchette (en %) dans laquelle elle est tirée.</summary>
    public readonly struct RecipeStat
    {
        public RecipeStatKind Stat { get; }
        public int Min { get; }
        public int Max { get; }
        public RecipeStat(RecipeStatKind stat, int min, int max) { Stat = stat; Min = min; Max = max; }
    }

    /// <summary>Recette du labo : ce qu'elle coûte, sa difficulté et ce que l'objet fabriqué peut valoir.</summary>
    public sealed class RecipeDef
    {
        public string Id { get; }
        /// <summary>Clé de texte du nom : <c>craft.recipe.&lt;id&gt;</c>.</summary>
        public string NameKey => "craft.recipe." + Id;
        public ItemType Type { get; }
        public ItemRarity Rarity { get; }
        /// <summary>De 1 à 10 : à comparer au niveau de compétence du type d'objet.</summary>
        public int Difficulty { get; }
        /// <summary>Scraps du type de la recette.</summary>
        public int ScrapCost { get; }
        /// <summary>Noyaux.</summary>
        public int CoreCost { get; }
        public IReadOnlyList<RecipeStat> Stats { get; }
        public ItemPerk Perk { get; }
        public int PerkValue { get; }

        public RecipeDef(string id, ItemType type, ItemRarity rarity, int difficulty, int scrapCost, int coreCost,
                         IReadOnlyList<RecipeStat> stats, ItemPerk perk = ItemPerk.None, int perkValue = 0)
        {
            Id = id; Type = type; Rarity = rarity; Difficulty = difficulty; ScrapCost = scrapCost; CoreCost = coreCost;
            Stats = stats; Perk = perk; PerkValue = perkValue;
        }
    }

    /// <summary>Catalogue des 12 recettes (3 par type d'objet : Commune, Rare, Épique).</summary>
    public static class Recipes
    {
        const RecipeStatKind Hp = RecipeStatKind.Hp, Atk = RecipeStatKind.Atk, Def = RecipeStatKind.Def, Spd = RecipeStatKind.Spd;

        static RecipeDef Common(string id, ItemType type, RecipeStatKind a)
            => new RecipeDef(id, type, ItemRarity.Common, 1, 6, 0, new[] { new RecipeStat(a, 8, 12) });

        static RecipeDef Rare(string id, ItemType type, RecipeStatKind a, RecipeStatKind b)
            => new RecipeDef(id, type, ItemRarity.Rare, 4, 12, 2, new[] { new RecipeStat(a, 8, 14), new RecipeStat(b, 8, 14) });

        static RecipeDef Epic(string id, ItemType type, RecipeStatKind a, RecipeStatKind b, ItemPerk perk, int perkValue)
            => new RecipeDef(id, type, ItemRarity.Epic, 7, 20, 5, new[] { new RecipeStat(a, 10, 16), new RecipeStat(b, 10, 16) }, perk, perkValue);

        public static readonly IReadOnlyList<RecipeDef> All = new[]
        {
            Common("scrap_blade", ItemType.Weapon, Atk),
            Rare("arc_baton", ItemType.Weapon, Atk, Spd),
            Epic("pulse_rifle", ItemType.Weapon, Atk, Spd, ItemPerk.CritChance, 8),
            Common("patch_vest", ItemType.Armor, Hp),
            Rare("plated_coat", ItemType.Armor, Hp, Def),
            Epic("aegis_plate", ItemType.Armor, Hp, Def, ItemPerk.OpeningShield, 10),
            Common("spring_boots", ItemType.Tool, Spd),
            Rare("grapple_rig", ItemType.Tool, Spd, Def),
            Epic("chrono_pack", ItemType.Tool, Spd, Hp, ItemPerk.StartGauge, 15),
            Common("neural_chip", ItemType.Implant, Atk),
            Rare("synapse_coil", ItemType.Implant, Atk, Spd),
            Epic("apex_core", ItemType.Implant, Atk, Hp, ItemPerk.SkillPower, 10),
        };

        /// <summary>Recettes connues dès le début : les quatre Communes.</summary>
        public static readonly IReadOnlyList<string> Starter = All.Where(r => r.Rarity == ItemRarity.Common).Select(r => r.Id).ToList();

        public static RecipeDef? Find(string id)
        {
            foreach (var r in All) if (r.Id == id) return r;
            return null;
        }
    }
}
