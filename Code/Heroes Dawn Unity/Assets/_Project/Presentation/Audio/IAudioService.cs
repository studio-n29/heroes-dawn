namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Audio du jeu (doc 07 §Audio) : musique par état avec fondus enchaînés, effets sonores, atténuation.
    /// Appelé par les présentateurs et les relais d'interface, jamais par le domaine.
    /// </summary>
    public interface IAudioService
    {
        MusicId CurrentMusic { get; }

        /// <summary>
        /// Enchaîne vers un thème (fondu à puissance constante). Même thème : rien. <paramref name="fadeSeconds"/> négatif :
        /// durée du catalogue. <paramref name="resume"/> : reprend la piste là où elle a été quittée si le thème l'autorise.
        /// </summary>
        void PlayMusic(MusicId music, float fadeSeconds = -1f, bool resume = false);

        void StopMusic(float fadeSeconds);

        /// <summary>Atténue la musique (1 = normal) sur un calque, avec fondu.</summary>
        void Duck(DuckLayer layer, float level, float fadeSeconds);

        /// <summary>Joue un effet (variante et hauteur tirées au hasard), sauf si sa limite de voix ou son délai l'interdit.</summary>
        void Play(SfxId sfx, float volume = 1f);

        float MusicVolume { get; set; }
        float SfxVolume { get; set; }
    }
}
