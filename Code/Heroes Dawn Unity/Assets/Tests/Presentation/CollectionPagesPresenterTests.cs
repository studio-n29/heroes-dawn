using System;
using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Pages Équipement et Grimoire, objets et boosters de la Boutique (plan 2026-10-10, sous-projets L et M).</summary>
    public sealed class CollectionPagesPresenterTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        FakeModalHost _host = null!;
        HubNavigator _nav = null!;
        readonly LocLocalization _loc = new LocLocalization();
        static MemberId Verre => new MemberId("verre");

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(Verre), 5));
            _view = new FakeListView();
            _toasts = new FakeToasts();
            _host = new FakeModalHost();
            _nav = new HubNavigator();
        }

        T Start<T>(T p) where T : IDisposable
        {
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        LoadoutPagePresenter Loadout() => Start(new LoadoutPagePresenter(_view, new HubLocation(HubPage.Loadout, Verre), _app.Collection, _app.Session, _nav, _loc, _toasts,
                                                                         _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));

        GrimoirePagePresenter Grimoire() => Start(new GrimoirePagePresenter(_view, _app.Collection, _app.Session, new ModalService(_host), _loc, _toasts,
                                                                           _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));

        int RowOf(string title) => Array.FindIndex(_view.Rows, r => r.Title != null && r.Title.Contains(title));

        [Test, Description("Vérifie que la page Équipement liste les objets de la réserve avec « ÉQUIPER » et qu'un toucher équipe l'objet puis le fait apparaître dans les objets équipés — parce que c'est le geste central de l'équipement")]
        public void Loadout_EquipsAnItemFromTheStock()
        {
            CollectionOps.AddItem(_app.Session, new ItemId("kevlar_vest"));
            var p = Loadout();
            int row = RowOf("Gilet kevlar");
            Assert.That(_view.Rows[row].A.Label, Is.EqualTo("ÉQUIPER"));
            Assert.That(_view.Rows[row].A.Enabled, Is.True);
            _view.ClickRow(row);
            Assert.That(_app.Session.Find(Verre)!.Items, Is.EqualTo(new[] { new ItemId("kevlar_vest") }));
            var after = RowOf("Gilet kevlar");
            Assert.That(_view.Rows[after].A.Label, Is.EqualTo("RETIRER"), "désormais dans les objets équipés");
            Assert.That(_view.Header.Line, Does.Contain("PV +10 %"));
            p.Dispose();
        }

        [Test, Description("Vérifie que les boutons « ÉQUIPER » sont grisés avec la raison quand l'agent a déjà deux objets, et que « RETIRER » rend l'objet à la réserve — parce qu'un bouton grisé doit dire pourquoi")]
        public void Loadout_FullSlots_DisableEquip_AndRemoveGivesItBack()
        {
            foreach (var id in new[] { "kevlar_vest", "running_shoes", "riot_shield" }) CollectionOps.AddItem(_app.Session, new ItemId(id));
            _app.Collection.EquipItem(Verre, new ItemId("kevlar_vest"));
            _app.Collection.EquipItem(Verre, new ItemId("running_shoes"));
            var p = Loadout();
            int stock = RowOf("Bouclier anti-émeute");
            Assert.That(_view.Rows[stock].A.Enabled, Is.False);
            Assert.That(_view.Rows[stock].A.Reason, Does.Contain("pleins"));
            int equipped = RowOf("Gilet kevlar");
            _view.ClickRow(equipped);
            Assert.That(_app.Session.Find(Verre)!.Items.Count, Is.EqualTo(1));
            Assert.That(_view.Rows[RowOf("Bouclier anti-émeute")].A.Enabled, Is.True, "un emplacement est libre");
            p.Dispose();
        }

        [Test, Description("Vérifie que seules les cartes de la catégorie de l'agent sont proposées, que « → 4 » est grisé tant que la carte 3 est vide et que « → 3 » équipe — parce qu'une carte ne va qu'à sa catégorie")]
        public void Loadout_OffersOnlyCompatibleCards_AndSlotFourNeedsSlotThree()
        {
            foreach (var id in new[] { "card_glass_wall", "card_shackle", "card_ember" }) CollectionOps.AddCard(_app.Session, new CardId(id));
            var p = Loadout();
            Assert.That(RowOf("Braise"), Is.EqualTo(-1), "carte Élémentaire : pas pour un agent Matière");
            int wall = RowOf("Mur de verre");
            Assert.That(_view.Rows[wall].A.Enabled, Is.True);
            Assert.That(_view.Rows[wall].B.Enabled, Is.False);
            _view.ClickRow(wall, 0);
            Assert.That(_app.Session.Find(Verre)!.Cards[0], Is.EqualTo(new CardId("card_glass_wall")));
            int shackle = RowOf("Entrave");
            Assert.That(_view.Rows[shackle].B.Enabled, Is.True);
            _view.ClickRow(shackle, 1);
            Assert.That(_app.Session.Find(Verre)!.Cards[1], Is.EqualTo(new CardId("card_shackle")));
            p.Dispose();
        }

        [Test, Description("Vérifie que l'en-tête du Grimoire grise « Ouvrir » sans booster, que l'achat en PR donne un booster et que son ouverture montre les cartes dans une fenêtre — parce que c'est la boucle de collection")]
        public void Grimoire_BuyThenOpenABooster()
        {
            _host.Register<IBoosterOpeningPopupView>(() => new FakeBoosterOpeningPopup());
            _app.Session.Wallet.Set(0, 500);
            var p = Grimoire();
            Assert.That(_view.Header.A.Enabled, Is.False);
            Assert.That(_view.Rows.Length, Is.EqualTo(_app.Session.Content.Collection.Cards.Count));
            _view.ClickHeader(1);
            Assert.That(CollectionOps.Boosters(_app.Session, BoosterDef.StandardId), Is.EqualTo(1));
            Assert.That(_view.Header.A.Enabled, Is.True);
            _view.ClickHeader(0);
            var popup = (FakeBoosterOpeningPopup)_host.Spawned.Single();
            Assert.That(popup.Shown!.Value.Cards.Length, Is.EqualTo(3), "trois cartes dans le booster");
            popup.Continue();
            Assert.That(_view.Rows.Count(r => !r.Dim), Is.GreaterThan(0), "au moins une carte possédée");
            Assert.That(_view.Header.Line, Does.Contain("0 booster(s)"));
            p.Dispose();
        }

        [Test, Description("Vérifie que la Boutique propose les objets du jour et un booster : l'achat d'un objet débite les Crédits et le bouton passe à « VENDU » — parce que la boutique d'objets tourne chaque jour")]
        public void Boutique_SellsDailyItems_AndABooster()
        {
            _app.Session.Wallet.Set(50000, 500);
            _nav.ShopTab = ShopTab.Items;
            var p = Start(new BoutiquePagePresenter(_view, _app.Boutique, _app.Session, new ModalService(_host), _loc, _toasts,
                                                    _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _app.Collection, _nav));
            Assert.That(_view.Rows.Length, Is.EqualTo(CollectionBook.ItemOfferCount), "4 objets du jour");
            var price = _app.Collection.ItemOffers()[0].Item.Price;
            int credits = _app.Session.Wallet.Credits;
            _view.ClickRow(0);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits - price));
            Assert.That(_view.Rows[0].A.Label, Is.EqualTo("VENDU"));
            _view.ClickShopTab((int)ShopTab.Boosters);
            _view.ClickRow(1);
            Assert.That(CollectionOps.Boosters(_app.Session, BoosterDef.StandardId), Is.EqualTo(1));
            _view.ClickHeader(1);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Grimoire));
            p.Dispose();
        }
    }
}
