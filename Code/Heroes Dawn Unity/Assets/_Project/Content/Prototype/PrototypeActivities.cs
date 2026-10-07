#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Activités du prototype : une par type (réflexe historique, potion, survie en neige, pêche, arène, dialogue).
    /// Pour régler une activité sans code : surcharger ses paramètres (Resources/Balance/activities.json, voir <see cref="ActivityParamsJson"/>).
    /// Pour en ajouter une : une <see cref="ActivityDef"/> ici (type existant) ou un nouveau IActivityKind dans le registre (Domain.Activities.ActivityRegistry).
    /// </summary>
    public static class PrototypeActivities
    {
        static ActivityParameters P(params (string Key, int Value)[] values)
        {
            var d = new Dictionary<string, int>();
            foreach (var (k, v) in values) d[k] = v;
            return new ActivityParameters(d);
        }

        public static List<ActivityDef> Build(BalanceConfig b) => new List<ActivityDef>
        {
            // Mini-jeu historique : les gains viennent de l'équilibrage (MiniGameCredits / MiniGamePr).
            new ActivityDef(ActivityIds.Reflex, ActivityKinds.Reflex, "activities.reflex.name", "activities.reflex.desc",
                            credits: b.MiniGameCredits, pr: b.MiniGamePr),

            // Potion : un membre de l'équipe à soigner ; un échec affaiblit l'agent de 15 % jusqu'à la fin du plateau.
            new ActivityDef(new ActivityId("potion_field"), ActivityKinds.Potion, "activities.potion.name", "activities.potion.desc",
                            P(("ingredients", 6), ("steps", 3), ("attempts", 2)), credits: 250, pr: 8, picksPatient: true,
                            failureMalus: new MalusDef(-15, "activities.potion.malus")),

            new ActivityDef(new ActivityId("survival_snow"), ActivityKinds.SnowSurvival, "activities.survival.name", "activities.survival.desc",
                            P(("time_limit_s", 90), ("totem_count", 3)), credits: 300, pr: 10),

            // Pêche : 40 Crédits par poisson (même si le quota n'est pas atteint) + 100 Crédits et 5 PR à la réussite.
            new ActivityDef(new ActivityId("fishing_docks"), ActivityKinds.Fishing, "activities.fishing.name", "activities.fishing.desc",
                            P(("time_limit_s", 60), ("fish_count", 10), ("success_fish", 4)), credits: 100, pr: 5, creditsPerScore: 40),

            new ActivityDef(new ActivityId("arena_training"), ActivityKinds.CombatArena, "activities.arena.name", "activities.arena.desc",
                            P(("waves", 3), ("enemies_per_wave", 4)), credits: 350, pr: 10),

            // Dialogue : un échec lance un combat Facile.
            new ActivityDef(new ActivityId("dialogue_dockers"), ActivityKinds.Dialogue, "activities.dialogue.name", "activities.dialogue.desc",
                            P(("lines", 4), ("choices_per_line", 3), ("required_percent", 60)), credits: 200, pr: 5,
                            failureFollowUp: FollowUpDef.Combat(CombatRank.Easy)),
        };
    }
}
