#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct ZoneId : IEquatable<ZoneId>, IComparable<ZoneId>
    {
        public readonly string Value;
        public ZoneId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(ZoneId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ZoneId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(ZoneId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(ZoneId a, ZoneId b) => a.Equals(b);
        public static bool operator !=(ZoneId a, ZoneId b) => !a.Equals(b);
    }
}
