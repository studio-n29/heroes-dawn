using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Table des 8 chapitres (New York et Moscou) et rattachement des zones (lot G de la 0.14.0).</summary>
    public sealed class ChaptersTests
    {
        [Test, Description("Vérifie que la table compte les 8 chapitres de la spec, avec des identifiants uniques et un ordre de 1 à 4 par ville — parce que la page Chapitres et les numéros « CHAPITRE n » s'appuient dessus")]
        public void Table_Has8Chapters_4PerCity()
        {
            Assert.That(Chapters.All.Select(c => c.Id), Is.EqualTo(new[]
            {
                "ny_brooklyn", "ny_queens", "ny_harlem", "ny_manhattan", "mos_sergueiv", "mos_kapotnya", "mos_sadovoye", "mos_place_rouge",
            }));
            Assert.That(Chapters.All.Select(c => c.Id), Is.Unique);
            foreach (var city in new[] { StoryRoutes.NewYork, StoryRoutes.Moscow })
                Assert.That(Chapters.OfCity(city).Select(c => c.Order), Is.EqualTo(new[] { 1, 2, 3, 4 }), city);
            Assert.That(Chapters.Find("mos_place_rouge")!.City, Is.EqualTo(StoryRoutes.Moscow));
            Assert.That(Chapters.Find("nope"), Is.Null);
            Assert.That(Chapters.Find("ny_harlem")!.TitleKey, Is.EqualTo("chapter.ny_harlem.title"));
            Assert.That(Chapters.Find("ny_harlem")!.IntroKey, Is.EqualTo("chapter.ny_harlem.intro"));
        }

        [Test, Description("Vérifie qu'une zone de Brooklyn appartient au chapitre Brooklyn et qu'une zone de tutoriel n'appartient à aucun chapitre — parce que le tutoriel ne doit pas déclencher d'ouverture de chapitre")]
        public void OfZone_ByBorough_AndTutorialHasNone()
        {
            var content = TestContent.Build();
            Assert.That(Chapters.OfZone(content.Zone(PrototypeContent.Zone11))!.Id, Is.EqualTo("ny_brooklyn"));
            var tutorial = content.Zones.First(z => z.IsTutorial);
            Assert.That(Chapters.OfZone(tutorial), Is.Null);
        }
    }
}
