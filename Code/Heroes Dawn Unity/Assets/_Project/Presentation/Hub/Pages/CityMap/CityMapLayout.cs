#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Quartier de la carte de la ville : forme (coordonnées normalisées, 0,0 = haut gauche) et acte qui l'ouvre.</summary>
    public readonly struct BoroughShape
    {
        public readonly string Id;
        public readonly int Act;
        public readonly Vector2 LabelAt;
        public readonly Vector2[] Polygon;

        public BoroughShape(string id, int act, Vector2 labelAt, params Vector2[] polygon) { Id = id; Act = act; LabelAt = labelAt; Polygon = polygon; }
    }

    /// <summary>Point précis d'une mission sur la carte (une zone jouable ou un point à venir d'un quartier verrouillé).</summary>
    public readonly struct MapSpot
    {
        public readonly string Borough;
        public readonly string Label;
        public readonly string? ZoneId;
        public readonly Vector2 Position;

        public MapSpot(string borough, string label, string? zoneId, Vector2 position)
        {
            Borough = borough; Label = label; ZoneId = zoneId; Position = position;
        }
    }

    /// <summary>
    /// Géographie stylisée de New York (maquette M60) : 5 quartiers, un par acte (Brooklyn 1, Queens 2, Bronx 3, Staten Island 4,
    /// Manhattan 5), et les points précis de mission. Seul Brooklyn est jouable dans cette version ; les autres sont grisés.
    /// </summary>
    public static class CityMapLayout
    {
        static Vector2 V(float x, float y) => new Vector2(x, y);

        public static readonly IReadOnlyList<BoroughShape> Boroughs = new[]
        {
            new BoroughShape("bronx", 3, V(0.67f, 0.10f), V(0.50f, 0.04f), V(0.78f, 0.06f), V(0.87f, 0.20f), V(0.72f, 0.27f), V(0.55f, 0.21f)),
            new BoroughShape("manhattan", 5, V(0.39f, 0.27f), V(0.33f, 0.16f), V(0.43f, 0.12f), V(0.49f, 0.24f), V(0.46f, 0.50f), V(0.38f, 0.56f), V(0.30f, 0.38f)),
            new BoroughShape("queens", 2, V(0.73f, 0.37f), V(0.53f, 0.30f), V(0.89f, 0.27f), V(0.96f, 0.54f), V(0.72f, 0.68f), V(0.53f, 0.60f)),
            new BoroughShape("brooklyn", 1, V(0.60f, 0.695f), V(0.29f, 0.60f), V(0.50f, 0.62f), V(0.69f, 0.72f), V(0.66f, 0.93f), V(0.40f, 0.97f), V(0.24f, 0.81f)),
            new BoroughShape("staten", 4, V(0.13f, 0.73f), V(0.04f, 0.68f), V(0.19f, 0.64f), V(0.21f, 0.87f), V(0.06f, 0.92f)),
        };

        public static readonly IReadOnlyList<MapSpot> Spots = new[]
        {
            // Brooklyn : les quatre zones jouables de l'Acte 1 (Red Hook, Brownsville, Coney Island, Dumbo et le pont).
            new MapSpot("brooklyn", "1.1", "zone_ny_1_1", V(0.33f, 0.82f)),
            new MapSpot("brooklyn", "1.2", "zone_ny_1_2", V(0.53f, 0.78f)),
            new MapSpot("brooklyn", "1.3", "zone_ny_1_3", V(0.50f, 0.91f)),
            new MapSpot("brooklyn", "1.4", "zone_ny_1_4", V(0.38f, 0.68f)),
            // Points à venir des quartiers verrouillés.
            new MapSpot("queens", "2.1", null, V(0.62f, 0.52f)),
            new MapSpot("queens", "2.2", null, V(0.80f, 0.53f)),
            new MapSpot("bronx", "3.1", null, V(0.60f, 0.20f)),
            new MapSpot("bronx", "3.2", null, V(0.76f, 0.20f)),
            new MapSpot("staten", "4.1", null, V(0.12f, 0.82f)),
            new MapSpot("manhattan", "5.1", null, V(0.39f, 0.44f)),
        };

        public static BoroughShape Borough(string id)
        {
            foreach (var b in Boroughs) if (b.Id == id) return b;
            throw new KeyNotFoundException("Quartier inconnu : " + id);
        }

        public static MapSpot? SpotOf(string zoneId)
        {
            foreach (var s in Spots) if (s.ZoneId == zoneId) return s;
            return null;
        }
    }
}
