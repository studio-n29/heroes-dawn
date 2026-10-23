#nullable enable
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Réglages d'import des illustrations chargées par identifiant depuis <c>Resources</c> (cinématique, key arts, portraits) : sprite unique,
    /// sans mipmaps, 2048 px au plus. Appliqués au premier import ; les retouches de l'inspecteur sont conservées.
    /// </summary>
    public sealed class ArtImportRules : AssetPostprocessor
    {
        static readonly string[] Folders =
        {
            "Assets/_Project/Resources/Intro/",
            "Assets/_Project/Resources/KeyArts/",
            "Assets/_Project/Resources/Portraits/",
            "Assets/_Project/Resources/Illustrations/",
        };

        void OnPreprocessTexture()
        {
            var importer = (TextureImporter)assetImporter;
            if (!importer.importSettingsMissing || !IsArtPath(assetPath)) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = assetPath.Contains("/Illustrations/") ? 1280 : 2048;
            importer.alphaIsTransparency = assetPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsArtPath(string path)
        {
            foreach (var f in Folders) if (path.StartsWith(f)) return true;
            return false;
        }
    }
}
