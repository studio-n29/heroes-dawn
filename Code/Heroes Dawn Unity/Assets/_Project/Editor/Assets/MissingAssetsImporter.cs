#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Importe la livraison 2D du 2026-11-16 (<c>03_Art_UI_UX/Production_2D_2026-11-16</c>) : copie les exports retenus vers leur destination
    /// du CSV (<c>Integration_Nouveaux.csv</c>) puis règle leur import selon le guide (sprite unique, sans mipmaps, rectangle plein, 9-slice du CSV).
    /// Seules les références branchées dans le jeu sont copiées : tout ce qui est sous <c>Resources</c> entre dans l'APK (limite visée : 200 Mo).
    /// Idempotent : un fichier identique n'est pas recopié. Batchmode : -executeMethod N29.HeroesDawn.EditorTools.Assets.MissingAssetsImporter.ImportBatch -quit.
    /// </summary>
    public static class MissingAssetsImporter
    {
        const string DeliveryFolder = "03_Art_UI_UX/Production_2D_2026-11-16";

        /// <summary>Références de la liste branchées dans le jeu. Les autres (tenues, thèmes, dés, pions, tournoi, états du booster, cartes 4K, store) restent dans la livraison.</summary>
        static readonly HashSet<string> WiredIds = new HashSet<string>
        {
            "M-01", "M-02", "M-03", "M-04", "M-05",
            "G-01", "G-02", "G-03", "G-04",
            "N-01", "S-01", "S-02",
            "U-01", "U-02", "U-03", "U-04",
        };

        /// <summary>Plus grande dimension conservée à l'import (poids de l'APK) ; les fichiers 1024 des portraits d'ennemis et les grands formats ne sont pas copiés.</summary>
        const int MaxSize = 1024;

        [MenuItem("Heroes Dawn/Assets/Importer la livraison 2D (2026-11-16)")]
        static void ImportFromMenu() => Debug.Log(Import());

        public static void ImportBatch()
        {
            var report = Import();
            Debug.Log(report);
            if (UnityEngine.Application.isBatchMode) EditorApplication.Exit(report.StartsWith("ERREUR") ? 1 : 0);
        }

        public static string Import()
        {
            // Dossier du projet Unity = <dépôt>/Code/Heroes Dawn Unity ; la livraison est à la racine du dépôt documentaire.
            string project = Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, ".."));
            string root = Path.GetFullPath(Path.Combine(project, "..", "..", DeliveryFolder));
            string csv = Path.Combine(root, "Integration_Nouveaux.csv");
            if (!File.Exists(csv)) return "ERREUR : CSV introuvable : " + csv;

            var rows = ReadRows(csv).Where(r => WiredIds.Contains(r.Id) && Wanted(r)).ToList();
            int copied = 0, unchanged = 0, missing = 0;
            var imported = new List<Row>();
            foreach (var row in rows)
            {
                string source = Path.Combine(root, "Exports", row.File.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(source)) { missing++; Debug.LogWarning("Export introuvable : " + source); continue; }
                string target = Path.Combine(project, row.Destination.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                if (File.Exists(target) && new FileInfo(target).Length == new FileInfo(source).Length) unchanged++;
                else { File.Copy(source, target, true); copied++; }
                imported.Add(row);
            }

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            int configured = 0;
            foreach (var row in imported) if (Configure(row)) configured++;
            AssetDatabase.SaveAssets();
            return $"Livraison 2D : {rows.Count} fichiers retenus, {copied} copiés, {unchanged} déjà présents, {missing} introuvables, {configured} réglages d'import appliqués.";
        }

        /// <summary>Exclut les formats non branchés : fichiers 1024 des portraits d'ennemis, plan de dialogue du Passeur par phase en pleine taille, SVG.</summary>
        static bool Wanted(Row row)
        {
            string f = row.File;
            if (f.EndsWith(".svg", StringComparison.OrdinalIgnoreCase)) return false;
            if (f.EndsWith("-1024.png", StringComparison.OrdinalIgnoreCase)) return false;
            if (f.StartsWith("Enemies/mos_passeur-phase-", StringComparison.OrdinalIgnoreCase) && !f.EndsWith("-128.png") && !f.EndsWith("-512.png")) return false;
            return true;
        }

        static bool Configure(Row row)
        {
            var importer = AssetImporter.GetAtPath(row.Destination) as TextureImporter;
            if (importer == null) { Debug.LogWarning("Import introuvable : " + row.Destination); return false; }

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.sRGBTexture = true;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = row.File.EndsWith(".png", StringComparison.OrdinalIgnoreCase);
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = Mathf.Min(MaxSize, Mathf.NextPowerOfTwo(Mathf.Max(row.Width, row.Height)));
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = row.Border;

            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return true;
        }

        sealed class Row
        {
            public string Id = "", File = "", Destination = "";
            public int Width, Height;
            public Vector4 Border; // gauche, bas, droite, haut (ordre de Unity)
        }

        static IEnumerable<Row> ReadRows(string csvPath)
        {
            var lines = SplitRecords(System.IO.File.ReadAllText(csvPath, Encoding.UTF8));
            for (int i = 1; i < lines.Count; i++)
            {
                var c = lines[i];
                if (c.Count < 5 || string.IsNullOrWhiteSpace(c[0])) continue;
                int.TryParse(c[2], out int w); int.TryParse(c[3], out int h);
                var numbers = c.Count > 6 ? Regex.Matches(c[6], @"-?\d+").Cast<Match>().Select(m => int.Parse(m.Value)).ToArray() : Array.Empty<int>();
                // Le guide donne le border dans l'ordre Left / Bottom / Right / Top : celui de Vector4 pour Unity.
                var border = numbers.Length == 4 ? new Vector4(numbers[0], numbers[1], numbers[2], numbers[3]) : Vector4.zero;
                yield return new Row { Id = c[0].Trim('﻿'), File = c[1], Width = w, Height = h, Destination = c[4], Border = border };
            }
        }

        /// <summary>Découpe un CSV avec champs entre guillemets (le border contient des virgules ou des espaces).</summary>
        static List<List<string>> SplitRecords(string text)
        {
            var records = new List<List<string>>();
            var record = new List<string>();
            var field = new StringBuilder();
            bool quoted = false;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];
                if (quoted)
                {
                    if (ch == '"' && i + 1 < text.Length && text[i + 1] == '"') { field.Append('"'); i++; }
                    else if (ch == '"') quoted = false;
                    else field.Append(ch);
                }
                else if (ch == '"') quoted = true;
                else if (ch == ',') { record.Add(field.ToString()); field.Clear(); }
                else if (ch == '\n' || ch == '\r')
                {
                    if (ch == '\r' && i + 1 < text.Length && text[i + 1] == '\n') i++;
                    record.Add(field.ToString()); field.Clear();
                    if (record.Count > 1 || record[0].Length > 0) records.Add(record);
                    record = new List<string>();
                }
                else field.Append(ch);
            }
            if (field.Length > 0 || record.Count > 0) { record.Add(field.ToString()); records.Add(record); }
            return records;
        }
    }
}
