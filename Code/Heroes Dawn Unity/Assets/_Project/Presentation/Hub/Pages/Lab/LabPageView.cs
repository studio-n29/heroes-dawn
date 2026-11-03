#nullable enable
namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Atelier de fabrication (M14) : recherches à cinq niveaux, compétences de création, recettes filtrables, panneau de fabrication.</summary>
    public sealed class LabPageView : ListPageView, ILabPageView
    {
        /// <summary>Tests PlayMode : sélectionne une rangée comme le ferait un toucher.</summary>
        public void SelectRowForTest(int row) => RaiseRowSelected(row);
    }
}
