#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Textes du framework d'activités : noms, descriptions, paramètres, environnements, toasts.</summary>
    public static partial class Loc
    {
        static partial void ExtendActivities(Dictionary<string, string> d)
        {
            d["activities.reflex.name"] = "Réflexe";
            d["activities.reflex.desc"] = "Arrête le curseur dans la zone verte.";
            d["activities.potion.name"] = "La potion magique";
            d["activities.potion.desc"] = "Un membre de l'équipe a besoin d'aide. Le livre donne des indices sur les ingrédients, l'ordre et les quantités : prépare la bonne potion. Un échec affaiblit l'agent jusqu'à la fin du plateau.";
            d["activities.potion.malus"] = "potion ratée";
            d["activities.survival.name"] = "Survie en neige";
            d["activities.survival.desc"] = "Traverse la neige, l'herbe haute et le sable ; la jauge de survie baisse, recharge-la près d'un totem, et trouve l'objet recherché.";
            d["activities.fishing.name"] = "Pêche";
            d["activities.fishing.desc"] = "Pêche le plus de poissons possible avant la fin du temps ; chaque poisson rapporte des Crédits.";
            d["activities.arena.name"] = "Arène de combat";
            d["activities.arena.desc"] = "Choisis un agent et vaincs des vagues d'ennemis en vue de dessus, avec deux compétences.";
            d["activities.dialogue.name"] = "Dialogue";
            d["activities.dialogue.desc"] = "Choisis tes réponses avec soin : un dialogue raté tourne mal.";

            d["activity.env"] = "Décor : {0} · {1}";
            d["activity.level"] = "Niveau généré : {0} éléments (graine {1})";
            d["activity.patient"] = "Patient : {0} · {1}";
            d["activity.params"] = "Réglages :";
            d["activity.wip"] = "Version de test : le jeu complet de cette activité arrive plus tard. Choisis le résultat à simuler.";
            d["activity.simulate_win"] = "SIMULER LA RÉUSSITE";
            d["activity.simulate_fail"] = "SIMULER L'ÉCHEC";
            d["activity.malus_applied"] = "{0} : -{1} % aux statistiques jusqu'à la fin du plateau ({2}).";
            d["activity.followup_combat"] = "Ça tourne mal : combat !";
            d["activity.followup_activity"] = "L'échec enchaîne une autre épreuve.";

            d["env.biome.City"] = "Ville"; d["env.biome.Harbor"] = "Port"; d["env.biome.Industrial"] = "Zone industrielle";
            d["env.biome.Forest"] = "Forêt"; d["env.biome.Snow"] = "Montagne enneigée"; d["env.biome.Desert"] = "Désert";
            d["env.weather.Day"] = "jour"; d["env.weather.Night"] = "nuit"; d["env.weather.Snow"] = "neige";
            d["env.weather.Rain"] = "pluie"; d["env.weather.Fog"] = "brouillard";

            d["potion.ailment.Fever"] = "fièvre"; d["potion.ailment.Wound"] = "blessure"; d["potion.ailment.Poison"] = "empoisonnement";
            d["potion.ailment.Exhaustion"] = "épuisement"; d["potion.ailment.Curse"] = "malédiction";

            d["activity.param.time_limit_s"] = "Durée (s)";
            d["activity.param.drain_permille_per_s"] = "Perte de survie (‰/s)";
            d["activity.param.refill_permille_per_s"] = "Recharge au totem (‰/s)";
            d["activity.param.totem_count"] = "Totems";
            d["activity.param.totem_radius_m"] = "Rayon des totems (m)";
            d["activity.param.target_distance_m"] = "Distance de l'objet (m)";
            d["activity.param.obstacle_count"] = "Obstacles";
            d["activity.param.terrain_slow_percent"] = "Ralentissement du terrain (%)";
            d["activity.param.fish_count"] = "Poissons";
            d["activity.param.bite_window_ms"] = "Fenêtre de touche (ms)";
            d["activity.param.bite_wait_min_ms"] = "Attente min. (ms)";
            d["activity.param.bite_wait_max_ms"] = "Attente max. (ms)";
            d["activity.param.success_fish"] = "Poissons pour réussir";
            d["activity.param.pond_radius_m"] = "Rayon de l'étang (m)";
            d["activity.param.waves"] = "Vagues";
            d["activity.param.enemies_per_wave"] = "Ennemis par vague";
            d["activity.param.arena_radius_m"] = "Rayon de l'arène (m)";
            d["activity.param.enemy_hp_percent"] = "PV des ennemis (%)";
            d["activity.param.enemy_damage_percent"] = "Dégâts des ennemis (%)";
            d["activity.param.ingredients"] = "Ingrédients";
            d["activity.param.steps"] = "Étapes";
            d["activity.param.max_quantity"] = "Quantité max.";
            d["activity.param.attempts"] = "Essais";
            d["activity.param.quantity_tolerance"] = "Tolérance de dosage";
            d["activity.param.lines"] = "Répliques";
            d["activity.param.choices_per_line"] = "Choix par réplique";
            d["activity.param.required_percent"] = "Score requis (%)";
            d["activity.param.extra_time_percent"] = "Temps en plus (%)";
        }
    }
}
