#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace N29.HeroesDawn.EditorTools.Design
{
    /// <summary>
    /// Rasterise les fonds SVG des mini-jeux (Art/MiniGames/bg_*.svg) en PNG 1280 × 640 (Resources/MiniGames) avec Edge sans interface, puis règle l'import en sprite.
    /// Menu : Heroes Dawn ▸ Setup ▸ Générer les fonds de mini-jeux. En lot : <c>-executeMethod N29.HeroesDawn.EditorTools.Design.MiniGameBackgroundBaker.BakeAllBatch</c>.
    /// Les SVG restent la source : une retouche se refait par cet outil.
    /// </summary>
    public static class MiniGameBackgroundBaker
    {
        const string SvgFolder = "Assets/_Project/Art/MiniGames";
        const string PngFolder = "Assets/_Project/Resources/MiniGames";

        static readonly string[] EdgePaths =
        {
            @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
            @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
        };

        [MenuItem("Heroes Dawn/Setup/Générer les fonds de mini-jeux")]
        public static void BakeAll()
        {
            string? edge = Array.Find(EdgePaths, File.Exists);
            if (edge == null) throw new InvalidOperationException("Edge est introuvable : impossible de rasteriser les SVG.");
            Directory.CreateDirectory(PngFolder);
            int count = 0;
            foreach (var svg in Directory.GetFiles(SvgFolder, "bg_*.svg"))
            {
                string png = Path.Combine(PngFolder, Path.GetFileNameWithoutExtension(svg) + ".png");
                Bake(edge, Path.GetFullPath(svg), Path.GetFullPath(png));
                AssetDatabase.ImportAsset(png.Replace('\\', '/'), ImportAssetOptions.ForceUpdate);
                SetSpriteImport(png.Replace('\\', '/'));
                count++;
            }
            AssetDatabase.Refresh();
            Debug.Log("[Heroes Dawn] Fonds de mini-jeux : " + count + " PNG générés.");
        }

        public static void BakeAllBatch()
        {
            try { BakeAll(); }
            catch (Exception e) { Debug.LogError(e); EditorApplication.Exit(1); }
        }

        static void Bake(string edge, string svgPath, string pngPath)
        {
            string profile = Path.Combine(Path.GetTempPath(), "hd_edge_bake");
            var info = new ProcessStartInfo(edge,
                "--headless=new --disable-gpu --no-sandbox --hide-scrollbars --user-data-dir=\"" + profile + "\" --screenshot=\"" + pngPath +
                "\" --window-size=1280,640 \"" + new Uri(svgPath).AbsoluteUri + "\"")
            { UseShellExecute = false, CreateNoWindow = true };
            if (File.Exists(pngPath)) File.Delete(pngPath);
            using var p = Process.Start(info)!;
            if (!p.WaitForExit(60000)) { p.Kill(); throw new TimeoutException("Edge n'a pas rendu " + svgPath); }
            // Le processus lanceur peut se terminer avant que le fichier soit écrit : on attend un PNG non vide dont la taille ne bouge plus.
            long last = -1;
            for (int i = 0; i < 60; i++)
            {
                long size = File.Exists(pngPath) ? new FileInfo(pngPath).Length : 0;
                if (size > 0 && size == last) return;
                last = size;
                System.Threading.Thread.Sleep(500);
            }
            throw new InvalidOperationException("Edge n'a rien produit pour " + svgPath);
        }

        static void SetSpriteImport(string assetPath)
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
    }
}
