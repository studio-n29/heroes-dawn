#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Audio;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Audio enregistreur : musiques demandées, effets joués, atténuations.</summary>
    public sealed class FakeAudio : IAudioService
    {
        public readonly List<(MusicId Music, bool Resume)> MusicRequests = new List<(MusicId, bool)>();
        public readonly List<SfxId> Played = new List<SfxId>();
        public readonly List<(DuckLayer Layer, float Level)> Ducks = new List<(DuckLayer, float)>();

        public MusicId CurrentMusic { get; private set; }
        public float MusicVolume { get; set; } = 1f;
        public float SfxVolume { get; set; } = 1f;

        public void PlayMusic(MusicId music, float fadeSeconds = -1f, bool resume = false)
        {
            CurrentMusic = music;
            MusicRequests.Add((music, resume));
        }

        public void StopMusic(float fadeSeconds) => CurrentMusic = MusicId.None;
        public void Duck(DuckLayer layer, float level, float fadeSeconds) => Ducks.Add((layer, level));
        public void Play(SfxId sfx, float volume = 1f) => Played.Add(sfx);
    }
}
