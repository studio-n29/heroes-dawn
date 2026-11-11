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

        [Test, Description("Vérifie que la boutique vend les cinq packs de la spec (100, 520, 1 080, 2 250 et 6 000 Gemmes) aux identifiants de produit prévus — parce que ces identifiants sont ceux à déclarer dans les stores")]
        public void Les_packs_sont_les_cinq_de_la_spec()
        {
            Assert.That(Boutique.Packs.Select(p => p.Id), Is.EqualTo(new[] { "gems_100", "gems_520", "gems_1080", "gems_2250", "gems_6000" }));
            Assert.That(Boutique.Packs.Select(p => p.Gems), Is.EqualTo(new[] { 100, 520, 1080, 2250, 6000 }));
            Assert.That(Boutique.Packs.Select(p => p.PriceText), Is.EqualTo(new[] { "0,99 €", "4,99 €", "9,99 €", "19,99 €", "49,99 €" }));
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
