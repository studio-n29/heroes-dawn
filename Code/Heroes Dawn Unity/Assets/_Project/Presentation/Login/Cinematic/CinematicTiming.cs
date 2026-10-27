#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>
    /// Durée d'un plan et visibilité de sa légende : avec une voix off, ils suivent la voix ; sans voix (cas actuel : les voix seront
    /// enregistrées plus tard), la légende reste le temps de la lire.
    /// </summary>
    public static class CinematicTiming
    {
        /// <summary>Délai avant le début de la voix ou de la légende, en secondes.</summary>
        public const float VoiceStart = 0.4f;
        const float ReadBase = 1.2f, ReadPerWord = 0.35f, Tail = 0.4f;

        public static int Words(string text)
            => string.IsNullOrWhiteSpace(text) ? 0 : text.Split((char[]?)null, System.StringSplitOptions.RemoveEmptyEntries).Length;

        /// <summary>Durée du plan : sa durée minimale, allongée par la voix ou, sans voix, par le temps de lecture de la légende.</summary>
        public static float ShotSeconds(float minSeconds, float voiceLength, string subtitle)
        {
            if (voiceLength > 0f) return Mathf.Max(minSeconds, VoiceStart + voiceLength + Tail);
            int words = Words(subtitle);
            return words == 0 ? minSeconds : Mathf.Max(minSeconds, ReadBase + ReadPerWord * words);
        }

        /// <summary>Opacité de la légende à l'instant <paramref name="t"/> du plan : fondu d'entrée, lisible, fondu de sortie avant la fin.</summary>
        public static float SubtitleAlpha(float t, float voiceLength, float shotSeconds)
        {
            float local = t - VoiceStart;
            if (local < 0f) return 0f;
            float end = voiceLength > 0f ? voiceLength + Tail : shotSeconds - VoiceStart - 0.2f;
            return Mathf.Clamp01(local / 0.3f) * Mathf.Clamp01((end - local) / Tail);
        }
    }
}
