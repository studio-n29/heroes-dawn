#nullable enable
using N29.HeroesDawn.Definitions;

using UnityEngine;


namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>
    /// Ce qu'affiche un portrait : silhouette provisoire (buste à la couleur de la famille) ou, plus tard, l'illustration de l'agent
    /// (<see cref="Art"/>) ; initiale et couronne de boss.
    /// </summary>
    public readonly struct PortraitData
    {
        public readonly string Initial;
        public readonly Family Family;
        public readonly bool Boss;
        /// <summary>Illustration de l'agent ; null : la silhouette provisoire est affichée.</summary>
        public readonly Sprite? Art;

        public PortraitData(string name, Family family, bool boss = false, Sprite? art = null)
        {
            Initial = string.IsNullOrEmpty(name) ? "?" : name.Substring(0, 1).ToUpperInvariant();
            Family = family;
            Boss = boss;
            Art = art;
        }
    }

}
