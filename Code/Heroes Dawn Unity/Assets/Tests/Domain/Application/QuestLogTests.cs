#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Suivi des quêtes du QG : quêtes principales (zones) et secondaires (compteurs à réclamer), version 0.9.0.</summary>
    public sealed class QuestLogTests
    {
        TestApp _app = null!;
        QuestLog Q => _app.Quests;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [Test, Description("Vérifie que les quêtes principales sont les quatre zones de Brooklyn dans l'ordre, la première en cours et les suivantes verrouillées — parce que la campagne se joue dans l'ordre")]
        public void Main_AreTheFourZones_InOrder()
        {
            var main = Q.Main();
            Assert.That(main.Select(m => m.Zone.Id.Value), Is.EqualTo(new[] { "zone_ny_1_1", "zone_ny_1_2", "zone_ny_1_3", "zone_ny_1_4" }));
            Assert.That(main.Select(m => m.Status), Is.EqualTo(new[] { ZoneStatus.Available, ZoneStatus.Locked, ZoneStatus.Locked, ZoneStatus.Locked }));
            Assert.That(Q.CurrentMain()!.Value.Zone.Id.Value, Is.EqualTo("zone_ny_1_1"));
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Assert.That(Q.Main()[0].Status, Is.EqualTo(ZoneStatus.Done));
            Assert.That(Q.CurrentMain()!.Value.Zone.Id.Value, Is.EqualTo("zone_ny_1_2"));
        }

        [Test, Description("Vérifie que seules les quêtes sans condition sont visibles au départ et que les autres se débloquent avec les zones validées — parce que les quêtes secondaires suivent l'histoire")]
        public void Side_UnlockWithTheZones()
        {
            Assert.That(Q.Side().Select(v => v.Quest.Id), Is.EqualTo(new[] { "docks" }));
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Assert.That(Q.Side().Select(v => v.Quest.Id), Is.EquivalentTo(new[] { "docks", "reinforcements", "drills" }));
        }

        [Test, Description("Vérifie que gagner des combats fait avancer la quête qui les compte jusqu'à son objectif, sans le dépasser — parce que la progression est plafonnée")]
        public void Record_AdvancesAndCaps()
        {
            Q.Record(QuestMetric.CombatsWon, 2);
            Assert.That(Q.Side().Single().Progress, Is.EqualTo(2));
            Q.Record(QuestMetric.CombatsWon, 5);
            Assert.That(Q.Side().Single().Progress, Is.EqualTo(3));
            Assert.That(Q.Side().Single().Claimable, Is.True);
            Assert.That(Q.ClaimableCount(), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que réclamer une quête terminée verse la récompense une seule fois, et qu'une quête non terminée ne se réclame pas — parce qu'une récompense ne se touche qu'une fois méritée")]
        public void Claim_GrantsOnce()
        {
            Assert.That(Q.Claim("docks").IsFailure, Is.True, "pas terminée");
            Q.Record(QuestMetric.CombatsWon, 3);
            int before = _app.Session.Wallet.Credits;
            var result = Q.Claim("docks");
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(before + result.Value.Credits));
            Assert.That(Q.Claim("docks").IsFailure, Is.True, "déjà réclamée");
            Assert.That(Q.ClaimableCount(), Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'une quête encore verrouillée ne progresse pas et ne se réclame pas — parce que son histoire n'a pas commencé")]
        public void Locked_DoesNotProgress()
        {
            Q.Record(QuestMetric.Recruits, 1);
            Assert.That(Q.Claim("reinforcements").IsFailure, Is.True);
            _app.Session.CompletedZones.Add("zone_ny_1_1");
            Assert.That(Q.Side().Single(v => v.Quest.Id == "reinforcements").Progress, Is.EqualTo(0));
        }
    }
}
