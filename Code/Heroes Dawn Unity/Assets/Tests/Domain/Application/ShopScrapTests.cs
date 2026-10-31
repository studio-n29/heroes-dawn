#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Shop;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Boutique du jour (L2 de la 0.15.0) : héros en vedette, six lots de scraps achetables contre des Crédits.</summary>
    public sealed class ShopScrapTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
        }

        [Test, Description("Vérifie que la boutique du jour propose six lots de scraps — parce que la spec de la 0.15.0 remplace les objets par des scraps dans la boutique du jour")]
        public void Shop_HasSixScrapLots()
            => Assert.That(_app.Session.Shop.Scraps.Count, Is.EqualTo(ShopRules.ScrapLotsPerDay));

        [Test, Description("Vérifie que chaque lot a la quantité et le prix du réglage — parce que le prix doit rester celui de l'équilibrage, pas une valeur en dur dans l'interface")]
        public void EveryScrapLot_UsesRulesCountAndPrice()
        {
            foreach (var lot in _app.Session.Shop.Scraps)
            {
                Assert.That(lot.Count, Is.EqualTo(ShopRules.ScrapLotCount));
                Assert.That(lot.Price, Is.EqualTo(ShopRules.ScrapLotPrice));
            }
        }

        [Test, Description("Vérifie que le héros le plus rare du tirage est en première place — parce que la vedette est le héros le plus rare de la boutique du jour")]
        public void FeaturedHero_IsTheRarest()
        {
            var offers = _app.Session.Shop.Offers;
            Assert.That(offers.Count, Is.GreaterThan(0), "la boutique doit proposer au moins un héros");
            int featured = ShopDraw.Rank(_app.Session.Content.Member(offers[0].Id).Rarity);
            foreach (var o in offers)
                Assert.That(ShopDraw.Rank(_app.Session.Content.Member(o.Id).Rarity), Is.LessThanOrEqualTo(featured));
        }

        [Test, Description("Vérifie qu'acheter un lot débite les Crédits et ajoute les scraps au profil — parce que c'est le seul moyen d'obtenir des scraps depuis la boutique")]
        public void BuyScrapLot_PaysAndGrantsScraps()
        {
            _app.Session.Wallet.Set(1000, 0);
            var lot = _app.Session.Shop.Scraps[0];
            int before = CraftingOps.Scraps(_app.Session, lot.Kind);

            var r = _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            Assert.That(r.IsSuccess, Is.True);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000 - lot.Price));
            Assert.That(CraftingOps.Scraps(_app.Session, lot.Kind), Is.EqualTo(before + lot.Count));
        }

        [Test, Description("Vérifie qu'un lot ne s'achète qu'une fois dans la journée — parce que les lots sont à usage unique par tirage")]
        public void BuyScrapLot_TwiceIsRefused()
        {
            _app.Session.Wallet.Set(1000, 0);
            _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            var r = _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            Assert.That(r.IsFailure, Is.True);
            Assert.That(r.Error, Is.EqualTo(ShopErrors.AlreadyBought));
        }

        [Test, Description("Vérifie qu'un achat sans assez de Crédits ne donne rien — parce qu'un lot gratuit ferait perdre toute valeur aux Crédits")]
        public void BuyScrapLot_WithoutCredits_GrantsNothing()
        {
            _app.Session.Wallet.Set(0, 0);
            var lot = _app.Session.Shop.Scraps[0];
            int before = CraftingOps.Scraps(_app.Session, lot.Kind);

            var r = _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            Assert.That(r.IsFailure, Is.True);
            Assert.That(CraftingOps.Scraps(_app.Session, lot.Kind), Is.EqualTo(before));
            Assert.That(_app.Session.Shop.Scraps[0].Bought, Is.False);
        }
    }
}
