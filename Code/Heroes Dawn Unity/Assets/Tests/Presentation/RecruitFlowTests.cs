using System;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class RecruitBalanceTests
    {
        [Test, Description("Vérifie le calcul du solde après achat quand le joueur peut payer — parce que la confirmation M31 montre avant, coût, après")]
        public void Affordable_ComputesAfter()
        {
            var b = RecruitBalance.Of(500, 450);
            Assert.That((b.Before, b.Cost, b.After, b.Missing, b.CanAfford), Is.EqualTo((500, 450, 50, 0, true)));
        }

        [Test, Description("Vérifie le calcul du manque quand le joueur ne peut pas payer — parce que la confirmation M31 affiche le manque en rouge")]
        public void NotAffordable_ComputesMissing()
        {
            var b = RecruitBalance.Of(160, 450);
            Assert.That((b.After, b.Missing, b.CanAfford), Is.EqualTo((0, 290, false)));
        }
    }

    public sealed class RecruitFlowTests
    {
        static readonly DateTime Noon = new DateTime(2026, 10, 3, 12, 0, 0);

        TestApp _app = null!;
        FakeModalHost _host = null!;
        ModalService _modals = null!;
        FakeToasts _toasts = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _app.Time.LocalNow = Noon;
            _app.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
            _host = new FakeModalHost();
            _host.Register<IRecruitConfirmPopupView>(() => new FakeRecruitConfirmPopup());
            _modals = new ModalService(_host);
            _toasts = new FakeToasts();
        }

        RecruitFlow Make(Application.Features.Shop.IRecruitFromShopUseCase recruit = null)
            => new RecruitFlow(_modals, recruit ?? _app.Shop.Recruit, _app.Session, _toasts, _loc);

        FakeRecruitConfirmPopup Popup => (FakeRecruitConfirmPopup)_host.Spawned.Last();

        Definitions.MemberDef Offer(int slot) => _app.Session.Content.Member(_app.Session.Shop.Offers[slot].Id);

        [Test, Description("Vérifie que confirmer recrute le héros, débite les PR et annonce l'arrivée — parce que l'achat passe par la confirmation M31")]
        public void Confirm_Recruits_AndToasts()
        {
            _app.Session.Wallet.Set(0, 5000);
            var def = Offer(0);
            int cost = MetaRules.RecruitCost(def.Rarity);
            var task = Make().RunAsync(0, default);
            var asked = Popup.Asked!.Value;
            Assert.That(asked.Before, Is.EqualTo("5000 PR"));
            Assert.That(asked.Cost, Is.EqualTo("-" + cost + " PR"));
            Assert.That(asked.After, Is.EqualTo((5000 - cost) + " PR"));
            Assert.That(asked.CanAfford, Is.True);
            Popup.Answer(true);
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Recruited));
            Assert.That(_app.Session.Owns(def.Id), Is.True);
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(5000 - cost));
            Assert.That(_toasts.Shown.Last(), Is.EqualTo((_loc.F("hub.recruited", _loc.MemberName(def)), ToastKind.Reward)));
            Assert.That(_host.Despawned, Has.Count.EqualTo(1));
        }

        [Test, Description("Vérifie qu'annuler ne débite rien — parce qu'une confirmation refusée n'a aucune conséquence")]
        public void Cancel_NoCharge()
        {
            _app.Session.Wallet.Set(0, 5000);
            var task = Make().RunAsync(0, default);
            Popup.Answer(false);
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Cancelled));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(5000));
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(4));
        }

        [Test, Description("Vérifie que Retour ferme la confirmation sans achat — parce que Retour sur une modale l'annule (Review Focus 4)")]
        public void Back_CancelsConfirmation()
        {
            _app.Session.Wallet.Set(0, 5000);
            var task = Make().RunAsync(0, default);
            Assert.That(_modals.TryCancelTop(), Is.True);
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Cancelled));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(5000));
            Assert.That(_modals.AnyOpen, Is.False);
        }

        [Test, Description("Vérifie que sans assez de PR la confirmation montre le manque, la piste pour en gagner et un bouton désactivé, sans appel d'achat — parce que le joueur doit savoir pourquoi et comment (M31)")]
        public void NotEnoughPr_ConfirmShowsMissingAndDisablesOk_NoUseCaseCall()
        {
            _app.Session.Wallet.Set(0, 100);
            var pending = new PendingRecruit();
            var task = Make(pending).RunAsync(0, default);
            var asked = Popup.Asked!.Value;
            int cost = MetaRules.RecruitCost(Offer(0).Rarity);
            Assert.That(asked.CanAfford, Is.False);
            Assert.That(asked.Missing, Is.EqualTo(_loc.F("recruit.missing", cost - 100)));
            Assert.That(asked.HowToEarn, Is.EqualTo(_loc.T("recruit.how_to_earn")));
            Popup.Answer(true);
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Cancelled));
            Assert.That(pending.Calls, Is.Zero);
        }

        [Test, Description("Vérifie que si 9h00 passe pendant la confirmation, l'achat est refusé avec un toast, sans débit, et la boutique du jour est tirée — parce que l'offre acceptée n'existe plus (Review Focus 2)")]
        public void Expired_ToastsRefreshesNoCharge()
        {
            _app.Session.Wallet.Set(0, 5000);
            var task = Make().RunAsync(0, default);
            _app.Time.LocalNow = new DateTime(2026, 10, 4, 9, 0, 0);
            Popup.Answer(true);
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Expired));
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(5000));
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(4));
            Assert.That(_app.Session.Shop.Day, Is.EqualTo("2026-10-04"));
            Assert.That(_toasts.Shown.Last(), Is.EqualTo((_loc.T("errors.shop.expired"), ToastKind.Error)));
        }

        [Test, Description("Vérifie qu'un achat interrompu (page quittée) n'affiche plus rien — parce que la page peut être quittée à tout moment (Review Focus 3)")]
        public void Dispose_DuringRecruit_NoViewOrToastAfterwards()
        {
            _app.Session.Wallet.Set(0, 5000);
            var pending = new PendingRecruit();
            var cts = new System.Threading.CancellationTokenSource();
            var task = Make(pending).RunAsync(0, cts.Token);
            Popup.Answer(true);
            Assert.That(pending.Calls, Is.EqualTo(1));
            cts.Cancel();
            pending.Done.TrySetResult(Result.Ok());
            Assert.That(task.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Cancelled));
            Assert.That(_toasts.Shown, Is.Empty);
        }

        [Test, Description("Vérifie qu'un second achat demandé pendant le premier est ignoré — parce qu'un double toucher ne doit pas ouvrir deux confirmations")]
        public void SecondRun_WhileBusy_Ignored()
        {
            _app.Session.Wallet.Set(0, 5000);
            var flow = Make();
            flow.RunAsync(0, default);
            var second = flow.RunAsync(1, default);
            Assert.That(second.GetAwaiter().GetResult(), Is.EqualTo(RecruitOutcome.Cancelled));
            Assert.That(_host.Spawned, Has.Count.EqualTo(1));
        }
    }
}
