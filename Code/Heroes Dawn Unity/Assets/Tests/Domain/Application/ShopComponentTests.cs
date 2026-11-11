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
    /// <summary>Boutique du jour (L2 de la 0.15.0) : héros en vedette, six lots de Composants (Commun et Raffiné) achetables contre des Crédits.</summary>
    public sealed class ShopComponentTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
        }

        [Test, Description("Vérifie que la boutique du jour propose six lots de Composants dont deux Raffinés — parce que la boutique de Composants remplace celle de scraps")]
        public void La_boutique_de_composants_propose_six_lots()
            => Assert.That(_app.Session.Shop.Components.Count, Is.EqualTo(ShopRules.ComponentLotsPerDay));

        [Test, Description("Vérifie que la boutique contient des lots Commun et des lots Raffiné — parce que la boutique de Composants vend les deux premières qualités")]
        public void La_boutique_vend_des_lots_Commun_et_Raffine()
            => Assert.That(_app.Session.Shop.Components.Select(l => l.Grade).Distinct().OrderBy(g => g), Is.EqualTo(new[] { ComponentGrade.Common, ComponentGrade.Refined }));

        [Test, Description("Vérifie que chaque lot a la quantité et le prix du réglage de sa qualité (6 Communs pour 120, 3 Raffinés pour 240) — parce que le prix doit rester celui de l'équilibrage")]
        public void Chaque_lot_suit_les_quantites_et_prix_de_sa_qualite()
        {
            foreach (var lot in _app.Session.Shop.Components)
            {
                bool refined = lot.Grade == ComponentGrade.Refined;
                Assert.That(lot.Count, Is.EqualTo(refined ? ShopRules.RefinedLotCount : ShopRules.CommonLotCount));
                Assert.That(lot.Price, Is.EqualTo(refined ? ShopRules.RefinedLotPrice : ShopRules.CommonLotPrice));
            }
        }

        [Test, Description("Vérifie que le héros le plus rare du tirage est en première place — parce que la vedette est le héros le plus rare de la boutique du jour")]
        public void FeaturedHero_IsTheRarest()
        {
            var offers = _app.Session.Shop.Offers;
            Assert.That(offers.Count, Is.GreaterThan(0), "la boutique doit proposer au moins un héros");
            int featured = _app.Session.Content.Member(offers[0].Id).Rank;
            foreach (var o in offers)
                Assert.That(_app.Session.Content.Member(o.Id).Rank, Is.LessThanOrEqualTo(featured));
        }

        [Test, Description("Vérifie qu'acheter un lot débite les Crédits et ajoute les Composants au profil — parce que c'est un moyen d'obtenir des Composants depuis la boutique")]
        public void Acheter_un_lot_ajoute_les_composants()
        {
            _app.Session.Wallet.Set(1000, 0);
            var lot = _app.Session.Shop.Components[0];
            int before = CraftingOps.Components(_app.Session, lot.Kind, lot.Grade);

            var r = _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            Assert.That(r.IsSuccess, Is.True);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000 - lot.Price));
            Assert.That(CraftingOps.Components(_app.Session, lot.Kind, lot.Grade), Is.EqualTo(before + lot.Count));
        }

        [Test, Description("Vérifie qu'un lot ne s'achète qu'une fois dans la journée — parce que les lots sont à usage unique par tirage")]
        public void Un_lot_ne_s_achete_qu_une_fois()
        {
            _app.Session.Wallet.Set(1000, 0);
            _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            var r = _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            Assert.That(r.IsFailure, Is.True);
            Assert.That(r.Error, Is.EqualTo(ShopErrors.AlreadyBought));
        }

        [Test, Description("Vérifie qu'un achat sans assez de Crédits ne donne rien — parce qu'un lot gratuit ferait perdre toute valeur aux Crédits")]
        public void Sans_credits_rien_n_est_donne()
        {
            _app.Session.Wallet.Set(0, 0);
            var lot = _app.Session.Shop.Components[0];
            int before = CraftingOps.Components(_app.Session, lot.Kind, lot.Grade);

            var r = _app.Shop.BuyScrap.ExecuteAsync(0).GetAwaiter().GetResult();

            Assert.That(r.IsFailure, Is.True);
            Assert.That(CraftingOps.Components(_app.Session, lot.Kind, lot.Grade), Is.EqualTo(before));
            Assert.That(_app.Session.Shop.Components[0].Bought, Is.False);
        }
    }
}
