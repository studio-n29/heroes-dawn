#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé d'une activité (instance configurée : « fishing_docks », « potion_infirmary »…).</summary>
    public readonly struct ActivityId : IEquatable<ActivityId>, IComparable<ActivityId>
    {
        public readonly string Value;
        public ActivityId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(ActivityId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ActivityId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(ActivityId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(ActivityId a, ActivityId b) => a.Equals(b);
        public static bool operator !=(ActivityId a, ActivityId b) => !a.Equals(b);
    }
}
