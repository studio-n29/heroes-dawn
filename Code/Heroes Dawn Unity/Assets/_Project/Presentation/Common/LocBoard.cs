#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du plateau agrandi : fiche d'information de la case visée, « S'y rendre », navigation.</summary>
    public static partial class Loc
    {
        static partial void ExtendBoard(Dictionary<string, string> d)
        {
            d["board.hint.choose"] = "Touche une case en surbrillance : sa fiche s'ouvre et « S'y rendre » valide. Flèches ou boutons CASE pour naviguer, T pour terminer.";
            d["cue.combat"] = "COMBAT !";
            d["cue.event"] = "ÉVÉNEMENT !";
            d["cue.minigame"] = "DÉFI !";
            d["cue.rest"] = "REPOS !";
            d["cue.bang"] = "BANG !";
            d["cue.ouch"] = "OUTCH !";
            d["board.preview.go"] = "S'Y RENDRE";
            d["board.preview.cancel"] = "ANNULER";
            d["board.preview.cost"] = "Coût : {0} PM · il t'en restera {1}.";
            d["board.preview.cost_all"] = "Coût : {0} PM · tous tes PM.";
            d["board.preview.stop_fog"] = "Arrêt forcé : cette case ouvre le brouillard. Les PM restants te permettent de repartir.";
            d["board.preview.stop_combat"] = "Arrêt forcé : le combat se déclenche en arrivant.";
            d["board.preview.fog_title"] = "ZONE INCONNUE";
            d["board.preview.fog_body"] = "Case sous le brouillard ({0} pas). Le déplacement s'arrête d'abord en {1}, qui ouvre la zone.";
            d["board.preview.fog_detail"] = "Aller en {0} : {1} PM.";
            d["board.preview.spent"] = "Déjà résolue : rien ne s'y passera.";
            d["board.case.Entry.title"] = "ENTRÉE DE LA ZONE";
            d["board.case.Entry.body"] = "Le point de départ. Rien ne s'y passe ; le bouton « Retour au QG » ramène l'équipe à la base.";
            d["board.case.Empty.title"] = "CARREFOUR";
            d["board.case.Empty.body"] = "Une case vide : aucun effet, on la traverse librement.";
            d["board.case.Combat.title"] = "COMBAT · {0}";
            d["board.case.Combat.body"] = "Un combat t'attend. On ne peut pas traverser cette case : le déplacement s'y arrête.";
            d["board.case.MiniGame.title"] = "MINI-JEU";
            d["board.case.MiniGame.body"] = "Un défi si tu t'y arrêtes. Tu peux l'ignorer, mais sans récompense.";
            d["board.case.Narrative.title"] = "ÉVÉNEMENT";
            d["board.case.Narrative.body"] = "Une situation et plusieurs choix si tu t'y arrêtes.";
            d["board.case.Resource.title"] = "RESSOURCE";
            d["board.case.Resource.body"] = "Un gain immédiat, même en passant dessus (une seule fois).";
            d["board.case.Rest.title"] = "REPOS";
            d["board.case.Rest.body"] = "Si tu t'y arrêtes : -30 % de fatigue pour toute l'équipe (une fois par ouverture de zone).";
            d["board.case.objective"] = "Case d'objectif de la zone.";
            d["board.nav.prev"] = "< CASE";
            d["board.nav.next"] = "CASE >";
        }
    }
}
