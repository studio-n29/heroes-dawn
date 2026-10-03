#nullable enable
namespace N29.HeroesDawn.Application.Flow
{
    /// <summary>Écrans qui vivent dans la scène Boot (connexion), affichés ou masqués par la machine d'états.</summary>
    public interface IScreenVisibility
    {
        void SetLoginVisible(bool visible);
    }
}
