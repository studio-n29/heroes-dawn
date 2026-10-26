#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Un groupe de filtres : ses options (la première : « Toutes ») et l'option choisie.</summary>
    public struct FilterGroupData
    {
        public string[] Labels;
        public int Selected;
    }

    /// <summary>Barre de filtres d'une page en grille (Grimoire, Équipement) : un rang de boutons par groupe ; sans groupe, la barre est masquée.</summary>
    public interface IFilterBarView
    {
        /// <summary>(groupe, option) touchés.</summary>
        event Action<int, int> FilterClicked;
        void ShowFilters(FilterGroupData[] groups);
    }
}
