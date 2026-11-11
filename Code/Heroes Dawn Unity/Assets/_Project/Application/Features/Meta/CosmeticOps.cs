#nullable enable
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Cosmétiques possédés (liste unique du profil, <see cref="MetaSave.cosmetics"/>) : une seule opération d'octroi, quelle que soit la source.</summary>
    public static class CosmeticOps
    {
        public static bool Owns(MetaSave m, string id) => m.cosmetics.Contains(id);

        /// <summary>Ajoute le cosmétique ; faux s'il était déjà possédé (ou si l'identifiant est vide).</summary>
        public static bool Unlock(MetaSave m, string id)
        {
            if (string.IsNullOrEmpty(id) || m.cosmetics.Contains(id)) return false;
            m.cosmetics.Add(id);
            return true;
        }
    }
}
