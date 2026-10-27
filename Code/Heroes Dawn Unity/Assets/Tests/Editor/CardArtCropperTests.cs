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
