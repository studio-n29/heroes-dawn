#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>Tirage des étoiles et des statistiques des objets fabriqués, et des Composants laissés par les combats.</summary>
    public static class CraftingRules
    {
        /// <summary>Étoiles : étoile de départ de la qualité, puis −1 (25 %), 0 (50 %) ou +1 (25 %), bornée de 1 à 6 (un seul tirage).</summary>
        public static int RollStars(IRandom rng, ComponentGrade grade)
        {
            int roll = rng.Range(0, 100);
            int delta = roll < 25 ? -1 : roll < 75 ? 0 : 1;
            return Math.Max(CraftStars.Min, Math.Min(CraftStars.Max, CraftStars.Base(grade) + delta));
        }

        /// <summary>
        /// Chaque statistique de la recette est tirée dans sa plage puis multipliée par l'étoile (arrondi au plus proche, au moins 1).
        /// Un catalyseur ajoute la statistique orientée (plage de la première statistique de la recette) ; si la recette la contient déjà, elle est tirée dans la moitié haute de sa plage.
        /// </summary>
        public static CraftedStats RollStats(IRandom rng, RecipeDef recipe, int stars, CatalystKind? catalyst)
        {
            int percent = CraftStars.Percent(stars);
            int hp = 0, atk = 0, def = 0, spd = 0;
            bool hasOriented = false;
            RecipeStatKind? oriented = catalyst.HasValue ? CraftStars.Oriented(catalyst.Value) : (RecipeStatKind?)null;

            void Roll(RecipeStatKind stat, int min, int max)
            {
                int value = Math.Max(1, (rng.Range(min, max + 1) * percent + 50) / 100);
                switch (stat)
                {
                    case RecipeStatKind.Hp: hp = value; break;
                    case RecipeStatKind.Atk: atk = value; break;
                    case RecipeStatKind.Def: def = value; break;
                    default: spd = value; break;
                }
            }

            foreach (var s in recipe.Stats)
            {
                if (oriented.HasValue && s.Stat == oriented.Value) { hasOriented = true; Roll(s.Stat, (s.Min + s.Max) / 2, s.Max); }
                else Roll(s.Stat, s.Min, s.Max);
            }
            if (oriented.HasValue && !hasOriented && recipe.Stats.Count > 0)
                Roll(oriented.Value, recipe.Stats[0].Min, recipe.Stats[0].Max);
            return new CraftedStats(hp, atk, def, spd);
        }

        /// <summary>
        /// Composants, catalyseur et recette laissés par une victoire : un type de Composant tiré au hasard (Facile 1-2 Commun, Normal 2-3 Commun,
        /// Élite 3-4 Commun + 1-2 Raffiné et catalyseur 10 %, Boss 3-4 Raffiné + 1 Exceptionnel une fois sur deux et catalyseur 30 %),
        /// et une recette inconnue (Boss 60 %, Élite 25 %) ; sans recette inconnue, 5 Composants Commun de plus.
        /// </summary>
        public static CraftDrops RollDrops(IRandom rng, CombatRank rank, bool boss, IReadOnlyList<string> unknownRecipes)
        {
            boss |= rank == CombatRank.Boss;
            bool elite = !boss && rank == CombatRank.Elite;
            var comps = new int[ComponentKinds.Count * ComponentKinds.GradeCount];
            var kind = (ComponentKind)rng.Range(0, ComponentKinds.Count);
            int Slot(ComponentGrade g) => ComponentKinds.Index(kind, g);

            if (boss)
            {
                comps[Slot(ComponentGrade.Refined)] = rng.Range(3, 5);
                if (rng.Chance(50)) comps[Slot(ComponentGrade.Exceptional)] = 1;
            }
            else if (elite)
            {
                comps[Slot(ComponentGrade.Common)] = rng.Range(3, 5);
                comps[Slot(ComponentGrade.Refined)] = rng.Range(1, 3);
            }
            else comps[Slot(ComponentGrade.Common)] = rank == CombatRank.Normal ? rng.Range(2, 4) : rng.Range(1, 3);

            CatalystKind? catalyst = null;
            int catalystChance = boss ? 30 : elite ? 10 : 0;
            if (catalystChance > 0 && rng.Chance(catalystChance)) catalyst = (CatalystKind)rng.Range(0, ComponentKinds.CatalystCount);

            string? recipe = null;
            int recipeChance = boss ? 60 : elite ? 25 : 0;
            if (recipeChance > 0 && rng.Chance(recipeChance))
            {
                if (unknownRecipes.Count > 0) recipe = unknownRecipes[rng.Range(0, unknownRecipes.Count)];
                else comps[Slot(ComponentGrade.Common)] += 5;
            }
            return new CraftDrops(comps, catalyst, recipe);
        }
    }
}
