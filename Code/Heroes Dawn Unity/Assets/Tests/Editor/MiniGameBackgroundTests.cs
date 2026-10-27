using System.IO;
using System.Xml;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Fonds des mini-jeux 2D : un SVG source par jeu, rasterisé en PNG importé comme sprite, chargé par <see cref="MiniGameArt"/>.</summary>
    public sealed class MiniGameBackgroundTests
    {
        static string ProjectPath(string relative) => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, relative));

        [Test, Description("Vérifie que chacun des 8 jeux à fond a son SVG source dans Art/MiniGames et son PNG dans Resources/MiniGames — parce que le PNG est ce que le jeu charge et le SVG ce qu'on retouche")]
        public void EveryGameKind_HasASvgSourceAndAPng()
        {
            Assert.That(MiniGameArt.Kinds.Length, Is.EqualTo(8));
            foreach (var kind in MiniGameArt.Kinds)
            {
                Assert.That(File.Exists(ProjectPath("_Project/Art/MiniGames/bg_" + kind + ".svg")), Is.True, "SVG " + kind);
                Assert.That(File.Exists(ProjectPath("_Project/Resources/MiniGames/bg_" + kind + ".png")), Is.True, "PNG " + kind);
            }
        }

        [Test, Description("Vérifie que chaque SVG est du XML valide au format 1280 × 640 (le rapport de la zone de jeu) et ne contient aucun texte — parce qu'un fond ne doit pas porter de mots non localisés")]
        public void Svg_IsWellFormedXml_WithTheFieldViewBox_AndNoText()
        {
            foreach (var kind in MiniGameArt.Kinds)
            {
                var doc = new XmlDocument();
                doc.Load(ProjectPath("_Project/Art/MiniGames/bg_" + kind + ".svg"));
                Assert.That(doc.DocumentElement!.GetAttribute("viewBox"), Is.EqualTo("0 0 1280 640"), kind);
                Assert.That(doc.GetElementsByTagName("text").Count, Is.EqualTo(0), kind);
            }
        }

        [Test, Description("Vérifie que chaque PNG est importé comme sprite et chargé par MiniGameArt (et que l'absence d'un fond rend null) — parce que la fenêtre affiche ces fonds")]
        public void MiniGameArt_ReturnsASprite_ForEveryKind()
        {
            foreach (var kind in MiniGameArt.Kinds)
            {
                var sprite = MiniGameArt.Background(kind);
                Assert.That(sprite, Is.Not.Null, kind);
                Assert.That(sprite!.rect.width, Is.EqualTo(1280f), kind);
                Assert.That(sprite.rect.height, Is.EqualTo(640f), kind);
            }
            Assert.That(MiniGameArt.Background("no_such_kind"), Is.Null);
        }

        [Test, Description("Vérifie que la teinte d'un fond reste claire (chaque composante au moins 0,6) — parce qu'une teinte trop sombre noircirait le dessin et masquerait les éléments de jeu")]
        public void Tint_StaysLight_ForEveryEnvironment()
        {
            foreach (Biome b in System.Enum.GetValues(typeof(Biome)))
                foreach (Weather w in System.Enum.GetValues(typeof(Weather)))
                {
                    var c = MiniGameArt.Tint(EnvironmentPalette.For(new ZoneEnvironment(b, w)));
                    Assert.That(Mathf.Min(c.r, c.g, c.b), Is.GreaterThanOrEqualTo(0.6f), b + "/" + w);
                    Assert.That(c.a, Is.EqualTo(1f));
                }
        }

        [Test, Description("Vérifie que les PNG sont importés en sprite unique non compressé à perte forte (type Sprite, mode Single) — parce que sinon Resources.Load<Sprite> ne les trouverait pas")]
        public void Pngs_AreImportedAsSingleSprites()
        {
            foreach (var kind in MiniGameArt.Kinds)
            {
                var importer = (TextureImporter)AssetImporter.GetAtPath("Assets/_Project/Resources/MiniGames/bg_" + kind + ".png");
                Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite), kind);
                Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single), kind);
            }
        }
    }
}
