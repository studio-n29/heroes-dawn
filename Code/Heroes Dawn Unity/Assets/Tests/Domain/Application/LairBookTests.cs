using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Repaires (L12 de la 0.15.0) : zone du donjon, boss retenus d'une partie à l'autre, repaires terminés.</summary>
    public sealed class LairBookTests
    {
        static LairDef Lair => Lairs.All[0];

        [Test, Description("Vérifie que la zone du repaire existe, est classée « repaire » et que ses boss fixes (ennemi seul sur la case) et ses objectifs correspondent à la définition du repaire — parce que la page Repaire et la validation de zone doivent parler des mêmes boss")]
        public void LairZone_MatchesItsDefinition()
        {
            var app = new TestApp();
            var zone = app.Session.Content.Zones.Single(z => z.Id.Value == Lair.Zone);
            Assert.That(zone.IsLair, Is.True);
            Assert.That(zone.Objectives.Count, Is.EqualTo(Lair.Bosses.Count));
            foreach (var boss in Lair.Bosses)
            {
                var node = zone.Nodes.Single(n => n.Id.Value == boss.Case);
                Assert.That(node.Rank, Is.EqualTo(CombatRank.Boss));
                Assert.That(node.IsFixed, Is.True);
                Assert.That(node.Combat!.EnemyTable.Select(e => e.Value), Is.EqualTo(new[] { boss.Enemy }));
                Assert.That(zone.Objectives.Any(o => o.Target.Value == boss.Case && o.Type == ObjectiveType.DefeatCombat), Is.True, boss.Case);
                Assert.That(app.Session.Content.Enemies.Any(e => e.Id.Value == boss.Enemy && e.IsBoss), Is.True, boss.Enemy);
            }
        }

        [Test, Description("Vérifie qu'on traverse le repaire case par case : chaque boss se bat seul face à l'escouade, la zone n'est validée qu'après le dernier — parce que c'est le « de bout en bout » du repaire")]
        public void WalkingTheLair_FightsEachBossAlone_AndValidatesAfterTheLastOne()
        {
            var app = new TestApp();
            var zone = app.Session.Content.Zones.Single(z => z.Id.Value == Lair.Zone);
            var run = N29.HeroesDawn.Domain.Board.ZoneRun.ForTests(app.Session.Content, zone, 5UL, 1, true, "n00");
            bool validated = false;
            int bosses = 0;
            foreach (var node in zone.Nodes.Skip(1))
            {
                run.SetPmForTests(1);
                var events = run.MoveTo(node.Id).ToList();
                if (run.Phase == N29.HeroesDawn.Domain.Board.ZoneRunPhase.InCombat)
                {
                    var boss = Lair.Bosses.FirstOrDefault(b => b.Case == node.Id.Value);
                    if (boss != null)
                    {
                        var enemies = run.PendingCase!.Encounter!.Waves.SelectMany(w => w).Select(e => e.Value).ToList();
                        Assert.That(enemies, Is.EqualTo(new[] { boss.Enemy }), node.Id.Value);
                        bosses++;
                        Assert.That(validated, Is.False, "pas de validation avant le dernier boss");
                    }
                    events = run.ResolveCombat(true).ToList();
                }
                else if (run.Phase == N29.HeroesDawn.Domain.Board.ZoneRunPhase.InCaseInteraction) events = run.ResolveCase(true).ToList();
                validated |= events.Any(e => e is N29.HeroesDawn.Domain.Board.ZoneValidatedEvent);
            }
            Assert.That(bosses, Is.EqualTo(3));
            Assert.That(validated, Is.True);
            Assert.That(run.Phase, Is.EqualTo(N29.HeroesDawn.Domain.Board.ZoneRunPhase.Completed));
        }

        [Test, Description("Vérifie que le repaire reste fermé tant que Dumbo n'est pas validé et que la zone ne devient jamais la zone recommandée de l'histoire — parce qu'un donjon facultatif ne doit pas détourner le fil conducteur")]
        public void Lair_OpensAfterBrooklyn_AndNeverBecomesTheRecommendedZone()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            Assert.That(app.Lairs.Unlocked(Lair), Is.False);
            foreach (var z in app.Session.Content.Zones.Where(z => z.Borough == "brooklyn")) app.Session.CompletedZones.Add(z.Id.Value);
            Assert.That(app.Lairs.Unlocked(Lair), Is.True);
            Assert.That(ZoneProgress.Recommended(app.Session).IsLair, Is.False);
        }

        [Test, Description("Vérifie qu'un boss de repaire vaincu est retenu une seule fois, que les autres cases et les autres zones sont ignorées, et que la liste des boss suit — parce que la progression entre boss doit se lire sur la page")]
        public void BossDefeats_AreRecorded_OnceAndOnlyForLairBosses()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            LairBook.RecordBossDefeat(app.Session.Meta, Lair.Zone, "n04");
            LairBook.RecordBossDefeat(app.Session.Meta, Lair.Zone, "n04");
            LairBook.RecordBossDefeat(app.Session.Meta, Lair.Zone, "n06");
            LairBook.RecordBossDefeat(app.Session.Meta, "zone_ny_1_3", "n18");
            Assert.That(app.Session.Meta.lairBosses, Is.EqualTo(new[] { "amif_vault:n04" }));
            var bosses = app.Lairs.Bosses(Lair);
            Assert.That(bosses.Select(b => b.Defeated), Is.EqualTo(new[] { true, false, false }));
            Assert.That(app.Lairs.DefeatedCount(Lair), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que chaque validation de la zone du repaire compte un repaire terminé de plus et qu'une autre zone n'en compte aucun — parce que le bilan affiche ce compteur")]
        public void Clears_AreCounted()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            LairBook.RecordClear(app.Session.Meta, Lair.Zone);
            LairBook.RecordClear(app.Session.Meta, Lair.Zone);
            LairBook.RecordClear(app.Session.Meta, "zone_ny_1_4");
            Assert.That(app.Lairs.Clears(Lair), Is.EqualTo(2));
        }

        [Test, Description("Vérifie que boss vaincus et repaires terminés survivent à une sauvegarde et qu'une ancienne sauvegarde sans le champ se charge — parce que la progression entre boss doit être sauvegardée sans casser les profils existants")]
        public void Progress_SurvivesSaveAndLoad()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            LairBook.RecordBossDefeat(app.Session.Meta, Lair.Zone, "n09");
            LairBook.RecordClear(app.Session.Meta, Lair.Zone);
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(app.Session.ToProfile(System.DateTime.UtcNow))));
            Assert.That(restored.Meta.lairBosses, Is.EqualTo(new[] { "amif_vault:n09" }));
            Assert.That(restored.Meta.GetCounter(restored.Meta.stats, "lair_clears:amif_vault"), Is.EqualTo(1));
            Assert.That(JsonUtility.FromJson<MetaSave>("{\"gems\":5}").lairBosses, Is.Not.Null.And.Empty);
        }
    }
}
