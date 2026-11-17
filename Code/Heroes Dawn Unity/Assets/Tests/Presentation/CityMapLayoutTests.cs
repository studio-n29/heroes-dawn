using System.Linq;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Mise en page de la carte de la ville : New York inchangée, Moscou avec ses quatre quartiers (Sergueiv jouable).</summary>
    public sealed class CityMapLayoutTests
    {
        static bool Inside(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
                if ((poly[i].y > p.y) != (poly[j].y > p.y) && p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x) inside = !inside;
            return inside;
        }

        [Test, Description("Vérifie que la carte de New York a quatre quartiers (un par chapitre) et seize points jouables, quatre par quartier — parce que chaque chapitre a ses quatre zones")]
        public void For_NewYork_HasFourBoroughs_WithSixteenPlayableSpots()
        {
            var ny = CityMapLayout.For("new_york");
            Assert.That(ny.Boroughs.Select(b => b.Id), Is.EqualTo(new[] { "harlem", "manhattan", "queens", "brooklyn" }));
            Assert.That(ny.Spots.Where(s => s.ZoneId != null).Select(s => s.ZoneId).Take(4), Is.EqualTo(new[] { "zone_ny_1_1", "zone_ny_1_2", "zone_ny_1_3", "zone_ny_1_4" }));
            Assert.That(ny.Spots.Count, Is.EqualTo(16));
            Assert.That(ny.Spots.All(s => s.ZoneId != null), Is.True);
            Assert.That(CityMapLayout.For("paris").Boroughs.Count, Is.EqualTo(4), "une ville sans carte propre retombe sur New York");
        }

        [Test, Description("Vérifie que la carte de Moscou a quatre quartiers et seize points jouables, quatre par quartier — parce que chaque chapitre a ses quatre zones")]
        public void For_Moscow_HasSixteenPlayableSpots_FourPerBorough()
        {
            var m = CityMapLayout.For("moscow");
            Assert.That(m.Boroughs.Select(b => b.Id), Is.EquivalentTo(new[] { "sergueiv", "kapotnya", "sadovoye", "place_rouge" }));
            var playable = m.Spots.Where(s => s.ZoneId != null).ToList();
            Assert.That(playable.Count, Is.EqualTo(16));
            Assert.That(playable.Take(4).Select(s => s.ZoneId), Is.EqualTo(new[] { "zone_mos_1_1", "zone_mos_1_2", "zone_mos_1_3", "zone_mos_1_4" }));
            Assert.That(playable.GroupBy(s => s.Borough).All(g => g.Count() == 4), Is.True);
            
            Assert.That(m.Boroughs.Where(b => b.Act == 1).Select(b => b.Id), Is.EqualTo(new[] { "sergueiv" }));
        }

        [Test, Description("Vérifie que chaque point de la carte de Moscou est dans son quartier et que deux points ne se chevauchent pas — parce que les boutons de zone doivent rester distincts sur les quatre formats")]
        public void Moscow_And_NewYork_SpotsAreInsideTheirBorough_AndDoNotOverlap()
        {
            foreach (var city in new[] { "moscow", "new_york" }) CheckCity(CityMapLayout.For(city));
        }

        static void CheckCity((System.Collections.Generic.IReadOnlyList<BoroughShape> Boroughs, System.Collections.Generic.IReadOnlyList<MapSpot> Spots) m)
        {
            foreach (var s in m.Spots)
            {
                var b = m.Boroughs.Single(x => x.Id == s.Borough);
                Assert.That(Inside(s.Position, b.Polygon), Is.True, s.Label + " dans " + b.Id);
            }
            for (int i = 0; i < m.Spots.Count; i++)
                for (int j = i + 1; j < m.Spots.Count; j++)
                    Assert.That(Vector2.Distance(m.Spots[i].Position, m.Spots[j].Position), Is.GreaterThan(0.08f), m.Spots[i].Label + " / " + m.Spots[j].Label);
        }

        [Test, Description("Vérifie que SpotOf retrouve les zones de Moscou comme celles de Brooklyn — parce que le présentateur positionne chaque zone par son point")]
        public void SpotOf_FindsZonesOfBothCities()
        {
            Assert.That(CityMapLayout.SpotOf("zone_mos_1_3")!.Value.Label, Is.EqualTo("M1.3"));
            Assert.That(CityMapLayout.SpotOf("zone_ny_1_3")!.Value.Label, Is.EqualTo("1.3"));
        }
    }
}
