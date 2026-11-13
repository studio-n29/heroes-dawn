using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.EditorTools.Balance;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>
    /// Équilibrage de Moscou (tâche 8 du plan, spec §8) : même simulateur et même protocole que Brooklyn
    /// (équipe de départ + objets, niveaux 5 et 10, graine fixe, 200 combats par rencontre). Rapport : docs/test-reports/2026-11-13_equilibrage-moscou.md.
    /// </summary>
    [Category("Balance")]
    public sealed class MoscowBalanceTests
    {
        static readonly MemberId[] Team = { new("giulia_rossi"), new("ilona_varga"), new("lucas_mendes") };

        static ContentCatalog Content() => PrototypeContent.Build().WithZones(ZoneLibrary.Load().Zones);

        /// <summary>Taux de victoire (%) de chaque rencontre du rang, pour chaque profil d'objets et chaque niveau (5 et 10).</summary>
        static List<double> Rates(ContentCatalog c, string zone, CombatRank rank)
        {
            var z = c.Zone(new ZoneId(zone));
            var rates = new List<double>();
            foreach (var profile in new[] { EquipmentProfile.ItemsOnly, EquipmentProfile.ItemsStrong })
            foreach (int level in new[] { 5, 10 })
            {
                var stats = BalanceSimulation.Run(c, z.Id, new SimulationRequest { Team = Team, TeamLevel = level, RunsPerEncounter = 200, BaseSeed = 7, Equipment = profile });
                foreach (var s in stats.Where(s => s.Rank == rank)) rates.Add(s.Wins * 100.0 / System.Math.Max(1, s.Runs));
            }
            return rates;
        }

        [Test, Timeout(900000), Description("Vérifie que le Passeur est battu entre 40 et 60 % (moyenne équipe de départ + objets, niveaux 5 et 10, tolérance ±5 points) sans combat interminable — parce que la cible de l'équilibrage du boss est de 40 à 60 % comme à Brooklyn")]
        public void Passeur_WinRate_InTarget()
        {
            var c = Content();
            var rates = Rates(c, "zone_mos_1_4", CombatRank.Boss);
            Assert.That(rates.Count, Is.EqualTo(4));
            double mean = rates.Average();
            Assert.That(mean, Is.InRange(35.0, 65.0), "taux moyen " + mean.ToString("0") + " % (" + string.Join(", ", rates.Select(r => r.ToString("0"))) + ")");
            Assert.That(rates.All(r => r >= 20 && r <= 80), Is.True, "aucun cas extrême : " + string.Join(", ", rates.Select(r => r.ToString("0"))));
        }

        [Test, Timeout(900000), Description("Vérifie que les élites de Moscou se battent à 60 à 85 % au niveau recommandé de leur zone (moyenne interpolée objets moyens et forts, tolérance ±8 points) — parce que l'élite AMIF de Brooklyn (13 à 31 %) ne doit pas être reproduite")]
        public void Elites_WinRate_InTarget()
        {
            var c = Content();
            // Niveaux recommandés 5, 6, 8, 10 : on pondère les mesures aux niveaux 5 et 10 par la distance au niveau recommandé.
            var zones = new[] { ("zone_mos_1_1", 5), ("zone_mos_1_2", 6), ("zone_mos_1_3", 8), ("zone_mos_1_4", 10) };
            foreach (var (zone, rec) in zones)
            {
                var z = c.Zone(new ZoneId(zone));
                var perLevel = new Dictionary<int, double>();
                foreach (int level in new[] { 5, 10 })
                {
                    var vals = new List<double>();
                    foreach (var profile in new[] { EquipmentProfile.ItemsOnly, EquipmentProfile.ItemsStrong })
                        foreach (var s in BalanceSimulation.Run(c, z.Id, new SimulationRequest { Team = Team, TeamLevel = level, RunsPerEncounter = 200, BaseSeed = 7, Equipment = profile }).Where(s => s.Rank == CombatRank.Elite))
                            vals.Add(s.Wins * 100.0 / System.Math.Max(1, s.Runs));
                    perLevel[level] = vals.Average();
                }
                double w = (rec - 5) / 5.0;
                double atRec = perLevel[5] * (1 - w) + perLevel[10] * w;
                Assert.That(atRec, Is.InRange(52.0, 93.0), zone + " : " + atRec.ToString("0") + " % au niveau " + rec);
            }
        }

        [Test, Timeout(900000), Description("Vérifie que les combats ordinaires de Moscou (faciles et normaux) sont gagnés par l'équipe de départ à 95 % ou plus au niveau 5 — parce que le parcours sans mort sur le chemin court doit rester possible")]
        public void OrdinaryFights_AreSafe_ForTheStartingTeam()
        {
            var c = Content();
            foreach (var zone in new[] { "zone_mos_1_1", "zone_mos_1_2", "zone_mos_1_3", "zone_mos_1_4" })
            {
                var z = c.Zone(new ZoneId(zone));
                foreach (var s in BalanceSimulation.Run(c, z.Id, new SimulationRequest { Team = Team, TeamLevel = 5, RunsPerEncounter = 100, BaseSeed = 7, Equipment = EquipmentProfile.ItemsOnly })
                             .Where(s => s.Rank == CombatRank.Easy || s.Rank == CombatRank.Normal))
                    Assert.That(s.Wins * 100.0 / s.Runs, Is.GreaterThanOrEqualTo(95.0), zone + " rencontre " + s.EncounterIndex);
            }
        }
    }
}
