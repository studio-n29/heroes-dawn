#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct MemberId : IEquatable<MemberId>, IComparable<MemberId>
    {
        public readonly string Value;
        public MemberId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(MemberId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is MemberId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(MemberId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(MemberId a, MemberId b) => a.Equals(b);
        public static bool operator !=(MemberId a, MemberId b) => !a.Equals(b);
    }
}
