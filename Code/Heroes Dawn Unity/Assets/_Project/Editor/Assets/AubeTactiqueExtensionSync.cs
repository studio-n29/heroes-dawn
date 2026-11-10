#nullable enable
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Dossier d'extension Aube tactique (agents détourés, icônes de compétence, objets, composants, mini-jeux…) :
    /// import en sprites (9-slice pour les composants extensibles) puis copie dans Resources/UI/AubeTactique/Ext pour que le jeu puisse les charger.
    /// </summary>
    public static class AubeTactiqueExtensionSync
    {
        public const string Source = "Assets/_Project/Art/UI/AubeTactiqueExtension";
        public const string Target = "Assets/_Project/Resources/UI/AubeTactique/Ext";

        /// <summary>Composants extensibles : bordures 9-slice (gauche, bas, droite, haut) en pixels.</summary>
        static readonly Dictionary<string, Vector4> Borders = new Dictionary<string, Vector4>
        {
            { "Components/objectives-paper", new Vector4(48, 40, 48, 48) },
            { "Components/reward-tile", new Vector4(40, 40, 40, 40) },
            { "Components/leaderboard-row", new Vector4(48, 24, 48, 24) },
            { "Components/leaderboard-player", new Vector4(48, 24, 48, 24) },
            { "Components/node-detail", new Vector4(48, 40, 48, 40) },
            { "Components/spiral-notebook", new Vector4(48, 48, 48, 48) },
        };

        public static int Run()
        {
            if (!Directory.Exists(Source)) return 0;
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            int count = 0;
            foreach (var file in Directory.GetFiles(Source, "*.png", SearchOption.AllDirectories))
            {
                string path = file.Replace('\\', '/');
                string relative = path.Substring(Source.Length + 1);
                string dest = Target + "/" + relative;
                Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
                Configure(path, relative);
                // Copie plutôt que déplacement : le dossier d'extension reste la source de travail de la direction artistique.
                if (File.Exists(dest) && new FileInfo(dest).Length == new FileInfo(path).Length) { Configure(dest, relative); count++; continue; } // copie déjà à jour
                if (File.Exists(dest)) AssetDatabase.DeleteAsset(dest);
                if (!AssetDatabase.CopyAsset(path, dest)) continue;
                Configure(dest, relative);
                count++;
            }
            AssetDatabase.SaveAssets();
            Debug.Log("[Heroes Dawn] Extension Aube tactique : " + count + " sprites importés et copiés dans Resources.");
            return count;
        }

        static void Configure(string path, string relative)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            var wantedBorder = Borders.TryGetValue(relative.Replace(".png", ""), out var wb) ? wb : Vector4.zero;
            if (importer.textureType == TextureImporterType.Sprite && importer.spriteBorder == wantedBorder && importer.spritePixelsPerUnit == 100) return; // déjà configuré
            string key = relative.Replace(".png", "");
            bool photo = relative.StartsWith("Skills/") || relative.StartsWith("Dossiers/") || relative.StartsWith("Cards/");
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = !photo;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.textureCompression = photo || relative.StartsWith("Agents/") ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
            importer.spriteBorder = Borders.TryGetValue(key, out var b) ? b : Vector4.zero;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}
