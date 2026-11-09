#nullable enable
using System;
using System.IO;
using N29.HeroesDawn.EditorTools.Prefabs;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>Importe le kit de production et reconstruit les interfaces depuis leurs recettes.</summary>
    public static class AubeTactiqueSetup
    {
        public const string ArtFolder = "Assets/_Project/Resources/UI/AubeTactique";
        static readonly string[] RequiredArt = { "shop-booster", "hq-new-york", "hq-moscow", "combat-red-hook", "room-workshop", "room-operations", "room-grimoire", "room-inventory", "room-register", "mini-dna_analysis", "mini-infiltration", "mini-gears", "mini-snow_survival", "mini-rhythm", "mini-free_throw", "mini-rescue_engineering", "mini-crate_pick" };

        [MenuItem("Heroes Dawn/Direction artistique/Appliquer Aube tactique")]
        public static void ApplyBatch()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            foreach (var name in RequiredArt)
            {
                string path = ArtFolder + "/" + name + ".png";
                if (!File.Exists(path)) throw new FileNotFoundException("Illustration Aube tactique manquante", path);
            }
            foreach (var path in Directory.GetFiles(ArtFolder, "*.png")) ConfigureIllustration(path.Replace('\\', '/'));
            UiAssetBaker.GenerateFromMenu();
            var report = PrefabGenerator.Generate(PrefabGenerator.DefaultRoot, OverwritePolicy.All);
            if (report.Failed.Count != 0) throw new InvalidOperationException("Aube tactique : " + report);
            int migrated = AubeTactiquePreservedPrefabs.Apply();
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory("../docs/test-reports/aube-tactique");
            File.WriteAllText("../docs/test-reports/aube-tactique/generation.txt", DateTime.UtcNow.ToString("O") + "\n" + report + "\nprefabs sans recette migrés : " + migrated);
            Debug.Log("[Heroes Dawn] Aube tactique appliquée : " + report);
        }

        static void ConfigureIllustration(string path)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 100;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.alphaIsTransparency = false;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
        }
    }
}

