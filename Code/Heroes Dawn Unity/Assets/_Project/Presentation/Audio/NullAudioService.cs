namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Audio muet : présentateurs construits sans service (tests), catalogue absent.</summary>
    public sealed class NullAudioService : IAudioService
    {
        public static readonly NullAudioService Instance = new NullAudioService();

        public MusicId CurrentMusic => MusicId.None;
        public float MusicVolume { get => 0f; set { } }
        public float SfxVolume { get => 0f; set { } }
        public void PlayMusic(MusicId music, float fadeSeconds = -1f, bool resume = false) { }
        public void StopMusic(float fadeSeconds) { }
        public void Duck(DuckLayer layer, float level, float fadeSeconds) { }
        public void Play(SfxId sfx, float volume = 1f) { }
    }
}
