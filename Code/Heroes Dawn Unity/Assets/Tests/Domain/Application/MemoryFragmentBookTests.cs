using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Fragments de souvenirs (L11 de la 0.15.0) et statut Locked des chapitres.</summary>
    public sealed class MemoryFragmentBookTests
    {
        [Test, Description("Vérifie que chaque fragment pointe vers un chapitre et une zone qui existent — parce qu'un fragment orphelin ne se débloquerait jamais")]
        public void EveryFragment_HasItsChapterAndZone()
        {
            var zones = TestContent.Build().Zones.Concat(PrototypeZones.Zones()).Select(z => z.Id.Value).ToList();
            foreach (var f in MemoryFragments.All)
            {
                Assert.That(Chapters.Find(f.ChapterId), Is.Not.Null, f.Id);
                Assert.That(zones, Does.Contain(f.UnlockZone), f.Id);
            }
            Assert.That(MemoryFragments.All.Select(f => f.Id).Distinct().Count(), Is.EqualTo(MemoryFragments.All.Count));
        }

        [Test, Description("Vérifie qu'un fragment se débloque quand sa zone est validée et que les compteurs du chapitre suivent — parce que la page Chapitres s'appuie dessus")]
        public void Unlock_FollowsTheCompletedZone()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            Assert.That(app.Fragments.UnlockedCount("ny_brooklyn"), Is.EqualTo(0));
            app.Session.CompletedZones.Add("zone_ny_1_2");
            Assert.That(app.Fragments.UnlockedCount("ny_brooklyn"), Is.EqualTo(1));
            Assert.That(app.Fragments.NewCount("ny_brooklyn"), Is.EqualTo(1));
            app.Fragments.MarkRead("brownsville_hostages");
            Assert.That(app.Fragments.NewCount("ny_brooklyn"), Is.EqualTo(0));
            Assert.That(app.Fragments.UnlockedCount("ny_queens"), Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un fragment se marque lu une seule fois, que la liste survit à une sauvegarde et qu'une ancienne sauvegarde sans le champ se charge — parce que l'ajout ne doit pas casser les profils existants")]
        public void Read_IsIdempotent_AndSurvivesSaveAndLoad()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Fragments.MarkRead("coney_trap");
            app.Fragments.MarkRead("coney_trap");
            Assert.That(app.Session.Meta.fragmentsRead, Is.EqualTo(new[] { "coney_trap" }));
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(app.Session.ToProfile(System.DateTime.UtcNow))));
            Assert.That(restored.Meta.fragmentsRead, Is.EqualTo(new[] { "coney_trap" }));
            Assert.That(JsonUtility.FromJson<MetaSave>("{\"gems\":5}").fragmentsRead, Is.Not.Null.And.Empty);
        }

        [Test, Description("Vérifie qu'un chapitre de la même ville qui a des zones mais vient après Brooklyn est « Verrouillé » — parce que le statut Locked n'était pas testé")]
        public void Chapter_AfterTheCurrentOne_IsLocked()
        {
            var content = TestContent.Build();
            var zones = content.Zones.Concat(PrototypeZones.Zones()).ToList();
            var z = zones.First(x => x.Borough == "brooklyn");
            zones.Add(new ZoneDef(new ZoneId("zone_ny_2_1"), z.NameKey, z.Entry, z.Nodes, z.Links, z.Objectives, z.Encounters, "2.1", 1, "queens"));
            var s = new GameSession(content.WithZones(zones));
            Assert.That(ChapterProgress.StatusOf(s, Chapters.Find("ny_brooklyn")!), Is.EqualTo(ChapterStatus.Current));
            Assert.That(ChapterProgress.StatusOf(s, Chapters.Find("ny_queens")!), Is.EqualTo(ChapterStatus.Locked));
        }
    }
}
