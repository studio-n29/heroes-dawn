#nullable enable
using System;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Sources audio du jeu, créées une fois au démarrage (aucune instanciation ensuite) : deux platines de musique
    /// pour les fondus enchaînés et un nombre fixe de voix d'effets, toutes en 2D. Porte l'écouteur de la scène Boot.
    /// </summary>
    public sealed class AudioRig : MonoBehaviour
    {
        public AudioSource[] Decks { get; private set; } = Array.Empty<AudioSource>();
        public AudioSource[] Voices { get; private set; } = Array.Empty<AudioSource>();

        /// <summary>Application mise en arrière-plan (vrai) ou reprise (faux).</summary>
        public event Action<bool>? Paused;

        public static AudioRig Create(Transform parent, int voices)
        {
            var go = new GameObject("Audio");
            go.transform.SetParent(parent, false);
            go.AddComponent<AudioListener>();
            var rig = go.AddComponent<AudioRig>();
            rig.Decks = new[] { NewSource(go, "Music A", 0), NewSource(go, "Music B", 0) };
            rig.Voices = new AudioSource[voices];
            for (int i = 0; i < voices; i++) rig.Voices[i] = NewSource(go, "Sfx", 64);
            return rig;
        }

        static AudioSource NewSource(GameObject host, string name, int priority)
        {
            var go = new GameObject(name);
            go.transform.SetParent(host.transform, false);
            var s = go.AddComponent<AudioSource>();
            s.playOnAwake = false;
            s.loop = false;
            s.spatialBlend = 0f;
            s.priority = priority; // musique (0) jamais virtualisée par Unity
            s.bypassReverbZones = true;
            s.bypassListenerEffects = true;
            s.dopplerLevel = 0f;
            s.volume = 0f;
            return s;
        }

        void OnApplicationPause(bool paused) => Paused?.Invoke(paused);
    }
}
