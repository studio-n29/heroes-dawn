#nullable enable
using System;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une recette de la liste centrale de l'Atelier : vignette, nom, « type · rareté ».</summary>
    public sealed class LabRecipeData
    {
        public string Name = "";
        public string Sub = "";
        public Sprite? Art;
        public bool Selected;
    }

    /// <summary>Une statistique de la recette choisie : icône du kit (attack, defense, health, speed, star) et texte (« ATQ +8–12 % »).</summary>
    public sealed class LabStatData
    {
        public string Icon = "star";
        public string Text = "";
    }

    /// <summary>Un composant requis : vignette, nom, quantité possédée et requise.</summary>
    public sealed class LabComponentData
    {
        public Sprite? Art;
        public string Name = "";
        public string Sub = "";
        public int Have;
        public int Need;
        public bool Enough => Have >= Need;
    }

    /// <summary>Tout ce que l'écran de l'Atelier affiche : filtres, recettes, détail de la recette choisie, stock. Textes déjà localisés.</summary>
    public sealed class LabData
    {
        public string FiltersTitle = "", TypeTitle = "", GradeTitle = "", RecipesTitle = "", StockTitle = "", StockText = "", RequiredTitle = "", CraftLabel = "";
        public FilterGroupData[] Filters = Array.Empty<FilterGroupData>();
        public LabRecipeData[] Recipes = Array.Empty<LabRecipeData>();

        /// <summary>Faux : aucune recette ne correspond aux filtres (le détail n'affiche que <see cref="Name"/>).</summary>
        public bool HasSelection;
        public string Name = "", Sub = "", Description = "", Info = "", State = "", CraftReason = "";
        public Sprite? Art;
        public LabStatData[] Stats = Array.Empty<LabStatData>();
        public LabComponentData[] Components = Array.Empty<LabComponentData>();
        public bool CanCraft;
    }

    /// <summary>
    /// Vue détaillée de l'Atelier (colonnes filtres, recettes, détail). Optionnelle : le présentateur ne l'alimente que si la vue l'implémente,
    /// le contrat <see cref="IListPageView"/> restant seul à servir les doublures de test.
    /// </summary>
    public interface ILabDetailView
    {
        void ShowLab(LabData data);
    }

    /// <summary>Illustrations de l'Atelier : objets des recettes et composants (extension graphique, Resources/UI/AubeTactique/Ext/Items).</summary>
    public static class LabArt
    {
        /// <summary>Illustration d'une recette ; null si le fichier manque.</summary>
        public static Sprite? Recipe(string recipeId)
        {
            string? file;
            switch (recipeId)
            {
                case "scrap_blade": file = "engineering-blade"; break;
                case "arc_baton": file = "engineering-handle"; break;
                case "pulse_rifle": file = "energy-cell"; break;
                case "patch_vest": file = "kevlar_vest"; break;
                case "plated_coat": file = "reactive_plating"; break;
                case "aegis_plate": file = "riot_shield"; break;
                case "spring_boots": file = "running_shoes"; break;
                case "grapple_rig": file = "engineering-hook"; break;
                case "chrono_pack": file = "time_shard"; break;
                case "neural_chip": file = "chip"; break;
                case "synapse_coil": file = "engineering-wire"; break;
                case "apex_core": file = "phoenix_core"; break;
                default: file = null; break;
            }
            return file != null ? TacticalArt.Ext("Items", file) : null;
        }

        /// <summary>Vignette d'un type de Composants (arme, armure, outil, implant).</summary>
        public static Sprite? Component(ComponentKind kind)
        {
            switch (kind)
            {
                case ComponentKind.Weapon: return TacticalArt.Ext("Items", "weapon-scrap");
                case ComponentKind.Armor: return TacticalArt.Ext("Items", "armor-scrap");
                case ComponentKind.Tool: return TacticalArt.Ext("Items", "tool-scrap");
                default: return TacticalArt.Ext("Items", "component-crate");
            }
        }
    }
}
