namespace N29.HeroesDawn.Core
{
    /// <summary>Aléatoire déterministe. Seule source d'aléa autorisée dans le domaine.</summary>
    public interface IRandom
    {
        uint NextUInt();
        /// <summary>Entier uniforme dans [minInclusive, maxExclusive[.</summary>
        int Range(int minInclusive, int maxExclusive);
        /// <summary>Vrai avec une probabilité de percent % (0-100).</summary>
        bool Chance(int percent);
        IRandom Fork(ulong stream);
        ulong State { get; }
    }
}
