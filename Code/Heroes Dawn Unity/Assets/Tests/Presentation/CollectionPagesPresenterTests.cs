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
        InventoryPagePresenter Inventory() => Start(new InventoryPagePresenter(_view, _app.Session, _loc, _toasts,
                                                                              _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time));

        // ------------------------------------------------------------------ Catégories (lot B 0.14.0)

        [Test, Description("Vérifie que chaque carte du grimoire affiche sa nature (Mur de verre : défensive) — parce que la nature aide à construire un deck équilibré")]
        public void Grimoire_ShowsTheNatureOfEachCard()
        {
            var p = Grimoire();
            Assert.That(_view.Rows[RowOf("Mur de verre")].Subtitle, Does.Contain(_loc.T("card.nature.defensive")));
            Assert.That(_view.Rows[RowOf("Braise")].Subtitle, Does.Contain(_loc.T("card.nature.offensive")));
            p.Dispose();
        }

        [Test, Description("Vérifie que chaque objet de l'équipement affiche son type (gilet kevlar : armure) — parce que le type décide de ce qu'un agent peut porter")]
        public void Inventory_ShowsTheTypeOfEachItem()
        {
            CollectionOps.AddItem(_app.Session, new ItemId("kevlar_vest"));
            var p = Inventory();
            Assert.That(_view.Rows[RowOf("Gilet kevlar")].Subtitle, Does.Contain(_loc.T("item.type.armor")));
            p.Dispose();
        }

        [Test, Description("Vérifie que la page Équipement d'un agent affiche le type de chaque objet, porté ou en réserve — parce que c'est là que le joueur choisit quoi équiper")]
        public void Loadout_ShowsTheTypeOfEachItem()
        {
            CollectionOps.AddItem(_app.Session, new ItemId("kevlar_vest"));
            CollectionOps.AddItem(_app.Session, new ItemId("steel_knuckles"));
            _app.Collection.EquipItem(Verre, new ItemId("kevlar_vest"));
            var p = Loadout();
            Assert.That(_view.Rows[RowOf("Gilet kevlar")].Subtitle, Does.Contain(_loc.T("item.type.armor")));
            Assert.That(_view.Rows[RowOf("Poings d'acier")].Subtitle, Does.Contain(_loc.T("item.type.weapon")));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un objet d'un type déjà porté est grisé avec « Type déjà porté », et qu'un objet d'un autre type reste équipable — parce qu'un bouton grisé doit dire pourquoi")]
        public void Loadout_GreysAnItemWhoseTypeIsAlreadyWorn()
        {
            foreach (var id in new[] { "kevlar_vest", "riot_shield", "running_shoes" }) CollectionOps.AddItem(_app.Session, new ItemId(id));
            _app.Collection.EquipItem(Verre, new ItemId("kevlar_vest"));
            var p = Loadout();
            var shield = _view.Rows[RowOf("Bouclier anti-émeute")];
            Assert.That(shield.A.Enabled, Is.False);
            Assert.That(shield.A.Reason, Is.EqualTo(_loc.T("equip.type_taken")));
            Assert.That(_view.Rows[RowOf("Chaussures de course")].A.Enabled, Is.True);
            p.Dispose();
        }
        static int Opt(Family f) => 1 + (int)f;
        static int Opt(CardNature n) => 1 + (int)n;
        static int Opt(ItemType t) => 1 + (int)t;
        string[] Titles() => _view.Rows.Select(r => r.Title).ToArray();

        [Test, Description("Vérifie que les filtres du grimoire se combinent (Matière × Défensive : seul le Mur de verre) et que la barre montre l'option choisie de chaque groupe — parce que le joueur doit trouver vite les cartes d'un type de deck")]
        public void Grimoire_FiltersByFamilyAndNature()
        {
            var p = Grimoire();
            Assert.That(_view.Filters.Length, Is.EqualTo(2));
            Assert.That(_view.Filters[0].Labels.Length, Is.EqualTo(5));
            Assert.That(_view.Filters[1].Labels.Length, Is.EqualTo(4));
            _view.ClickFilter(0, Opt(Family.Matter));
            _view.ClickFilter(1, Opt(CardNature.Defensive));
            Assert.That(Titles(), Is.EqualTo(new[] { "Mur de verre" }));
            Assert.That(_view.Filters[0].Selected, Is.EqualTo(Opt(Family.Matter)));
            Assert.That(_view.Filters[1].Selected, Is.EqualTo(Opt(CardNature.Defensive)));
            _view.ClickFilter(0, 0);
            _view.ClickFilter(1, 0);
            Assert.That(_view.Rows.Length, Is.EqualTo(_app.Session.Content.Collection.Cards.Count), "« Toutes » partout : toutes les cartes");
            p.Dispose();
        }

        [Test, Description("Vérifie qu'une combinaison de filtres sans carte (Perception × Défensive) affiche une note grisée au lieu d'une grille vide — parce qu'une page vide ressemble à un bug")]
        public void Grimoire_EmptyFilter_ShowsANote()
        {
            var p = Grimoire();
            _view.ClickFilter(0, Opt(Family.Perception));
            _view.ClickFilter(1, Opt(CardNature.Defensive));
            Assert.That(_view.Rows.Length, Is.EqualTo(1));
            Assert.That(_view.Rows[0].Dim, Is.True);
            Assert.That(_view.Rows[0].Title, Is.EqualTo(_loc.T("grimoire.none_filtered")));
            Assert.That(_view.Rows[0].Tag, Is.Not.Empty, "une ligne grisée sans pastille serait prise pour une section et masquée");
            p.Dispose();
        }

        [Test, Description("Vérifie que les filtres restent actifs après une action qui rafraîchit la page (achat d'un booster) — parce que le joueur ne doit pas refaire ses filtres à chaque geste")]
        public void Grimoire_KeepsItsFilters_AfterARefresh()
        {
            _app.Session.Wallet.Add(0, 1000);
            var p = Grimoire();
            _view.ClickFilter(0, Opt(Family.Elemental));
            var before = Titles();
            _view.ClickHeader(1);
            Assert.That(Titles(), Is.EqualTo(before));
            Assert.That(_view.Filters[0].Selected, Is.EqualTo(Opt(Family.Elemental)));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'ouvert sur une carte précise (depuis la fiche d'un agent), le grimoire règle le filtre de famille sur celle de la carte et laisse toutes les natures — parce que le joueur cherche alors une carte pour cet agent")]
        public void Grimoire_FocusedCard_PresetsItsFamily()
        {
            _nav.FocusCard = "card_mark";
            var p = Start(new GrimoirePagePresenter(_view, _app.Collection, _app.Session, new ModalService(_host), _loc, _toasts,
                                                    _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time, _nav));
            Assert.That(_view.Filters[0].Selected, Is.EqualTo(Opt(Family.Perception)));
            Assert.That(_view.Filters[1].Selected, Is.EqualTo(0));
            p.Dispose();
        }

        [Test, Description("Vérifie que l'équipement se filtre par type (Armure : seul le gilet kevlar) et qu'un filtre sans objet affiche une note — parce que le type décide de ce qu'un agent peut porter")]
        public void Inventory_FiltersByType()
        {
            CollectionOps.AddItem(_app.Session, new ItemId("kevlar_vest"));
            CollectionOps.AddItem(_app.Session, new ItemId("steel_knuckles"));
            var p = Inventory();
            Assert.That(_view.Filters.Length, Is.EqualTo(1));
            Assert.That(_view.Filters[0].Labels.Length, Is.EqualTo(5));
            _view.ClickFilter(0, Opt(ItemType.Armor));
            Assert.That(Titles(), Is.EqualTo(new[] { "Gilet kevlar" }));
            _view.ClickFilter(0, Opt(ItemType.Implant));
            Assert.That(_view.Rows.Length, Is.EqualTo(1));
            Assert.That(_view.Rows[0].Title, Is.EqualTo(_loc.T("inventory.none_filtered")));
            Assert.That(_view.Rows[0].Tag, Is.Not.Empty);
            p.Dispose();
        }
    }
}
