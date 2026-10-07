#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Réglages d'activités modifiables sans code : un objet JSON plat d'entiers dont les clés sont « identifiant_activité.paramètre »
    /// (ex. <c>{ "fishing_docks.time_limit_s": 90, "arena_training.waves": 5 }</c>), lu dans Resources/Balance/activities.json.
    /// Les valeurs sont bornées par le schéma du type d'activité ; une clé inconnue est ignorée.
    /// </summary>
    public static class ActivityParamsJson
    {
        static readonly DomainError InvalidJson = new DomainError("activities.invalid_json", "errors.balance.invalid_json");

        public static Result<IReadOnlyDictionary<string, int>> Parse(string json)
        {
            var map = new Dictionary<string, int>();
            var s = json.Trim();
            if (s.Length < 2 || s[0] != '{' || s[s.Length - 1] != '}') return Result<IReadOnlyDictionary<string, int>>.Fail(InvalidJson);
            var body = s.Substring(1, s.Length - 2).Trim();
            if (body.Length > 0)
            {
                foreach (var pair in body.Split(','))
                {
                    int colon = pair.IndexOf(':');
                    if (colon < 0) return Result<IReadOnlyDictionary<string, int>>.Fail(InvalidJson);
                    var key = pair.Substring(0, colon).Trim().Trim('"');
                    if (key.Length == 0 || !int.TryParse(pair.Substring(colon + 1).Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                        return Result<IReadOnlyDictionary<string, int>>.Fail(InvalidJson);
                    map[key] = value;
                }
            }
            return Result<IReadOnlyDictionary<string, int>>.Ok(map);
        }

        /// <summary>Applique les surcharges aux activités ; renvoie une nouvelle liste.</summary>
        public static List<ActivityDef> Apply(IReadOnlyList<ActivityDef> activities, IReadOnlyDictionary<string, int> overrides)
        {
            var result = new List<ActivityDef>();
            foreach (var a in activities)
            {
                var p = a.Parameters;
                string prefix = a.Id.Value + ".";
                foreach (var kv in overrides)
                    if (kv.Key.StartsWith(prefix, StringComparison.Ordinal)) p = p.With(kv.Key.Substring(prefix.Length), kv.Value);
                result.Add(ReferenceEquals(p, a.Parameters) ? a : a.WithParameters(p));
            }
            return result;
        }
    }
}
