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
            d["combat.combo_sync"] = "COMBO PROCHE";
            d["combat.avatar"] = "AVATAR";
            d["combat.avatar_ready"] = "PRÊT";
            d["combat.avatar_pick"] = "TOUCHE UNE CIBLE POUR {0} · ÉCHAP = ANNULER";
            d["combat.wave"] = "VAGUE {0}/{1}";
            d["combat.wave_banner"] = "VAGUE {0} !";
            d["combat.axis_start"] = "100";
            d["combat.axis_end"] = "0";
            d["avatar.avatar_strike.name"] = "FRAPPE"; d["avatar.avatar_strike.desc"] = "Dégâts + Vulnérable";
            d["avatar.avatar_mend.name"] = "SOIN"; d["avatar.avatar_mend.desc"] = "Soigne toute l équipe";
            d["avatar.avatar_barrier.name"] = "BARRIÈRE"; d["avatar.avatar_barrier.desc"] = "Bouclier + Ancrage";
            d["avatar.avatar_rally.name"] = "ÉLAN"; d["avatar.avatar_rally.desc"] = "Hâte + Régénération";
            d["avatar.avatar_rewind.name"] = "RECUL"; d["avatar.avatar_rewind.desc"] = "Repousse les ennemis sur la frise";
            d["combat.planned"] = "PRÉPARE…";
            d["combat.planned_heavy"] = "GROSSE ATTAQUE !";
            d["combat.prepares"] = "prépare {0}";
            d["combat.interrupted"] = "INTERROMPU !";
            d["combat.pushed"] = "RECUL -{0} %";
            d["combat.timeline_action"] = "ACTION";
            d["combat.turn_passed"] = "passe son tour";
            d["combat.parade"] = "PARADE !";
            d["combat.encounter_combo"] = "COMBO POSSIBLE !";
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
            d["combat.auto_hint"] = "AUTO [A]";
            d["combat.speed_hint"] = "{0} [S]";
        }
    }
}
