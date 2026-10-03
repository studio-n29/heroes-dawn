#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Musiques et effets du jeu (Settings/AudioCatalog.asset), rempli par Heroes Dawn ▸ Setup ▸ Audio ▸ Reconstruire le catalogue
    /// depuis les dossiers <c>Audio/Music/&lt;MusicId&gt;</c> et <c>Audio/Sfx/&lt;SfxId&gt;</c> ; les réglages (volumes, délais…)
    /// se retouchent dans l'inspecteur et survivent à la reconstruction. Jamais modifié à l'exécution (règle 9).
    /// </summary>
    [CreateAssetMenu(menuName = "Heroes Dawn/Audio Catalog", fileName = "AudioCatalog")]
    public sealed class AudioCatalog : ScriptableObject
    {
        [Serializable]
        public sealed class Track
        {
            [SerializeField] AudioClip? clip;
            [Tooltip("Secondes de silence ou de fin à ignorer : la piste suivante démarre avant.")]
            [SerializeField, Min(0f)] float trimEnd;

            public AudioClip? Clip => clip;
            public float TrimEnd => trimEnd;

            public Track() { }
            public Track(AudioClip? clip, float trimEnd) { this.clip = clip; this.trimEnd = trimEnd; }
        }

        [Serializable]
        public sealed class Music
        {
            [SerializeField] MusicId id;
            [SerializeField] Track[] tracks = Array.Empty<Track>();
            [SerializeField, Range(0f, 1f)] float volume = 0.8f;
            [Tooltip("Durée du fondu enchaîné quand ce thème arrive (secondes).")]
            [SerializeField, Min(0f)] float fadeIn = 2f;
            [Tooltip("Durée du fondu entre deux pistes du thème.")]
            [SerializeField, Min(0f)] float trackCrossfade = 4f;
            [Tooltip("Reprendre la piste là où elle a été quittée (retour du combat au plateau).")]
            [SerializeField] bool resumePosition;

            public MusicId Id => id;
            public Track[] Tracks => tracks;
            public float Volume => volume;
            public float FadeIn => fadeIn;
            public float TrackCrossfade => trackCrossfade;
            public bool ResumePosition => resumePosition;

            public Music() { }

#if UNITY_EDITOR
            public Music(MusicId id) { this.id = id; }
            public void SetTracks(Track[] value) => tracks = value;
#endif
        }

        [Serializable]
        public sealed class Sfx
        {
            [SerializeField] SfxId id;
            [SerializeField] AudioClip[] clips = Array.Empty<AudioClip>();
            [SerializeField, Range(0f, 1f)] float volume = 0.8f;
            [Tooltip("Variation aléatoire de hauteur (± fraction) : évite l'effet mitraillette.")]
            [SerializeField, Range(0f, 0.3f)] float pitchJitter = 0.05f;
            [Tooltip("Délai minimal entre deux déclenchements (secondes).")]
            [SerializeField, Min(0f)] float cooldown = 0.05f;
            [SerializeField, Min(1)] int maxInstances = 2;
            [Tooltip("Plus grand = plus important (peut voler la voix d'un effet moins important).")]
            [SerializeField, Range(0, 255)] int priority = 128;
            [Tooltip("Atténue la musique à ce niveau pendant l'effet (1 = pas d'atténuation).")]
            [SerializeField, Range(0f, 1f)] float duckMusicTo = 1f;

            public SfxId Id => id;
            public AudioClip[] Clips => clips;
            public float Volume => volume;
            public float PitchJitter => pitchJitter;
            public float Cooldown => cooldown;
            public int MaxInstances => maxInstances;
            public int Priority => priority;
            public float DuckMusicTo => duckMusicTo;

            public Sfx() { }

#if UNITY_EDITOR
            public Sfx(SfxId id) { this.id = id; }
            public void SetClips(AudioClip[] value) => clips = value;
#endif
        }

        [Tooltip("Voix d'effets simultanées (mobile : 8 à 12).")]
        [SerializeField, Range(4, 24)] int voices = 10;
        [SerializeField] Music[] music = Array.Empty<Music>();
        [SerializeField] Sfx[] sfx = Array.Empty<Sfx>();

        public int Voices => voices;
        public Music[] MusicEntries => music;
        public Sfx[] SfxEntries => sfx;

#if UNITY_EDITOR
        /// <summary>Remplissage par l'outil de reconstruction (éditeur).</summary>
        public void Fill(Music[] musicEntries, Sfx[] sfxEntries)
        {
            music = musicEntries;
            sfx = sfxEntries;
        }
#endif
    }
}
