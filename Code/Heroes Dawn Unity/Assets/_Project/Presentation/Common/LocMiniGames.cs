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
            d["activity.param.hits"] = "Touches à réussir";
        }
    }
}
