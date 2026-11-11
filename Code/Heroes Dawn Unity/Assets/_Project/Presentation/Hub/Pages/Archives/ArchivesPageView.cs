#nullable enable
namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Archives (M15, UI Toolkit) : liste des fiches à gauche, texte de la fiche choisie à droite.</summary>
    public sealed class ArchivesPageView : HdListPageView, IArchivesPageView
    {
        protected override bool WideSide => true;
    }
}
