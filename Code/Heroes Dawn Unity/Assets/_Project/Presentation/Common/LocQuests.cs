#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du suivi des quêtes du QG : quêtes principales (zones) et secondaires.</summary>
    public static partial class Loc
    {
        static partial void ExtendQuests(Dictionary<string, string> d)
        {
            d["qg.fil.open"] = "SUIVI DES QUÊTES ›";
            d["quests.title"] = "SUIVI DES QUÊTES";
            d["quests.main"] = "QUÊTES PRINCIPALES · BROOKLYN";
            d["quests.side"] = "QUÊTES SECONDAIRES";
            d["quests.side.empty"] = "Aucune quête secondaire pour l'instant. Avance dans la campagne.";
            d["quests.locked"] = "Termine la zone précédente pour la débloquer.";
            d["quests.main.done"] = "Zone sécurisée.";
            d["quests.state.active"] = "EN COURS";
            d["quests.state.done"] = "TERMINÉE";
            d["quests.state.locked"] = "VERROUILLÉE";
            d["quests.claim"] = "RÉCLAMER";
            d["quests.claimed"] = "Récompense : {0}";

            d["quest.docks.title"] = "Nettoyer les quais";
            d["quest.docks.desc"] = "Gagne 3 combats : les dockers de Red Hook veulent retrouver leur quartier.";
            d["quest.reinforcements.title"] = "Des renforts pour Rayner";
            d["quest.reinforcements.desc"] = "Recrute un nouvel agent : l'Aube manque de bras.";
            d["quest.drills.title"] = "À l'entraînement";
            d["quest.drills.desc"] = "Entraîne tes agents de 2 niveaux au total.";
            d["quest.hostage_aid.title"] = "Aide aux otages libérés";
            d["quest.hostage_aid.desc"] = "Gagne 8 combats : Brownsville a besoin de sécurité pour se relever.";
            d["quest.coney_watch.title"] = "Veille sur Coney Island";
            d["quest.coney_watch.desc"] = "Sécurise 2 zones de plus pour rassurer les rescapés.";
            d["quest.bridge_shield.title"] = "Le bouclier du pont";
            d["quest.bridge_shield.desc"] = "Gagne 15 combats : l'AMIF ne doit plus oser s'approcher de Brooklyn.";
        }
    }
}
