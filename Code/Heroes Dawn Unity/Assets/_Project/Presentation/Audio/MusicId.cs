namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Thèmes musicaux (doc 02_Direction_Artistique_Audio). Valeurs sérialisées dans AudioCatalog : ne jamais renuméroter.
    /// Le dossier <c>Audio/Music/&lt;nom&gt;/</c> contient les pistes du thème (jouées à tour de rôle).
    /// </summary>
    public enum MusicId
    {
        None = 0,
        MainTheme = 1,
        Headquarters = 2,
        Board = 3,
        Combat = 4,
        Boss = 5,
        Cinematic = 6,
    }
}
