using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Les six chapitres suivants (Queens, Harlem, Manhattan, Kapotnya, Sadovoye Koltso, Place Rouge) : quatre plateaux valides chacun, boss en 4e zone, textes FR et EN.</summary>
    public sealed class ChaptersCampaignTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static readonly (string Borough, string Prefix, string Last)[] All =
        {
            ("queens", "zone_ny_2_", "zone_ny_1_4"), ("harlem", "zone_ny_3_", "zone_ny_2_4"), ("manhattan", "zone_ny_4_", "zone_ny_3_4"),
            ("kapotnya", "zone_mos_2_", "zone_mos_1_4"), ("sadovoye", "zone_mos_3_", "zone_mos_2_4"), ("place_rouge", "zone_mos_4_", "zone_mos_3_4"),
        };

        static List<ZoneDef> Zones(string borough) => C.Zones.Where(z => z.Borough == borough).ToList();

        [Test, Description("Vérifie que chaque chapitre a quatre zones aux ids attendus, à déblocage linéaire (la première derrière la dernière du chapitre précédent) — parce que les chapitres se jouent dans l'ordre")]
        public void EveryChapter_HasFourLinearZones()
        {
            foreach (var (borough, prefix, last) in All)
            {
                var zones = Zones(borough);
                Assert.That(zones.Select(z => z.Id.Value), Is.EqualTo(Enumerable.Range(1, 4).Select(i => prefix + i)), borough);
                Assert.That(zones[0].Requires.Select(r => r.Value), Is.EqualTo(new[] { last }), borough);
                for (int i = 1; i < 4; i++) Assert.That(zones[i].Requires.Select(r => r.Value), Is.EqualTo(new[] { zones[i - 1].Id.Value }), zones[i].Id.Value);
            }
        }

        [Test, Description("Vérifie qu'aucun id de zone, aucun code d'une même ville et aucun id d'ennemi n'est en double dans le catalogue — parce qu'un doublon écraserait une zone à l'import")]
        public void NoDuplicateIds()
        {
            Assert.That(C.Zones.GroupBy(z => z.Id.Value).Where(g => g.Count() > 1).Select(g => g.Key), Is.Empty);
            Assert.That(C.Enemies.GroupBy(e => e.Id.Value).Where(g => g.Count() > 1).Select(g => g.Key), Is.Empty);
            Assert.That(C.Zones.Where(z => !z.IsTutorial && !z.IsLair).GroupBy(z => z.Code).Where(g => g.Count() > 1).Select(g => g.Key), Is.Empty);
        }

        [Test, Description("Vérifie que chaque plateau passe le validateur sans erreur — parce qu'une zone invalide ne se jouerait pas")]
        public void EveryZone_IsPlayable()
        {
            foreach (var (borough, _, _) in All)
                foreach (var z in Zones(borough))
                    Assert.That(ZoneValidator.Validate(z, C).Where(i => i.Severity == IssueSeverity.Error).Select(i => i.ToString()), Is.Empty, z.Id.Value);
        }

        [Test, Description("Vérifie que chaque zone a trois ou quatre objectifs dont un combat clé, des rencontres Facile, Normal et Élite, et que seule la 4e zone a un boss — parce que c'est la règle du plateau")]
        public void ObjectivesEncountersAndBoss_FollowTheRules()
        {
            foreach (var (borough, _, _) in All)
            {
                var zones = Zones(borough);
                for (int i = 0; i < 4; i++)
                {
                    var z = zones[i];
                    Assert.That(z.Objectives.Count, Is.InRange(3, 4), z.Id.Value);
                    Assert.That(z.Objectives.Any(o => o.Type == ObjectiveType.DefeatCombat), Is.True, z.Id.Value);
                    foreach (var rank in new[] { CombatRank.Easy, CombatRank.Normal, CombatRank.Elite })
                        Assert.That(z.Encounters.Any(e => e.Rank == rank), Is.True, z.Id.Value + " " + rank);
                    Assert.That(z.Encounters.Any(e => e.Rank == CombatRank.Boss), Is.EqualTo(i == 3), z.Id.Value);
                    Assert.That(z.Nodes.Any(n => n.Type == CaseType.Combat && n.Rank == CombatRank.Boss), Is.EqualTo(i == 3), z.Id.Value);
                    Assert.That(z.Act, Is.EqualTo(zones[0].Act), z.Id.Value);
                }
            }
        }

        [Test, Description("Vérifie qu'aucun ennemi de l'AMIF n'apparaît dans les rencontres de Moscou avant la Place Rouge, et que chaque ennemi cité existe — parce que l'alliance reste dans l'ombre jusque-là")]
        public void NoAmifEnemy_InMoscowBeforePlaceRouge()
        {
            foreach (var (borough, _, _) in All)
            {
                var enemies = Zones(borough).SelectMany(z => z.Encounters).SelectMany(e => e.Enemies).Select(e => e.Value).Distinct().ToList();
                Assert.That(enemies.All(e => C.Enemies.Any(d => d.Id.Value == e)), Is.True, borough);
                if (StoryRoutes.CityOfBorough(borough) == StoryRoutes.Moscow && borough != "place_rouge")
                    Assert.That(enemies.Any(e => e.StartsWith("amif_") || e == "bridge_titan"), Is.False, borough);
            }
        }

        static IEnumerable<string> Keys(string borough)
        {
            foreach (var z in Zones(borough))
            {
                yield return z.NameKey; yield return z.NameKey.Replace(".name", ".short"); yield return z.NameKey.Replace(".name", ".code"); yield return z.NameKey.Replace(".name", ".desc");
                yield return "qg.fil." + z.Id.Value; yield return "archives.zone." + z.Id.Value;
                foreach (var o in z.Objectives) yield return o.NameKey;
                foreach (var n in z.Nodes.Where(n => n.Narration != null))
                    foreach (var l in n.Narration.Before.Concat(n.Narration.After)) { yield return l.TextKey; if (!C.Members.Any(m => m.Id.Value == l.Speaker)) yield return "speaker." + l.Speaker; }
                foreach (var id in z.Encounters.SelectMany(e => e.Enemies).Distinct())
                {
                    var e = C.Enemy(id);
                    yield return "enemies." + id.Value + ".name";
                    foreach (var s in e.Skills) yield return "skills." + s.Id.Value;
                }
            }
        }

        [Test, Description("Vérifie que chaque nom, description, objectif, réplique, locuteur, ennemi et compétence des six chapitres a un texte français — parce qu'une clé manquante s'afficherait telle quelle")]
        public void EveryText_ExistsInFrench()
        {
            var missing = All.SelectMany(a => Keys(a.Borough)).Distinct().Where(k => !Loc.Has(k)).ToList();
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test, Description("Vérifie que les textes propres aux six chapitres (zones, objectifs, ennemis nouveaux) ont aussi une version anglaise (les répliques attendent la passe anglaise issue du script) — parce que le jeu se joue en anglais")]
        public void EveryChapterText_ExistsInEnglish()
        {
            var en = new HashSet<string>(Loc.EnglishKeys());
            var prefixes = new[] { "syn_", "hrl_", "mnh_", "kap_", "sad_", "pr_" };
            bool Own(string k) => k.StartsWith("zones.") || k.StartsWith("qg.fil.") || k.StartsWith("archives.zone.") || k.StartsWith("objectives.") || k.StartsWith("narration.")
                                  || prefixes.Any(p => k.Contains("." + p) || k.Contains("skills." + p));
            var missing = All.SelectMany(a => Keys(a.Borough)).Distinct().Where(k => Own(k) && !en.Contains(k)).ToList();
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test, Description("Vérifie que « Aube tactique » n'apparaît dans aucun texte des six chapitres — parce que ce nom ne doit jamais être visible dans le jeu")]
        public void NoText_ContainsAubeTactique()
        {
            var bad = All.SelectMany(a => Keys(a.Borough)).Distinct().Where(k => Loc.Has(k) && (Loc.T(k).Contains("Aube tactique") || Loc.T(k).Contains("Tactical Dawn"))).ToList();
            Assert.That(bad, Is.Empty, string.Join(", ", bad));
        }
    }
}
