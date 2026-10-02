using System.Linq;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class ZoneUseCaseTests
    {
        TestApp _app = null!;
        TestApp.ZoneUseCases Z => _app.Zone;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static T Ok<T>(Core.Result<T> r) { Assert.That(r.IsSuccess, r.Error?.Code); return r.Value; }

        [Test]
        public void Start_UnknownZone_FailsWithUnknownZone()
        {
            var r = Z.Start.ExecuteAsync(new ZoneId("zone_inconnue")).GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.unknown"));
        }

        [Test]
        public void Start_SquadExhausted_Fails()
        {
            foreach (var m in _app.Session.Roster) m.Fatigue = 100;
            var r = Z.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.exhausted"));
            Assert.That(_app.Session.Run, Is.Null);
        }

        [Test]
        public void Roll_WithoutRun_FailsWithNoRun()
        {
            var r = Z.Roll.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.no_run"));
        }

        [Test]
        public void Roll_TwiceInARow_SecondFailsWithNotAllowed()
        {
            Assert.That(Z.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult().IsSuccess);
            Ok(Z.Roll.ExecuteAsync().GetAwaiter().GetResult());
            var pm = _app.Session.Run!.PmInHand;
            var r = Z.Roll.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.not_allowed"));
            Assert.That(_app.Session.Run!.PmInHand, Is.EqualTo(pm));
        }

        [Test]
        public void ResourceCase_CreditsWallet_AndPublishesWalletChanged()
        {
            _app.Place("n00", pm: 1);
            int credits = _app.Session.Wallet.Credits, pr = _app.Session.Wallet.Pr;
            var events = Ok(Z.Move.ExecuteAsync(new CaseId("n01")).GetAwaiter().GetResult());
            var rc = events.OfType<ResourceCollectedEvent>().Single();
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + rc.Credits));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(pr + rc.Pr));
            Assert.That(_app.Received<WalletChanged>().Last().Credits, Is.EqualTo(_app.Session.Wallet.Credits));
        }

        [Test]
        public void Narrative_Credits_Adds250_Research_Adds10Pr_Morale_RemovesFatigue15()
        {
            int credits = _app.Session.Wallet.Credits;
            _app.Place("n04", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n05")).GetAwaiter().GetResult());
            Ok(Z.Narrative.ExecuteAsync(NarrativeChoice.Credits).GetAwaiter().GetResult());
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + ResolveNarrativeUseCase.Credits));

            int pr = _app.Session.Wallet.Pr;
            _app.Place("n04", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n05")).GetAwaiter().GetResult());
            Ok(Z.Narrative.ExecuteAsync(NarrativeChoice.Research).GetAwaiter().GetResult());
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(pr + ResolveNarrativeUseCase.Pr));

            foreach (var m in _app.Session.Squad) ((TeamMember)m).Fatigue = 40;
            _app.Place("n04", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n05")).GetAwaiter().GetResult());
            Ok(Z.Narrative.ExecuteAsync(NarrativeChoice.Morale).GetAwaiter().GetResult());
            Assert.That(_app.Session.Squad.All(m => m.Fatigue == 40 - ResolveNarrativeUseCase.Morale));
            Assert.That(ResolveNarrativeUseCase.Credits, Is.EqualTo(250));
            Assert.That(ResolveNarrativeUseCase.Pr, Is.EqualTo(10));
            Assert.That(ResolveNarrativeUseCase.Morale, Is.EqualTo(15));
        }

        [Test]
        public void Rest_Take_RemovesRestFatigueReduction_Skip_KeepsFatigue()
        {
            int reduction = _app.Session.Content.Balance.RestFatigueReduction;
            foreach (var m in _app.Session.Squad) ((TeamMember)m).Fatigue = 50;
            _app.Place("n09", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n10")).GetAwaiter().GetResult());
            Ok(Z.Rest.ExecuteAsync(false).GetAwaiter().GetResult());
            Assert.That(_app.Session.Squad.All(m => m.Fatigue == 50));

            _app.Place("n09", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n10")).GetAwaiter().GetResult());
            Ok(Z.Rest.ExecuteAsync(true).GetAwaiter().GetResult());
            Assert.That(_app.Session.Squad.All(m => m.Fatigue == 50 - reduction));
        }

        [Test]
        public void MiniGame_Success_AddsMiniGameCreditsAndPr_Failure_AddsNothing()
        {
            var b = _app.Session.Content.Balance;
            int credits = _app.Session.Wallet.Credits, pr = _app.Session.Wallet.Pr;
            _app.Place("n09", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n08")).GetAwaiter().GetResult());
            Ok(Z.MiniGame.ExecuteAsync(false).GetAwaiter().GetResult());
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits));

            _app.Place("n09", pm: 1);
            Ok(Z.Move.ExecuteAsync(new CaseId("n08")).GetAwaiter().GetResult());
            Ok(Z.MiniGame.ExecuteAsync(true).GetAwaiter().GetResult());
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + b.MiniGameCredits));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(pr + b.MiniGamePr));
        }

        [Test]
        public void ValidatingZone_FirstTime_Publishes1500Credits40Pr_SecondTime_ReplayCredits()
        {
            var b = _app.Session.Content.Balance;
            _app.DriveZone11ToLastStep();
            int credits = _app.Session.Wallet.Credits, pr = _app.Session.Wallet.Pr;
            Ok(Z.Move.ExecuteAsync(new CaseId("n24")).GetAwaiter().GetResult());
            var first = _app.Received<ZoneValidated>().Single().Rewards;
            Assert.That(first.FirstClear, Is.True);
            Assert.That(first.Credits, Is.EqualTo(b.ZoneValidationCredits));
            Assert.That(first.Pr, Is.EqualTo(b.ZoneValidationPr));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + b.ZoneValidationCredits));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(pr + b.ZoneValidationPr));

            _app.DriveZone11ToLastStep();
            Ok(Z.Move.ExecuteAsync(new CaseId("n24")).GetAwaiter().GetResult());
            var second = _app.Received<ZoneValidated>().Last().Rewards;
            Assert.That(second.FirstClear, Is.False);
            Assert.That(second.Credits, Is.EqualTo(MetaRules.ZoneReplayCredits));
            Assert.That(second.Pr, Is.EqualTo(0));
        }

        [Test] // Review Focus 5
        public void Leave_AfterValidation_And_Leave_Midway_ClearRun_NoDoubleReward()
        {
            _app.DriveZone11ToLastStep();
            Ok(Z.Move.ExecuteAsync(new CaseId("n24")).GetAwaiter().GetResult());
            int credits = _app.Session.Wallet.Credits, pr = _app.Session.Wallet.Pr;
            Assert.That(Z.Leave.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Run, Is.Null);
            Assert.That(_app.Session.RunZone, Is.Null);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(pr));

            Assert.That(Z.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult().IsSuccess);
            Assert.That(Z.Leave.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Run, Is.Null);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits));
            Assert.That(_app.Received<ZoneValidated>().Count, Is.EqualTo(1));
            Assert.That(Z.Leave.ExecuteAsync().GetAwaiter().GetResult().IsSuccess, "sans partie : sans effet");
        }
    }
}
