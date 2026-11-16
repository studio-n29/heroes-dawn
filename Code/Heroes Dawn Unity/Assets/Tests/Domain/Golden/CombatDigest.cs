#nullable enable
using System;
using System.Collections;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>
    /// Empreinte d'un combat terminé : journal d'événements (propriétés lues par ordre alphabétique), état final des unités (PV, énergie, jauge, statuts) et valeur suivante du générateur.
    /// Sert de garde-fou aux refontes sans changement de comportement.
    /// </summary>
    public static class CombatDigest
    {
        /// <summary>Empreinte Fnv1a64 (16 caractères hexadécimaux) ; tire une valeur du générateur : à appeler une seule fois, en fin de combat.</summary>
        public static string Of(CombatSimulation sim)
        {
            var text = new StringBuilder();
            foreach (var e in sim.Log)
            {
                var type = e.GetType();
                text.Append(type.Name);
                foreach (var p in type.GetProperties(BindingFlags.Public | BindingFlags.Instance).OrderBy(x => x.Name, StringComparer.Ordinal))
                    text.Append('|').Append(p.Name).Append('=').Append(Format(p.GetValue(e)));
                text.Append('\n');
            }
            foreach (var u in sim.Units)
            {
                text.Append(u.Id).Append(':').Append(u.Hp).Append(':').Append(u.Energy).Append(':').Append(u.Point).Append(':');
                text.Append(string.Join(",", u.Statuses.Select(st => st.Type + ":" + st.TurnsLeft.ToString(CultureInfo.InvariantCulture) + ":" + st.Value.ToString(CultureInfo.InvariantCulture))));
                text.Append('\n');
            }
            text.Append(sim.RngProbeForTests().ToString(CultureInfo.InvariantCulture));
            var h = new Fnv1a64();
            h.Add(text.ToString());
            return h.Value.ToString("x16", CultureInfo.InvariantCulture);
        }

        static string Format(object? v)
        {
            if (v == null) return "null";
            if (v is string s) return s;
            if (v is IEnumerable en) return string.Join(",", en.Cast<object?>().Select(Format));
            return Convert.ToString(v, CultureInfo.InvariantCulture) ?? "";
        }
    }
}
