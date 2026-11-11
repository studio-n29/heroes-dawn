#nullable enable
namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Atelier de fabrication : recherches, recettes, panneau de fabrication (UI Toolkit).</summary>
    public sealed class LabPageView : HdListPageView, ILabPageView
    {
        /// <summary>Tests PlayMode : sélectionne une rangée comme le ferait un toucher.</summary>
        public void SelectRowForTest(int row) => RaiseRowSelected(row);
    }
}
