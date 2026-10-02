#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct EnemyId : IEquatable<EnemyId>, IComparable<EnemyId>
    {
        public readonly string Value;
        public EnemyId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(EnemyId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is EnemyId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(EnemyId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(EnemyId a, EnemyId b) => a.Equals(b);
        public static bool operator !=(EnemyId a, EnemyId b) => !a.Equals(b);
    }
}
