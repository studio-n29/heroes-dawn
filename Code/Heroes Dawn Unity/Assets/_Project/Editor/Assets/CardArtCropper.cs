#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Presentation.Common;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.EditorTools.Assets
{
    /// <summary>
    /// Trace des sources des illustrations de cartes (Resources/Illustrations/Cards/SOURCES.md) : sources réservées (jamais prises)
    /// et sources attribuées (une par carte). Chemins relatifs à 03_Art_UI_UX/Illustrations/, séparateur « / ».
    /// </summary>
    public sealed class SourcesFile
    {
        const string ReservedTitle = "## Réservées";
        const string AssignedTitle = "## Attribuées";
        const string Arrow = " → ";

        public List<string> Reserved = new List<string>();
        public Dictionary<string, string> Assigned = new Dictionary<string, string>();

        public static SourcesFile Parse(string text)
        {
            var f = new SourcesFile();
            string section = "";
            foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
            {
                var line = raw.Trim();
                if (line.StartsWith("## ")) { section = line; continue; }
                if (!line.StartsWith("- ")) continue;
                var item = line.Substring(2).Trim();
                if (section == ReservedTitle) f.Reserved.Add(item);
                else if (section == AssignedTitle)
                {
                    int at = item.IndexOf(Arrow, StringComparison.Ordinal);
                    if (at > 0) f.Assigned[item.Substring(0, at).Trim()] = item.Substring(at + Arrow.Length).Trim();
                }
            }
            return f;
        }

        public string Write()
        {
            var sb = new StringBuilder();
            sb.Append("# Sources des illustrations de cartes\n\n");
            sb.Append("Recadrages teintés de l'art de `03_Art_UI_UX/Illustrations/` (chemins relatifs à ce dossier), une source différente par carte.\n");
            sb.Append("Outil : menu « Heroes Dawn ▸ Setup ▸ Recadrer les illustrations de cartes ». Retirer une ligne de « Réservées » la rend disponible.\n\n");
            sb.Append(ReservedTitle).Append('\n');
            foreach (var r in Reserved) sb.Append("- ").Append(r).Append('\n');
            sb.Append('\n').Append(AssignedTitle).Append('\n');
            foreach (var kv in Assigned.OrderBy(k => k.Key, StringComparer.Ordinal)) sb.Append("- ").Append(kv.Key).Append(Arrow).Append(kv.Value).Append('\n');
            return sb.ToString();
        }
    }

    /// <summary>
    /// Illustrations temporaires des cartes : pour chaque carte sans image, une source libre du Drive recadrée en 640 × 360,
    /// teintée vers la couleur de sa famille et enregistrée dans Resources/Illustrations/Cards/&lt;id&gt;.jpg.
    /// </summary>
    public static class CardArtCropper
    {
        const string CardsFolder = "Assets/_Project/Resources/Illustrations/Cards";
        const string SourcesName = "SOURCES.md";
        static readonly string[] Extensions = { ".jpg", ".jpeg", ".png" };

        /// <summary>
        /// Une source libre (ni réservée ni attribuée, dans l'ordre des chemins) par carte, dans l'ordre des cartes ;
        /// s'il en manque, aucune attribution et le nombre de cartes restées sans source.
        /// </summary>
        public static (Dictionary<string, string> Assignments, int Missing) Assign(IReadOnlyList<string> cardsWithoutArt, IReadOnlyList<string> sources, SourcesFile file)
        {
            var taken = new HashSet<string>(file.Reserved.Concat(file.Assigned.Values), StringComparer.OrdinalIgnoreCase);
            var free = sources.Where(s => !taken.Contains(s)).Distinct(StringComparer.OrdinalIgnoreCase)
                              .OrderBy(s => s, StringComparer.OrdinalIgnoreCase).ToList();
            int missing = Math.Max(0, cardsWithoutArt.Count - free.Count);
            var result = new Dictionary<string, string>();
            if (missing > 0) return (result, missing);
            for (int i = 0; i < cardsWithoutArt.Count; i++) result[cardsWithoutArt[i]] = free[i];
            return (result, 0);
        }

        /// <summary>Recadrage centré au rapport largeur/hauteur, réduction bilinéaire, puis teinte : Lerp(pixel, pixel × teinte, part).</summary>
        public static Texture2D Crop(Texture2D source, Color tint, int width = 640, int height = 360, float tintAmount = 0.3f)
        {
            float target = (float)width / height, ratio = (float)source.width / source.height;
            // Zone gardée de la source, en coordonnées normalisées.
            var scale = ratio > target ? new Vector2(target / ratio, 1f) : new Vector2(1f, ratio / target);
            var offset = new Vector2((1f - scale.x) / 2f, (1f - scale.y) / 2f);
            // Échantillonnage bilinéaire sur le processeur : marche aussi sans carte graphique (éditeur en -nographics).
            var output = new Texture2D(width, height, TextureFormat.RGB24, false);
            var pixels = new Color[width * height];
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    var p = source.GetPixelBilinear(offset.x + (x + 0.5f) / width * scale.x, offset.y + (y + 0.5f) / height * scale.y);
                    var c = Color.Lerp(p, p * tint, tintAmount);
                    c.a = 1f;
                    pixels[y * width + x] = c;
                }
            output.SetPixels(pixels);
            output.Apply();
            return output;
        }

        [MenuItem("Heroes Dawn/Setup/Recadrer les illustrations de cartes")]
        public static void CropMissing() => Run();

        /// <summary>Ligne de commande (-executeMethod) : sort avec le code 1 s'il manque des sources.</summary>
        public static void CropMissingBatch()
        {
            if (!Run()) EditorApplication.Exit(1);
        }

        static string ArtRoot => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "..", "03_Art_UI_UX", "Illustrations"));

        static bool Run()
        {
            string sourcesPath = Path.Combine(CardsFolder, SourcesName);
            var file = File.Exists(sourcesPath) ? SourcesFile.Parse(File.ReadAllText(sourcesPath)) : new SourcesFile();
            var collection = PrototypeCollection.Build();
            var missingArt = collection.Cards.Select(c => c.Id.Value).Where(id => !File.Exists(Path.Combine(CardsFolder, id + ".jpg"))).ToList();
            var root = ArtRoot;
            var sources = Directory.Exists(root)
                ? Directory.GetFiles(root, "*", SearchOption.AllDirectories)
                           .Where(p => Extensions.Contains(Path.GetExtension(p).ToLowerInvariant()))
                           .Select(p => p.Substring(root.Length + 1).Replace('\\', '/')).ToList()
                : new List<string>();
            var (assignments, missing) = Assign(missingArt, sources, file);
            if (missing > 0)
            {
                Debug.LogError("[Heroes Dawn] Illustrations de cartes : il manque " + missing + " image(s) source libre(s) dans 03_Art_UI_UX/Illustrations/ (" + missingArt.Count + " carte(s) à illustrer).");
                return false;
            }
            bool ok = Generate(assignments, root, CardsFolder, file, sourcesPath, id => Theme.FamilyColor(collection.Card(new Core.CardId(id)).Family));
            AssetDatabase.Refresh();
            Debug.Log("[Heroes Dawn] Illustrations de cartes : " + file.Assigned.Count(kv => assignments.ContainsKey(kv.Key)) + " créée(s) sur " + assignments.Count + ".");
            return ok;
        }

        /// <summary>
        /// Crée l'image de chaque carte depuis sa source et inscrit l'attribution dans SOURCES.md aussitôt après (une erreur en cours de route
        /// ne perd aucune trace). Une source illisible n'est ni recadrée ni attribuée. Rend faux si au moins une image n'a pas pu être créée.
        /// </summary>
        public static bool Generate(IReadOnlyDictionary<string, string> assignments, string sourceRoot, string outFolder, SourcesFile file,
                                    string sourcesPath, Func<string, Color> tintOf)
        {
            bool ok = true;
            foreach (var kv in assignments)
            {
                var src = new Texture2D(2, 2);
                Texture2D? cropped = null;
                try
                {
                    if (!src.LoadImage(File.ReadAllBytes(Path.Combine(sourceRoot, kv.Value))))
                    {
                        Debug.LogError("[Heroes Dawn] Illustrations de cartes : source illisible « " + kv.Value + " » (carte " + kv.Key + ") ; rien n'est créé.");
                        ok = false;
                        continue;
                    }
                    src.wrapMode = TextureWrapMode.Clamp;
                    cropped = Crop(src, tintOf(kv.Key));
                    File.WriteAllBytes(Path.Combine(outFolder, kv.Key + ".jpg"), cropped.EncodeToJPG(85));
                    file.Assigned[kv.Key] = kv.Value;
                    File.WriteAllText(sourcesPath, file.Write());
                }
                catch (Exception e)
                {
                    Debug.LogError("[Heroes Dawn] Illustrations de cartes : échec pour " + kv.Key + " (" + kv.Value + ") : " + e.Message);
                    ok = false;
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(src);
                    if (cropped != null) UnityEngine.Object.DestroyImmediate(cropped);
                }
            }
            return ok;
        }
    }
}
