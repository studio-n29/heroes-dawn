#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Mécanismes du script narratif : variantes par parcours, choix mémorisé et définitif, épilogues, cohérence du registre avec les plateaux.</summary>
    public sealed class NarrativeMechanismsTests
    {
        static NarrationLineDef L(string key, string? route = null, string? replaces = null) => new NarrationLineDef("x", key, route, replaces);

        [Test, Description("Vérifie qu'une variante .d remplace sa réplique de base pour Agent D seulement, et que la base reste pour Agent M — parce que chaque parcours lit sa version")]
        public void ForRoute_VariantReplacesTheBase_ForItsRouteOnly()
        {
            var lines = new[] { L("a.0"), L("a.1"), L("a.1.d", "agent_d", "a.1"), L("a.1.m", "agent_m", "a.1"), L("a.2") };
            Assert.That(NarrationLineDef.ForRoute(lines, "agent_d").Select(l => l.TextKey), Is.EqualTo(new[] { "a.0", "a.1.d", "a.2" }));
            Assert.That(NarrationLineDef.ForRoute(lines, "agent_m").Select(l => l.TextKey), Is.EqualTo(new[] { "a.0", "a.1.m", "a.2" }));
        }

        [Test, Description("Vérifie qu'une réplique réservée à un parcours n'est jouée que pour lui, et qu'une base sans variante pour un parcours est conservée — parce que les indices de la cathédrale ne concernent qu'Agent M")]
        public void ForRoute_RouteOnlyLines_AndBaseWithoutVariant()
        {
            var lines = new[] { L("a.0"), L("a.1", "agent_m"), L("a.2.d", "agent_d", "a.2"), L("a.2") };
            Assert.That(NarrationLineDef.ForRoute(lines, "agent_d").Select(l => l.TextKey), Is.EqualTo(new[] { "a.0", "a.2.d" }));
            Assert.That(NarrationLineDef.ForRoute(lines, "agent_m").Select(l => l.TextKey), Is.EqualTo(new[] { "a.0", "a.1", "a.2" }));
            Assert.That(NarrationLineDef.ForRoute(lines, null).Select(l => l.TextKey), Is.EqualTo(new[] { "a.0", "a.2" }));
        }

        [Test, Description("Vérifie qu'un choix d'histoire est définitif : le premier choix est retenu, un second appel ne le change pas, et une option inconnue est refusée — parce que le script veut un seul choix par profil")]
        public void Choice_IsFinal_AndRejectsUnknownOption()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            Assert.That(app.Choices.Chosen(StoryChoices.HarlemPriority), Is.Null);
            Assert.That(app.Choices.Choose(StoryChoices.HarlemPriority, "nope"), Is.Null);
            Assert.That(app.Choices.Choose(StoryChoices.HarlemPriority, "clinic"), Is.EqualTo("clinic"));
            Assert.That(app.Choices.Choose(StoryChoices.HarlemPriority, "market"), Is.EqualTo("clinic"));
            Assert.That(app.Choices.Chosen(StoryChoices.HarlemPriority), Is.EqualTo("clinic"));
            Assert.That(app.Choices.Choose("inconnu", "clinic"), Is.Null);
        }

        [Test, Description("Vérifie que le choix survit à la sauvegarde et au chargement du profil, et qu'une ancienne sauvegarde sans le champ se charge avec une liste vide — parce que les profils existants doivent rester valides")]
        public void Choice_SurvivesSaveAndLoad_AndOldSaveLoads()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Choices.Choose(StoryChoices.PlaceRougeTrust, "listen");
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(app.Session.ToProfile(System.DateTime.UtcNow))));
            Assert.That(restored.Meta.storyChoices.Select(s => (s.id, s.option)), Is.EqualTo(new[] { (StoryChoices.PlaceRougeTrust, "listen") }));
            Assert.That(JsonUtility.FromJson<MetaSave>("{\"gems\":5}").storyChoices, Is.Not.Null.And.Empty);
        }

        [Test, Description("Vérifie que l'épilogue joue la scène de l'option retenue puis la scène commune, et seulement la commune si le choix n'a pas été fait — parce que le script ne fait varier que l'épilogue")]
        public void Epilogue_PlaysTheChosenScene_ThenTheCommonOne()
        {
            var ep = StoryChoices.EpilogueOf("ny_harlem")!;
            Assert.That(ep.For("clinic").Select(l => l.TextKey).First(), Is.EqualTo("narration.ny_3.epilogue.clinic.0"));
            Assert.That(ep.For("clinic").Select(l => l.TextKey).Last(), Is.EqualTo("narration.ny_3.epilogue.common.1"));
            Assert.That(ep.For(null).Select(l => l.TextKey), Is.EqualTo(ep.Common.Select(l => l.TextKey)));
        }

        [Test, Description("Vérifie que chaque choix cible une zone et une case qui existent, que sa réplique d'insertion figure dans le dialogue de cette case, qu'il a deux options et un épilogue — parce qu'un choix mal branché ne se déclencherait jamais")]
        public void Registry_IsConsistentWithTheBoards()
        {
            var content = PrototypeContent.Build();
            foreach (var c in StoryChoices.All)
            {
                var zone = content.Zones.Single(z => z.Id.Value == c.ZoneId);
                var node = zone.Nodes.Single(n => n.Id.Value == c.CaseId);
                var after = node.Narration?.After.Select(l => l.TextKey) ?? Enumerable.Empty<string>();
                if (c.AfterKey.Length > 0) Assert.That(after, Does.Contain(c.AfterKey), c.Id);
                else Assert.That(after, Is.Not.Empty, c.Id);
                Assert.That(c.Options.Count, Is.EqualTo(2), c.Id);
                var chapter = Chapters.OfZone(zone)!;
                var epilogue = StoryChoices.EpilogueOf(chapter.Id);
                Assert.That(epilogue, Is.Not.Null, c.Id);
                Assert.That(epilogue!.ChoiceId, Is.EqualTo(c.Id));
                Assert.That(c.Options.Select(o => o.Id), Is.EquivalentTo(epilogue.ByOption.Keys), c.Id);
            }
        }

        [Test, Description("Vérifie que le dialogue de phase d'un boss est transporté par l'événement de changement de phase sans changer la simulation — parce que seul l'affichage lit les répliques")]
        public void BossPhase_CarriesItsLines()
        {
            var phase = new BossPhaseDef(66, "enemies.x.phase2", 10, 0, 5, null, null, new[] { L("narration.x.boss.phase1.0") });
            Assert.That(phase.Lines.Select(l => l.TextKey), Is.EqualTo(new[] { "narration.x.boss.phase1.0" }));
            Assert.That(new BossPhaseDef(33, "k", 0, 0, 0).Lines, Is.Empty);
        }
    }
}
