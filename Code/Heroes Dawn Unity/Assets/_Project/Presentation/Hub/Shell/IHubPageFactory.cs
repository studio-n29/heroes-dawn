namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Crée la page d'un emplacement du hub (vue + présentateur).</summary>
    public interface IHubPageFactory
    {
        IHubPageHandle Create(HubLocation location);
    }
}
