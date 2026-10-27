#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Un paramètre réglable d'un type d'activité : valeur par défaut et bornes (clamp à la lecture).</summary>
    public sealed class ParameterDef
    {
        public string Key { get; }
        public int Default { get; }
        public int Min { get; }
        public int Max { get; }
        /// <summary>Clé de localisation du libellé (outils, fiche d'activité).</summary>
        public string LabelKey { get; }

        public ParameterDef(string key, int @default, int min, int max, string? labelKey = null)
        {
            Key = key; Default = @default; Min = min; Max = max; LabelKey = labelKey ?? "activity.param." + key;
        }

        public int Clamp(int v) => v < Min ? Min : v > Max ? Max : v;
    }

    /// <summary>Valeurs de paramètres d'une activité (entiers : déterministe, sérialisable). Les clés absentes prennent la valeur par défaut du type.</summary>
    public sealed class ActivityParameters
    {
        public static readonly ActivityParameters Empty = new ActivityParameters(new Dictionary<string, int>());
        readonly Dictionary<string, int> _values;

        public ActivityParameters(IReadOnlyDictionary<string, int> values)
        {
            _values = new Dictionary<string, int>();
            foreach (var kv in values) _values[kv.Key] = kv.Value;
        }

        public IReadOnlyDictionary<string, int> Values => _values;
        public bool Has(string key) => _values.ContainsKey(key);
        public int Get(string key, int fallback = 0) => _values.TryGetValue(key, out var v) ? v : fallback;

        public ActivityParameters With(string key, int value)
        {
            var copy = new Dictionary<string, int>(_values) { [key] = value };
            return new ActivityParameters(copy);
        }
    }

    /// <summary>Malus infligé à un agent pour toute la durée du plateau (pourcentage sur toutes ses statistiques, négatif).</summary>
    public sealed class MalusDef
    {
        public int StatPercent { get; }
        public string ReasonKey { get; }
        public MalusDef(int statPercent, string reasonKey) { StatPercent = statPercent; ReasonKey = reasonKey; }
    }

    public enum FollowUpType { None, Combat, Activity }

    /// <summary>Ce qui se lance après un échec : un combat (rang donné) ou une autre activité.</summary>
    public sealed class FollowUpDef
    {
        public FollowUpType Type { get; }
        public CombatRank Rank { get; }
        public ActivityId Activity { get; }

        FollowUpDef(FollowUpType type, CombatRank rank, ActivityId activity) { Type = type; Rank = rank; Activity = activity; }
        public static FollowUpDef Combat(CombatRank rank) => new FollowUpDef(FollowUpType.Combat, rank, default);
        public static FollowUpDef ThenActivity(ActivityId activity) => new FollowUpDef(FollowUpType.Activity, CombatRank.Easy, activity);
    }

    /// <summary>
    /// Une activité configurée : un type (<see cref="Kind"/>, enregistré dans le registre) + ses paramètres, ses gains et ses conséquences d'échec.
    /// Ajouter un type d'activité = enregistrer un <c>IActivityKind</c> ; ajouter une activité = ajouter une ActivityDef (données, sans code).
    /// </summary>
    public sealed class ActivityDef
    {
        public ActivityId Id { get; }
        public string Kind { get; }
        public string NameKey { get; }
        public string DescriptionKey { get; }
        public ActivityParameters Parameters { get; }
        public int Credits { get; }
        public int Pr { get; }
        /// <summary>Crédits supplémentaires par point de score (ex. par poisson pêché).</summary>
        public int CreditsPerScore { get; }
        /// <summary>L'activité aide un membre de l'équipe (potion) : un « patient » est tiré dans l'escouade.</summary>
        public bool PicksPatient { get; }
        public MalusDef? FailureMalus { get; }
        public FollowUpDef? FailureFollowUp { get; }

        public ActivityDef(ActivityId id, string kind, string nameKey, string descriptionKey, ActivityParameters? parameters = null,
                           int credits = 0, int pr = 0, int creditsPerScore = 0, bool picksPatient = false,
                           MalusDef? failureMalus = null, FollowUpDef? failureFollowUp = null)
        {
            Id = id; Kind = kind; NameKey = nameKey; DescriptionKey = descriptionKey;
            Parameters = parameters ?? ActivityParameters.Empty;
            Credits = credits; Pr = pr; CreditsPerScore = creditsPerScore; PicksPatient = picksPatient;
            FailureMalus = failureMalus; FailureFollowUp = failureFollowUp;
        }

        /// <summary>Même activité avec d'autres valeurs de paramètres (réglages externes, voir ActivityParamsJson).</summary>
        public ActivityDef WithParameters(ActivityParameters parameters)
            => new ActivityDef(Id, Kind, NameKey, DescriptionKey, parameters, Credits, Pr, CreditsPerScore, PicksPatient, FailureMalus, FailureFollowUp);
    }

    /// <summary>Identifiants des activités intégrées au prototype.</summary>
    public static class ActivityIds
    {
        /// <summary>Mini-jeu historique (barre de réflexe), utilisé par défaut sur les cases Mini-jeu.</summary>
        public static readonly ActivityId Reflex = new ActivityId("reflex");
    }

    /// <summary>Identifiants des types d'activité intégrés (le registre du domaine y associe un générateur de niveau).</summary>
    public static class ActivityKinds
    {
        public const string Reflex = "reflex";
        public const string Potion = "potion";
        public const string SnowSurvival = "snow_survival";
        public const string Deflect = "deflect";
        public const string RooftopRun = "rooftop_run";
        public const string CombatArena = "combat_arena";
        public const string Dialogue = "dialogue";
        // Mini-jeux 2D du lot H (0.14.0)
        public const string Rhythm = "rhythm";
        public const string FreeThrow = "free_throw";
        public const string CratePick = "crate_pick";
    }
}
