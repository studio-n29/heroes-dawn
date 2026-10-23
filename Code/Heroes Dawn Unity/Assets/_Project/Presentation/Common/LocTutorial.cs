#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du tutoriel : bulle de Rayner (2 lignes maximum) et astuces contextuelles.</summary>
    public static partial class Loc
    {
        static partial void ExtendTutorial(Dictionary<string, string> d)
        {
            d["tutorial.progress"] = "{0} / {1}";
            d["tutorial.ok"] = "COMPRIS";
            d["tutorial.skip"] = "Passer le tutoriel";
            d["tutorial.skip.title"] = "Passer le tutoriel ?";
            d["tutorial.skip.body"] = "Tu pourras le revoir dans Paramètres ▸ Support & aide.";
            d["tutorial.done"] = "TUTORIEL TERMINÉ — À TOI DE JOUER !";

            d["tutorial.step.Welcome.title"] = "BIENVENUE À RED HOOK";
            d["tutorial.step.Welcome.body"] = "Je suis Rayner. Voici ta section : le QG, ta base. On va faire ta première mission ensemble.";
            d["tutorial.step.OpenTeam.title"] = "TON ÉQUIPE";
            d["tutorial.step.OpenTeam.body"] = "Tu diriges l’unité à travers ton agent, mais ce sont tes recrues qui se battent. Touche l’onglet ÉQUIPE.";
            d["tutorial.step.Squad.title"] = "TA PREMIÈRE RECRUE";
            d["tutorial.step.Squad.body"] = "Tu démarres avec un seul agent. Il va t’apprendre tout le reste, puis d’autres le rejoindront.";
            d["tutorial.step.OpenSheet.title"] = "LA FICHE";
            d["tutorial.step.OpenSheet.body"] = "Touche ta recrue pour ouvrir sa fiche : niveau, histoire, compétences, objets.";
            d["tutorial.step.Train.title"] = "L’ENTRAÎNEMENT";
            d["tutorial.step.Train.body"] = "Les Crédits font monter un agent de niveau. Touche ENTRAÎNER.";
            d["tutorial.step.EquipSlot.title"] = "L’ÉQUIPEMENT";
            d["tutorial.step.EquipSlot.body"] = "Chaque agent porte des objets et des cartes. Touche le premier emplacement d’objet.";
            d["tutorial.step.EquipPick.title"] = "L’INVENTAIRE";
            d["tutorial.step.EquipPick.body"] = "Voici l’objet offert : touche-le pour l’équiper.";
            d["tutorial.step.OpenRecruit.title"] = "LE RECRUTEMENT";
            d["tutorial.step.OpenRecruit.body"] = "Les PR recrutent de nouveaux héros. Touche l’onglet BOUTIQUE.";
            d["tutorial.step.Recruit.title"] = "UN NOUVEAU HÉROS";
            d["tutorial.step.Recruit.body"] = "Recrute l’un des héros du jour, puis confirme.";
            d["tutorial.step.BackToTeam.title"] = "TON DUO";
            d["tutorial.step.BackToTeam.body"] = "Retourne sur l’onglet ÉQUIPE.";
            d["tutorial.step.Duo.title"] = "LES SYNERGIES";
            d["tutorial.step.Duo.body"] = "Deux agents de familles différentes se renforcent : une synergie s active sur leur escouade.";
            d["tutorial.step.StartMission.title"] = "LES QUAIS";
            d["tutorial.step.StartMission.body"] = "Les quais sont aux mains des pillards. Ton duo est prêt : touche MISSION.";
            d["tutorial.step.Deploy.title"] = "LA CARTE DE NEW YORK";
            d["tutorial.step.Deploy.body"] = "Voici la zone 1.1, Red Hook. Touche DÉPLOYER pour y envoyer l'escouade.";
            d["tutorial.step.Objectives.title"] = "LES OBJECTIFS";
            d["tutorial.step.Objectives.body"] = "Voici les trois objectifs de la zone. Remplis-les tous pour la sécuriser.";
            d["tutorial.step.Roll.title"] = "LE DÉ";
            d["tutorial.step.Roll.body"] = "Lance le dé pour avancer : chaque point est un point de mouvement.";
            d["tutorial.step.Move.title"] = "AVANCER";
            d["tutorial.step.Move.body"] = "Les cases cerclées sont à ta portée. Touche la case que je t’indique, puis « S’y rendre ».";
            d["tutorial.step.CombatAtb.title"] = "LE COMBAT";
            d["tutorial.step.CombatAtb.body"] = "Chaque héros avance sur la frise : à 100 %, il agit. Le rouge, c'est la zone de préparation.";
            d["tutorial.step.CombatSkill.title"] = "À TOI DE JOUER";
            d["tutorial.step.CombatSkill.body"] = "Fais glisser une carte de compétence sur un ennemi pour la lancer.";
            d["tutorial.step.Results.title"] = "LA FATIGUE";
            d["tutorial.step.Results.body"] = "Chaque combat fatigue l'équipe. Touche CONTINUER.";

            d["tutorial.hint.title"] = "ASTUCE";
            d["tutorial.hint.ok"] = "OK";
            d["tutorial.hint.auto"] = "AUTO laisse l'équipe combattre seule ; x1 / x2 / x3 change la vitesse du combat.";
            d["tutorial.hint.bonus_die"] = "Un dé bonus est disponible : il ajoute des points de mouvement à ta réserve.";
            d["tutorial.hint.end_move"] = "TERMINER met les points de mouvement restants en réserve pour le prochain lancer.";
            d["tutorial.hint.event"] = "Une case Événement : un choix à faire, avec des Crédits, de la recherche ou du moral à la clé.";
            d["tutorial.hint.minigame"] = "Un mini-jeu : réussis-le pour gagner des Crédits et des PR, et valider certains objectifs.";
            d["tutorial.hint.rest"] = "Une case Repos : l'équipe y retire de la fatigue.";
            d["tutorial.hint.defeat"] = "Une défaite fait reculer l'équipe d'une case et la fatigue davantage. Repose-la au QG.";
            d["tutorial.hint.fatigue"] = "Un héros très fatigué perd en puissance. Passe par l'Infirmerie pour le soigner.";
            d["tutorial.hint.zone_done"] = "Zone sécurisée ! Tu peux la rejouer pour des Crédits, ou ouvrir la suivante sur la carte.";
            d["tutorial.hint.recruit"] = "Trois héros sont proposés chaque jour, les Légendes sont rares. Ils coûtent des PR.";
            d["tutorial.hint.infirmary"] = "Un de tes héros est fatigué : l'Infirmerie le remet sur pied pour quelques Crédits.";
            d["tutorial.hint.timeline"] = "La frise va de 100 à 0 : à 0, c est le tour de l unité et tout le monde attend. Un ennemi qui entre en zone rouge annonce son attaque : un contrôle (Entrave, Choc, Gel) ou un critique l'interrompt.";
        }
    }
}
