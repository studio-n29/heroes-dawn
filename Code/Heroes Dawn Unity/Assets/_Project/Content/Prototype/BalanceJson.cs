#nullable enable
using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Content.Prototype
{
    /// <summary>
    /// Sérialise <see cref="BalanceConfig"/> en objet JSON plat d'entiers (clés = noms des propriétés, ordre alphabétique).
    /// Même format côté client (Resources/Balance/balance.json) et Remote Config (clé « balance »).
    /// </summary>
    public static class BalanceJson
    {
        static readonly DomainError InvalidJson = new DomainError("balance.invalid_json", "errors.balance.invalid_json");

        static PropertyInfo[] IntProperties => typeof(BalanceConfig)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.PropertyType == typeof(int) && p.CanRead && p.CanWrite)
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToArray();

        public static string Serialize(BalanceConfig config)
        {
            var props = IntProperties;
            var sb = new StringBuilder("{\n");
            for (int i = 0; i < props.Length; i++)
            {
                sb.Append("  \"").Append(props[i].Name).Append("\": ")
                  .Append(((int)props[i].GetValue(config)).ToString(CultureInfo.InvariantCulture));
                sb.Append(i < props.Length - 1 ? ",\n" : "\n");
            }
            return sb.Append('}').ToString();
        }

        public static Result<BalanceConfig> Deserialize(string json)
        {
            var config = new BalanceConfig();
            var props = IntProperties.ToDictionary(p => p.Name, StringComparer.Ordinal);
            var s = json.Trim();
            if (s.Length < 2 || s[0] != '{' || s[s.Length - 1] != '}') return Result<BalanceConfig>.Fail(InvalidJson);

            var body = s.Substring(1, s.Length - 2).Trim();
            if (body.Length == 0) return Result<BalanceConfig>.Ok(config);
            foreach (var pair in body.Split(','))
            {
                int colon = pair.IndexOf(':');
                if (colon < 0) return Result<BalanceConfig>.Fail(InvalidJson);
                var key = pair.Substring(0, colon).Trim();
                var raw = pair.Substring(colon + 1).Trim();
                if (key.Length < 2 || key[0] != '"' || key[key.Length - 1] != '"') return Result<BalanceConfig>.Fail(InvalidJson);
                if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                    return Result<BalanceConfig>.Fail(InvalidJson);
                if (props.TryGetValue(key.Substring(1, key.Length - 2), out var prop)) prop.SetValue(config, value);
            }
            return Result<BalanceConfig>.Ok(config);
        }
    }
}
