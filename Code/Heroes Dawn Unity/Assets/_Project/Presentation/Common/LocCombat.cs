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
            d["combat.wave"] = "VAGUE {0}/{1}";
            d["combat.wave_banner"] = "VAGUE {0} !";
            d["combat.axis_start"] = "100";
            d["combat.axis_end"] = "0";
            d["combat.planned"] = "PRÉPARE…";
            d["combat.planned_heavy"] = "GROSSE ATTAQUE !";
            d["combat.prepares"] = "prépare {0}";
            d["combat.interrupted"] = "INTERROMPU !";
            d["combat.synergy"] = "SYNERGIE · {0} ({1}) : {2} + {3}";
            d["combat.pushed"] = "RECUL -{0} %";
            d["combat.timeline_action"] = "ACTION";
            d["combat.turn_passed"] = "passe son tour";
            d["combat.parade"] = "PARADE !";
            d["combat.advanced"] = "ÉLAN +{0} %";
            d["combat.propelled"] = "propulsé au point d'action !";
            d["fx.pushback"] = "Recule la cible de {0} % sur la frise";
            d["fx.advance"] = "Propulse la cible de {0} % vers l'action";
            d["combat.pause"] = "II";
            d["combat.pause.title"] = "Pause";
            d["combat.pause.body"] = "Le combat est figé. Reprends quand tu es prêt.";
            d["combat.pause.resume"] = "REPRENDRE";
            d["combat.pause.abandon"] = "ABANDONNER";
            d["combat.choose_target"] = "TOUCHE UNE CIBLE · ÉCHAP = ANNULER";
            d["combat.choose_ally"] = "TOUCHE UN ALLIÉ · ÉCHAP = ANNULER";
            d["combat.waiting"] = "Les jauges se remplissent…";
            d["combat.drop_here"] = "Lâche la carte ici";
            d["combat.auto_hint"] = "AUTO [A]";
            d["combat.speed_hint"] = "{0} [S]";
        }
    }
}
