using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class HubUseCaseTests
    {
        TestApp _app = null!;
        TestApp.HubUseCases H => _app.Hub;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie qu'on ne peut pas changer l'escouade pendant une partie de zone — parce que l'équipe engagée sur le plateau est figée jusqu'au retour au QG")]
        public void ToggleSquad_DuringRun_FailsInMission()
        {
            _app.Zone.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            var r = H.ToggleSquad.ExecuteAsync(new MemberId("ancre")).GetAwaiter().GetResult();
            Assert.That(Code(r), Is.EqualTo("hub.in_mission"));
        }

        [Test, Description("Vérifie qu'on ne peut ni vider l'escouade ni dépasser 4 membres — parce que l'équipe de combat compte de 1 à 4 méta-humains (GDD §6)")]
        public void ToggleSquad_LastMember_FailsSquadEmpty_FifthMember_FailsSquadFull()
        {
            var squad = _app.Session.Squad.Select(m => m.Id).ToList();
            for (int i = 0; i < squad.Count - 1; i++)
                Assert.That(H.ToggleSquad.ExecuteAsync(squad[i]).GetAwaiter().GetResult().IsSuccess);
            Assert.That(Code(H.ToggleSquad.ExecuteAsync(squad.Last()).GetAwaiter().GetResult()), Is.EqualTo("hub.squad_empty"));

            for (int i = 0; i < squad.Count - 1; i++) H.ToggleSquad.ExecuteAsync(squad[i]).GetAwaiter().GetResult();
            _app.Session.Wallet.Set(0, 1000);
            Assert.That(H.Recruit.ExecuteAsync(new MemberId("etincelle")).GetAwaiter().GetResult().IsSuccess);
            Assert.That(Code(H.ToggleSquad.ExecuteAsync(new MemberId("etincelle")).GetAwaiter().GetResult()), Is.EqualTo("hub.squad_full"));
            Assert.That(_app.Received<RosterChanged>().Count, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie que l'entraînement coûte 100 × niveau en Crédits, fait monter d'un niveau et s'arrête au niveau 50 — parce que le niveau max est 50 pour tous (GDD §7)")]
        public void Train_CostsTrainCostOfLevel_AndLevelsUp_MaxLevel_Fails()
        {
            var ancre = _app.Session.Find(new MemberId("ancre"))!;
            int level = ancre.Level;
            _app.Session.Wallet.Set(MetaRules.TrainCost(level), 0);
            Assert.That(H.Train.ExecuteAsync(ancre.Id).GetAwaiter().GetResult().IsSuccess);
            Assert.That(ancre.Level, Is.EqualTo(level + 1));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(0));
            Assert.That(_app.Received<WalletChanged>().Last().Credits, Is.EqualTo(0));
            Assert.That(Code(H.Train.ExecuteAsync(ancre.Id).GetAwaiter().GetResult()), Is.EqualTo("hub.credits"));

            ancre.Level = MetaRules.MaxLevel;
            _app.Session.Wallet.Set(1_000_000, 0);
            Assert.That(Code(H.Train.ExecuteAsync(ancre.Id).GetAwaiter().GetResult()), Is.EqualTo("hub.max_level"));
        }

        [Test, Description("Vérifie que recruter coûte le prix de la rareté en PR, place la recrue dans l'escouade s'il y a de la place et refuse doublon, PR insuffisants ou membre inconnu — parce que le recrutement est un choix direct à coût fixe (GDD §7)")]
        public void Recruit_CostsRecruitCostByRarity_JoinsSquadIfRoom_AlreadyOwned_Fails()
        {
            var def = _app.Session.Content.Member(new MemberId("etincelle"));
            int cost = MetaRules.RecruitCost(def.Rarity);
            _app.Session.Wallet.Set(0, cost);
            var first = _app.Session.Squad.First().Id;
            H.ToggleSquad.ExecuteAsync(first).GetAwaiter().GetResult();     // une place libre
            Assert.That(H.Recruit.ExecuteAsync(def.Id).GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(0));
            Assert.That(_app.Session.Find(def.Id)!.InSquad, Is.True);
            Assert.That(_app.Session.Find(def.Id)!.Level, Is.EqualTo(MetaRules.RecruitLevel));
            Assert.That(Code(H.Recruit.ExecuteAsync(def.Id).GetAwaiter().GetResult()), Is.EqualTo("hub.owned"));
            Assert.That(Code(H.Recruit.ExecuteAsync(new MemberId("arc")).GetAwaiter().GetResult()), Is.EqualTo("hub.pr"));
            Assert.That(Code(H.Recruit.ExecuteAsync(new MemberId("inconnu")).GetAwaiter().GetResult()), Is.EqualTo("hub.unknown"));
        }

        [Test, Description("Vérifie que l'infirmerie refuse sans fatigue, puis coûte 150 Crédits et retire 30 % de fatigue à tout l'effectif — parce que ce sont les règles du QG du prototype")]
        public void Heal_NoFatigue_FailsNothingToHeal_Else_CostsInfirmaryCost_ReducesAll()
        {
            Assert.That(Code(H.Heal.ExecuteAsync().GetAwaiter().GetResult()), Is.EqualTo("hub.no_fatigue"));
            foreach (var m in _app.Session.Roster) m.Fatigue = 50;
            _app.Session.Wallet.Set(MetaRules.InfirmaryCost, 0);
            Assert.That(H.Heal.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Roster.All(m => m.Fatigue == 50 - MetaRules.InfirmaryReduction));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(0));
            Assert.That(Code(H.Heal.ExecuteAsync().GetAwaiter().GetResult()), Is.EqualTo("hub.credits"));
        }

        [Test, Description("Vérifie que la liste de recrutement exclut les membres déjà possédés — parce qu'il n'y a pas de doublons au recrutement (GDD §7)")]
        public void AvailableRecruits_ExcludesOwned()
        {
            var owned = _app.Session.Roster.Select(m => m.Id).ToList();
            var available = ((ISessionReadModel)_app.Session).AvailableRecruits();
            Assert.That(available.Any(d => owned.Contains(d.Id)), Is.False);
            Assert.That(available.Count + owned.Count, Is.EqualTo(_app.Session.Content.Members.Count));
        }
    }
}
