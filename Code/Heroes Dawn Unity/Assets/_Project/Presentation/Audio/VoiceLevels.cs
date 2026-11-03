#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Niveaux des voix : crête visée après normalisation et baisse de la musique pendant une réplique.</summary>
    public static class VoiceLevels
    {
        /// <summary>Crête visée pour chaque réplique (les fichiers ElevenLabs n'ont pas tous le même niveau).</summary>
        public const float TargetPeak = 0.9f;
        /// <summary>Niveau de la musique pendant une réplique doublée (1 = normal).</summary>
        public const float MusicDuck = 0.3f;
        public const float DuckAttack = 0.2f;
        public const float DuckRelease = 0.6f;
        const float MinGain = 0.25f, MaxGain = 4f;

        /// <summary>Gain qui ramène la crête d'un clip à <see cref="TargetPeak"/> (borné pour ne pas gonfler un bruit ni saturer).</summary>
        public static float GainFor(float peak) => peak <= 0.0001f ? 1f : Mathf.Clamp(TargetPeak / peak, MinGain, MaxGain);

        /// <summary>Nom de fichier d'une réplique : la clé de texte avec des soulignés (« narration_ny_1_3_n18_before_0 »).</summary>
        public static string FileName(string lineKey) => lineKey.Replace('.', '_');

        /// <summary>Chemin Resources d'une réplique dans la langue donnée (« fr » ou « en »).</summary>
        public static string ResourcePath(string language, string lineKey) => "Voices/" + language + "/" + FileName(lineKey);
    }
}
