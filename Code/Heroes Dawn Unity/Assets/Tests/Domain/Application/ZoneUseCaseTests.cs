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

        [Test, Description("Vérifie qu'une zone inexistante est refusée — parce qu'un identifiant de contenu erroné ne doit pas créer de partie")]
        public void Start_UnknownZone_FailsWithUnknownZone()
        {
            var r = Z.Start.ExecuteAsync(new ZoneId("zone_inconnue")).GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.unknown"));
        }

        [Test, Description("Vérifie qu'on ne peut pas partir en mission avec une escouade épuisée — parce qu'un membre à 100 % de fatigue ne peut plus combattre (GDD §6)")]
        public void Start_SquadExhausted_Fails()
        {
            foreach (var m in _app.Session.Roster) m.Fatigue = 100;
            var r = Z.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.exhausted"));
            Assert.That(_app.Session.Run, Is.Null);
        }

        [Test, Description("Vérifie qu'on ne peut pas lancer le dé sans partie de zone — parce que l'interface doit afficher une erreur au lieu de planter")]
        public void Roll_WithoutRun_FailsWithNoRun()
        {
            var r = Z.Roll.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.no_run"));
        }

        [Test, Description("Vérifie qu'un deuxième lancer sans déplacement est refusé et ne change pas les PM — parce que le dé ne se relance qu'après le déplacement (GDD §5)")]
        public void Roll_TwiceInARow_SecondFailsWithNotAllowed()
        {
            Assert.That(Z.Start.ExecuteAsync(PrototypeContent.Zone11).GetAwaiter().GetResult().IsSuccess);
            Ok(Z.Roll.ExecuteAsync().GetAwaiter().GetResult());
            var pm = _app.Session.Run!.PmInHand;
            var r = Z.Roll.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(r.Error!.Code, Is.EqualTo("zone.not_allowed"));
            Assert.That(_app.Session.Run!.PmInHand, Is.EqualTo(pm));
        }

        [Test, Description("Vérifie qu'une case Ressource crédite le portefeuille et met à jour l'affichage — parce que le gain doit apparaître dès que le pion passe sur la case")]
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

        [Test, Description("Vérifie les trois choix d'événement : +250 Crédits, +10 PR ou −15 % de fatigue — parce que chaque choix narratif a une conséquence chiffrée")]
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

        [Test, Description("Vérifie que se reposer retire 30 % de fatigue à l'escouade et que passer ne change rien — parce que la case Repos réduit la fatigue en mission (GDD §5)")]
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

        [Test, Description("Vérifie qu'un mini-jeu réussi rapporte ses Crédits et PR et qu'un échec ne rapporte rien — parce que la case reste rejouable en cas d'échec (GDD §5)")]
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

        [Test, Description("Vérifie que valider la zone rapporte 1 500 Crédits + 40 PR la première fois puis le gain de rejeu — parce que la validation rapporte 1 500 Crédits et 40 PR la première fois (GDD §5)")]
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

        [Test, Description("Vérifie que quitter une zone, validée ou non, vide la partie sans recréditer de récompense — parce que les gains sont acquis à la validation, jamais au départ (Review Focus 5)")] // Review Focus 5
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
