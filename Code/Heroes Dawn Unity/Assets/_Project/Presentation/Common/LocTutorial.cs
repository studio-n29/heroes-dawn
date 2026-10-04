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
            d["tutorial.step.OpenTeam.body"] = "Une mission se joue à quatre. Touche l'onglet ÉQUIPE.";
            d["tutorial.step.Squad.title"] = "L'ESCOUADE";
            d["tutorial.step.Squad.body"] = "Voici tes quatre héros, prêts à partir. Plus tard, tu pourras les changer ici.";
            d["tutorial.step.StartMission.title"] = "LES QUAIS";
            d["tutorial.step.StartMission.body"] = "Les quais sont aux mains des pillards. Touche MISSION.";
            d["tutorial.step.Deploy.title"] = "LA CARTE DE NEW YORK";
            d["tutorial.step.Deploy.body"] = "Voici la zone 1.1, Red Hook. Touche DÉPLOYER pour y envoyer l'escouade.";
            d["tutorial.step.Objectives.title"] = "LES OBJECTIFS";
            d["tutorial.step.Objectives.body"] = "Voici les trois objectifs de la zone. Remplis-les tous pour la sécuriser.";
            d["tutorial.step.Roll.title"] = "LE DÉ";
            d["tutorial.step.Roll.body"] = "Lance le dé pour avancer : chaque point est un point de mouvement.";
            d["tutorial.step.Move.title"] = "AVANCER";
            d["tutorial.step.Move.body"] = "Les cases cerclées sont à ta portée. Touche celle que je t'indique.";
            d["tutorial.step.CombatAtb.title"] = "LE COMBAT";
            d["tutorial.step.CombatAtb.body"] = "Chaque héros avance sur la frise : à 100 %, il agit. Le rouge, c'est la zone de préparation.";
            d["tutorial.step.CombatSkill.title"] = "À TOI DE JOUER";
            d["tutorial.step.CombatSkill.body"] = "Choisis une compétence, puis touche un ennemi pour la lancer.";
            d["tutorial.step.Results.title"] = "LA FATIGUE";
            d["tutorial.step.Results.body"] = "Chaque combat fatigue l'équipe. Touche CONTINUER.";

            d["tutorial.hint.title"] = "ASTUCE";
            d["tutorial.hint.ok"] = "OK";
            d["tutorial.hint.combo"] = "Deux héros de familles qui se complètent peuvent lancer un COMBO : touche le bouton rouge quand il apparaît.";
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
            d["tutorial.hint.timeline"] = "La frise montre qui agit quand. Un ennemi qui entre en zone rouge annonce son attaque : un contrôle (Entrave, Choc, Gel) ou un critique l'interrompt.";
            d["tutorial.hint.timebank"] = "Le Temps d'équipe se dépense : DÉPASSER fait agir un allié tout de suite et coupe l'action d'un ennemi ; COMBO SYNC rattrape un partenaire en retard.";
        }
    }
}
