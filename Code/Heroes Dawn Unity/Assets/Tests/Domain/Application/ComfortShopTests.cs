using System;
using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Shop;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Boutique de confort (bloc E) : emplacements d'expédition, renouvellements en Gemmes, accélérations.</summary>
    public sealed class ComfortShopTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code(Result r) => r.Error?.Code ?? "ok";

        void FreeMembers(int n)
        {
            foreach (var id in new[] { "etincelle", "arc", "rempart", "verre" })
            {
                if (n-- <= 0) break;
                _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(new MemberId(id)), 10));
            }
        }

        [Test, Description("Vérifie que le 4e emplacement coûte 300 Gemmes et le 5e 500, achetés dans l'ordre et déduits du solde — parce que ce sont les prix de la spec, et que l'achat est permanent")]
        public void Le_4e_emplacement_coute_300_gemmes_et_le_5e_500()
        {
            Assert.That((Boutique.SlotPrice(4), Boutique.SlotPrice(5)), Is.EqualTo((300, 500)));
            Assert.That(Expeditions.SlotCount(_app.Session.Meta), Is.EqualTo(3));
            _app.Session.Wallet.SetGems(1000);
            Assert.That(_app.Boutique.BuyExpeditionSlot().IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(700));
            Assert.That(Expeditions.SlotCount(_app.Session.Meta), Is.EqualTo(4));
            Assert.That(_app.Boutique.BuyExpeditionSlot().IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(200));
            Assert.That(Expeditions.SlotCount(_app.Session.Meta), Is.EqualTo(5));
            Assert.That(_app.Operations.SlotViews().Count, Is.EqualTo(5));
        }

        [Test, Description("Vérifie qu'un achat d'emplacement sans assez de Gemmes est refusé sans rien dépenser ni ajouter — parce qu'un refus ne doit rien coûter")]
        public void Acheter_sans_assez_de_gemmes_ne_depense_rien()
        {
            _app.Session.Wallet.SetGems(299);
            Assert.That(Code(_app.Boutique.BuyExpeditionSlot()), Is.EqualTo("meta.gems"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(299));
            Assert.That(Expeditions.SlotCount(_app.Session.Meta), Is.EqualTo(3));
            Assert.That(_app.Session.Meta.expeditionSlotsBought, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'au-delà du 5e emplacement l'achat est refusé sans débit — parce que le maximum est de cinq emplacements")]
        public void Au_dela_du_5e_emplacement_l_achat_est_refuse()
        {
            _app.Session.Wallet.SetGems(5000);
            _app.Boutique.BuyExpeditionSlot();
            _app.Boutique.BuyExpeditionSlot();
            int gems = _app.Session.Wallet.Gems;
            Assert.That(Code(_app.Boutique.BuyExpeditionSlot()), Is.EqualTo("meta.max_level"));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems));
            Assert.That(Expeditions.SlotCount(_app.Session.Meta), Is.EqualTo(Expeditions.MaxSlots));
        }

        [Test, Description("Vérifie qu'une sauvegarde qui compterait trop d'emplacements achetés est ramenée au maximum de cinq — parce que le compteur sauvegardé ne doit jamais donner plus que la spec")]
        public void Le_compteur_d_emplacements_est_plafonne()
        {
            _app.Session.Meta.expeditionSlotsBought = 9;
            Assert.That(Expeditions.SlotCount(_app.Session.Meta), Is.EqualTo(5));
        }

        [Test, Description("Vérifie qu'une expédition en cours dans l'emplacement 3 reste au même rang, avec les mêmes agents, quand un emplacement s'ajoute — parce que l'achat ne doit déplacer aucune mission")]
        public void Les_expeditions_en_cours_ne_bougent_pas_quand_un_emplacement_s_ajoute()
        {
            FreeMembers(1);
            _app.Operations.Start(2, "patrol");
            var before = _app.Operations.SlotViews()[2];
            _app.Session.Wallet.SetGems(300);
            _app.Boutique.BuyExpeditionSlot();
            var views = _app.Operations.SlotViews();
            Assert.That(views.Count, Is.EqualTo(4));
            Assert.That(views[2].Template, Is.SameAs(before.Template));
            Assert.That(views[2].Members, Is.EqualTo(before.Members));
            Assert.That(views[2].Running);
            Assert.That(views[3].Empty);
        }

        [Test, Description("Vérifie que le renouvellement est gratuit le premier du jour, puis coûte 100, 200, 400 et 800 Crédits, puis 10 Gemmes — parce que c'est l'échelle de prix de la spec")]
        public void Le_premier_renouvellement_du_jour_est_gratuit_puis_Credits_puis_10_gemmes()
        {
            var b = new BalanceConfig();
            Assert.That(b.ShopRerollGemCost, Is.EqualTo(10));
            var prices = Enumerable.Range(0, 8).Select(i => ShopRules.RerollPrice(i, b)).ToArray();
            Assert.That(prices, Is.EqualTo(new[] { (0, 0), (100, 0), (200, 0), (400, 0), (800, 0), (0, 10), (0, 10), (0, 10) }));
        }

        [Test, Description("Vérifie qu'un renouvellement au-delà de l'échelle en Crédits se paie 10 Gemmes, et qu'il est refusé sans rien changer quand le solde de Gemmes manque — parce que le joueur ne doit jamais perdre de ressource sur un refus")]
        public void Un_renouvellement_se_paie_en_gemmes_apres_l_echelle_en_credits()
        {
            var sh = _app.Shop;
            sh.Open.ExecuteAsync().GetAwaiter().GetResult();
            _app.Session.Wallet.Add(100000, 0);
            for (int i = 0; i < 5; i++) Assert.That(sh.Reroll.ExecuteAsync().GetAwaiter().GetResult().IsSuccess, "renouvellement " + (i + 1));
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(5));
            _app.Session.Wallet.SetGems(9);
            int credits = _app.Session.Wallet.Credits;
            Assert.That(sh.Reroll.ExecuteAsync().GetAwaiter().GetResult().Error!.Code, Is.EqualTo("meta.gems"));
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(5));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(9));
            _app.Session.Wallet.SetGems(25);
            Assert.That(sh.Reroll.ExecuteAsync().GetAwaiter().GetResult().IsSuccess);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(15));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(credits), "aucun Crédit pour un renouvellement en Gemmes");
        }

        [Test, Description("Vérifie qu'une accélération coûte 20 Gemmes par heure arrondie à la minute supérieure (60 min → 20, 61 min → 21, 1 min → 1) — parce que c'est le tarif de la spec")]
        public void Une_acceleration_coute_20_gemmes_par_heure_arrondie_a_la_minute()
        {
            Assert.That(Expeditions.SpeedUpCost(TimeSpan.FromMinutes(60)), Is.EqualTo(20));
            Assert.That(Expeditions.SpeedUpCost(TimeSpan.FromMinutes(61)), Is.EqualTo(21));
            Assert.That(Expeditions.SpeedUpCost(TimeSpan.FromMinutes(1)), Is.EqualTo(1));
            Assert.That(Expeditions.SpeedUpCost(TimeSpan.FromSeconds(30)), Is.EqualTo(1), "toute minute entamée compte");
            Assert.That(Expeditions.SpeedUpCost(TimeSpan.FromMinutes(240)), Is.EqualTo(80));
        }

        [Test, Description("Vérifie qu'un accélérateur est dépensé avant les Gemmes, qu'il couvre d'abord les minutes restantes et que les Gemmes ne paient que le reste — parce que le temps offert ne doit jamais coûter deux fois")]
        public void Un_accelerateur_est_depense_avant_les_gemmes()
        {
            FreeMembers(3);
            _app.Operations.Start(0, "recon"); // 60 min, 2 agents
            _app.Session.Meta.speedUpTokens = 100;
            _app.Session.Wallet.SetGems(0);
            Assert.That(_app.Operations.SpeedUp(0).IsSuccess, "l'accélérateur couvre toute l'heure");
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(40));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(0));
            Assert.That(_app.Operations.SlotViews()[0].Ready);

            _app.Operations.Claim(0);
            _app.Operations.Start(0, "convoy"); // 240 min, 3 agents
            _app.Session.Meta.speedUpTokens = 60;
            _app.Session.Wallet.SetGems(59);
            Assert.That(Code(_app.Operations.SpeedUp(0)), Is.EqualTo("meta.gems"), "il reste 180 minutes à payer : 60 Gemmes");
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(60), "un refus n'entame pas l'accélérateur");
            _app.Session.Wallet.SetGems(60);
            Assert.That(_app.Operations.SpeedUp(0).IsSuccess);
            Assert.That(_app.Session.Meta.speedUpTokens, Is.EqualTo(0));
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(0));
        }
    }
}
