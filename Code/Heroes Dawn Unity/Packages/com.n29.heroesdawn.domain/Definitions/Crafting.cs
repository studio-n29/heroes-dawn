#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Type de Composant : un par type d'objet (correspond à <see cref="ItemType"/>).</summary>
    public enum ComponentKind { Weapon, Armor, Tool, Implant }

    /// <summary>Qualité d'un Composant : une fabrication n'engage qu'une seule qualité.</summary>
    public enum ComponentGrade { Common, Refined, Exceptional }

    /// <summary>Catalyseur thématique (optionnel, un par fabrication) qui oriente les statistiques.</summary>
    public enum CatalystKind { Metal, Energy, Bio, Crystal }

    /// <summary>D'où vient une recette : départ, aventure, repaires (bloc C) ou tournois (bloc D).</summary>
    public enum RecipeSource { Starter, Adventure, Lair, Tournament }

    public static class ComponentKinds
    {
        public const int Count = 4;
        public const int GradeCount = 3;
        public const int CatalystCount = 4;

        public static ComponentKind For(ItemType type)
        {
            switch (type)
            {
                case ItemType.Weapon: return ComponentKind.Weapon;
                case ItemType.Armor: return ComponentKind.Armor;
                case ItemType.Tool: return ComponentKind.Tool;
                default: return ComponentKind.Implant;
            }
        }

        /// <summary>Index d'un stock dans un tableau de <see cref="Count"/> × <see cref="GradeCount"/> entiers (<c>kind × 3 + grade</c>).</summary>
        public static int Index(ComponentKind kind, ComponentGrade grade) => (int)kind * GradeCount + (int)grade;
    }

    /// <summary>Étoiles d'un objet fabriqué (1 à 6) : étoile de départ selon la qualité des Composants, multiplicateur de statistiques, statistique orientée par un catalyseur.</summary>
    public static class CraftStars
    {
        public const int Min = 1, Max = 6;

        /// <summary>Étoile de départ avant le tirage −1 / 0 / +1 : Commun 2, Raffiné 4, Exceptionnel 5.</summary>
        public static int Base(ComponentGrade grade)
        {
            switch (grade)
            {
                case ComponentGrade.Refined: return 4;
                case ComponentGrade.Exceptional: return 5;
                default: return 2;
            }
        }

        /// <summary>Multiplicateur des statistiques, en % (80, 100, 115, 130, 150, 175).</summary>
        public static int Percent(int stars)
        {
            switch (Math.Max(Min, Math.Min(Max, stars)))
            {
                case 1: return 80;
                case 3: return 115;
                case 4: return 130;
                case 5: return 150;
                case 6: return 175;
                default: return 100;
            }
        }

        /// <summary>Métal → DEF, Énergie → ATQ, Biologique → PV, Cristal → VIT.</summary>
        public static RecipeStatKind Oriented(CatalystKind catalyst)
        {
            switch (catalyst)
            {
                case CatalystKind.Metal: return RecipeStatKind.Def;
                case CatalystKind.Energy: return RecipeStatKind.Atk;
                case CatalystKind.Bio: return RecipeStatKind.Hp;
                default: return RecipeStatKind.Spd;
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

    /// <summary>Ce qu'un combat gagné laisse pour l'Atelier : Composants (12 entiers, voir <see cref="ComponentKinds.Index"/>), catalyseur et recette apprise éventuels.</summary>
    public readonly struct CraftDrops
    {
        public IReadOnlyList<int> Components { get; }
        public CatalystKind? Catalyst { get; }
        public string? NewRecipe { get; }
        public CraftDrops(IReadOnlyList<int> components, CatalystKind? catalyst, string? newRecipe) { Components = components; Catalyst = catalyst; NewRecipe = newRecipe; }
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

    /// <summary>Recette de l'Atelier : Composants demandés, qualité minimale, plages de statistiques et origine.</summary>
    public sealed class RecipeDef
    {
        public string Id { get; }
        /// <summary>Clé de texte du nom : <c>craft.recipe.&lt;id&gt;</c>.</summary>
        public string NameKey => "craft.recipe." + Id;
        public ItemType Type { get; }
        public ItemRarity Rarity { get; }
        /// <summary>Composants du type de la recette.</summary>
        public int ComponentCost { get; }
        public ComponentGrade MinGrade { get; }
        public IReadOnlyList<RecipeStat> Stats { get; }
        public ItemPerk Perk { get; }
        public int PerkValue { get; }
        public RecipeSource Source { get; }

        public RecipeDef(string id, ItemType type, ItemRarity rarity, int componentCost, ComponentGrade minGrade,
                         IReadOnlyList<RecipeStat> stats, ItemPerk perk = ItemPerk.None, int perkValue = 0, RecipeSource source = RecipeSource.Adventure)
        {
            Id = id; Type = type; Rarity = rarity; ComponentCost = componentCost; MinGrade = minGrade;
            Stats = stats; Perk = perk; PerkValue = perkValue; Source = source;
        }
    }

    /// <summary>Catalogue des 12 recettes (3 par type d'objet : Commune, Rare, Épique). Les Communes sont de départ ; les autres viennent de l'aventure en attendant les repaires et les tournois.</summary>
    public static class Recipes
    {
        const RecipeStatKind Hp = RecipeStatKind.Hp, Atk = RecipeStatKind.Atk, Def = RecipeStatKind.Def, Spd = RecipeStatKind.Spd;

        static RecipeDef Common(string id, ItemType type, RecipeStatKind a)
            => new RecipeDef(id, type, ItemRarity.Common, 6, ComponentGrade.Common, new[] { new RecipeStat(a, 8, 12) }, source: RecipeSource.Starter);

        static RecipeDef Rare(string id, ItemType type, RecipeStatKind a, RecipeStatKind b, RecipeSource source = RecipeSource.Adventure)
            => new RecipeDef(id, type, ItemRarity.Rare, 8, ComponentGrade.Refined, new[] { new RecipeStat(a, 8, 14), new RecipeStat(b, 8, 14) }, source: source);

        static RecipeDef Epic(string id, ItemType type, RecipeStatKind a, RecipeStatKind b, ItemPerk perk, int perkValue, RecipeSource source = RecipeSource.Adventure)
            => new RecipeDef(id, type, ItemRarity.Epic, 10, ComponentGrade.Refined, new[] { new RecipeStat(a, 10, 16), new RecipeStat(b, 10, 16) }, perk, perkValue, source);

        public static readonly IReadOnlyList<RecipeDef> All = new[]
        {
            Common("scrap_blade", ItemType.Weapon, Atk),
            Rare("arc_baton", ItemType.Weapon, Atk, Spd),
            Epic("pulse_rifle", ItemType.Weapon, Atk, Spd, ItemPerk.CritChance, 8),
            Common("patch_vest", ItemType.Armor, Hp),
            Rare("plated_coat", ItemType.Armor, Hp, Def, RecipeSource.Lair),
            Epic("aegis_plate", ItemType.Armor, Hp, Def, ItemPerk.OpeningShield, 10, RecipeSource.Lair),
            Common("spring_boots", ItemType.Tool, Spd),
            Rare("grapple_rig", ItemType.Tool, Spd, Def, RecipeSource.Lair),
            Epic("chrono_pack", ItemType.Tool, Spd, Hp, ItemPerk.StartGauge, 15),
            Common("neural_chip", ItemType.Implant, Atk),
            Rare("synapse_coil", ItemType.Implant, Atk, Spd),
            Epic("apex_core", ItemType.Implant, Atk, Hp, ItemPerk.SkillPower, 10),
        };

        /// <summary>Recettes connues dès le début : les quatre Communes.</summary>
        public static readonly IReadOnlyList<string> Starter = All.Where(r => r.Source == RecipeSource.Starter).Select(r => r.Id).ToList();

        public static RecipeDef? Find(string id)
        {
            foreach (var r in All) if (r.Id == id) return r;
            return null;
        }
    }
}
