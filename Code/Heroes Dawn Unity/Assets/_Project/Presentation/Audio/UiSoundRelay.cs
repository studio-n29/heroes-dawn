#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Point d'accès audio d'une hiérarchie d'interface (racine de Boot, zone d'écran de chaque scène) : les boutons des prefabs
    /// le trouvent parmi leurs parents, sans injection ni accès global. Branché par le scope de la scène.
    /// </summary>
    public sealed class UiSoundRelay : MonoBehaviour
    {
        IAudioService? _audio;

        public void Bind(IAudioService audio) => _audio = audio;

        public void Play(SfxId sfx) => _audio?.Play(sfx);

        /// <summary>Relais de <paramref name="host"/> (créé au besoin) relié à <paramref name="audio"/>.</summary>
        public static void Attach(GameObject host, IAudioService audio)
        {
            var relay = host.GetComponent<UiSoundRelay>();
            if (relay == null) relay = host.AddComponent<UiSoundRelay>();
            relay.Bind(audio);
        }
    }
}
