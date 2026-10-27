#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Chapitre de l'histoire : un ensemble de zones d'une ville (New York : Brooklyn, Queens, Harlem, Manhattan ; Moscou : Sergueiv, Kapotnya,
    /// Sadovoye Koltso, Place Rouge). Les chapitres sont indépendants du dessin de la carte : seuls les quartiers de zones les relient aux zones.
    /// </summary>
    public sealed class ChapterDef
    {
        public string Id { get; }
        public string City { get; }
        /// <summary>Numéro dans la ville (1 à 4) : « CHAPITRE 1 ».</summary>
        public int Order { get; }
        /// <summary>Quartiers (<see cref="ZoneDef.Borough"/>) dont les zones appartiennent au chapitre.</summary>
        public IReadOnlyList<string> Boroughs { get; }
        public string TitleKey => "chapter." + Id + ".title";
        public string IntroKey => "chapter." + Id + ".intro";

        public ChapterDef(string id, string city, int order, params string[] boroughs)
        {
            Id = id; City = city; Order = order; Boroughs = boroughs;
        }
    }

    public static class Chapters
    {
        public static readonly IReadOnlyList<ChapterDef> All = new[]
        {
            new ChapterDef("ny_brooklyn", StoryRoutes.NewYork, 1, "brooklyn"),
            new ChapterDef("ny_queens", StoryRoutes.NewYork, 2, "queens"),
            new ChapterDef("ny_harlem", StoryRoutes.NewYork, 3, "harlem"),
            new ChapterDef("ny_manhattan", StoryRoutes.NewYork, 4, "manhattan"),
            new ChapterDef("mos_sergueiv", StoryRoutes.Moscow, 1, "sergueiv"),
            new ChapterDef("mos_kapotnya", StoryRoutes.Moscow, 2, "kapotnya"),
            new ChapterDef("mos_sadovoye", StoryRoutes.Moscow, 3, "sadovoye"),
            new ChapterDef("mos_place_rouge", StoryRoutes.Moscow, 4, "place_rouge"),
        };

        public static ChapterDef? Find(string id) => All.FirstOrDefault(c => c.Id == id);

        /// <summary>Chapitre d'une zone (par son quartier) ; null pour une zone de tutoriel ou sans chapitre.</summary>
        public static ChapterDef? OfZone(ZoneDef zone)
            => zone.IsTutorial ? null : All.FirstOrDefault(c => c.Boroughs.Contains(zone.Borough));

        public static IReadOnlyList<ChapterDef> OfCity(string city) => All.Where(c => c.City == city).OrderBy(c => c.Order).ToList();
    }
}
