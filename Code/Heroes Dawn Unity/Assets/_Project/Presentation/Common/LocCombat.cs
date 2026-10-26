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
            d["combat.team_effect"] = "ÉQUIPE · {0}";
            d["link.level"] = "lien niv. {0}";
            d["team_effect.Quartet.name"] = "Quatuor";
            d["team_effect.Quartet.text"] = "4 familles différentes, +10 % de critique";
            d["team_effect.Trinity.name"] = "Trinité";
            d["team_effect.Trinity.text"] = "tank, dégâts et soutien, +1 énergie par pas";
            d["team_effect.DoubleFront.name"] = "Double front";
            d["team_effect.DoubleFront.text"] = "2 tanks, bouclier d ouverture de 10 % des PV";
            d["team_effect.Spark.name"] = "Étincelle";
            d["team_effect.Spark.text"] = "2 agents de contrôle, +20 énergie au départ";
            d["team_effect.Quartet.short"] = "+10 % critique";
            d["team_effect.Trinity.short"] = "+1 énergie / pas";
            d["team_effect.DoubleFront.short"] = "Bouclier 10 % PV";
            d["team_effect.Spark.short"] = "+20 énergie";
            d["combat.bonus_more"] = "+{0} AUTRES";
            d["combat.bonus_more_sub"] = "voir le journal";
            d["combat.pushed"] = "RECUL -{0} %";
            d["combat.timeline_action"] = "ACTION";
            d["combat.turn_passed"] = "passe son tour";
            d["combat.parade"] = "PARADE !";
            d["combat.advanced"] = "ÉLAN +{0} %";
            d["combat.propelled"] = "propulsé au point d'action !";
            d["fx.damage_raw"] = "Dégâts {0}"; d["fx.heal_raw"] = "Soin {0} PV"; d["fx.shield_raw"] = "Bouclier {0}";
            d["fx.pushback"] = "Recule la cible de {0} % sur la frise";
            d["fx.advance"] = "Propulse la cible de {0} % vers l'action";
            d["fx.lifesteal"] = "Vol de vie {0} %";
            d["fx.purge"] = "Purge ({0})";
            d["fx.purge_all"] = "Purge totale";
            d["fx.bounce"] = "Rebond {0} %";
            d["fx.execute"] = "Exécution +{0} % sous {1} % PV";
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
