#nullable enable
using N29.HeroesDawn.Definitions;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Silhouettes « non recruté » par famille (livraison 2D S-02, Resources/UI/AubeTactique/Missing20261116/Silhouettes), pour les agents
    /// absents du registre. Le cadenas du kit reste une couche séparée : aucune serrure n'est incrustée dans l'image.
    /// </summary>
    public static class UnrecruitedArt
    {
        const string Folder = "Missing20261116/Silhouettes/unrecruited-";

        /// <summary>Silhouette de la famille ; null si l'image manque (l'appelant garde alors l'illustration de l'agent).</summary>
        public static Sprite? For(Family family) => TacticalArt.Load(Folder + family switch
        {
            Family.Matter => "matter",
            Family.Elemental => "elemental",
            Family.Perception => "perception",
            _ => "bio",
        });
    }
}
