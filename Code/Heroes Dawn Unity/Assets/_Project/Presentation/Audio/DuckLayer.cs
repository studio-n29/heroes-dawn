namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Calques d'atténuation de la musique : le plus bas l'emporte.</summary>
    public enum DuckLayer
    {
        /// <summary>Une modale est ouverte.</summary>
        Modal = 0,
        /// <summary>Un effet marquant (victoire, recrutement) passe devant la musique.</summary>
        Stinger = 1,
        /// <summary>Une réplique doublée est en cours : la musique baisse pour laisser passer la voix.</summary>
        Voice = 2,
    }
}
