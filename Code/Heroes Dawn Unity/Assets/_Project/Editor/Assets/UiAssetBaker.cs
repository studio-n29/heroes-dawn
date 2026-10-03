#nullable enable
using System;
using System.IO;
using N29.HeroesDawn.Presentation.Common;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Enregistre en assets ce que l'interface fabriquait au lancement : les sprites d'interface (Resources/UI, découpés en 9)
    /// et les illustrations stockées en octets (Art/UI, résolution d'origine). Un prefab ne peut référencer qu'un asset.
    /// Menu : Heroes Dawn ▸ Setup ▸ Générer les assets d'interface (exécutable en batchmode).
    /// </summary>
    public static class UiAssetBaker
    {
        public const string SpriteFolder = "Assets/_Project/Resources/UI";
        public const string ArtFolder = Sprites.ArtAssetFolder;
        const string ArtBytesFolder = "Assets/_Project/Resources/Art";

        [MenuItem("Heroes Dawn/Setup/Générer les assets d'interface")]
        public static void GenerateFromMenu()
        {
            BakeAll();
            FontAssetGenerator.GenerateAll();
            Debug.Log("[Heroes Dawn] Assets d'interface générés (sprites, illustrations, polices TMP).");
        }

        /// <summary>Écrit (ou réécrit) les sprites et les illustrations, puis règle leur import. Idempotent.</summary>
        public static void BakeAll()
        {
            Directory.CreateDirectory(SpriteFolder);
            Directory.CreateDirectory(ArtFolder);

            foreach (var name in Sprites.UiSpriteNames)
            {
                var sprite = Sprites.Procedural(name);
                var path = $"{SpriteFolder}/{name}.png";
                WriteIfChanged(path, sprite.texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(sprite.texture);
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                ConfigureSprite(path, sprite.border, mipmaps: false);
            }

            foreach (var bytesPath in Directory.GetFiles(ArtBytesFolder, "*.bytes"))
            {
                var stem = Path.GetFileNameWithoutExtension(bytesPath); // HD_Logo_png
                int dot = stem.LastIndexOf('_');
                var path = $"{ArtFolder}/{stem.Substring(0, dot)}.{stem.Substring(dot + 1)}";
                WriteIfChanged(path, File.ReadAllBytes(bytesPath));
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
                ConfigureSprite(path, Vector4.zero, mipmaps: true);
            }
            AssetDatabase.SaveAssets();
        }

        static void WriteIfChanged(string path, byte[] data)
        {
            if (File.Exists(path) && File.ReadAllBytes(path).AsSpan().SequenceEqual(data)) return;
            File.WriteAllBytes(path, data);
        }

        static void ConfigureSprite(string path, Vector4 border, bool mipmaps)
        {
            var imp = (TextureImporter)AssetImporter.GetAtPath(path);
            var settings = new TextureImporterSettings();
            imp.ReadTextureSettings(settings);
            settings.textureType = TextureImporterType.Sprite;
            settings.spriteMode = (int)SpriteImportMode.Single;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteBorder = border;
            settings.spritePixelsPerUnit = 100f;
            settings.npotScale = TextureImporterNPOTScale.None;
            settings.mipmapEnabled = mipmaps;
            settings.wrapMode = TextureWrapMode.Clamp;
            settings.filterMode = mipmaps ? FilterMode.Trilinear : FilterMode.Bilinear;
            settings.alphaIsTransparency = true;
            imp.SetTextureSettings(settings);
            imp.maxTextureSize = 4096;
            imp.textureCompression = TextureImporterCompression.CompressedHQ;
            imp.SaveAndReimport();
        }
    }
}
