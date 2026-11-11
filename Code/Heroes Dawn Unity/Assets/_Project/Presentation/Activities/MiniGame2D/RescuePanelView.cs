#nullable enable
namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Pilote qui a besoin du panneau de secours : la fenêtre le lui remet avant le début de la partie.</summary>
    public interface IRescuePanelUser
    {
        void Bind(RescuePanelHud panel);
    }
}
