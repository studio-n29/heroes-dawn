#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>État d'un chapitre : terminé, en cours, verrouillé, ou « bientôt » (aucune zone n'existe encore).</summary>
    public enum ChapterStatus { Done, Current, Locked, Soon }

    public static class ChapterProgress
    {
        static IReadOnlyList<ZoneDef> ZonesOf(ISessionReadModel session, ChapterDef chapter)
            => session.Content.Zones.Where(z => Chapters.OfZone(z)?.Id == chapter.Id).ToList();

        static bool IsDone(ISessionReadModel session, ChapterDef chapter)
        {
            var zones = ZonesOf(session, chapter);
            return zones.Count > 0 && zones.All(z => ZoneProgress.StatusOf(session, z) == ZoneStatus.Done);
        }

        public static ChapterStatus StatusOf(ISessionReadModel session, ChapterDef chapter)
        {
            if (ZonesOf(session, chapter).Count == 0) return ChapterStatus.Soon;
            if (IsDone(session, chapter)) return ChapterStatus.Done;
            string city = StoryProgress.Current(session).City;
            if (chapter.City != city) return ChapterStatus.Locked;
            var first = Chapters.OfCity(city).FirstOrDefault(c => ZonesOf(session, c).Count > 0 && !IsDone(session, c));
            return first != null && first.Id == chapter.Id ? ChapterStatus.Current : ChapterStatus.Locked;
        }

        /// <summary>Chapitres de la ville de l'acte en cours, puis ceux des autres villes dans l'ordre du parcours (puis les villes restantes).</summary>
        public static IReadOnlyList<ChapterDef> InDisplayOrder(ISessionReadModel session)
        {
            var cities = new List<string> { StoryProgress.Current(session).City };
            foreach (var c in session.Route.ActCities) if (!cities.Contains(c)) cities.Add(c);
            foreach (var c in Chapters.All) if (!cities.Contains(c.City)) cities.Add(c.City);
            return cities.SelectMany(Chapters.OfCity).ToList();
        }
    }
}
