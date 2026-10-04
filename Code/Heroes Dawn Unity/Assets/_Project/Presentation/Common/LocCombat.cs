#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes de la vue de combat refondue (M65/M66).</summary>
    public static partial class Loc
    {
        static partial void ExtendCombat(Dictionary<string, string> d)
        {
            d["status.Anchor"] = "Ancrage";
            d["combat.combo_sync"] = "COMBO SYNC · {0}";
            d["combat.skip"] = "DÉPASSER · {0}";
            d["combat.skip_pick"] = "TOUCHE L'ENNEMI À COUPER · ÉCHAP = ANNULER";
            d["combat.skip_missing"] = "Il manque {0} de Temps d'équipe.";
            d["combat.skip_nobody"] = "Aucun allié à faire avancer.";
            d["combat.bank"] = "TEMPS {0}/{1}";
            d["combat.planned"] = "PRÉPARE…";
            d["combat.planned_heavy"] = "GROSSE ATTAQUE !";
            d["combat.prepares"] = "prépare {0}";
            d["combat.interrupted"] = "INTERROMPU !";
            d["combat.pushed"] = "RECUL -{0} %";
            d["combat.timeskip"] = "DÉPASSEMENT !";
            d["combat.pause"] = "II";
            d["combat.pause.title"] = "Pause";
            d["combat.pause.body"] = "Le combat est figé. Reprends quand tu es prêt.";
            d["combat.pause.resume"] = "REPRENDRE";
            d["combat.pause.abandon"] = "ABANDONNER";
            d["combat.choose_target"] = "TOUCHE UNE CIBLE · ÉCHAP = ANNULER";
            d["combat.choose_ally"] = "TOUCHE UN ALLIÉ · ÉCHAP = ANNULER";
            d["combat.waiting"] = "Les jauges se remplissent…";
            d["combat.auto_hint"] = "AUTO [A]";
            d["combat.speed_hint"] = "{0} [S]";
        }
    }
}
