using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Chapitre Sergueiv (Moscou, spec 2026-11-13) : quatre zones, objectifs, ennemis, mini-jeux et textes cohérents avec l'histoire.</summary>
    public sealed class MoscowCampaignTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();
        static System.Collections.Generic.List<ZoneDef> Zones() => C.Zones.Where(z => z.Borough == "sergueiv").OrderBy(z => z.Code).ToList();
        static ZoneDef Z(string code) => Zones().Single(z => z.Code == code);

        [Test, Description("Vérifie que Sergueiv compte quatre zones M1.1 à M1.4 dans l'ordre, acte 1, biome ville, déblocage linéaire (la première sans prérequis) — parce que c'est le chapitre validé par l'utilisateur")]
        public void FourZones_InOrder_Linear()
        {
            var zones = Zones();
            Assert.That(zones.Select(z => z.Code), Is.EqualTo(new[] { "M1.1", "M1.2", "M1.3", "M1.4" }));
            Assert.That(zones.Select(z => z.Id.Value), Is.EqualTo(new[] { "zone_mos_1_1", "zone_mos_1_2", "zone_mos_1_3", "zone_mos_1_4" }));
            Assert.That(zones.All(z => z.Act == 1 && z.Environment.Biome == Biome.City), Is.True);
            Assert.That(zones[0].Requires, Is.Empty);
            for (int i = 1; i < zones.Count; i++) Assert.That(zones[i].Requires.Select(r => r.Value), Is.EqualTo(new[] { zones[i - 1].Id.Value }), zones[i].Code);
        }

        [Test, Description("Vérifie que les objectifs de chaque zone ont les types de la spec §4, dans l'ordre — parce qu'ils viennent du game design de Moscou")]
        public void Objectives_MatchTheSpec()
        {
            var R = ObjectiveType.ReachCase; var M = ObjectiveType.CompleteMiniGame; var D = ObjectiveType.DefeatCombat;
            Assert.That(Z("M1.1").Objectives.Select(o => o.Type), Is.EqualTo(new[] { R, M, D, R }));
            Assert.That(Z("M1.2").Objectives.Select(o => o.Type), Is.EqualTo(new[] { R, M, D, R }));
            Assert.That(Z("M1.3").Objectives.Select(o => o.Type), Is.EqualTo(new[] { M, D, R }));
            Assert.That(Z("M1.4").Objectives.Select(o => o.Type), Is.EqualTo(new[] { M, D, D }));
            Assert.That(Z("M1.1").Objectives.Select(o => o.Target.Value), Is.EqualTo(new[] { "n03", "n13", "n14", "n19" }));
            Assert.That(Z("M1.2").Objectives.Select(o => o.Target.Value), Is.EqualTo(new[] { "n04", "n11", "n14", "n18" }));
            Assert.That(Z("M1.3").Objectives.Select(o => o.Target.Value), Is.EqualTo(new[] { "n11", "n14", "n18" }));
            Assert.That(Z("M1.4").Objectives.Select(o => o.Target.Value), Is.EqualTo(new[] { "n11", "n14", "n18" }));
        }

        [Test, Description("Vérifie que seule M1.4 a un boss (le Passeur) et que les autres zones n'en ont pas — parce que le chapitre se conclut sur un seul boss")]
        public void OnlyM14_HasTheBoss_Passeur()
        {
            foreach (var z in Zones().Where(z => z.Code != "M1.4"))
            {
                Assert.That(z.Encounters.Any(e => e.Rank == CombatRank.Boss), Is.False, z.Code);
                Assert.That(z.Nodes.Any(n => n.Rank == CombatRank.Boss), Is.False, z.Code);
            }
            var z4 = Z("M1.4");
            Assert.That(z4.Encounters.Single(e => e.Rank == CombatRank.Boss).Enemies.Select(e => e.Value), Is.EqualTo(new[] { "mos_passeur", "mos_smuggler", "mos_guard" }));
            Assert.That(z4.Nodes.Single(n => n.Id.Value == "n18").Rank, Is.EqualTo(CombatRank.Boss));
        }

        [Test, Description("Vérifie qu'aucune rencontre de Moscou n'inclut un ennemi de l'AMIF — parce que l'alliance reste dans l'ombre jusqu'à la Place Rouge")]
        public void NoAmifEnemy_InMoscowEncounters()
        {
            var enemies = Zones().SelectMany(z => z.Encounters).SelectMany(e => e.Enemies).Select(e => e.Value).Distinct().ToList();
            Assert.That(enemies.Any(e => e.StartsWith("amif_") || e == "bridge_titan"), Is.False, string.Join(",", enemies));
            Assert.That(enemies.All(e => C.Enemies.Any(d => d.Id.Value == e)), Is.True, "tous les ennemis existent");
        }

        [Test, Description("Vérifie que les mini-jeux des zones sont les existants (infiltrer, engrenages, sauvetage, caisses) sur les bonnes cases — parce qu'aucun nouveau mini-jeu n'est créé pour la 1.0.0")]
        public void MiniGames_AreTheExistingOnes()
        {
            string Act(string zone, string node) => Z(zone).Nodes.Single(n => n.Id.Value == node).Activity?.Value;
            Assert.That(Act("M1.1", "n13"), Is.EqualTo("infiltration_camp"));
            Assert.That(Act("M1.2", "n11"), Is.EqualTo("gears_workshop"));
            Assert.That(Act("M1.3", "n11"), Is.EqualTo("rooftop_rescue"));
            Assert.That(Act("M1.4", "n11"), Is.EqualTo("crate_pick_depot"));
            foreach (var z in Zones())
                foreach (var n in z.Nodes.Where(n => n.Activity != null)) Assert.That(C.HasActivity(n.Activity.Value), Is.True, z.Code + " " + n.Id.Value);
        }

        [Test, Description("Vérifie que deux zones ne partagent jamais le même code d'affichage — parce que des tests et des lectures cherchent les zones par code")]
        public void NoCodeCollision_WithBrooklyn()
        {
            var codes = C.Zones.Where(z => !z.IsTutorial).Select(z => z.Code).ToList();
            Assert.That(codes.Distinct().Count(), Is.EqualTo(codes.Count), string.Join(",", codes));
        }

        static readonly N29.HeroesDawn.Presentation.Mvp.LocLocalization L = new N29.HeroesDawn.Presentation.Mvp.LocLocalization();

        // Toutes les clés de texte de la campagne de Moscou (hors ennemis, couverts par MoscowEnemyTests).
        static System.Collections.Generic.List<string> CampaignKeys()
        {
            var keys = new System.Collections.Generic.List<string>();
            foreach (var z in Zones())
            {
                keys.Add(z.NameKey);
                keys.Add(z.NameKey.Replace(".name", ".short"));
                keys.Add(z.NameKey.Replace(".name", ".code"));
                keys.Add(z.NameKey.Replace(".name", ".desc"));
                keys.Add("qg.fil." + z.Id.Value);
                foreach (var o in z.Objectives) keys.Add(o.NameKey);
                foreach (var n in z.Nodes.Where(n => n.Narration != null))
                    foreach (var line in n.Narration.Before.Concat(n.Narration.After)) keys.Add(line.TextKey);
                foreach (var id in z.Encounters.SelectMany(e => e.Enemies).Distinct())
                {
                    keys.Add("enemies." + id.Value + ".name");
                    foreach (var s in C.Enemy(id).Skills) keys.Add("skills." + s.Id.Value);
                }
            }
            return keys.Distinct().ToList();
        }

        [Test, Description("Vérifie que chaque nom, description, objectif, réplique, ennemi et compétence des quatre zones de Moscou a un texte français — parce qu'une clé manquante s'afficherait telle quelle à l'écran")]
        public void EveryTextOfTheCampaign_Exists()
        {
            var missing = CampaignKeys().Where(k => !L.Has(k)).ToList();
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test, Description("Vérifie que chaque texte propre à Moscou (zones, objectifs, répliques) a aussi une version anglaise — parce que le jeu se joue en anglais sans clé française résiduelle")]
        public void EveryTextOfTheCampaign_ExistsInEnglish()
        {
            var en = new System.Collections.Generic.HashSet<string>(N29.HeroesDawn.Presentation.Common.Loc.EnglishKeys());
            // Les ennemis réutilisés de Brooklyn (thug, brute, street_pyro) n'ont pas de version anglaise : hors périmètre de Moscou.
            var missing = CampaignKeys().Where(k => (k.StartsWith("zones.mos") || k.StartsWith("qg.fil.zone_mos") || k.StartsWith("objectives.mos") || k.StartsWith("narration.mos") || k.Contains("mos_")) && !en.Contains(k)).ToList();
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test, Description("Vérifie que chaque personnage qui parle à Moscou a un nom affichable (agent_m, ilona_varga, passeur, enforcer) — parce que la boîte de dialogue montre le nom du locuteur")]
        public void EverySpeaker_HasADisplayName()
        {
            var members = C.Members.Select(m => m.Id.Value).ToHashSet();
            var speakers = Zones().SelectMany(z => z.Nodes).Where(n => n.Narration != null).SelectMany(n => n.Narration.Before.Concat(n.Narration.After)).Select(l => l.Speaker).Distinct().ToList();
            Assert.That(speakers, Is.SupersetOf(new[] { "agent_m", "ilona_varga", "passeur", "enforcer" }));
            var missing = speakers.Where(s => !members.Contains(s) && !L.Has("speaker." + s)).ToList();
            Assert.That(missing, Is.Empty, string.Join(", ", missing));
            Assert.That(L.Has("speaker.agent_m"), Is.True);
        }

        [Test, Description("Vérifie que « Aube tactique » n'apparaît dans aucun texte de Moscou, en français comme en anglais — parce que ce nom ne doit jamais être visible dans le jeu")]
        public void NoText_ContainsAubeTactique()
        {
            var bad = CampaignKeys().Where(k => L.Has(k) && (L.T(k).Contains("Aube tactique") || L.T(k).Contains("Tactical Dawn"))).ToList();
            Assert.That(bad, Is.Empty, string.Join(", ", bad));
        }

        [Test, Description("Vérifie que chaque rencontre ne dépasse pas trois ennemis et que chaque rang de combat présent dans le plateau a une rencontre — parce qu'un combat sans table planterait")]
        public void EveryCombatRank_HasAnEncounter()
        {
            foreach (var z in Zones())
            {
                Assert.That(z.Encounters.All(e => e.Enemies.Count >= 1 && e.Enemies.Count <= 3), Is.True, z.Code);
                foreach (var r in z.Nodes.Where(n => n.Type == CaseType.Combat).Select(n => n.Rank).Distinct())
                    Assert.That(z.Encounters.Any(e => e.Rank == r), Is.True, z.Code + " " + r);
            }
        }
    }
}
