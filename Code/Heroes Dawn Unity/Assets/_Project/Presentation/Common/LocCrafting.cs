#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes de l'Atelier de fabrication : recettes, étoiles, Composants, catalyseurs.</summary>
    public static partial class Loc
    {
        static partial void ExtendCrafting(Dictionary<string, string> d)
        {
            d["craft.recipe.scrap_blade"] = "Lame de fortune";
            d["craft.recipe.arc_baton"] = "Matraque à arc";
            d["craft.recipe.pulse_rifle"] = "Fusil à impulsion";
            d["craft.recipe.patch_vest"] = "Gilet rapiécé";
            d["craft.recipe.plated_coat"] = "Manteau blindé";
            d["craft.recipe.aegis_plate"] = "Plastron égide";
            d["craft.recipe.spring_boots"] = "Bottes à ressorts";
            d["craft.recipe.grapple_rig"] = "Harnais grappin";
            d["craft.recipe.chrono_pack"] = "Sac chrono";
            d["craft.recipe.neural_chip"] = "Puce neurale";
            d["craft.recipe.synapse_coil"] = "Bobine synaptique";
            d["craft.recipe.apex_core"] = "Noyau d'Apex";
            d["craft.name"] = "{0} ({1})";
            d["lab.workshop"] = "Atelier";
            d["lab.odds"] = "{0} {1} %";
            d["lab.craft"] = "Fabriquer";
            d["lab.crafted"] = "Fabriqué : {0} · {1}";
            d["lab.recipe_sub"] = "Coût : {0} · qualité min. {1} · départ {2}";
            d["errors.not_enough_components"] = "Il manque des Composants.";
            d["errors.grade_too_low"] = "Ces Composants sont de trop faible qualité.";
            d["errors.no_catalyst"] = "Tu n'as pas ce catalyseur.";
            for (int s = 1; s <= 6; s++) d["craft.stars." + s] = new string('★', s);
            string[] kinds = { "arme", "armure", "outil", "implant" };
            string[] kindNames = { "Weapon", "Armor", "Tool", "Implant" };
            string[] grades = { "Common", "Refined", "Exceptional" };
            string[] gradeNames = { "Commun", "Raffiné", "Exceptionnel" };
            for (int k = 0; k < kinds.Length; k++)
                for (int g = 0; g < grades.Length; g++)
                    d["component." + kindNames[k] + "." + grades[g]] = "Composants d'" + kinds[k] + " (" + gradeNames[g] + ")";
            d["catalyst.Metal"] = "Catalyseur Métal (DEF)";
            d["catalyst.Energy"] = "Catalyseur Énergie (ATQ)";
            d["catalyst.Bio"] = "Catalyseur Biologique (PV)";
            d["catalyst.Crystal"] = "Catalyseur Cristal (VIT)";
            d["grade.Common"] = "Commun";
            d["grade.Refined"] = "Raffiné";
            d["grade.Exceptional"] = "Exceptionnel";
            d["combat.craft_drop"] = "Composants : {0}";
            d["combat.craft_catalyst"] = "Catalyseur : {0}";
            d["combat.craft_recipe"] = "Recette apprise : {0}";
        }
    }
}
