using System.IO;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Boutique en cartes : chaque offre a une illustration et une description (lot F de la 0.14.0).</summary>
    public sealed class ShopCardsTests
    {
        static readonly string[] IconIds = { "booster", "gems_small", "gems_medium", "gems_large", "exchange_credits", "exchange_pr", "exchange_rest", "gift" };

        TestApp _app = null!;
        FakeListView _view = null!;
        HubNavigator _nav = null!;
        readonly LocLocalization _loc = new LocLocalization();

        BoutiquePagePresenter Shop(ShopTab tab)
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _nav = new HubNavigator { ShopTab = tab };
            var p = new BoutiquePagePresenter(_view, _app.Boutique, _app.Session, new ModalService(new FakeModalHost()), _loc, new FakeToasts(),
                                              _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time,
                                              _app.Collection, _nav);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que chaque offre des onglets Boosters, Gemmes et crédits et Cadeau porte une description et une illustration de boutique — parce que la boutique est faite de cartes avec icône et texte")]
        public void Rows_HaveADescription_AndAnIllustration()
        {
            foreach (var tab in new[] { ShopTab.Boosters, ShopTab.Gems })
            {
                var p = Shop(tab);
                Assert.That(_view.Rows.Length, Is.GreaterThan(0), tab.ToString());
                foreach (var r in _view.Rows)
                {
                    Assert.That(r.Subtitle, Is.Not.Empty, tab + " : " + r.Title);
                    Assert.That(r.Art, Is.Not.Null, tab + " : " + r.Title);
                }
                p.Dispose();
            }
        }

        [Test, Description("Vérifie que l'onglet Gemmes propose les packs, un accélérateur, l'emplacement d'expédition suivant, les échanges de Crédits et de repos et les cosmétiques de la semaine, sans aucun échange de PR — parce que les Gemmes achètent du temps et du confort, jamais de puissance")]
        public void GemsTab_ListsTimeComfortAndCosmetics_ButNoPrExchange()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _nav = new HubNavigator { ShopTab = ShopTab.Gems };
            var p = new BoutiquePagePresenter(_view, _app.Boutique, _app.Session, new ModalService(new FakeModalHost()), _loc, new FakeToasts(),
                                              _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time,
                                              _app.Collection, _nav, null, _app.Cosmetics);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            var titles = _view.Rows.Select(r => r.Title).ToList();
            Assert.That(titles, Does.Contain(_loc.F("shop.accel", 8)), "accélérateur de 8 h");
            Assert.That(titles, Does.Contain(_loc.F("shop.slot", 4)), "4e emplacement d'expédition");
            Assert.That(titles, Does.Contain(_loc.T("shop.ex_credits")));
            Assert.That(titles, Does.Not.Contain("60 PR"), "plus d'échange de PR");
            int cosmetics = _app.Cosmetics.WeeklyNow().Count;
            Assert.That(cosmetics, Is.GreaterThan(0));
            Assert.That(_view.Rows.Count(r => r.Subtitle == _loc.F("shop.desc.cosmetic", _app.Cosmetics.WeeklyNow().First().Def.Price)), Is.GreaterThan(0));
            Assert.That(_view.Rows.Length, Is.EqualTo(1 + Boutique.Packs.Count + 3 + 1 + Boutique.Exchanges.Count + 1 + cosmetics), "titre, packs, confort (titre, accélérateur, emplacement), échanges (titre + lignes), cosmétiques (titre + articles)");
            p.Dispose();
        }

        [Test, Description("Vérifie que l'en-tête de la boutique n'affiche plus le texte « achats simulés » — parce que l'écran doit rester léger")]
        public void Header_NoLongerShowsTheSimulatedNote()
        {
            var p = Shop(ShopTab.Boosters);
            Assert.That(_view.Header.Line, Does.Not.Contain(_loc.T("shop.simulated")));
            p.Dispose();
        }

        [Test, Description("Vérifie que le chemin de ressource d'une icône de boutique est Illustrations/Shop/<identifiant> — parce que les icônes sont chargées par identifiant")]
        public void ShopPath_IsByIdentifier()
        {
            Assert.That(AgentArt.ShopPath("booster"), Is.EqualTo("Illustrations/Shop/booster"));
        }

        [Test, Description("Vérifie que les huit icônes existent en SVG (source) et en PNG (ressource du jeu) — parce que les visuels de la boutique sont dessinés en SVG et rasterisés")]
        public void Icons_ExistAsSvgAndPng()
        {
            string assets = UnityEngine.Application.dataPath;
            string svg = Path.GetFullPath(Path.Combine(assets, "../../../03_Art_UI_UX/Illustrations/Shop"));
            foreach (var id in IconIds)
            {
                Assert.That(File.Exists(Path.Combine(svg, id + ".svg")), Is.True, id + ".svg");
                Assert.That(File.Exists(Path.Combine(assets, "_Project/Resources/Illustrations/Shop", id + ".png")), Is.True, id + ".png");
            }
        }
    }
}
