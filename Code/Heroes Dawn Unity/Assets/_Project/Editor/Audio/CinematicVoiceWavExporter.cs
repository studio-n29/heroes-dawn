#nullable enable
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Audio
{
    /// <summary>
    /// Convertit les voix off MP3 de la cinématique vidéo en WAV PCM 16 bits dans <c>Resources/CinematicVideo/Voice</c>, nom attendu par le lecteur.
    /// Distinct de <c>Resources/Cinematic</c>, réservé à l'ancienne cinématique procédurale.
    /// Le point d'un plan noir devient un tiret (<c>vo_S05.5.mp3</c> → <c>vo_S05-5.wav</c>) : <c>Resources.Load</c> prendrait le point pour une extension.
    /// Le dossier source est retiré du projet une fois la conversion faite, pour ne pas doubler les fichiers dans le build.
    /// </summary>
    public static class CinematicVoiceWavExporter
    {
        const string SourceFolder = "Assets/_Project/Audio/Cinematic/VoiceSource";
        const string TargetFolder = "Assets/_Project/Resources/CinematicVideo/Voice";

        [MenuItem("Heroes Dawn/Cinématique/Convertir les voix off en WAV")]
        public static void Export()
        {
            if (!AssetDatabase.IsValidFolder(SourceFolder))
            {
                Debug.LogWarning($"Aucun dossier de voix off à convertir : {SourceFolder}");
                return;
            }

            Directory.CreateDirectory(TargetFolder);
            int count = 0;
            foreach (var file in Directory.GetFiles(SourceFolder, "*.mp3"))
            {
                string sourcePath = $"{SourceFolder}/{Path.GetFileName(file)}";
                if (ConvertOne(sourcePath)) count++;
            }

            AssetDatabase.DeleteAsset(SourceFolder);
            AssetDatabase.Refresh();
            Debug.Log($"Voix off converties en WAV : {count} fichier(s) dans {TargetFolder}");
        }

        /// <summary>Point d'entrée batchmode (<c>-executeMethod</c>).</summary>
        public static void ExportFromBatch() => Export();

        static bool ConvertOne(string sourcePath)
        {
            var importer = AssetImporter.GetAtPath(sourcePath) as AudioImporter;
            if (importer == null) return false;

            var settings = importer.defaultSampleSettings;
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            importer.defaultSampleSettings = settings;
            importer.SaveAndReimport();

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(sourcePath);
            if (clip == null) return false;

            var samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            string stem = Path.GetFileNameWithoutExtension(sourcePath).Replace('.', '-');
            WriteWav(Path.Combine(TargetFolder, stem + ".wav"), samples, clip.channels, clip.frequency);
            return true;
        }

        static void WriteWav(string path, float[] samples, int channels, int sampleRate)
        {
            int dataSize = samples.Length * 2;
            using var stream = File.Create(path);
            using var writer = new BinaryWriter(stream);

            writer.Write(Encoding.ASCII.GetBytes("RIFF"));
            writer.Write(36 + dataSize);
            writer.Write(Encoding.ASCII.GetBytes("WAVE"));
            writer.Write(Encoding.ASCII.GetBytes("fmt "));
            writer.Write(16);
            writer.Write((short)1);
            writer.Write((short)channels);
            writer.Write(sampleRate);
            writer.Write(sampleRate * channels * 2);
            writer.Write((short)(channels * 2));
            writer.Write((short)16);
            writer.Write(Encoding.ASCII.GetBytes("data"));
            writer.Write(dataSize);

            foreach (var sample in samples)
                writer.Write((short)Mathf.Clamp(Mathf.RoundToInt(sample * 32767f), -32768, 32767));
        }
    }
}
