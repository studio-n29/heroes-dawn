using System;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class CosmeticShopTests
    {
        TestApp _app = null!;
        static readonly DateTime Monday = new DateTime(2026, 10, 5, 12, 0, 0);

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Time.LocalNow = Monday;
        }

        static string Code(N29.HeroesDawn.Core.Result r) => r.Error?.Code ?? "ok";
        static string Ids(System.Collections.Generic.IEnumerable<CosmeticOffer> o) => string.Join(",", o.Select(x => x.Def.Id));

        [Test, Description("Vérifie que la rotation est la même tous les jours d'une semaine ISO et qu'elle change d'une semaine à l'autre — parce que la boutique est hebdomadaire et que deux appareils doivent voir la même chose")]
        public void La_rotation_hebdomadaire_est_deterministe_et_change_chaque_semaine()
        {
            var week = Ids(_app.Cosmetics.Weekly(Monday));
            Assert.That(Ids(_app.Cosmetics.Weekly(Monday.AddDays(6))), Is.EqualTo(week), "dimanche = lundi");
            Assert.That(Ids(new CosmeticShop(_app.MetaCtx, TestApp.TestCosmetics).Weekly(Monday)), Is.EqualTo(week), "indépendante de l'instance");
            var seen = Enumerable.Range(0, 8).Select(w => Ids(_app.Cosmetics.Weekly(Monday.AddDays(7 * w)))).Distinct().Count();
            Assert.That(seen, Is.GreaterThan(1), "la rotation varie");
        }

        [Test, Description("Vérifie que la rotation compte six articles de source Boutique, jamais un cosmétique possédé ni de pass ou de tournoi — parce que seule la source Boutique est en vente")]
        public void La_rotation_ne_propose_que_des_articles_de_source_Shop_et_aucun_possede()
        {
            var week = _app.Cosmetics.Weekly(Monday);
            Assert.That(week.Count, Is.EqualTo(6));
            Assert.That(week.All(o => o.Def.Source == CosmeticSource.Shop && o.Def.Price > 0));
            CosmeticOps.Unlock(_app.Session.Meta, week[0].Def.Id);
            var next = _app.Cosmetics.Weekly(Monday);
            Assert.That(next.Select(o => o.Def.Id), Does.Not.Contain(week[0].Def.Id));
            Assert.That(next.Count, Is.EqualTo(6));
        }

        [Test, Description("Vérifie qu'un achat débite le prix en Gemmes et ajoute le cosmétique au profil — parce que c'est tout l'effet d'un achat")]
        public void Acheter_debite_les_gemmes_et_ajoute_le_cosmetique()
        {
            var offer = _app.Cosmetics.Weekly(Monday)[0].Def;
            _app.Session.Wallet.SetGems(2000);
            Assert.That(_app.Cosmetics.Buy(offer.Id).IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(2000 - offer.Price));
            Assert.That(CosmeticOps.Owns(_app.Session.Meta, offer.Id));
            var other = _app.Cosmetics.Weekly(Monday).First().Def;
            _app.Session.Wallet.SetGems(other.Price - 1);
            Assert.That(Code(_app.Cosmetics.Buy(other.Id)), Is.EqualTo("meta.gems"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(other.Price - 1), "un refus ne coûte rien");
        }

        [Test, Description("Vérifie qu'un cosmétique possédé ne se rachète pas et que rien n'est débité — parce que le joueur ne doit pas payer deux fois")]
        public void Un_cosmetique_possede_ne_se_rachete_pas()
        {
            var id = _app.Cosmetics.Weekly(Monday)[0].Def.Id;
            _app.Session.Wallet.SetGems(5000);
            _app.Cosmetics.Buy(id);
            int gems = _app.Session.Wallet.Gems;
            Assert.That(Code(_app.Cosmetics.Buy(id)), Is.EqualTo("meta.done"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems));
            Assert.That(_app.Session.Meta.cosmetics.Count(c => c == id), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que les cosmétiques de tournoi, de pass et hors rotation ne s'achètent jamais — parce qu'ils se gagnent seulement")]
        public void Un_cosmetique_de_tournoi_n_est_jamais_en_vente()
        {
            _app.Session.Wallet.SetGems(100000);
            foreach (var d in TestApp.TestCosmetics.Where(d => d.Source != CosmeticSource.Shop))
            {
                Assert.That(d.Price, Is.EqualTo(0), d.Id);
                Assert.That(Code(_app.Cosmetics.Buy(d.Id)), Is.Not.EqualTo("ok"), d.Id);
            }
            Assert.That(TestApp.TestCosmetics.Count(d => d.Source == CosmeticSource.Tournament && d.Type == CosmeticType.Frame), Is.EqualTo(2));
            Assert.That(TestApp.TestCosmetics.Count(d => d.Source == CosmeticSource.Tournament && d.Type == CosmeticType.Title), Is.EqualTo(2));
            Assert.That(_app.Session.Meta.cosmetics, Is.Empty);
        }

        [Test, Description("Vérifie que le catalogue de lancement a les prix et les volumes de la spec, qu'aucun article ne dépasse 1 000 Gemmes et que chaque identifiant de pass existe en source Pass — parce que le pass donne des cosmétiques qui doivent exister")]
        public void Aucun_article_ne_depasse_1000_gemmes()
        {
            var cat = TestApp.TestCosmetics;
            Assert.That(cat.Max(d => d.Price), Is.LessThanOrEqualTo(1000));
            var shop = cat.Where(d => d.Source == CosmeticSource.Shop).ToList();
            Assert.That(shop.Count(d => d.Type == CosmeticType.Outfit && d.Price == 1000), Is.EqualTo(4));
            Assert.That(shop.Count(d => d.Type == CosmeticType.HqTheme && d.Price == 400), Is.EqualTo(2));
            Assert.That(shop.Count(d => (d.Type == CosmeticType.Dice || d.Type == CosmeticType.Pawn) && d.Price == 150), Is.EqualTo(3));
            Assert.That(shop.Select(d => d.Id), Is.SupersetOf(new[] { "outfit.agent_d", "outfit.agent_m", "outfit.victor_sorel", "outfit.odile_ferrand" }));
            Assert.That(cat.Select(d => d.Id).Distinct().Count(), Is.EqualTo(cat.Count), "identifiants uniques");
            foreach (var id in TestApp.TestPassRewards.Free.Concat(TestApp.TestPassRewards.Premium).SelectMany(d => d.Cosmetics))
                Assert.That(cat.Any(d => d.Id == id && d.Source == CosmeticSource.Pass), Is.True, "pass : " + id);
        }

        [Test, Description("Vérifie que Unlock est idempotent et qu'une ancienne sauvegarde dont les cosmétiques de tournoi étaient rangés dans l'arène est migrée sans doublon — parce que tous les cosmétiques partagent la même liste")]
        public void Unlock_est_idempotent()
        {
            var m = _app.Session.Meta;
            Assert.That(CosmeticOps.Unlock(m, "dice.chrome"), Is.True);
            Assert.That(CosmeticOps.Unlock(m, "dice.chrome"), Is.False);
            Assert.That(CosmeticOps.Unlock(m, ""), Is.False);
            Assert.That(m.cosmetics.Count, Is.EqualTo(1));
            m.arena.cosmetics.Add("cosmetic:christmas_gold");
            m.arena.cosmetics.Add("dice.chrome");
            MetaMigration.CosmeticsToProfile(m);
            MetaMigration.CosmeticsToProfile(m);
            Assert.That(m.cosmetics, Is.EqualTo(new[] { "dice.chrome", "cosmetic:christmas_gold" }));
            Assert.That(m.arena.cosmetics, Is.Empty);
        }
    }
}
