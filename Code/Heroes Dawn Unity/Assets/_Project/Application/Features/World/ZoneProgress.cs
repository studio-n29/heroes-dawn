#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.World
{
    public enum ZoneStatus { Locked, Available, Done }

    /// <summary>Progression de l'histoire : quelles zones sont ouvertes, laquelle jouer ensuite.</summary>
    public static class ZoneProgress
    {
        /// <summary>État d'une zone selon l'acte courant du parcours : une zone non validée n'est ouverte que dans la ville de l'acte en cours (Agent D : New York, Agent M : Moscou).</summary>
        public static ZoneStatus StatusOf(ISessionReadModel session, ZoneDef zone)
        {
            var raw = RawStatusOf(session, zone);
            if (raw != ZoneStatus.Available || zone.IsTutorial) return raw; // le tutoriel est jouable quel que soit le parcours
            return StoryRoutes.CityOfBorough(zone.Borough) == StoryProgress.Current(session).City ? ZoneStatus.Available : ZoneStatus.Locked;
        }

        /// <summary>État d'une zone selon ses seuls prérequis, sans tenir compte de l'acte courant (sert au calcul de la fin d'un acte).</summary>
        public static ZoneStatus RawStatusOf(ISessionReadModel session, ZoneDef zone)
        {
            if (Contains(session.CompletedZones, zone.Id.Value)) return ZoneStatus.Done;
            foreach (var req in zone.Requires)
                if (!Contains(session.CompletedZones, req.Value)) return ZoneStatus.Locked;
            return ZoneStatus.Available;
        }

        /// <summary>Zones d'un quartier, dans l'ordre du catalogue.</summary>
        public static IReadOnlyList<ZoneDef> InBorough(ISessionReadModel session, string borough)
        {
            var list = new List<ZoneDef>();
            foreach (var z in session.Content.Zones) if (z.Borough == borough) list.Add(z);
            return list;
        }

        /// <summary>Première zone ouverte et pas encore validée ; à défaut la dernière validée ; à défaut la première du catalogue.</summary>
        public static ZoneDef Recommended(ISessionReadModel session)
        {
            ZoneDef? lastDone = null;
            foreach (var z in session.Content.Zones)
            {
                if (z.IsTutorial || z.IsLair) continue;
                var st = StatusOf(session, z);
                if (st == ZoneStatus.Available) return z;
                if (st == ZoneStatus.Done) lastDone = z;
            }
            return lastDone ?? session.Content.Zones[0];
        }

        static bool Contains(IReadOnlyCollection<string> set, string id)
        {
            foreach (var s in set) if (s == id) return true;
            return false;
        }
    }
}
