#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    /// <summary>Tirage de la qualité et des statistiques des objets fabriqués, et progression de la compétence de création.</summary>
    public static class CraftingRules
    {
        public const int MaxSkill = 10;

        // Probabilités en % (Grossier, Standard, Soigné, Supérieur, Chef-d'œuvre) par tranche d'écart compétence − difficulté.
        static readonly int[] EdgeAtMost3 = { 60, 35, 5, 0, 0 };
        static readonly int[] EdgeMinus2To1 = { 35, 50, 13, 2, 0 };
        static readonly int[] EdgeZero = { 15, 50, 25, 9, 1 };
        static readonly int[] EdgePlus1To2 = { 5, 35, 35, 20, 5 };
        static readonly int[] EdgePlus3To4 = { 0, 15, 40, 30, 15 };
        static readonly int[] EdgeAtLeast5 = { 0, 5, 30, 40, 25 };

        /// <summary>Probabilités (somme 100) des cinq qualités, dans l'ordre de <see cref="CraftQuality"/>.</summary>
        public static IReadOnlyList<int> QualityOdds(int skill, int difficulty)
        {
            int edge = skill - difficulty;
            if (edge <= -3) return EdgeAtMost3;
            if (edge <= -1) return EdgeMinus2To1;
            if (edge == 0) return EdgeZero;
            if (edge <= 2) return EdgePlus1To2;
            if (edge <= 4) return EdgePlus3To4;
            return EdgeAtLeast5;
        }

        public static CraftQuality RollQuality(IRandom rng, int skill, int difficulty)
        {
            var odds = QualityOdds(skill, difficulty);
            int roll = rng.Range(0, 100), cumulative = 0;
            for (int q = 0; q < odds.Count; q++)
            {
                cumulative += odds[q];
                if (roll < cumulative) return (CraftQuality)q;
            }
            return CraftQuality.Standard;
        }

        /// <summary>Chaque stat de la recette est tirée dans sa fourchette puis multipliée par la qualité (arrondi au plus proche, au moins 1).</summary>
        public static CraftedStats RollStats(IRandom rng, RecipeDef recipe, CraftQuality quality)
        {
            int percent = CraftQualities.Percent(quality);
            int hp = 0, atk = 0, def = 0, spd = 0;
            foreach (var s in recipe.Stats)
            {
                int value = Math.Max(1, (rng.Range(s.Min, s.Max + 1) * percent + 50) / 100);
                switch (s.Stat)
                {
                    case RecipeStatKind.Hp: hp = value; break;
                    case RecipeStatKind.Atk: atk = value; break;
                    case RecipeStatKind.Def: def = value; break;
                    default: spd = value; break;
                }
            }
            return new CraftedStats(hp, atk, def, spd);
        }

        /// <summary>Expérience totale nécessaire pour atteindre un niveau (40 × n pour passer du niveau n au suivant).</summary>
        public static int XpForLevel(int level) => 20 * level * (level - 1);

        public static int SkillLevel(int xp)
        {
            int level = 1;
            while (level < MaxSkill && xp >= XpForLevel(level + 1)) level++;
            return level;
        }

        /// <summary>Expérience gagnée par fabrication : pleine tant que la recette n'est pas trop facile (écart ≤ 2), sinon 5.</summary>
        public static int XpGain(int skill, int difficulty) => skill - difficulty <= 2 ? 10 + 4 * difficulty : 5;
    }
}
