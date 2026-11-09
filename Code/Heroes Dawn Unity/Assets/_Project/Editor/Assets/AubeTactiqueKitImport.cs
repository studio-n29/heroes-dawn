using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>Import du kit seulement : ne reconstruit aucun menu et ne change aucune référence runtime.</summary>
    public static class AubeTactiqueKitImport
    {
        const string Root = "Assets/_Project/Art/UI/AubeTactiqueKit";
        [Serializable] public sealed class Manifest { public Entry[] items; }
        [Serializable] public sealed class Entry { public string id; public string file; public string kind; public float[] border; public int width; public int height; public string original; public float focusX; public float focusY; public string variant; }

        [MenuItem("Heroes Dawn/Direction artistique/Importer le kit complet Aube tactique")]
        public static void Batch()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(Root + "/manifest.json"));
            int imported = 0, referenced = 0;
            foreach (var entry in manifest.items)
            {
                int start = entry.file.IndexOf("Assets/", StringComparison.Ordinal);
                if (start < 0) throw new InvalidDataException("Chemin sans Assets : " + entry.file);
                string path = entry.file.Substring(start);
                if (entry.kind == "portrait-sprite")
                {
                    CreatePortrait(entry, path);
                    imported++;
                    continue;
                }
                if (!File.Exists(path)) throw new FileNotFoundException("Asset du kit manquant : " + entry.id, path);
                if (!path.StartsWith(Root + "/", StringComparison.Ordinal) || !(path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) || path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))) { referenced++; continue; }
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null) throw new InvalidDataException("Texture non importée : " + path);
                bool illustration = entry.kind == "illustration";
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.spritePixelsPerUnit = 100;
                importer.mipmapEnabled = false;
                importer.alphaIsTransparency = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                importer.filterMode = FilterMode.Bilinear;
                importer.maxTextureSize = 2048;
                importer.textureCompression = illustration ? TextureImporterCompression.CompressedHQ : TextureImporterCompression.Uncompressed;
                importer.spriteBorder = entry.border != null && entry.border.Length == 4
                    ? new Vector4(entry.border[0], entry.border[1], entry.border[2], entry.border[3]) : Vector4.zero;
                var settings = new TextureImporterSettings(); importer.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect; importer.SetTextureSettings(settings);
                importer.SaveAndReimport();
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (sprite == null) throw new InvalidDataException("Sprite manquant après import : " + path);
                if (sprite.border.x + sprite.border.z >= sprite.rect.width || sprite.border.y + sprite.border.w >= sprite.rect.height)
                    throw new InvalidDataException("Bordures 9-slice invalides : " + path);
                imported++;
            }
            AssetDatabase.SaveAssets();
            string report = "{\"importedSprites\":" + imported + ",\"existingReferences\":" + referenced + ",\"missing\":0,\"menusModified\":false}";
            File.WriteAllText(Root + "/unity-import-report.json", report);
            Debug.Log("Aube tactique — kit importé : " + imported + " sprites, " + referenced + " références existantes, aucun menu modifié.");
        }

        static void CreatePortrait(Entry entry, string path)
        {
            string sourcePath = entry.original.Substring(entry.original.IndexOf("Assets/", StringComparison.Ordinal));
            var source = AssetDatabase.LoadAssetAtPath<Sprite>(sourcePath);
            if (source == null) throw new FileNotFoundException("Portrait source introuvable", sourcePath);
            var r = source.rect;
            float width = r.width * (entry.variant == "head" ? .25f : .30f);
            float height = entry.variant == "head" ? width : width * 1.5f;
            float x = Mathf.Clamp(r.width * entry.focusX - width * .5f, 0, r.width - width);
            float y = Mathf.Clamp(r.height * entry.focusY - height * (entry.variant == "head" ? .5f : .74f), 0, r.height - height);
            var sprite = Sprite.Create(source.texture, new Rect(r.x + x, r.y + y, width, height), new Vector2(.5f,.5f), source.pixelsPerUnit, 0, SpriteMeshType.FullRect);
            sprite.name = Path.GetFileNameWithoutExtension(path);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (existing == null) AssetDatabase.CreateAsset(sprite, path);
            else { EditorUtility.CopySerialized(sprite, existing); UnityEngine.Object.DestroyImmediate(sprite); EditorUtility.SetDirty(existing); }
        }
    }
}


