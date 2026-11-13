#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Textes du chapitre Sergueiv (Moscou, parcours d'Agent M) : ennemis, zones, objectifs, dialogues, fragments, archives, carte.
    /// Le français complète le dictionnaire de base (<c>ExtendMoscow</c>) ; l'anglais est fusionné dans <c>En</c> (<see cref="WithMoscowEnglish"/>).
    /// Source des répliques et des fragments : spec 2026-11-13, annexes A et B.
    /// </summary>
    public static partial class Loc
    {
        static partial void ExtendMoscow(Dictionary<string, string> d)
        {
            // --- Ennemis ---
            d["enemies.mos_guard.name"] = "Garde de la mafia";
            d["enemies.mos_smuggler.name"] = "Contrebandier";
            d["enemies.mos_enforcer.name"] = "Homme de main";
            d["enemies.mos_passeur.name"] = "LE PASSEUR";
            d["enemies.mos_passeur.phase2"] = "Les clients arrivent";
            d["enemies.mos_passeur.phase3"] = "Tout doit partir";
            d["skills.mos_guard_a1"] = "Coup de crosse";
            d["skills.mos_guard_a2"] = "Cordon de sécurité";
            d["skills.mos_smuggler_a1"] = "Tir d'avertissement";
            d["skills.mos_smuggler_a2"] = "Livraison express";
            d["skills.mos_enforcer_a1"] = "Poing ganté";
            d["skills.mos_enforcer_a2"] = "Visite de courtoisie";
            d["skills.mos_enforcer_a3"] = "Manteau doublé";
            d["skills.mos_passeur_a1"] = "Marchandage";
            d["skills.mos_passeur_a2"] = "Cargaison scellée";
            d["skills.mos_passeur_a3"] = "Livraison";
            d["skills.mos_passeur_a4"] = "Mise en demeure";
            d["skills.mos_passeur_p2_a2"] = "Clients pressés";
            d["archives.enemy.mos_guard"] = "Garde de la mafia : un cordon humain posté aux entrées. Il se couvre d'un bouclier et attire les coups.";
            d["archives.enemy.mos_smuggler"] = "Contrebandier : tireur de la filière du maglev. Il marque sa cible avant de la livrer au plomb.";
            d["archives.enemy.mos_enforcer"] = "Homme de main : l'élite du comité d'accueil. Ses visites de courtoisie secouent toute l'escouade.";
            d["archives.enemy.mos_passeur"] = "LE PASSEUR : il ne parle jamais de victimes ni d'ennemis, seulement de clients. Tout, chez lui, a un prix.";
        }

        /// <summary>Ajoute les textes anglais de Moscou au dictionnaire anglais (méthode statique sans état : sûre dans un initialiseur de champ).</summary>
        static Dictionary<string, string> WithMoscowEnglish(Dictionary<string, string> d)
        {
            // --- Enemies ---
            d["enemies.mos_guard.name"] = "Mob Guard";
            d["enemies.mos_smuggler.name"] = "Smuggler";
            d["enemies.mos_enforcer.name"] = "Enforcer";
            d["enemies.mos_passeur.name"] = "THE SMUGGLER";
            d["enemies.mos_passeur.phase2"] = "The Customers Arrive";
            d["enemies.mos_passeur.phase3"] = "Everything Must Go";
            d["skills.mos_guard_a1"] = "Rifle Butt";
            d["skills.mos_guard_a2"] = "Security Cordon";
            d["skills.mos_smuggler_a1"] = "Warning Shot";
            d["skills.mos_smuggler_a2"] = "Express Delivery";
            d["skills.mos_enforcer_a1"] = "Gloved Fist";
            d["skills.mos_enforcer_a2"] = "Courtesy Call";
            d["skills.mos_enforcer_a3"] = "Padded Coat";
            d["skills.mos_passeur_a1"] = "Haggling";
            d["skills.mos_passeur_a2"] = "Sealed Cargo";
            d["skills.mos_passeur_a3"] = "Delivery";
            d["skills.mos_passeur_a4"] = "Formal Notice";
            d["skills.mos_passeur_p2_a2"] = "Impatient Customers";
            d["archives.enemy.mos_guard"] = "Mob guard: a human cordon posted at the entrances. He raises a shield and draws the blows.";
            d["archives.enemy.mos_smuggler"] = "Smuggler: a gunman of the maglev route. He marks his target before delivering lead.";
            d["archives.enemy.mos_enforcer"] = "Enforcer: the elite of the welcoming committee. His courtesy calls rattle the whole squad.";
            d["archives.enemy.mos_passeur"] = "THE SMUGGLER: he never says victims or enemies, only customers. Everything he owns has a price.";
            return d;
        }
    }
}
