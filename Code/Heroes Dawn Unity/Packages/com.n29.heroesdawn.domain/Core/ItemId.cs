#nullable enable
using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>Identifiant typé (évite de confondre deux chaînes).</summary>
    public readonly struct ItemId : IEquatable<ItemId>, IComparable<ItemId>
    {
        public readonly string Value;
        public ItemId(string value) { Value = value ?? throw new ArgumentNullException(nameof(value)); }
        public bool Equals(ItemId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object? obj) => obj is ItemId o && Equals(o);
        public override int GetHashCode() => Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        public int CompareTo(ItemId other) => string.CompareOrdinal(Value, other.Value);
        public override string ToString() => Value ?? string.Empty;
        public static bool operator ==(ItemId a, ItemId b) => a.Equals(b);
        public static bool operator !=(ItemId a, ItemId b) => !a.Equals(b);
    }
}
