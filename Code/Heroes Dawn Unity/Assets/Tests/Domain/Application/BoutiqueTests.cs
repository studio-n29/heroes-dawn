using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class BoutiqueTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code<T>(N29.HeroesDawn.Core.Result<T> r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie que la boutique vend les six packs (100, 520, 1 080, 2 250, 6 000 et 13 000 Gemmes) aux identifiants de produit prévus — parce que ces identifiants sont ceux à déclarer dans les stores")]
        public void Les_packs_sont_les_six_de_la_spec()
        {
            Assert.That(Boutique.Packs.Select(p => p.Id), Is.EqualTo(new[] { "gems_100", "gems_520", "gems_1080", "gems_2250", "gems_6000", "gems_13000" }));
            Assert.That(Boutique.Packs.Select(p => p.Gems), Is.EqualTo(new[] { 100, 520, 1080, 2250, 6000, 13000 }));
            Assert.That(Boutique.Packs.Select(p => p.PriceText), Is.EqualTo(new[] { "0,99 €", "4,99 €", "9,99 €", "19,99 €", "49,99 €", "99,99 €" }));
        }

        [Test, Description("Vérifie que le pack à 99,99 € donne 13 000 Gemmes après confirmation du store — parce que le plus gros pack suit la progression du bonus (130 Gemmes par euro)")]
        public void Le_pack_de_99_99_donne_13000_gemmes()
        {
            int before = _app.Session.Wallet.Gems;
            var r = _app.Boutique.BuyPackAsync("gems_13000", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(before + 13000));
        }

        [Test, Description("Vérifie qu'aucun échange de Gemmes ne donne de PR et que la boutique ne vend plus l'échange « ex_pr » — parce que les PR achètent des recrues et des boosters tirés au hasard : les vendre contre de l'argent ferait des objets aléatoires payants")]
        public void Aucun_echange_ne_donne_de_PR()
        {
            Assert.That(Boutique.Exchanges.Select(e => e.Id), Is.EqualTo(new[] { "ex_credits", "ex_rest" }));
            Assert.That(Boutique.Exchanges.All(e => e.Reward.Pr == 0));
            Assert.That(_app.Boutique.Exchange("ex_pr").Error!.Code, Is.EqualTo("meta.unknown"));
        }

        [Test, Description("Vérifie que l'échange de Crédits est limité à cinq par jour, puis de nouveau possible le lendemain — parce que les Gemmes achètent du temps et du confort, pas toute la progression d'un coup")]
        public void L_echange_de_credits_est_limite_a_cinq_par_jour()
        {
            _app.Session.Wallet.SetGems(1000);
            int credits = _app.Session.Wallet.Credits;
            for (int i = 0; i < 5; i++) Assert.That(_app.Boutique.Exchange("ex_credits").IsSuccess, "échange " + (i + 1));
            var sixth = _app.Boutique.Exchange("ex_credits");
            Assert.That(sixth.Error!.Code, Is.EqualTo("meta.daily_limit"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(900), "le sixième ne dépense rien");
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits + 5000));
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(1);
            Assert.That(_app.Boutique.Exchange("ex_credits").IsSuccess, "le lendemain, la limite repart");
        }

        [Test, Description("Vérifie que l'accélérateur de 8 heures coûte 140 Gemmes, ajoute 480 minutes d'accélération et est refusé sans assez de Gemmes — parce que c'est du temps gagné (pay-to-fast), pas de la puissance")]
        public void L_accelerateur_de_8_heures_s_achete_en_gemmes()
        {
            _app.Session.Wallet.SetGems(139);
            int tokens = _app.Session.Meta.speedUpTokens;
            Assert.That(_app.Boutique.BuyAccelerator().Error!.Code, Is.EqualTo("meta.gems"));
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(tokens));
            _app.Session.Wallet.SetGems(140);
            Assert.That(_app.Boutique.BuyAccelerator().IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(0));
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(tokens + 480));
        }

        [Test, Description("Vérifie qu'un pack acheté donne ses Gemmes une fois le paiement confirmé — parce que les Gemmes ne se versent qu'après la confirmation du store")]
        public void Acheter_un_pack_donne_les_gemmes_apres_confirmation()
        {
            int before = _app.Session.Wallet.Gems;
            var r = _app.Boutique.BuyPackAsync("gems_520", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess);
            Assert.That(r.Value, Is.EqualTo(520));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(before + 520));
            Assert.That(_app.Purchases.Calls, Is.EqualTo(new[] { "gems_520" }));
        }

        [Test, Description("Vérifie qu'un paiement refusé ne donne aucune Gemme — parce qu'on ne livre jamais sans paiement")]
        public void Un_paiement_refuse_ne_donne_rien()
        {
            _app.Purchases.Fail = true;
            int before = _app.Session.Wallet.Gems;
            var r = _app.Boutique.BuyPackAsync("gems_100", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(Code(r), Is.EqualTo("meta.payment"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(before));
        }

        [Test, Description("Vérifie que l'offre de bienvenue donne 500 Gemmes et un accélérateur de 8 heures (480 minutes) — parce que c'est le contenu de l'offre de la spec")]
        public void L_offre_de_bienvenue_donne_500_gemmes_et_un_accelerateur_de_8_heures()
        {
            int before = _app.Session.Wallet.Gems;
            var r = _app.Boutique.BuyPackAsync("welcome", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(before + 500));
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(480));
            Assert.That(_app.Session.Meta.welcomeClaimed);
        }

        [Test, Description("Vérifie que l'offre de bienvenue déjà prise est refusée avant tout paiement — parce qu'un joueur ne doit jamais être débité pour une offre qu'il ne peut pas recevoir")]
        public void L_offre_de_bienvenue_ne_s_achete_qu_une_fois_sans_debit()
        {
            _app.Boutique.BuyPackAsync("welcome", CancellationToken.None).GetAwaiter().GetResult();
            int gems = _app.Session.Wallet.Gems;
            var second = _app.Boutique.BuyPackAsync("welcome", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(Code(second), Is.EqualTo("meta.done"));
            Assert.That(_app.Purchases.Calls.Count(c => c == "welcome"), Is.EqualTo(1), "le store n'a pas été sollicité la seconde fois");
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems));
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(480));
        }

        [Test, Description("Vérifie qu'une sauvegarde 0.15.0 portant des compteurs gems_550 et gems_1200 reste lisible et que les achats continuent — parce que ces packs ont disparu mais ne doivent casser aucun profil")]
        public void Les_anciens_compteurs_gems_550_sont_ignores()
        {
            _app.Session.Meta.SetCounter(_app.Session.Meta.purchases, "gems_550", 3);
            _app.Session.Meta.SetCounter(_app.Session.Meta.purchases, "gems_1200", 1);
            var r = _app.Boutique.BuyPackAsync("gems_100", CancellationToken.None).GetAwaiter().GetResult();
            Assert.That(r.IsSuccess);
            Assert.That(Code(_app.Boutique.BuyPackAsync("gems_550", CancellationToken.None).GetAwaiter().GetResult()), Is.EqualTo("meta.unknown"), "l'ancien produit n'est plus vendu");
            Assert.That(_app.Session.Meta.GetCounter(_app.Session.Meta.purchases, "gems_550"), Is.EqualTo(3), "le compteur ancien n'est ni lu ni modifié");
        }
    }
}
