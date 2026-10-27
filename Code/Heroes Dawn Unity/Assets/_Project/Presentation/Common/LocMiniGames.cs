#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes des mini-jeux 2D du lot H (0.14.0) : fenêtre commune, puis un bloc par jeu.</summary>
    public static partial class Loc
    {
        static partial void ExtendMiniGames(Dictionary<string, string> d)
        {
            d["mg2d.start"] = "TOUCHE POUR COMMENCER";
            d["mg2d.abandon"] = "ABANDONNER";
            d["mg2d.time"] = "{0} s";
            d["mg2d.won"] = "<size=140%>RÉUSSI !</size>";
            d["mg2d.lost"] = "<size=140%>ÉCHEC…</size>";
            d["activity.param.notes"] = "Notes";
            d["activity.param.lanes"] = "Pistes";
            d["activity.param.lives"] = "Vies";
            d["activity.param.speed_percent"] = "Vitesse (%)";

            // ---- Le rythme
            d["activities.rhythm.name"] = "Le rythme";
            d["activities.rhythm.desc"] = "Une séquence d'ennemis arrive : touche la bonne piste au bon moment pour les vaincre.";
            d["mg2d.rhythm.instructions"] = "Les ennemis arrivent sur les pistes : touche la piste quand ils passent la ligne. Les ratés coûtent une vie.";
            d["mg2d.rhythm.score"] = "Notes {0} / {1}";

            // ---- Tir franc
            d["activities.freethrow.name"] = "Tir franc";
            d["activities.freethrow.desc"] = "Maintiens pour remplir la jauge de puissance, relâche quand elle atteint la cible.";
            d["mg2d.free_throw.instructions"] = "Maintiens pour remplir la jauge, relâche quand le curseur est dans la zone verte : plus la cible est loin, plus il faut de puissance.";
            d["mg2d.freethrow.score"] = "Touches {0} / {1}";
            d["activity.param.throws"] = "Lancers";

            // ---- Caisse au choix
            d["activities.cratepick.name"] = "Caisse au choix";
            d["activities.cratepick.desc"] = "Trois caisses scellées : retiens celle qui brille, puis frappe-la au bon moment.";
            d["mg2d.crate_pick.instructions"] = "Retiens la caisse qui brille. Maintiens pour charger, relâche quand le curseur est au-dessus de la bonne caisse. Une erreur coûte une vie.";
            d["mg2d.cratepick.score"] = "Caisses {0} / {1}";
            d["activity.param.rounds"] = "Manches";

            // ---- Analyse ADN
            d["activities.dna.name"] = "Analyse ADN";
            d["activities.dna.desc"] = "Un brin d'ADN est sous le microscope : retire les molécules parasites sans toucher les nœuds sains.";
            d["mg2d.dna_analysis.instructions"] = "Touche les molécules vertes qui pulsent pour les retirer. Toucher un nœud sain coûte une vie.";
            d["mg2d.dna.score"] = "Parasites {0} / {1}";
            d["activity.param.parasites"] = "Parasites";

            // ---- Engrenage
            d["activities.gears.name"] = "Engrenage";
            d["activities.gears.desc"] = "Plusieurs engrenages sont bloqués : choisis le sens de rotation de chacun pour remettre tout le mécanisme en marche.";
            d["mg2d.gears.instructions"] = "Touche un engrenage pour inverser son sens. Deux engrenages en contact tournent en sens contraires. Remets tout le mécanisme en marche depuis le moteur doré avant d'épuiser tes coups.";
            d["mg2d.gears.score"] = "Engrenages {0} / {1}  ·  Coups {2}";
            d["activity.param.gears"] = "Engrenages";
            d["activity.param.moves"] = "Coups";

            // ---- Ingénierie de secours
            d["activities.rescue.name"] = "Ingénierie de secours";
            d["activities.rescue.desc"] = "Il faut fabriquer un outil pour se sortir de là : lis le manuel et assemble les pièces dans le bon ordre.";
            d["mg2d.rescue_engineering.instructions"] = "Lis le manuel, choisis les pièces dans l'ordre et valide. Les pièces inutiles sont signalées. Un mauvais assemblage coûte un essai.";
            d["mg2d.rescue.score"] = "Pièces {0} / {1}";
            d["mg2d.rescue.manual"] = "MANUEL";
            d["mg2d.rescue.clue.first"] = "{0} se place en premier.";
            d["mg2d.rescue.clue.last"] = "{0} se place en dernier.";
            d["mg2d.rescue.clue.after"] = "{0} se place juste après {1}.";
            d["mg2d.rescue.clue.unused"] = "{0} est inutile : laisse-la de côté.";
            d["mg2d.rescue.undo"] = "ANNULER";
            d["mg2d.rescue.submit"] = "VALIDER";
            d["mg2d.rescue.wrong"] = "Raté : {0} pièce(s) bien placée(s) sur {1}.";
            d["mg2d.rescue.part.tube"] = "Tube";
            d["mg2d.rescue.part.spring"] = "Ressort";
            d["mg2d.rescue.part.handle"] = "Poignée";
            d["mg2d.rescue.part.hook"] = "Crochet";
            d["mg2d.rescue.part.rope"] = "Corde";
            d["mg2d.rescue.part.lens"] = "Lentille";
            d["mg2d.rescue.part.battery"] = "Pile";
            d["mg2d.rescue.part.wire"] = "Fil";
            d["mg2d.rescue.part.blade"] = "Lame";
            d["mg2d.rescue.part.magnet"] = "Aimant";
            d["mg2d.rescue.part.pulley"] = "Poulie";
            d["mg2d.rescue.part.plank"] = "Planche";
            d["mg2d.rescue.part.bolt"] = "Boulon";
            d["mg2d.rescue.part.cloth"] = "Chiffon";
            d["activity.param.parts"] = "Pièces";
            d["activity.param.distractors"] = "Pièces en trop";
            d["activity.param.attempts"] = "Essais";

            // ---- Infiltration
            d["activities.infiltration.name"] = "Infiltration";
            d["activities.infiltration.desc"] = "Infiltre-toi dans le camp : évite le champ de vision des gardes et rejoins la sortie.";
            d["mg2d.infiltration.instructions"] = "Glisse le doigt pour te déplacer. Les cônes jaunes sont le champ de vision des gardes : reste hors de leur vue et rejoins l'anneau vert. Être repéré trop longtemps te renvoie au départ et coûte une vie.";
            d["mg2d.infiltration.score"] = "Alerte {0} %";
            d["activity.param.guards"] = "Gardes";
            d["activity.param.detect_ms"] = "Détection (ms)";

            // ---- Survie en neige (vue de dessus)
            d["mg2d.snow_survival.instructions"] = "Glisse le doigt pour avancer. La neige est rapide mais froide, l'herbe haute et le sable ralentissent. Recharge ta jauge près d'un totem et trouve l'objet signalé par la flèche dorée.";
            d["mg2d.survival.score"] = "Survie {0} %";
            d["activity.param.hits"] = "Touches à réussir";
        }
    }
}
