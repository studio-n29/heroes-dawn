using System;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class RecruitPagePresenterTests
    {
        static readonly DateTime Noon = new DateTime(2026, 10, 3, 12, 0, 0);

        TestApp _app = null!;
        FakeRecruitPageView _view = null!;
        FakeModalHost _host = null!;
        ModalService _modals = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        RecruitPagePresenter _p = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _app.Time.LocalNow = Noon;
            _view = new FakeRecruitPageView();
            _host = new FakeModalHost();
            _host.Register<IRecruitConfirmPopupView>(() => new FakeRecruitConfirmPopup());
            _modals = new ModalService(_host);
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
            _nav.SelectTab(HubPage.Recruit);
        }

        RecruitPagePresenter Make()
        {
            var flow = new RecruitFlow(_modals, _app.Shop.Recruit, _app.Session, _toasts, _loc);
            _p = new RecruitPagePresenter(_view, _app.Session, _app.Shop.Open, _app.Shop.Reroll, _app.Shop.MarkSeen, flow, _nav, _app.Time,
                                          _modals, _toasts, _loc, _app.Subscriber<ShopChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        void Own(string id) => _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(new MemberId(id)), MetaRules.RecruitLevel));

        [Test, Description("Vérifie que la page montre 3 cartes avec leur rareté et que seule la Légende a le halo — parce que la Légende est mise en avant (M30)")]
        public void ShowsThreeCardsWithRarityAndHalo_LegendOnly()
        {
            Own("arc");
            _app.Session.Roster.RemoveAll(m => m.Id.Value == "givre");
            Make();
            var cards = _view.Last.Cards;
            Assert.That(cards.Length, Is.EqualTo(3));
            Assert.That(cards.Select(c => c.Card.Id.Value), Is.EquivalentTo(new[] { "rempart", "etincelle", "givre" }));
            Assert.That(cards.Where(c => c.Halo).Select(c => c.Card.Id.Value), Is.EqualTo(new[] { "givre" }));
            Assert.That(cards.Single(c => c.Card.Id.Value == "givre").Rarity, Is.EqualTo(Rarity.Legend));
            Assert.That(cards.Single(c => c.Card.Id.Value == "rempart").PriceLabel, Is.EqualTo("RECRUTER · 450 PR"));
            Assert.That(_view.Last.Countdown, Is.EqualTo("Nouveaux héros dans 21 h 00"));
            Assert.That(_view.Last.Progress, Is.EqualTo(_loc.F("recruit.progress", 4, 8)));
        }

        [Test, Description("Vérifie qu'une carte recrutée affiche « Dans l'équipe » — parce que l'emplacement reste visible jusqu'au prochain tirage (M30)")]
        public void InTeamCard_ShowsDansLEquipe()
        {
            _app.Session.Wallet.Set(0, 5000);
            Make();
            _view.ClickRecruit(0);
            ((FakeRecruitConfirmPopup)_host.Spawned.Last()).Answer(true);
            var card = _view.Last.Cards[0];
            Assert.That(card.InTeam, Is.True);
            Assert.That(card.InTeamText, Is.EqualTo(_loc.T("recruit.in_team")));
            Assert.That(_view.Last.Cards.Skip(1).All(c => !c.InTeam), Is.True);
        }

        [Test, Description("Vérifie que l'état « payable » des cartes suit le solde de PR — parce que le prix doit dire si le joueur peut payer")]
        public void Affordability_FollowsWallet()
        {
            _app.Session.Wallet.Set(0, 5000);
            Make();
            Assert.That(_view.Last.Cards.All(c => c.Affordable), Is.True);
            _app.Session.Wallet.Set(0, 100);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(0, 100));
            Assert.That(_view.Last.Cards.All(c => !c.Affordable), Is.True);
        }

        [Test, Description("Vérifie qu'ouvrir la page marque la boutique du jour comme vue — parce que le badge « N » disparaît une fois la boutique consultée (spec §5)")]
        public void OpenPage_MarksShopSeen_ClearsBadge()
        {
            Make();
            Assert.That(_app.Session.Shop.Day, Is.Not.Empty);
            Assert.That(_app.Session.Shop.SeenDay, Is.EqualTo(_app.Session.Shop.Day));
        }

        [Test, Description("Vérifie que le renouvellement gratuit se fait sans confirmation — parce que seule une dépense demande confirmation")]
        public void Reroll_Free_NoConfirm()
        {
            Make();
            Assert.That(_view.Last.RerollLabel, Is.EqualTo(_loc.T("shop.reroll.free")));
            _view.ClickReroll();
            Assert.That(_host.Spawned, Is.Empty);
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un renouvellement payant demande confirmation et ne débite qu'après acceptation — parce que toute dépense passe par une confirmation")]
        public void Reroll_Paid_AsksConfirmationThenCharges()
        {
            _app.Session.Wallet.Set(1000, _app.Session.Wallet.Pr);
            Make();
            _view.ClickReroll();
            Assert.That(_view.Last.RerollLabel, Is.EqualTo(_loc.F("shop.reroll.cost", 100)));
            _view.ClickReroll();
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000));
            ((FakeConfirmPopup)_host.Spawned.Single()).Answer(true);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(900));
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que la boutique se renouvelle d'elle-même à 9h00 si la page est affichée, et reste marquée vue — parce que le joueur peut rester sur la page à ce moment")]
        public void Tick_DayChangesWhileShown_RefreshesOffers()
        {
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 8, 59, 0);
            Make();
            Assert.That(_app.Session.Shop.Day, Is.EqualTo("2026-10-02"));
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 9, 0, 0);
            _p.Tick();
            Assert.That(_app.Session.Shop.Day, Is.EqualTo("2026-10-03"));
            Assert.That(_app.Session.Shop.SeenDay, Is.EqualTo("2026-10-03"));
            Assert.That(_view.Last.Countdown, Is.EqualTo("Nouveaux héros dans 24 h 00"));
        }

        [Test, Description("Vérifie que si 9h00 passe pendant la confirmation d'un renouvellement payant, il est refusé sans débit — parce que le joueur ne doit pas payer pour une boutique expirée")]
        public void DayChangeDuringPaidRerollConfirm_RefusedNoCharge()
        {
            _app.Session.Wallet.Set(1000, _app.Session.Wallet.Pr);
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 8, 58, 0);
            Make();
            _view.ClickReroll();
            _view.ClickReroll();
            var confirm = (FakeConfirmPopup)_host.Spawned.Single();
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 9, 0, 0);
            _p.Tick();
            confirm.Answer(true);
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(0));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000));
            Assert.That(_toasts.Shown.Last().Text, Is.EqualTo(_loc.T("errors.shop.expired")));
        }

        [Test, Description("Vérifie qu'une boutique sans héros éligible affiche « collection complète » et désactive Renouveler — parce qu'il n'y a plus rien à tirer")]
        public void EmptyShop_ShowsCollectionComplete_RerollDisabled()
        {
            foreach (var id in new[] { "arc", "rempart", "etincelle" }) Own(id);
            Make();
            Assert.That(_view.Last.Cards, Is.Empty);
            Assert.That(_view.Last.EmptyText, Is.EqualTo(_loc.T("shop.empty")));
            Assert.That(_view.Last.RerollEnabled, Is.False);
        }

        [Test, Description("Vérifie que toucher une carte ouvre la fiche du héros en mode boutique — parce que la fiche détaille le héros avant l'achat (spec §4)")]
        public void CardTap_OpensSheetInShopMode()
        {
            Make();
            _view.TapCard(1);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.MemberSheet));
            Assert.That(_nav.Current.FromShop, Is.True);
            Assert.That(_nav.Current.Member, Is.EqualTo(_app.Session.Shop.Offers[1].Id));
        }

        [Test, Description("Vérifie que le titre de la page est celui de la boutique du jour — parce que la barre haute porte le titre")]
        public void Title_IsRecruitPage()
        {
            Make();
            Assert.That(_p.Title, Is.EqualTo(_loc.T("page.recruit")));
        }
    }
}
