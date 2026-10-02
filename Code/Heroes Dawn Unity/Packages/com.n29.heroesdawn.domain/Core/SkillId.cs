#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct SkillId : IEquatable<SkillId>, IComparable<SkillId>
    {
        public readonly string Value;
        public SkillId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(SkillId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is SkillId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(SkillId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(SkillId a, SkillId b) => a.Equals(b);
        public static bool operator !=(SkillId a, SkillId b) => !a.Equals(b);
    }
}
