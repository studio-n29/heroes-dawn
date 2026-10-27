#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes de la fabrication au labo (lot E) : recettes, qualités, scraps.</summary>
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
            d["craft.quality.rough"] = "Grossier";
            d["craft.quality.standard"] = "Standard";
            d["craft.quality.fine"] = "Soigné";
            d["craft.quality.superior"] = "Supérieur";
            d["craft.quality.masterwork"] = "Chef-d'œuvre";
            d["craft.name"] = "{0} ({1})";
            d["scrap.Weapon"] = "Scraps d'arme";
            d["scrap.Armor"] = "Scraps d'armure";
            d["scrap.Tool"] = "Scraps d'outil";
            d["scrap.Implant"] = "Scraps d'implant";
            d["scrap.Core"] = "Noyaux";
            d["errors.not_enough_scraps"] = "Il manque des scraps.";
        }
    }
}
