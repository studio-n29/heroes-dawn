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
            // Liens : thèmes des couples de rôles et bonus de paire (lot D)
            d["link.theme.frontline_frontline"] = "Double rempart";
            d["link.theme.frontline_attacker"] = "Rempart et lame";
            d["link.theme.frontline_controller"] = "Étau";
            d["link.theme.frontline_support"] = "Veilleurs";
            d["link.theme.frontline_ranged"] = "Garde et regard";
            d["link.theme.attacker_attacker"] = "Double tranchant";
            d["link.theme.attacker_controller"] = "Marteau et étau";
            d["link.theme.attacker_support"] = "Lame soutenue";
            d["link.theme.attacker_ranged"] = "Tir croisé";
            d["link.theme.controller_controller"] = "Tisseurs";
            d["link.theme.controller_support"] = "Sérénité";
            d["link.theme.controller_ranged"] = "Piège";
            d["link.theme.support_support"] = "Cercle de soin";
            d["link.theme.support_ranged"] = "Appui précis";
            d["link.theme.ranged_ranged"] = "Double regard";
            d["link.bonus.HpPercent"] = "PV +{0} %";
            d["link.bonus.AtkPercent"] = "ATQ +{0} %";
            d["link.bonus.DefPercent"] = "DEF +{0} %";
            d["link.bonus.SpdPercent"] = "VIT +{0} %";
            d["link.bonus.CritPercent"] = "Critique +{0} %";
            d["link.bonus.OpeningShieldPercent"] = "Bouclier d'ouverture +{0} %";
            d["link.bonus.StartGaugePercent"] = "Jauge de départ +{0} %";
            d["link.bonus.StartEnergy"] = "Énergie de départ +{0}";
            d["link.bonus.EnergyPerStep"] = "Énergie +{0} par pas";
            d["link.level"] = "lien niv. {0}";
            d["team.pair_line"] = "{0} ({1} + {2}, niv. {3}) : {4}";
            d["combat.pair"] = "LIEN · {0} ({1} + {2}, niv. {3}) : {4}";
            d["link.tier_line"] = "Niv. {0} · {1}";
            d["affinity.next_wins"] = "encore {0} victoire(s)";
            d["affinity.max"] = "lien au maximum";
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
            d["combat.executed"] = "EXÉCUTION !";
            d["combat.purged"] = "PURGE";
            d["fx.lifesteal"] = "Vol de vie {0} %";
            d["fx.purge"] = "Purge ({0})";
            d["fx.purge_all"] = "Purge totale";
            d["fx.bounce"] = "Rebond {0} %";
            d["fx.execute"] = "Exécution +{0} % sous {1} % PV";
            // Conditions, modificateurs et ciblage fin des effets (plan P1)
            d["fx.conditional"] = "Si {0} : {1}";
            d["fx.mod_flat"] = "{0} si {1}";
            d["fx.mod_always"] = "{0}";
            d["fx.mod_scaled"] = "{0} par {1} (max {2})";
            d["fx.bonus_pct"] = "{0:+0;-0;0} %";
            d["fx.bonus_damage_raw"] = "{0:+0;-0;0} de dégâts";
            d["fx.bonus_heal_raw"] = "{0:+0;-0;0} PV de soin";
            d["fx.bonus_shield_raw"] = "{0:+0;-0;0} de bouclier";
            d["cond.TargetHasStatus"] = "la cible subit {0}";
            d["cond.TargetLacksStatus"] = "la cible ne subit pas {0}";
            d["cond.TargetHpBelow"] = "la cible est sous {1} % PV";
            d["cond.TargetHpAbove"] = "la cible est au-dessus de {1} % PV";
            d["cond.CasterHasStatus"] = "le lanceur a {0}";
            d["cond.CasterHpBelow"] = "le lanceur est sous {1} % PV";
            d["cond.AnyAllyHasStatus"] = "un allié a {0}";
            d["cond.TargetStatusCountAtLeast"] = "la cible a {1} statut(s) ou plus";
            d["cond.TargetPlannedAction"] = "la cible prépare une action";
            d["cond.EnemiesAliveAtLeast"] = "{1} ennemi(s) ou plus en vie";
            d["cond.EnemiesAliveAtMost"] = "{1} ennemi(s) ou moins en vie";
            d["scale.TargetStatusCount"] = "statut sur la cible";
            d["scale.EnemiesAlive"] = "ennemi en vie";
            d["scale.AlliesAlive"] = "allié en vie";
            d["scale.TargetMissingHpTenths"] = "10 % de PV manquants de la cible";
            d["scale.CasterMissingHpTenths"] = "10 % de PV manquants du lanceur";
            d["scale.EquippedArchetypeCards"] = "carte {0} équipée dans l'escouade";
            // Archétypes, réactions armées, Consomme et Propage (plan P2-P4)
            d["archetype.Burn"] = "Brûlure"; d["archetype.Restrain"] = "Entrave"; d["archetype.Hunt"] = "Traque";
            d["archetype.Tempo"] = "Tempo"; d["archetype.Bulwark"] = "Rempart"; d["archetype.Renewal"] = "Renouveau";
            // Réaction armée : « lanceur » comme les conditions ; la cible s'écrit toujours, avec sa préposition. {0} = tours, {1} = événement, {2} = effet, {3} = cible
            d["reaction.line"] = "Riposte du lanceur ({0} de ses tours) : quand {1} → {2} {3}";
            d["reaction.line_solo"] = "Riposte du lanceur ({0} de ses tours, seul le lanceur riposte) : quand {1} → {2} {3}"; // carte à cible ZA ou 1A
            d["reaction.trigger.EnemyReceivesStatus"] = "un ennemi reçoit {0}";
            d["reaction.trigger.AllyReceivesStatus"] = "un allié (lanceur compris) reçoit {0}";
            d["reaction.trigger.EnemyDefeated"] = "un ennemi est vaincu";
            d["reaction.trigger.AllyDefeated"] = "un autre allié est vaincu"; // le moteur exclut le lanceur (il ne réagit plus une fois vaincu)
            d["reaction.trigger.CarrierHit"] = "le lanceur est frappé";
            d["reaction.trigger.CarrierCrit"] = "le lanceur réussit un coup critique";
            // Cible « déclencheur » : l'unité concernée par l'événement
            d["reaction.trigger_unit.EnemyReceivesStatus"] = "à cet ennemi";
            d["reaction.trigger_unit.AllyReceivesStatus"] = "à cet allié";
            d["reaction.trigger_unit.EnemyDefeated"] = "à l'ennemi vaincu";
            d["reaction.trigger_unit.AllyDefeated"] = "à l'allié vaincu";
            d["reaction.trigger_unit.CarrierHit"] = "à l'attaquant";
            d["reaction.trigger_unit.CarrierCrit"] = "à l'ennemi touché";
            // Cible nommée : règle, puis choix pour une cible unique ({0} = statut du choix « porteur de »)
            d["reaction.to.Self"] = "au lanceur";
            d["reaction.to.AllEnemies"] = "à tous les ennemis";
            d["reaction.to.AllEnemies.TwoRandom"] = "à deux ennemis au hasard";
            d["reaction.to.AllAllies"] = "à toute l'équipe";
            d["reaction.to.SingleEnemy.Default"] = "à un ennemi";
            d["reaction.to.SingleEnemy.LowestHp"] = "à l'ennemi le plus faible";
            d["reaction.to.SingleEnemy.HighestHp"] = "à l'ennemi le plus en forme";
            d["reaction.to.SingleEnemy.StatusHolder"] = "à un ennemi porteur de {0}";
            d["reaction.to.SingleEnemy.NextToAct"] = "au prochain ennemi à agir";
            d["reaction.to.SingleEnemy.Random"] = "à un ennemi au hasard";
            d["reaction.to.SingleEnemy.TwoRandom"] = "à deux ennemis au hasard";
            d["reaction.to.SingleAlly.Default"] = "à un allié";
            d["reaction.to.SingleAlly.LowestHp"] = "à l'allié le plus faible";
            d["reaction.to.SingleAlly.HighestHp"] = "à l'allié le plus en forme";
            d["reaction.to.SingleAlly.StatusHolder"] = "à un allié porteur de {0}";
            d["reaction.to.SingleAlly.NextToAct"] = "au prochain allié à agir";
            d["reaction.to.SingleAlly.Random"] = "à un allié au hasard";
            d["reaction.to.SingleAlly.TwoRandom"] = "à deux alliés au hasard";
            // Effets de réaction sans « la cible » (la cible suit en fin de phrase)
            d["fx.reaction_advance"] = "Avance {0} % sur la frise";
            d["fx.reaction_pushback"] = "Recule de {0} % sur la frise";
            // Valeurs en % même avec les statistiques de l'agent (par tour restant : jamais en montant brut)
            d["fx.consume_damage"] = "Retire {0} de la cible : Dégâts {1} % par tour qu'il lui restait (au plus {2} tours)";
            d["fx.consume_damage_one"] = "Retire {0} de la cible : Dégâts {1} % par tour qu'il lui restait (au plus 1 tour)";
            d["fx.consume_heal"] = "Retire {0} de la cible : Soin {1} % PV par tour qu'il lui restait (au plus {2} tours)";
            d["fx.consume_heal_one"] = "Retire {0} de la cible : Soin {1} % PV par tour qu'il lui restait (au plus 1 tour)";
            d["fx.spread"] = "Copie {0} de la cible (mêmes tours restants) sur tous les autres ennemis";
            // Échelle d'archétype : {0} = bonus, {1} = archétype (distinct du statut du même nom), {2} = nombre maximal de cartes comptées
            d["fx.mod_archetype"] = "{0} par carte d'archétype {1} équipée dans l'escouade, celle-ci comprise (au plus {2} cartes)";
            d["fx.mod_archetype_one"] = "{0} par carte d'archétype {1} équipée dans l'escouade, celle-ci comprise (au plus 1 carte)";
            // Statut sans durée (Marque : jusqu'au prochain coup reçu)
            d["fx.status_untimed"] = "{0}";
            d["fx.status_untimed_chance"] = "{0} ({1} %)";
            d["pick.LowestHp"] = "Cible : le plus faible en PV";
            d["pick.HighestHp"] = "Cible : le plus en forme (PV en %)";
            d["pick.StatusHolder"] = "Cible : porteur de {0}";
            d["pick.NextToAct"] = "Cible : le prochain à agir";
            d["pick.Random"] = "Cible : au hasard";
            d["pick.TwoRandom"] = "Cible : deux au hasard";
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
            // Aperçu de l'effet de la carte tenue (dry-run)
            d["preview.variable"] = "variable";
            d["preview.shield"] = "Bouclier {0}";
            d["preview.plan_cancelled"] = "Action annulée";
            d["preview.plan_cancelled_maybe"] = "Action annulée ?";
            d["preview.victory"] = "VICTOIRE !";
            d["preview.victory_maybe"] = "Victoire possible";
            d["preview.next_wave"] = "VAGUE SUIVANTE";
            d["preview.next_wave_maybe"] = "Vague suivante possible";
            d["preview.defeat"] = "Défaite possible";
        }
    }
}
