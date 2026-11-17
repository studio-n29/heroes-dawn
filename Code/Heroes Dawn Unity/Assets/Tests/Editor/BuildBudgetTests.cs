using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.EditorTools.Assets;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>
    /// Garde-fou du poids du build : tout ce qui est sous un dossier « Resources » entre dans l'APK. L'APK de mesure du 2026-11-12 pesait 1,59 Go
    /// (2,4 Go de textures) à cause d'illustrations d'art copiées en entier et non compressées.
    /// </summary>
    public sealed class BuildBudgetTests
    {
        /// <summary>Plafond des textures sous Resources, en Mo, estimé (compression 4x4 ≈ 1 octet par pixel, non compressé = 4).</summary>
        const double TextureBudgetMb = 300;

        static IEnumerable<(string Path, double Mb)> Textures()
        {
            foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets" }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.Contains("/Resources/")) continue;
                if (!(AssetImporter.GetAtPath(path) is TextureImporter importer)) continue;
                importer.GetSourceTextureWidthAndHeight(out int w, out int h);
                int max = importer.maxTextureSize;
                float scale = Mathf.Min(1f, (float)max / Mathf.Max(w, h));
                double pixels = (double)Mathf.Max(1, Mathf.RoundToInt(w * scale)) * Mathf.Max(1, Mathf.RoundToInt(h * scale));
                double bytesPerPixel = importer.textureCompression == TextureImporterCompression.Uncompressed ? 4.0 : 1.0;
                yield return (path, pixels * bytesPerPixel / 1048576.0);
            }
        }

        [Test, Description("Vérifie que les groupes d'illustrations non utilisés par le jeu (exports, planches, masques de carte…) ne sont pas copiés sous Resources — parce que tout ce qui s'y trouve entre dans l'APK, dont 1,4 Go d'illustrations inutilisées avant ce garde-fou")]
        public void UnusedArtGroups_AreNotInResources()
        {
            var present = AubeTactiqueExtensionSync.ExcludedGroups
                .Where(g => Directory.Exists(AubeTactiqueExtensionSync.Target + "/" + g)).ToList();
            Assert.That(present, Is.Empty, "groupes exclus encore présents sous Resources : " + string.Join(", ", present));
        }

        [Test, Description("Vérifie que les textures sous Resources restent dans le budget du build — parce que l'APK de mesure pesait 1,59 Go et que Google Play limite la base à 200 Mo")]
        public void ResourcesTextures_StayWithinTheBuildBudget()
        {
            var all = Textures().ToList();
            double total = all.Sum(t => t.Mb);
            var top = string.Join("\n", all.OrderByDescending(t => t.Mb).Take(8).Select(t => $"  {t.Mb:F1} Mo  {t.Path}"))
                      + "\nPar dossier :\n" + string.Join("\n", all.GroupBy(t => string.Join("/", t.Path.Split('/').Take(4)))
                          .OrderByDescending(g => g.Sum(t => t.Mb)).Take(12).Select(g => $"  {g.Sum(t => t.Mb):F0} Mo ({g.Count()} fichiers)  {g.Key}"));
            Assert.That(total, Is.LessThanOrEqualTo(TextureBudgetMb), $"textures sous Resources : {total:F0} Mo estimés (budget {TextureBudgetMb} Mo). Les plus lourdes :\n{top}");
        }

        [Test, Description("Vérifie que les groupes exclus n'ont pas de budget absurde et que les pastilles (objets, compétences) sont plafonnées à 512 px et compressées — parce qu'elles s'affichent à moins de 300 px")]
        public void Budget_CapsTheSmallIcons()
        {
            Assert.That(AubeTactiqueExtensionSync.Budget("Items/kevlar_vest.png"), Is.EqualTo((512, true)));
            Assert.That(AubeTactiqueExtensionSync.Budget("Skills/x.png"), Is.EqualTo((512, true)));
            Assert.That(AubeTactiqueExtensionSync.IsExcluded("Exports/Map3840x2160/queens-normal.png"), Is.True);
            Assert.That(AubeTactiqueExtensionSync.IsExcluded("Items/kevlar_vest.png"), Is.False);
        }
    }
}
