#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.EditorTools.Assets;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Outil de recadrage des illustrations de cartes (lot C2) : attribution des sources, fichier SOURCES.md, recadrage teinté.</summary>
    public sealed class CardArtCropperTests
    {
        [Test, Description("Vérifie que SOURCES.md se relit à l'identique, y compris un chemin avec espaces, accents et majuscules — parce que le fichier est l'unique trace des sources attribuées")]
        public void SourcesFile_RoundTrips()
        {
            var file = new SourcesFile();
            file.Reserved.Add("Lieux/paris_porte_des_enfers.jpg");
            file.Assigned["card_glass_shards"] = "Nouveau Dossier/Héros 01.PNG";
            var back = SourcesFile.Parse(file.Write());
            Assert.That(back.Reserved, Is.EqualTo(file.Reserved));
            Assert.That(back.Assigned, Is.EquivalentTo(file.Assigned));
        }

        [Test, Description("Vérifie que chaque carte reçoit une source libre (ni réservée ni déjà attribuée), dans l'ordre des chemins — parce que chaque carte doit avoir une source différente")]
        public void Assign_SkipsReservedAndAssigned_OnePerCard()
        {
            var file = new SourcesFile();
            file.Reserved.Add("b.jpg");
            file.Assigned["x"] = "a.jpg";
            var (assignments, missing) = CardArtCropper.Assign(new[] { "y", "z" }, new[] { "d.jpg", "c.jpg", "b.jpg", "a.jpg" }, file);
            Assert.That(missing, Is.EqualTo(0));
            Assert.That(assignments, Is.EquivalentTo(new Dictionary<string, string> { ["y"] = "c.jpg", ["z"] = "d.jpg" }));
        }

        [Test, Description("Vérifie qu'une source nommée d'après une carte (card_monolith.jpg, dans n'importe quel dossier, casse indifférente) revient à cette carte avant l'attribution par ordre alphabétique — parce que l'utilisateur génère chaque image pour une carte précise")]
        public void Assign_GivesANamedSourceToItsCard()
        {
            // Par ordre alphabétique seul, card_monolith (1re carte) recevrait aaa_first.jpg.
            var (assignments, missing) = CardArtCropper.Assign(new[] { "card_monolith", "card_ember" },
                new[] { "Competences/aaa_first.jpg", "Competences/CARD_MONOLITH.png" }, new SourcesFile());
            Assert.That(missing, Is.EqualTo(0));
            Assert.That(assignments, Is.EquivalentTo(new Dictionary<string, string>
            {
                ["card_monolith"] = "Competences/CARD_MONOLITH.png",
                ["card_ember"] = "Competences/aaa_first.jpg",
            }));
        }

        [Test, Description("Vérifie qu'il n'y a aucune attribution quand les sources libres manquent, avec le nombre de cartes sans source — parce que l'outil doit s'arrêter sans écrire à moitié")]
        public void Assign_StopsWhenSourcesAreMissing()
        {
            var (assignments, missing) = CardArtCropper.Assign(new[] { "a", "b", "c" }, new[] { "1.jpg", "2.jpg" }, new SourcesFile());
            Assert.That(missing, Is.EqualTo(1));
            Assert.That(assignments, Is.Empty);
        }

        [Test, Description("Vérifie qu'une relance sans carte à illustrer n'attribue rien — parce que l'outil ne doit jamais réattribuer ni regénérer")]
        public void Assign_IsIdempotent()
        {
            var file = new SourcesFile();
            file.Assigned["x"] = "a.jpg";
            var (assignments, missing) = CardArtCropper.Assign(new string[0], new[] { "a.jpg", "b.jpg" }, file);
            Assert.That((assignments.Count, missing), Is.EqualTo((0, 0)));
        }

        [Test, Description("Vérifie qu'une source illisible n'est ni recadrée ni attribuée, et que les images déjà créées sont bien inscrites dans SOURCES.md avant l'erreur — parce qu'un échec en cours de route ne doit ni perdre la trace d'une source ni graver une image cassée")]
        public void Generate_RecordsEachImage_AndRejectsUnreadableSources()
        {
            string dir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "hd_cropper_" + System.Guid.NewGuid().ToString("N"));
            string src = System.IO.Path.Combine(dir, "src"), outDir = System.IO.Path.Combine(dir, "out");
            System.IO.Directory.CreateDirectory(src);
            System.IO.Directory.CreateDirectory(outDir);
            try
            {
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(src, "a_ok.png"), Solid(64, 36, Color.gray).EncodeToPNG());
                System.IO.File.WriteAllBytes(System.IO.Path.Combine(src, "b_broken.jpg"), new byte[] { 1, 2, 3, 4 });
                string sourcesPath = System.IO.Path.Combine(outDir, "SOURCES.md");
                var file = new SourcesFile();
                var assignments = new Dictionary<string, string> { ["card_one"] = "a_ok.png", ["card_two"] = "b_broken.jpg" };
                UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("source illisible.*b_broken"));
                bool ok = CardArtCropper.Generate(assignments, src, outDir, file, sourcesPath, id => Color.white);
                Assert.That(ok, Is.False);
                Assert.That(System.IO.File.Exists(System.IO.Path.Combine(outDir, "card_one.jpg")), Is.True);
                Assert.That(System.IO.File.Exists(System.IO.Path.Combine(outDir, "card_two.jpg")), Is.False);
                var saved = SourcesFile.Parse(System.IO.File.ReadAllText(sourcesPath));
                Assert.That(saved.Assigned, Is.EquivalentTo(new Dictionary<string, string> { ["card_one"] = "a_ok.png" }));
            }
            finally { System.IO.Directory.Delete(dir, true); }
        }

        static Texture2D Solid(int w, int h, Color c)
        {
            var t = new Texture2D(w, h, TextureFormat.RGBA32, false);
            t.SetPixels(Enumerable.Repeat(c, w * h).ToArray());
            t.Apply();
            return t;
        }

        [Test, Description("Vérifie qu'une source portrait ou paysage donne toujours une image 640 × 360 teintée à 30 % vers la couleur de la famille — parce que les tuiles du grimoire attendent ce format")]
        public void Crop_IsAlways640x360_AndTinted()
        {
            var tint = new Color(1f, 0.5f, 0.2f);
            var portrait = CardArtCropper.Crop(Solid(300, 600, Color.white), tint);
            Assert.That((portrait.width, portrait.height), Is.EqualTo((640, 360)));
            var expected = Color.Lerp(Color.white, tint, 0.3f);
            var mid = portrait.GetPixel(320, 180);
            Assert.That(mid.r, Is.EqualTo(expected.r).Within(0.02f));
            Assert.That(mid.g, Is.EqualTo(expected.g).Within(0.02f));
            Assert.That(mid.b, Is.EqualTo(expected.b).Within(0.02f));
            var landscape = CardArtCropper.Crop(Solid(1920, 1080, Color.gray), tint);
            Assert.That((landscape.width, landscape.height), Is.EqualTo((640, 360)));
        }
    }
}
