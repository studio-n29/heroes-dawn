#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Activities
{
    /// <summary>Une activité prête à être jouée : l'activité, l'environnement du plateau, la graine, les paramètres complets, le niveau généré.</summary>
    public sealed class ActivityPlan
    {
        public ActivityDef Def { get; }
        public ZoneEnvironment Environment { get; }
        public ulong Seed { get; }
        public ActivityParameters Parameters { get; }
        public ActivityLevel Level { get; }
        /// <summary>Membre de l'équipe à aider (activités qui en demandent un), sinon vide.</summary>
        public MemberId? Patient { get; }
        /// <summary>Rang de la tentative sur cette case (1 = première ; augmente avec les activités enchaînées).</summary>
        public int Attempt { get; }

        public ActivityPlan(ActivityDef def, ZoneEnvironment environment, ulong seed, ActivityParameters parameters, ActivityLevel level, MemberId? patient, int attempt)
        {
            Def = def; Environment = environment; Seed = seed; Parameters = parameters; Level = level; Patient = patient; Attempt = attempt;
        }
    }

    /// <summary>Prépare une activité : choisit le patient, dérive la graine (case + activité + tentative) et génère le niveau.</summary>
    public static class ActivityPlanner
    {
        public static ActivityPlan Plan(ActivityRegistry registry, ActivityDef def, ZoneEnvironment env, ulong runSeed, CaseId at, int attempt,
                                        IReadOnlyList<MemberId> squad)
        {
            var h = new Fnv1a64();
            h.Add((long)runSeed);
            h.Add(at.Value);
            h.Add(def.Id.Value);
            h.Add(attempt);
            ulong seed = h.Value;

            MemberId? patient = null;
            if (def.PicksPatient && squad.Count > 0) patient = squad[new Pcg32(seed).Fork(21).Range(0, squad.Count)];

            var parameters = registry.Resolve(def);
            var level = registry.Get(def.Kind).Generate(new ActivityContext(def, parameters, env, seed));
            return new ActivityPlan(def, env, seed, parameters, level, patient, attempt);
        }
    }

    /// <summary>Résultat brut d'une activité jouée : réussite et score (poissons pêchés, vagues vaincues…).</summary>
    public readonly struct ActivityResult
    {
        public bool Success { get; }
        public int Score { get; }
        public ActivityResult(bool success, int score = 0) { Success = success; Score = score < 0 ? 0 : score; }
        public static ActivityResult Won(int score = 0) => new ActivityResult(true, score);
        public static ActivityResult Lost(int score = 0) => new ActivityResult(false, score);
    }

    /// <summary>Malus d'un agent jusqu'à la fin du plateau : pourcentage appliqué à toutes ses statistiques (négatif).</summary>
    public sealed class AgentMalus
    {
        public MemberId Member { get; }
        public int StatPercent { get; }
        public string ReasonKey { get; }
        public AgentMalus(MemberId member, int statPercent, string reasonKey) { Member = member; StatPercent = statPercent; ReasonKey = reasonKey; }
    }

    /// <summary>Conséquences d'une activité jouée : gains, malus éventuel, suite éventuelle (combat ou autre activité).</summary>
    public sealed class ActivityOutcome
    {
        public ActivityPlan Plan { get; }
        public ActivityResult Result { get; }
        public int Credits { get; }
        public int Pr { get; }
        public AgentMalus? Malus { get; }
        public FollowUpDef? FollowUp { get; }

        public ActivityOutcome(ActivityPlan plan, ActivityResult result, int credits, int pr, AgentMalus? malus, FollowUpDef? followUp)
        {
            Plan = plan; Result = result; Credits = credits; Pr = pr; Malus = malus; FollowUp = followUp;
        }
    }

    /// <summary>
    /// Règles communes à toutes les activités : gains (base à la réussite + pièces au prorata du score, même en cas d'échec),
    /// malus et suite à l'échec.
    /// </summary>
    public static class ActivityRules
    {
        public const int MaxMalusPercent = 50;

        public static ActivityOutcome Resolve(ActivityPlan plan, ActivityResult result)
        {
            var def = plan.Def;
            int credits = (result.Success ? def.Credits : 0) + def.CreditsPerScore * result.Score;
            int pr = result.Success ? def.Pr : 0;
            AgentMalus? malus = null;
            FollowUpDef? follow = null;
            if (!result.Success)
            {
                if (def.FailureMalus != null && plan.Patient.HasValue)
                    malus = new AgentMalus(plan.Patient.Value, def.FailureMalus.StatPercent, def.FailureMalus.ReasonKey);
                follow = def.FailureFollowUp;
            }
            return new ActivityOutcome(plan, result, credits, pr, malus, follow);
        }
    }
}
