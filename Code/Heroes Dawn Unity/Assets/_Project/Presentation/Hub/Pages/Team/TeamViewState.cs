#nullable enable

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Filtre et tri de l'écran Équipe : vivent dans le scope du hub (Singleton), pas dans la page, qui est recréée à chaque retour
    /// (spec §3 : l'état utile vit hors de la vue).
    /// </summary>
    public sealed class TeamViewState
    {
        /// <summary>0 = toutes les familles, 1 + (int)Family sinon.</summary>
        public int FilterIndex { get; set; }
        public MemberSort Sort { get; set; } = MemberSort.Level;
    }
}
