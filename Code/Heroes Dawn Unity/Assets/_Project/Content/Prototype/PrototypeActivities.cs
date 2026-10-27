#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Activités du prototype : réflexe historique, potion, survie en neige, arène, dialogue, et les deux mini-jeux 2D de héros (rempart, course des toits).
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

            // Rempart (2D) : 10 Crédits par projectile intercepté (même si l'objectif n'est pas atteint) + 100 Crédits et 5 PR à la réussite.
            new ActivityDef(new ActivityId("deflect_docks"), ActivityKinds.Deflect, "activities.deflect.name", "activities.deflect.desc",
                            P(("time_limit_s", 40), ("target", 14), ("lives", 3), ("speed_percent", 100)), credits: 100, pr: 5, creditsPerScore: 10),

            // Course des toits (2D) : 15 Crédits par civil sauvé + 150 Crédits et 5 PR à la réussite.
            new ActivityDef(new ActivityId("rooftop_rescue"), ActivityKinds.RooftopRun, "activities.rooftop.name", "activities.rooftop.desc",
                            P(("time_limit_s", 40), ("target", 10), ("lives", 3), ("speed_percent", 100)), credits: 150, pr: 5, creditsPerScore: 15),

            new ActivityDef(new ActivityId("arena_training"), ActivityKinds.CombatArena, "activities.arena.name", "activities.arena.desc",
                            P(("waves", 3), ("enemies_per_wave", 4)), credits: 350, pr: 10),

            // Dialogue : un échec lance un combat Facile.
            new ActivityDef(new ActivityId("dialogue_dockers"), ActivityKinds.Dialogue, "activities.dialogue.name", "activities.dialogue.desc",
                            P(("lines", 4), ("choices_per_line", 3), ("required_percent", 60)), credits: 200, pr: 5,
                            failureFollowUp: FollowUpDef.Combat(CombatRank.Easy)),

            // Le rythme (2D) : 5 Crédits par note touchée + 200 Crédits et 8 PR à la réussite.
            new ActivityDef(new ActivityId("rhythm_rooftop"), ActivityKinds.Rhythm, "activities.rhythm.name", "activities.rhythm.desc",
                            P(("notes", 24), ("lanes", 3), ("lives", 5), ("speed_percent", 100)), credits: 200, pr: 8, creditsPerScore: 5),

            // Tir franc (2D) : 10 Crédits par cible touchée + 180 Crédits et 6 PR à la réussite.
            new ActivityDef(new ActivityId("free_throw_court"), ActivityKinds.FreeThrow, "activities.freethrow.name", "activities.freethrow.desc",
                            P(("throws", 5), ("hits", 3), ("speed_percent", 100)), credits: 180, pr: 6, creditsPerScore: 10),

            // Caisse au choix (2D) : 15 Crédits par caisse trouvée + 220 Crédits et 8 PR à la réussite.
            new ActivityDef(new ActivityId("crate_pick_depot"), ActivityKinds.CratePick, "activities.cratepick.name", "activities.cratepick.desc",
                            P(("rounds", 3), ("lives", 3), ("speed_percent", 100)), credits: 220, pr: 8, creditsPerScore: 15),

            // Analyse ADN (2D) : 10 Crédits par parasite retiré + 200 Crédits et 6 PR à la réussite.
            new ActivityDef(new ActivityId("dna_lab"), ActivityKinds.DnaAnalysis, "activities.dna.name", "activities.dna.desc",
                            P(("time_limit_s", 45), ("parasites", 6), ("lives", 3)), credits: 200, pr: 6, creditsPerScore: 10),

            // Engrenage (2D) : 220 Crédits et 8 PR à la réussite.
            new ActivityDef(new ActivityId("gears_workshop"), ActivityKinds.Gears, "activities.gears.name", "activities.gears.desc",
                            P(("gears", 6), ("moves", 12)), credits: 220, pr: 8),

            // Ingénierie de secours (2D) : 250 Crédits et 10 PR à la réussite.
            new ActivityDef(new ActivityId("rescue_tool"), ActivityKinds.RescueEngineering, "activities.rescue.name", "activities.rescue.desc",
                            P(("parts", 5), ("distractors", 3), ("attempts", 3)), credits: 250, pr: 10),
        };
    }
}
