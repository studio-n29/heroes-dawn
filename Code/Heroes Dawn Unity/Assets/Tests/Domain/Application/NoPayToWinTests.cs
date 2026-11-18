using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>
    /// Garde anti pay-to-win (bloc E, spec §Principe) : l'argent réel et les Gemmes achètent du temps, du confort et des cosmétiques, jamais de la puissance ni un avantage de compétition.
    /// </summary>
    public sealed class NoPayToWinTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        /// <summary>Achète tout ce qui s'achète : packs, offre de bienvenue, pass premium, échanges, emplacements, cosmétiques de la semaine.</summary>
        void BuyEverything()
        {
            foreach (var p in Boutique.Packs.Concat(new[] { Boutique.Welcome }))
                Assert.That(_app.Boutique.BuyPackAsync(p.Id, CancellationToken.None).GetAwaiter().GetResult().IsSuccess, p.Id);
            Assert.That(_app.Pass.BuyPremiumAsync(CancellationToken.None).GetAwaiter().GetResult().IsSuccess);
            _app.Session.Wallet.SetGems(1_000_000);
            foreach (var e in Boutique.Exchanges) _app.Boutique.Exchange(e.Id);
            _app.Boutique.BuyExpeditionSlot();
            _app.Boutique.BuyExpeditionSlot();
            foreach (var o in _app.Cosmetics.Weekly(_app.Time.LocalNow)) _app.Cosmetics.Buy(o.Def.Id);
        }

        [Test, Description("Vérifie qu'aucun pack, aucun échange de Gemmes ni aucun article de cosmétique ne donne de Composant, de catalyseur ou de recette — parce que la puissance de fabrication ne se vend pas")]
        public void Aucun_pack_ni_echange_ni_article_ne_donne_de_composant_catalyseur_ou_recette()
        {
            int recipes = _app.Session.Meta.recipes.Count;
            BuyEverything();
            var m = _app.Session.Meta;
            Assert.That(m.components.Sum(c => c.value), Is.EqualTo(0), "aucun Composant");
            Assert.That(m.catalysts.Sum(c => c.value), Is.EqualTo(0), "aucun catalyseur");
            Assert.That(m.recipes.Count, Is.EqualTo(recipes), "aucune recette");
            Assert.That(m.crafted, Is.Empty);
            // Les lots des packs et des échanges n'ont que des Crédits, des PR ou des Gemmes.
            foreach (var e in Boutique.Exchanges) Assert.That(e.Reward.PassXp, Is.EqualTo(0), e.Id);
        }

        [Test, Description("Vérifie qu'aucun produit payé en Gemmes ni en argent réel ne donne de PR, et que l'accélérateur et les emplacements ne donnent que du temps — parce que les PR achètent des recrues et des boosters aléatoires : ce serait de la puissance et un objet aléatoire payant")]
        public void Les_gemmes_n_achetent_ni_PR_ni_recrue_ni_booster()
        {
            Assert.That(Boutique.Exchanges.All(e => e.Reward.Pr == 0), "aucun échange de Gemmes ne donne de PR");
            Assert.That(Boutique.Exchanges.All(e => e.Reward.Gems == 0), "aucun échange ne donne de Gemmes");
            int prBefore = _app.Session.Wallet.Pr;
            int boostersBefore = _app.Collection.BoosterStock(N29.HeroesDawn.Definitions.BoosterDef.StandardId);
            _app.Session.Wallet.SetGems(1_000_000);
            _app.Boutique.BuyAccelerator();
            _app.Boutique.BuyExpeditionSlot();
            foreach (var e in Boutique.Exchanges) _app.Boutique.Exchange(e.Id);
            foreach (var o in _app.Cosmetics.Weekly(_app.Time.LocalNow)) _app.Cosmetics.Buy(o.Def.Id);
            Assert.That(_app.Session.Wallet.Pr, Is.EqualTo(prBefore), "aucun achat en Gemmes ne change les PR");
            Assert.That(_app.Collection.BoosterStock(N29.HeroesDawn.Definitions.BoosterDef.StandardId), Is.EqualTo(boostersBefore), "aucun booster n'est donné");
        }

        [Test, Description("Vérifie que les deux pistes du pass ne donnent ni recette ni point de tournoi, et que ses Composants sont limités aux qualités Commun et Raffiné — parce que seul le jeu donne l'Exceptionnel et les recettes")]
        public void Les_pistes_du_pass_ne_donnent_ni_recette_ni_composant_exceptionnel()
        {
            _app.Pass.AddXp(PassTrack.MaxXp);
            _app.Pass.GrantPremium();
            _app.Pass.ClaimAll();
            var m = _app.Session.Meta;
            Assert.That(m.recipes, Is.Empty);
            Assert.That(m.components.Any(c => c.id.EndsWith(".Exceptional") && c.value > 0), Is.False);
            var names = typeof(PassRewardDef).GetProperties().Select(p => p.Name.ToLowerInvariant()).ToList();
            Assert.That(names.Any(n => n.Contains("recipe") || n.Contains("duel") || n.Contains("point")), Is.False, "la définition d'un palier n'a aucun champ de recette, de duel ni de point");
        }

        [Test, Description("Vérifie qu'aucun achat ni aucun palier du pass ne donne de duel ni de point de tournoi — parce que la compétition ne s'achète pas")]
        public void Aucun_achat_ne_donne_de_duel_ni_de_point_de_tournoi()
        {
            _app.Pass.AddXp(PassTrack.MaxXp);
            BuyEverything();
            _app.Pass.ClaimAll();
            var a = _app.Session.Meta.arena;
            Assert.That((a.points, a.duelsToday, a.claimedTiers.Count, a.tournamentId), Is.EqualTo((0, 0, 0, "")));
        }

        [Test, Description("Vérifie que le pass premium ne change ni les recrues proposées par la boutique du jour ni le pool de recrutement, et qu'aucun palier ne donne d'agent — parce qu'aucune recrue n'est exclusive au premium")]
        public void Aucune_recrue_n_est_exclusive_au_premium()
        {
            var other = new TestApp();
            other.SignInDirect("p1");
            _app.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
            other.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
            other.Pass.BuyPremiumAsync(CancellationToken.None).GetAwaiter().GetResult();
            other.Shop.Reroll.ExecuteAsync().GetAwaiter().GetResult();
            _app.Shop.Reroll.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(other.Session.Shop.Offers.Select(o => o.Id.Value), Is.EqualTo(_app.Session.Shop.Offers.Select(o => o.Id.Value)));

            var cosmetics = TestApp.TestPassRewards.Premium.SelectMany(d => d.Cosmetics).ToList();
            var members = _app.Session.Content.Members.Select(x => x.Id.Value).ToList();
            Assert.That(cosmetics.Any(c => members.Any(id => c == id)), Is.False, "un palier ne donne pas d'agent");
            Assert.That(typeof(PassRewardDef).GetProperties().Any(p => p.Name.Contains("Member") || p.Name.Contains("Agent") || p.Name.Contains("Recruit")), Is.False);
        }

        [Test, Description("Vérifie que la puissance de combat (UnitFactory et le domaine de combat) ne dépend d'aucun type de cosmétique — parce qu'un cosmétique est purement visuel")]
        public void Aucun_cosmetique_ne_modifie_une_statistique()
        {
            var combat = typeof(N29.HeroesDawn.Domain.Combat.UnitFactory).Assembly;
            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            foreach (var t in combat.GetTypes())
            {
                Assert.That(t.Name, Does.Not.Contain("Cosmetic"), t.FullName);
                foreach (var f in t.GetFields(all)) Assert.That(f.Name + f.FieldType.Name, Does.Not.Contain("Cosmetic"), t.FullName);
                foreach (var p in t.GetProperties(all)) Assert.That(p.Name + p.PropertyType.Name, Does.Not.Contain("Cosmetic"), t.FullName);
                foreach (var mth in t.GetMethods(all))
                    foreach (var prm in mth.GetParameters()) Assert.That(prm.Name + prm.ParameterType.Name, Does.Not.Contain("Cosmetic"), t.FullName + "." + mth.Name);
            }
            // Un profil qui possède tous les cosmétiques garde exactement les mêmes unités.
            var before = _app.Session.Roster.Select(m => (m.Id.Value, m.Level)).ToList();
            foreach (var d in TestApp.TestCosmetics) CosmeticOps.Unlock(_app.Session.Meta, d.Id);
            Assert.That(_app.Session.Roster.Select(m => (m.Id.Value, m.Level)).ToList(), Is.EqualTo(before));
        }
    }
}
