#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct CardId : IEquatable<CardId>, IComparable<CardId>
    {
        public readonly string Value;
        public CardId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(CardId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is CardId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(CardId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(CardId a, CardId b) => a.Equals(b);
        public static bool operator !=(CardId a, CardId b) => !a.Equals(b);
    }
}
