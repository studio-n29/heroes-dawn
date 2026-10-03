namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Écran qui réagit à Retour (Échap, retour Android). Renvoie true si l'appui est consommé.</summary>
    public interface IBackHandler
    {
        bool HandleBack();
    }
}
