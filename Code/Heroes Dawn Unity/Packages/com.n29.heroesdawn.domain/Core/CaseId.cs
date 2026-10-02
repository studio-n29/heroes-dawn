#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct CaseId : IEquatable<CaseId>, IComparable<CaseId>
    {
        public readonly string Value;
        public CaseId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(CaseId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is CaseId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(CaseId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(CaseId a, CaseId b) => a.Equals(b);
        public static bool operator !=(CaseId a, CaseId b) => !a.Equals(b);
    }
}
