#nullable enable
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Audio
{
    /// <summary>
    /// Réglages d'import audio pensés pour le mobile, appliqués au premier import d'un son (les retouches de l'inspecteur sont conservées) :
    /// <list type="bullet">
    /// <item>Musique (<c>Audio/Music</c>) : streaming (quelques centaines de Ko en mémoire au lieu de ~30 Mo par piste), Vorbis qualité 50, 44,1 kHz, stéréo.</item>
    /// <item>Effets (<c>Audio/Sfx</c>) : décompressés au chargement en ADPCM (décodage quasi gratuit pour le processeur), mono, fréquence optimisée, préchargés.</item>
    /// </list>
    /// </summary>
    public sealed class AudioImportRules : AssetPostprocessor
    {
        public const string MusicFolder = "Assets/_Project/Audio/Music/";
        public const string SfxFolder = "Assets/_Project/Audio/Sfx/";

        void OnPreprocessAudio()
        {
            var importer = (AudioImporter)assetImporter;
            if (!importer.importSettingsMissing) return;
            if (assetPath.StartsWith(MusicFolder)) ApplyMusic(importer);
            else if (assetPath.StartsWith(SfxFolder)) ApplySfx(importer);
        }

        public static void ApplyMusic(AudioImporter importer)
        {
            var s = importer.defaultSampleSettings;
            s.loadType = AudioClipLoadType.Streaming;
            s.compressionFormat = AudioCompressionFormat.Vorbis;
            s.quality = 0.5f;
            s.sampleRateSetting = AudioSampleRateSetting.OverrideSampleRate;
            s.sampleRateOverride = 44100;
            s.preloadAudioData = false;
            importer.defaultSampleSettings = s;
            importer.forceToMono = false;
            importer.loadInBackground = true;
            importer.ambisonic = false;
        }

        public static void ApplySfx(AudioImporter importer)
        {
            var s = importer.defaultSampleSettings;
            s.loadType = AudioClipLoadType.DecompressOnLoad;
            s.compressionFormat = AudioCompressionFormat.ADPCM;
            s.sampleRateSetting = AudioSampleRateSetting.OptimizeSampleRate;
            s.preloadAudioData = true;
            importer.defaultSampleSettings = s;
            importer.forceToMono = true;
            importer.loadInBackground = false;
            importer.ambisonic = false;
        }
    }
}
