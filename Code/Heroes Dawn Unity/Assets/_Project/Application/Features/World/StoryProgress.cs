#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    /// <summary>Acte en cours du parcours du personnage joueur : son numéro (1 à 4), sa ville, et s'il est jouable (zones créées).</summary>
    public readonly struct ActInfo
    {
        public readonly int Number;
        public readonly string City;
        public readonly bool HasZones;
        public ActInfo(int number, string city, bool hasZones) { Number = number; City = city; HasZones = hasZones; }
    }

    /// <summary>Avancement dans les quatre actes du parcours (voir <see cref="StoryRoute"/>).</summary>
    public static class StoryProgress
    {
        /// <summary>Zones du catalogue situées dans cette ville, dans l'ordre du catalogue.</summary>
        public static IReadOnlyList<ZoneDef> ZonesOf(ISessionReadModel session, string city)
        {
            var list = new List<ZoneDef>();
            foreach (var z in session.Content.Zones) if (!z.IsTutorial && StoryRoutes.CityOfBorough(z.Borough) == city) list.Add(z);
            return list;
        }

        /// <summary>Ville dont on montre les zones : celle de l'acte en cours si elle en a, sinon la dernière ville déjà jouée du parcours (acte sans zone : fin de contenu).</summary>
        public static string ZoneCity(ISessionReadModel session)
        {
            var cities = session.Route.ActCities;
            var act = Current(session);
            string last = cities[0];
            for (int i = 0; i < cities.Count; i++)
            {
                if (ZonesOf(session, cities[i]).Count > 0) last = cities[i];
                if (i + 1 == act.Number) break;
            }
            return act.HasZones ? act.City : last;
        }

        /// <summary>Zones de l'acte I du parcours (hors repaires), dans l'ordre : leur rang règle les déblocages du QG et des quêtes (Brooklyn pour Agent D, Sergueiv pour Agent M).</summary>
        public static IReadOnlyList<ZoneDef> FirstActZones(ISessionReadModel session)
        {
            var list = new List<ZoneDef>();
            foreach (var z in ZonesOf(session, session.Route.ActCities[0])) if (!z.IsLair) list.Add(z);
            return list;
        }

        /// <summary>Zone de l'acte I du parcours qui a le même rang que cette zone de Brooklyn (identique pour Agent D ; Sergueiv pour Agent M). Identifiant inchangé s'il n'y a pas d'équivalent.</summary>
        public static string FirstActEquivalent(ISessionReadModel session, string brooklynZoneId)
        {
            int rank = -1, i = 0;
            foreach (var z in session.Content.Zones) if (z.Borough == "brooklyn") { if (z.Id.Value == brooklynZoneId) rank = i; i++; }
            var first = FirstActZones(session);
            return rank >= 0 && rank < first.Count ? first[rank].Id.Value : brooklynZoneId;
        }

        /// <summary>Un acte est terminé quand sa ville a des zones et qu'elles sont toutes validées.</summary>
        public static bool IsActDone(ISessionReadModel session, string city)
        {
            var zones = ZonesOf(session, city);
            if (zones.Count == 0) return false;
            foreach (var z in zones) if (ZoneProgress.RawStatusOf(session, z) != ZoneStatus.Done) return false;
            return true;
        }

        /// <summary>Premier acte du parcours qui n'est pas terminé ; le dernier si tout est terminé.</summary>
        public static ActInfo Current(ISessionReadModel session)
        {
            var cities = session.Route.ActCities;
            for (int i = 0; i < cities.Count; i++)
                if (!IsActDone(session, cities[i])) return new ActInfo(i + 1, cities[i], ZonesOf(session, cities[i]).Count > 0);
            int last = cities.Count - 1;
            return new ActInfo(last + 1, cities[last], ZonesOf(session, cities[last]).Count > 0);
        }
    }
}
