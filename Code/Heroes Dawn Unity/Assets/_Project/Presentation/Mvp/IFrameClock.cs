namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Durée de l'image écoulée (temps réel, indépendant de Time.timeScale) ; doublée dans les tests.</summary>
    public interface IFrameClock
    {
        float UnscaledDeltaTime { get; }
    }
}
