namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Appui sur Retour pendant l'image courante.</summary>
    public interface IBackInput
    {
        bool BackPressedThisFrame { get; }
    }
}
