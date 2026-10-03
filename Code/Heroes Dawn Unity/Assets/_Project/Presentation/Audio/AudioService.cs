#nullable enable
using System;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;
using VContainer.Unity;
using ILogger = N29.HeroesDawn.Application.Ports.ILogger;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Audio du jeu, pensé pour le mobile : sources créées une fois (<see cref="AudioRig"/>), tables indexées par identifiant
    /// (aucune allocation ni dictionnaire par appel), musique en streaming déchargée après chaque fondu, voix d'effets bornées.
    /// Musique : deux platines en fondu enchaîné à puissance constante ; les pistes d'un thème s'enchaînent avant leur fin
    /// (variété sans coupure), un thème peut reprendre là où il a été quitté, et la musique s'atténue sous les modales et les effets marquants.
    /// </summary>
    public sealed class AudioService : IAudioService, ITickable, IDisposable
    {
        sealed class Deck
        {
            public AudioSource Source = null!;
            public Fade Gain = new Fade(0f);
            public AudioCatalog.Music? Music;
            public int Track;
            public bool Stopping;
            public float StartedAt;
        }

        const float StingerAttack = 0.12f;
        const float StingerRelease = 0.9f;
        const float ResumeFadeIn = 0.6f;

        readonly AudioRig _rig;
        readonly IFrameClock _clock;
        readonly AudioCatalog.Music?[] _music;
        readonly AudioCatalog.Sfx?[] _sfx;
        readonly Deck[] _decks = new Deck[2];
        readonly int[] _nextTrack;
        readonly int[] _resumeTrack;
        readonly float[] _resumeTime;
        readonly float[] _duckLevels = { 1f, 1f };
        readonly VoicePool _pool;
        readonly bool[] _playing;
        readonly System.Random _rng = new System.Random();
        int _active;
        Fade _duck = new Fade(1f);
        Fade _master = new Fade(1f);
        float _now;
        float _stingerUntil = -1f;
        float _musicVolume = 1f;
        float _sfxVolume = 1f;
        bool _dirty;

        public MusicId CurrentMusic { get; private set; }

        public float MusicVolume
        {
            get => _musicVolume;
            set { _musicVolume = Mathf.Clamp01(value); _dirty = true; }
        }

        public float SfxVolume
        {
            get => _sfxVolume;
            set => _sfxVolume = Mathf.Clamp01(value);
        }

        public AudioService(AudioRig rig, AudioCatalog? catalog, IFrameClock clock, ILogger log)
        {
            _rig = rig;
            _clock = clock;
            if (catalog == null) log.Warn("AudioCatalog manquant sur RootLifetimeScope : jeu muet (Heroes Dawn ▸ Setup ▸ Audio ▸ Reconstruire le catalogue).");
            _music = Index(catalog != null ? catalog.MusicEntries : Array.Empty<AudioCatalog.Music>(), m => (int)m.Id);
            _sfx = Index(catalog != null ? catalog.SfxEntries : Array.Empty<AudioCatalog.Sfx>(), s => (int)s.Id);
            _nextTrack = new int[_music.Length];
            _resumeTrack = new int[_music.Length];
            _resumeTime = new float[_music.Length];
            for (int i = 0; i < _music.Length; i++)
            {
                _resumeTrack[i] = -1;
                var tracks = _music[i]?.Tracks;
                if (tracks != null && tracks.Length > 1) _nextTrack[i] = _rng.Next(tracks.Length); // première piste variable d'une partie à l'autre
            }
            for (int i = 0; i < 2; i++) _decks[i] = new Deck { Source = rig.Decks[i] };
            _pool = new VoicePool(rig.Voices.Length, _sfx.Length);
            _playing = new bool[rig.Voices.Length];
            _rig.Paused += OnPaused;
        }

        static T?[] Index<T>(T[] entries, Func<T, int> key) where T : class
        {
            int size = 0;
            foreach (var e in entries) if (e != null) size = Math.Max(size, key(e) + 1);
            var table = new T?[size];
            foreach (var e in entries) if (e != null && key(e) >= 0) table[key(e)] = e;
            return table;
        }

        // ------------------------------------------------------------------ Musique

        public void PlayMusic(MusicId music, float fadeSeconds = -1f, bool resume = false)
        {
            var current = _decks[_active];
            if (music == CurrentMusic && current.Source.isPlaying && !current.Stopping) return;
            CurrentMusic = music;
            int id = (int)music;
            var entry = id > 0 && id < _music.Length ? _music[id] : null;
            if (entry == null || entry.Tracks.Length == 0)
            {
                StopMusic(fadeSeconds < 0f ? 1f : fadeSeconds);
                CurrentMusic = music;
                return;
            }
            float fade = fadeSeconds < 0f ? entry.FadeIn : fadeSeconds;
            int track;
            float at = 0f;
            if (resume && entry.ResumePosition && _resumeTrack[id] >= 0)
            {
                track = _resumeTrack[id];
                at = _resumeTime[id];
            }
            else
            {
                track = _nextTrack[id] % entry.Tracks.Length;
                _nextTrack[id] = track + 1;
            }
            _resumeTrack[id] = -1;
            CrossfadeTo(entry, track, at, fade);
        }

        public void StopMusic(float fadeSeconds)
        {
            CurrentMusic = MusicId.None;
            foreach (var d in _decks) FadeOut(d, fadeSeconds);
            _dirty = true;
        }

        void CrossfadeTo(AudioCatalog.Music entry, int track, float at, float fade)
        {
            FadeOut(_decks[_active], fade);
            _active = 1 - _active;
            var d = _decks[_active];
            if (d.Source.isPlaying) Release(d); // troisième thème pendant un fondu : la platine la plus ancienne est coupée
            var clip = entry.Tracks[track].Clip;
            if (clip == null) return;
            d.Music = entry;
            d.Track = track;
            d.Stopping = false;
            d.StartedAt = _now;
            d.Gain = new Fade(0f);
            d.Gain.To(1f, fade);
            d.Source.clip = clip;
            d.Source.volume = 0f;
            d.Source.Play();
            if (at > 0f && at < clip.length) d.Source.time = at;
            _dirty = true;
        }

        void FadeOut(Deck d, float fade)
        {
            if (d.Music == null || d.Stopping) return;
            // Mémorise la position pour une reprise (combat → plateau).
            int id = (int)d.Music.Id;
            if (d.Music.ResumePosition && d.Source.isPlaying)
            {
                _resumeTrack[id] = d.Track;
                _resumeTime[id] = d.Source.time;
            }
            d.Stopping = true;
            d.Gain.To(0f, fade);
            if (!d.Gain.Active) Release(d);
        }

        /// <summary>Arrête la platine et libère le flux de la piste (mémoire mobile).</summary>
        void Release(Deck d)
        {
            var clip = d.Source.clip;
            d.Source.Stop();
            d.Source.clip = null;
            d.Music = null;
            d.Stopping = false;
            if (clip != null && _decks[0].Source.clip != clip && _decks[1].Source.clip != clip) clip.UnloadAudioData();
        }

        public void Duck(DuckLayer layer, float level, float fadeSeconds)
        {
            _duckLevels[(int)layer] = Mathf.Clamp01(level);
            float target = Mathf.Min(_duckLevels[0], _duckLevels[1]);
            if (Math.Abs(target - _duck.Target) > 0.001f) _duck.To(target, fadeSeconds);
            _dirty = true;
        }

        // ------------------------------------------------------------------ Effets

        public void Play(SfxId sfx, float volume = 1f)
        {
            int id = (int)sfx;
            var e = id > 0 && id < _sfx.Length ? _sfx[id] : null;
            if (e == null || e.Clips.Length == 0 || _sfxVolume <= 0f) return;
            var voices = _rig.Voices;
            for (int i = 0; i < voices.Length; i++) _playing[i] = voices[i].isPlaying;
            int v = _pool.Acquire(sfx, e.Priority, e.MaxInstances, e.Cooldown, _now, _playing);
            if (v < 0) return;
            var clip = e.Clips[e.Clips.Length == 1 ? 0 : _rng.Next(e.Clips.Length)];
            if (clip == null) return;
            var s = voices[v];
            float pitch = 1f + e.PitchJitter * (float)(_rng.NextDouble() * 2.0 - 1.0);
            s.Stop();
            s.clip = clip;
            s.pitch = pitch;
            s.volume = e.Volume * volume * _sfxVolume * _master.Value;
            s.Play();
            if (e.DuckMusicTo < 1f)
            {
                Duck(DuckLayer.Stinger, e.DuckMusicTo, StingerAttack);
                _stingerUntil = Math.Max(_stingerUntil, _now + clip.length / pitch);
            }
        }

        // ------------------------------------------------------------------ Boucle

        public void Tick()
        {
            float dt = _clock.UnscaledDeltaTime;
            _now += dt;
            bool changed = _dirty;
            _dirty = false;
            changed |= _duck.Step(dt);
            changed |= _master.Step(dt);
            for (int i = 0; i < 2; i++)
            {
                var d = _decks[i];
                if (d.Music == null) continue;
                changed |= d.Gain.Step(dt);
                if (d.Stopping && !d.Gain.Active) Release(d);
            }
            if (_stingerUntil >= 0f && _now >= _stingerUntil)
            {
                _stingerUntil = -1f;
                Duck(DuckLayer.Stinger, 1f, StingerRelease);
            }
            ChainTrackIfEnding();
            if (changed) ApplyMusicVolumes();
        }

        /// <summary>Avant la fin d'une piste (silence final ignoré), enchaîne la suivante du thème en fondu : la musique ne s'arrête jamais.</summary>
        void ChainTrackIfEnding()
        {
            var d = _decks[_active];
            var m = d.Music;
            if (m == null || d.Stopping) return;
            var clip = d.Source.clip;
            if (clip == null) return;
            float end = clip.length - m.Tracks[d.Track].TrimEnd - m.TrackCrossfade;
            // Piste finie d'elle-même : seulement après le démarrage du flux, qui peut prendre quelques images.
            bool ended = !d.Source.isPlaying && _now - d.StartedAt > 1f;
            if (ended || d.Source.time >= end)
            {
                int next = (d.Track + 1) % m.Tracks.Length;
                _nextTrack[(int)m.Id] = next + 1;
                CrossfadeTo(m, next, 0f, m.TrackCrossfade);
            }
        }

        void ApplyMusicVolumes()
        {
            float bus = _musicVolume * _duck.Value * _master.Value;
            foreach (var d in _decks)
                if (d.Music != null) d.Source.volume = d.Gain.Value * d.Music.Volume * bus;
        }

        /// <summary>Retour d'arrière-plan : remontée douce plutôt qu'un son à plein volume.</summary>
        void OnPaused(bool paused)
        {
            if (paused) return;
            _master = new Fade(0f);
            _master.To(1f, ResumeFadeIn);
            _dirty = true;
        }

        public void Dispose() => _rig.Paused -= OnPaused;
    }
}
