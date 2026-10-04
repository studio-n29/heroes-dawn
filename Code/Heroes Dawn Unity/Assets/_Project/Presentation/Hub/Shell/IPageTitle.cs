namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Titre affiché dans la barre haute pendant que la page est ouverte (implémenté par chaque présentateur de page).</summary>
    public interface IPageTitle
    {
        string Title { get; }
    }
}
