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
    /// Géographie stylisée de New York (maquette M60) : 4 quartiers, un par chapitre (Brooklyn 1, Queens 2, Harlem 3, Manhattan 4),
    /// et les points précis de mission (quatre par quartier).
    /// </summary>
    public static class CityMapLayout
    {
        static Vector2 V(float x, float y) => new Vector2(x, y);

        public static readonly IReadOnlyList<BoroughShape> Boroughs = new[]
        {
            new BoroughShape("harlem", 3, V(0.67f, 0.10f), V(0.50f, 0.04f), V(0.78f, 0.06f), V(0.87f, 0.20f), V(0.72f, 0.27f), V(0.55f, 0.21f)),
            new BoroughShape("manhattan", 4, V(0.39f, 0.27f), V(0.33f, 0.16f), V(0.43f, 0.12f), V(0.49f, 0.24f), V(0.46f, 0.50f), V(0.38f, 0.56f), V(0.30f, 0.38f)),
            new BoroughShape("queens", 2, V(0.73f, 0.37f), V(0.53f, 0.30f), V(0.89f, 0.27f), V(0.96f, 0.54f), V(0.72f, 0.68f), V(0.53f, 0.60f)),
            new BoroughShape("brooklyn", 1, V(0.60f, 0.695f), V(0.29f, 0.60f), V(0.50f, 0.62f), V(0.69f, 0.72f), V(0.66f, 0.93f), V(0.40f, 0.97f), V(0.24f, 0.81f)),
        };

        public static readonly IReadOnlyList<MapSpot> Spots = new[]
        {
            // Brooklyn : les quatre zones jouables de l'Acte 1 (Red Hook, Brownsville, Coney Island, Dumbo et le pont).
            new MapSpot("brooklyn", "1.1", "zone_ny_1_1", V(0.33f, 0.82f)),
            new MapSpot("brooklyn", "1.2", "zone_ny_1_2", V(0.53f, 0.78f)),
            new MapSpot("brooklyn", "1.3", "zone_ny_1_3", V(0.50f, 0.91f)),
            new MapSpot("brooklyn", "1.4", "zone_ny_1_4", V(0.38f, 0.68f)),
            // Queens : le marché de Flushing, les indics d'Astoria, le triage de Jamaica, le terminal de fret.
            new MapSpot("queens", "2.1", "zone_ny_2_1", V(0.62f, 0.38f)),
            new MapSpot("queens", "2.2", "zone_ny_2_2", V(0.82f, 0.37f)),
            new MapSpot("queens", "2.3", "zone_ny_2_3", V(0.84f, 0.55f)),
            new MapSpot("queens", "2.4", "zone_ny_2_4", V(0.64f, 0.55f)),
            // Harlem : le bloc incendié, la clinique, le marché de rue, la rue de l'atelier.
            new MapSpot("harlem", "3.1", "zone_ny_3_1", V(0.60f, 0.12f)),
            new MapSpot("harlem", "3.2", "zone_ny_3_2", V(0.72f, 0.11f)),
            new MapSpot("harlem", "3.3", "zone_ny_3_3", V(0.78f, 0.18f)),
            new MapSpot("harlem", "3.4", "zone_ny_3_4", V(0.66f, 0.20f)),
            // Manhattan : Times Square, les tours de Midtown, le siège de l'ONU, l'esplanade.
            new MapSpot("manhattan", "4.1", "zone_ny_4_1", V(0.40f, 0.22f)),
            new MapSpot("manhattan", "4.2", "zone_ny_4_2", V(0.42f, 0.32f)),
            new MapSpot("manhattan", "4.3", "zone_ny_4_3", V(0.40f, 0.43f)),
            new MapSpot("manhattan", "4.4", "zone_ny_4_4", V(0.38f, 0.52f)),
        };

        // --- Moscou (chapitres du parcours d'Agent M) : Sergueiv au nord-ouest, Kapotnya au sud-est, l'anneau Sadovoye Koltso autour de la Place Rouge.
        // « Act » vaut ici le numéro du chapitre ; chaque quartier a ses quatre points de mission.

        /// <summary>Anneau ouvert à l'est (Sadovoye Koltso) : arc extérieur puis arc intérieur en sens inverse, polygone simple.</summary>
        static Vector2[] RingPolygon(float cx, float cy, float outer, float inner, int gapDeg)
        {
            var pts = new List<Vector2>();
            int from = gapDeg, to = 360 - gapDeg;
            for (int a = from; a <= to; a += 30) pts.Add(V(cx + outer * (float)System.Math.Cos(a * System.Math.PI / 180), cy + outer * (float)System.Math.Sin(a * System.Math.PI / 180)));
            for (int a = to; a >= from; a -= 30) pts.Add(V(cx + inner * (float)System.Math.Cos(a * System.Math.PI / 180), cy + inner * (float)System.Math.Sin(a * System.Math.PI / 180)));
            return pts.ToArray();
        }

        static readonly IReadOnlyList<BoroughShape> MoscowBoroughs = new[]
        {
            new BoroughShape("sergueiv", 1, V(0.24f, 0.24f), V(0.05f, 0.12f), V(0.40f, 0.06f), V(0.46f, 0.30f), V(0.30f, 0.46f), V(0.08f, 0.42f)),
            new BoroughShape("kapotnya", 2, V(0.78f, 0.76f), V(0.62f, 0.60f), V(0.92f, 0.56f), V(0.95f, 0.88f), V(0.66f, 0.92f), V(0.56f, 0.76f)),
            new BoroughShape("sadovoye", 3, V(0.50f, 0.33f), RingPolygon(0.50f, 0.54f, 0.17f, 0.10f, 25)),
            // Octogone dans le vide de l'anneau (rayon intérieur 0,10) : assez large pour ses quatre points espacés de plus de 0,08.
            new BoroughShape("place_rouge", 4, V(0.50f, 0.54f), V(0.50f, 0.448f), V(0.565f, 0.475f), V(0.592f, 0.54f), V(0.565f, 0.605f), V(0.50f, 0.632f), V(0.435f, 0.605f), V(0.408f, 0.54f), V(0.435f, 0.475f)),
        };

        static readonly IReadOnlyList<MapSpot> MoscowSpots = new[]
        {
            // Sergueiv : les quatre zones jouables du chapitre (gare du maglev, monastère, voie du maglev, dépôt).
            new MapSpot("sergueiv", "M1.1", "zone_mos_1_1", V(0.15f, 0.20f)),
            new MapSpot("sergueiv", "M1.2", "zone_mos_1_2", V(0.30f, 0.14f)),
            new MapSpot("sergueiv", "M1.3", "zone_mos_1_3", V(0.34f, 0.30f)),
            new MapSpot("sergueiv", "M1.4", "zone_mos_1_4", V(0.18f, 0.34f)),
            // Kapotnya : la raffinerie, la route du convoi, le laboratoire clandestin, le quai de chargement.
            new MapSpot("kapotnya", "M2.1", "zone_mos_2_1", V(0.70f, 0.68f)),
            new MapSpot("kapotnya", "M2.2", "zone_mos_2_2", V(0.85f, 0.66f)),
            new MapSpot("kapotnya", "M2.3", "zone_mos_2_3", V(0.84f, 0.80f)),
            new MapSpot("kapotnya", "M2.4", "zone_mos_2_4", V(0.69f, 0.82f)),
            // Sadovoye Koltso : sur l'anneau, les tours de verre, les enchères, le studio d'hologrammes, la tour Orlan.
            new MapSpot("sadovoye", "M3.1", "zone_mos_3_1", V(0.373f, 0.494f)),
            new MapSpot("sadovoye", "M3.2", "zone_mos_3_2", V(0.50f, 0.675f)),
            new MapSpot("sadovoye", "M3.3", "zone_mos_3_3", V(0.634f, 0.6175f)),
            new MapSpot("sadovoye", "M3.4", "zone_mos_3_4", V(0.50f, 0.405f)),
            // Place Rouge : les rues sous couvre-feu, les tunnels, la salle du conseil (deux plateaux).
            new MapSpot("place_rouge", "M4.1", "zone_mos_4_1", V(0.4575f, 0.4975f)),
            new MapSpot("place_rouge", "M4.2", "zone_mos_4_2", V(0.5425f, 0.4975f)),
            new MapSpot("place_rouge", "M4.3", "zone_mos_4_3", V(0.4575f, 0.5825f)),
            new MapSpot("place_rouge", "M4.4", "zone_mos_4_4", V(0.5425f, 0.5825f)),
        };

        /// <summary>Carte d'une ville : Moscou a la sienne ; toute autre ville (sans carte propre) reprend New York.</summary>
        public static (IReadOnlyList<BoroughShape> Boroughs, IReadOnlyList<MapSpot> Spots) For(string city)
            => city == "moscow" ? (MoscowBoroughs, MoscowSpots) : (Boroughs, Spots);

        public static BoroughShape Borough(string id)
        {
            foreach (var b in Boroughs) if (b.Id == id) return b;
            foreach (var b in MoscowBoroughs) if (b.Id == id) return b;
            throw new KeyNotFoundException("Quartier inconnu : " + id);
        }

        public static MapSpot? SpotOf(string zoneId)
        {
            foreach (var s in Spots) if (s.ZoneId == zoneId) return s;
            foreach (var s in MoscowSpots) if (s.ZoneId == zoneId) return s;
            return null;
        }
    }
}
