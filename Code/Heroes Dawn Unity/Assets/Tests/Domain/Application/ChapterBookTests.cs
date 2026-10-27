using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Statuts des chapitres et chapitres déjà ouverts (lot G de la 0.14.0).</summary>
    public sealed class ChapterBookTests
    {
        /// <summary>Session avec les quatre zones de Brooklyn et le tutoriel.</summary>
        static GameSession FullBrooklyn()
        {
            var content = TestContent.Build();
            var zones = content.Zones.Concat(PrototypeZones.Zones()).ToList();
            return new GameSession(content.WithZones(zones));
        }

        static ChapterStatus Status(GameSession s, string id) => ChapterProgress.StatusOf(s, Chapters.Find(id)!);

        [Test, Description("Vérifie que les chapitres sans zone sont « Bientôt », que Brooklyn est « En cours » au départ et les suivants « Bientôt » — parce que seul Brooklyn est jouable aujourd'hui")]
        public void Status_AtTheStart()
        {
            var s = FullBrooklyn();
            Assert.That(Status(s, "ny_brooklyn"), Is.EqualTo(ChapterStatus.Current));
            foreach (var id in new[] { "ny_queens", "ny_harlem", "ny_manhattan", "mos_sergueiv", "mos_kapotnya", "mos_sadovoye", "mos_place_rouge" })
                Assert.That(Status(s, id), Is.EqualTo(ChapterStatus.Soon), id);
        }

        [Test, Description("Vérifie que Brooklyn reste « En cours » tant qu'une de ses zones n'est pas validée et devient « Terminé » quand toutes le sont — parce que le chapitre se termine avec ses zones")]
        public void Status_BrooklynPartiallyThenFullyDone()
        {
            var s = FullBrooklyn();
            var brooklyn = s.Content.Zones.Where(z => z.Borough == "brooklyn").Select(z => z.Id.Value).ToList();
            Assert.That(brooklyn.Count, Is.EqualTo(4));
            s.CompletedZones.Add(brooklyn[0]);
            Assert.That(Status(s, "ny_brooklyn"), Is.EqualTo(ChapterStatus.Current));
            foreach (var z in brooklyn) s.CompletedZones.Add(z);
            Assert.That(Status(s, "ny_brooklyn"), Is.EqualTo(ChapterStatus.Done));
        }

        [Test, Description("Vérifie que l'ordre d'affichage commence par les chapitres de la ville de l'acte en cours (New York pour Agent D) puis ceux des autres villes — parce que le joueur voit d'abord son chapitre")]
        public void DisplayOrder_StartsWithTheActCity()
        {
            var s = FullBrooklyn();
            var order = ChapterProgress.InDisplayOrder(s).Select(c => c.Id).ToList();
            Assert.That(order.Count, Is.EqualTo(8));
            Assert.That(order.Take(4), Is.EqualTo(new[] { "ny_brooklyn", "ny_queens", "ny_harlem", "ny_manhattan" }));
            Assert.That(order.Skip(4).All(id => id.StartsWith("mos_")), Is.True);
        }

        [Test, Description("Vérifie que le compte des chapitres terminés suit leur statut — parce que l'en-tête de la page Chapitres l'affiche")]
        public void DoneCount_FollowsTheStatus()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            Assert.That(app.Chapters.DoneCount(), Is.EqualTo(0));
            app.Session.CompletedZones.Add(PrototypeContent.Zone11.Value);
            Assert.That(app.Chapters.DoneCount(), Is.EqualTo(0), "une zone sur quatre ne termine pas le chapitre");
            foreach (var z in app.Session.Content.Zones.Where(z => z.Borough == "brooklyn")) app.Session.CompletedZones.Add(z.Id.Value);
            Assert.That(app.Chapters.DoneCount(), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un chapitre se marque comme vu une seule fois et que la liste survit à une sauvegarde — parce que l'ouverture ne doit s'afficher qu'au premier déploiement")]
        public void Seen_IsIdempotent_AndSurvivesSaveAndLoad()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            Assert.That(app.Chapters.Seen("ny_brooklyn"), Is.False);
            app.Chapters.MarkSeen("ny_brooklyn");
            app.Chapters.MarkSeen("ny_brooklyn");
            Assert.That(app.Session.Meta.chaptersSeen, Is.EqualTo(new[] { "ny_brooklyn" }));
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(app.Session.ToProfile(System.DateTime.UtcNow))));
            Assert.That(restored.Meta.chaptersSeen, Is.EqualTo(new[] { "ny_brooklyn" }));
        }

        [Test, Description("Vérifie qu'un profil sans le champ des chapitres vus (ancienne sauvegarde) se charge avec une liste vide — parce que les anciennes sauvegardes doivent rester valides")]
        public void OldSave_WithoutChaptersSeen_Loads()
        {
            var json = "{\"gems\":5}";
            var meta = JsonUtility.FromJson<MetaSave>(json);
            Assert.That(meta.chaptersSeen, Is.Not.Null.And.Empty);
        }
    }
}
