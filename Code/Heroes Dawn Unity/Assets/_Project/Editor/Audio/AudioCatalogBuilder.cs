#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Presentation.Audio;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Audio
{
    /// <summary>
    /// Reconstruit Settings/AudioCatalog.asset depuis les dossiers : <c>Audio/Music/&lt;MusicId&gt;/</c> (pistes d'un thème, par ordre
    /// alphabétique) et <c>Audio/Sfx/&lt;SfxId&gt;/</c> (variantes d'un effet). Ajouter un son = déposer un fichier, aucun code.
    /// Les réglages déjà saisis (volumes, délais, fin ignorée…) sont conservés. Menu : Heroes Dawn ▸ Setup ▸ Audio ▸ Reconstruire le catalogue.
    /// </summary>
    public static class AudioCatalogBuilder
    {
        public const string CatalogPath = "Assets/_Project/Settings/AudioCatalog.asset";

        [MenuItem("Heroes Dawn/Setup/Audio/Reconstruire le catalogue")]
        static void RebuildFromMenu()
        {
            var report = Rebuild();
            EditorUtility.DisplayDialog("Catalogue audio", report, "OK");
        }

        [MenuItem("Heroes Dawn/Setup/Audio/Réappliquer les réglages d'import mobile")]
        static void ReapplyImportSettings()
        {
            int count = 0;
            foreach (var path in ClipPaths("Assets/_Project/Audio"))
            {
                if (!(AssetImporter.GetAtPath(path) is AudioImporter importer)) continue;
                if (path.StartsWith(AudioImportRules.MusicFolder)) AudioImportRules.ApplyMusic(importer);
                else if (path.StartsWith(AudioImportRules.SfxFolder)) AudioImportRules.ApplySfx(importer);
                else continue;
                importer.SaveAndReimport();
                count++;
            }
            Debug.Log($"[Heroes Dawn] Réglages d'import audio réappliqués : {count} son(s).");
        }

        /// <summary>Catalogue existant, créé vide au besoin (branché sur RootLifetimeScope par le générateur de scènes).</summary>
        public static AudioCatalog EnsureCatalog()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AudioCatalog>(CatalogPath);
            if (catalog != null) return catalog;
            catalog = ScriptableObject.CreateInstance<AudioCatalog>();
            AssetDatabase.CreateAsset(catalog, CatalogPath);
            return catalog;
        }

        /// <summary>Reconstruction sans interaction (batchmode : -executeMethod … AudioCatalogBuilder.RebuildBatch -quit).</summary>
        public static void RebuildBatch() => Debug.Log("[Heroes Dawn] " + Rebuild());

        public static string Rebuild()
        {
            var catalog = EnsureCatalog();
            var music = new List<AudioCatalog.Music>();
            var sfx = new List<AudioCatalog.Sfx>();
            var missing = new List<string>();

            foreach (MusicId id in Enum.GetValues(typeof(MusicId)))
            {
                if (id == MusicId.None) continue;
                var clips = Clips("Assets/_Project/Audio/Music/" + id);
                if (clips.Length == 0) { missing.Add("musique " + id); continue; }
                var entry = catalog.MusicEntries.FirstOrDefault(m => m != null && m.Id == id) ?? new AudioCatalog.Music(id);
                entry.SetTracks(clips.Select(c => new AudioCatalog.Track(c, entry.Tracks.FirstOrDefault(t => t.Clip == c)?.TrimEnd ?? 0f)).ToArray());
                music.Add(entry);
            }
            foreach (SfxId id in Enum.GetValues(typeof(SfxId)))
            {
                if (id == SfxId.None) continue;
                var clips = Clips("Assets/_Project/Audio/Sfx/" + id);
                if (clips.Length == 0) { missing.Add("effet " + id); continue; }
                var entry = catalog.SfxEntries.FirstOrDefault(s => s != null && s.Id == id) ?? new AudioCatalog.Sfx(id);
                entry.SetClips(clips);
                sfx.Add(entry);
            }

            catalog.Fill(music.ToArray(), sfx.ToArray());
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            return $"{music.Count} thème(s), {sfx.Count} effet(s)." + (missing.Count > 0 ? "\nSans fichier : " + string.Join(", ", missing) : "");
        }

        static AudioClip[] Clips(string folder)
            => ClipPaths(folder).Select(AssetDatabase.LoadAssetAtPath<AudioClip>).Where(c => c != null).ToArray();

        static IEnumerable<string> ClipPaths(string folder)
        {
            if (!AssetDatabase.IsValidFolder(folder)) return Array.Empty<string>();
            return AssetDatabase.FindAssets("t:AudioClip", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .OrderBy(p => Path.GetFileName(p), StringComparer.Ordinal);
        }
    }
}
