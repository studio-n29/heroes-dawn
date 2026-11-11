using System;
using System.Linq;
using System.Threading;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class PassTrackTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code(N29.HeroesDawn.Core.Result r) => r.Error?.Code ?? "ok";
        static string Code<T>(N29.HeroesDawn.Core.Result<T> r) => r.Error?.Code ?? "ok";
        N29.HeroesDawn.Core.Result BuyPremium() => _app.Pass.BuyPremiumAsync(CancellationToken.None).GetAwaiter().GetResult();

        [Test, Description("Vérifie que le pass compte 50 paliers de 1 300 points sur 42 jours, avec une récompense écrite pour chaque palier des deux pistes — parce que le barème est celui de la spec et que le fichier de données doit le couvrir en entier")]
        public void Le_pass_a_50_paliers_de_1300_XP()
        {
            Assert.That((PassTrack.Levels, PassTrack.XpPerLevel, PassTrack.SeasonDays), Is.EqualTo((50, 1300, 42)));
            Assert.That(PassTrack.PremiumProduct, Is.EqualTo("pass_premium"));
            Assert.That(TestApp.TestPassRewards.Free.Count, Is.EqualTo(50));
            Assert.That(TestApp.TestPassRewards.Premium.Count, Is.EqualTo(50));
            for (int n = 1; n <= 50; n++)
            {
                Assert.That(_app.Pass.Free(n).IsEmpty, Is.False, "piste gratuite, palier " + n);
                Assert.That(_app.Pass.Premium(n).IsEmpty, Is.False, "piste premium, palier " + n);
            }
        }

        [Test, Description("Vérifie que le niveau se calcule à partir des points (0 → 0, 1 299 → 0, 1 300 → 1, 65 000 → 50, 70 000 → 50) — parce que le sommet du pass est plafonné")]
        public void Le_niveau_se_calcule_a_partir_de_l_XP()
        {
            Assert.That(new[] { 0, 1299, 1300, 65000, 70000 }.Select(PassTrack.LevelOf), Is.EqualTo(new[] { 0, 0, 1, 50, 50 }));
            _app.Pass.AddXp(1_000_000);
            Assert.That(_app.Pass.Xp, Is.EqualTo(65000), "les points s'arrêtent au sommet");
            Assert.That(_app.Pass.IsMax);
        }

        [Test, Description("Vérifie que les totaux des pistes respectent la spec (gratuit : 540 à 660 PR, aucune tenue ; premium : 810 à 990 PR, 300 Gemmes, 3 tenues et 1 thème de QG) — parce que ces totaux fixent la valeur du pass")]
        public void Les_totaux_des_pistes_respectent_la_spec()
        {
            var free = TestApp.TestPassRewards.Free;
            var premium = TestApp.TestPassRewards.Premium;
            Assert.That(free.Sum(d => d.Pr), Is.InRange(540, 660));
            Assert.That(premium.Sum(d => d.Pr), Is.InRange(810, 990));
            Assert.That(premium.Sum(d => d.Gems), Is.EqualTo(300));
            Assert.That(free.Sum(d => d.Gems), Is.EqualTo(0), "la piste gratuite ne donne pas de Gemmes");
            Assert.That(premium.SelectMany(d => d.Cosmetics).Count(c => c.StartsWith("outfit.")), Is.EqualTo(3));
            Assert.That(premium.SelectMany(d => d.Cosmetics).Count(c => c.StartsWith("hq.")), Is.EqualTo(1));
            Assert.That(free.SelectMany(d => d.Cosmetics).Count(c => c.StartsWith("outfit.")), Is.EqualTo(0));
            Assert.That(free.SelectMany(d => d.Cosmetics).Count(c => c.StartsWith("dice.") || c.StartsWith("pawn.")), Is.EqualTo(2));
            Assert.That(free.SelectMany(d => d.Components).Select(c => c.Grade).Distinct().OrderBy(g => g), Is.EqualTo(new[] { ComponentGrade.Common, ComponentGrade.Refined }));
            Assert.That(premium.SelectMany(d => d.Components).All(c => c.Grade == ComponentGrade.Refined), "la piste premium donne du Raffiné");
        }

        [Test, Description("Vérifie que le premium s'achète en argent réel par le produit pass_premium, une seule fois par saison, le second achat étant refusé sans appeler le store — parce qu'un joueur ne doit jamais payer deux fois la même saison")]
        public void Le_premium_s_achete_en_argent_reel_et_une_fois_par_saison()
        {
            _app.Pass.AddXp(1300);
            Assert.That(Code(_app.Pass.Claim(1, true)), Is.EqualTo("meta.premium"));
            int gems = _app.Session.Wallet.Gems;
            Assert.That(BuyPremium().IsSuccess);
            Assert.That(_app.Pass.PremiumOwned);
            Assert.That(_app.Session.Wallet.Gems, Is.EqualTo(gems), "aucune Gemme n'est dépensée");
            Assert.That(_app.Purchases.Calls, Is.EqualTo(new[] { "pass_premium" }));
            Assert.That(Code(BuyPremium()), Is.EqualTo("meta.done"));
            Assert.That(_app.Purchases.Calls.Count, Is.EqualTo(1), "le store n'a pas été sollicité");
            Assert.That(_app.Pass.Claim(1, true).IsSuccess);
        }

        [Test, Description("Vérifie qu'un paiement refusé laisse le pass gratuit — parce qu'on n'active jamais le premium sans paiement")]
        public void Un_paiement_refuse_n_active_pas_le_premium()
        {
            _app.Purchases.Fail = true;
            Assert.That(Code(BuyPremium()), Is.EqualTo("meta.payment"));
            Assert.That(_app.Pass.PremiumOwned, Is.False);
        }

        [Test, Description("Vérifie que la saison suivante le premium est à racheter — parce que le pass premium n'est valable que pour la saison en cours")]
        public void La_saison_suivante_le_premium_est_a_racheter()
        {
            BuyPremium();
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(PassTrack.SeasonDays);
            _app.Pass.EnsureSeason();
            Assert.That(_app.Pass.PremiumOwned, Is.False);
            Assert.That(BuyPremium().IsSuccess);
            Assert.That(_app.Pass.PremiumOwned);
            Assert.That(_app.Purchases.Calls.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'une nouvelle saison remet les points et les paliers réclamés à zéro — parce que le pass est saisonnier")]
        public void Une_nouvelle_saison_remet_l_XP_et_les_paliers_reclames_a_zero()
        {
            _app.Pass.AddXp(5000);
            _app.Pass.Claim(1, false);
            Assert.That(_app.Session.Meta.passFreeClaimed, Is.Not.Empty);
            _app.Time.LocalNow = _app.Time.LocalNow.AddDays(PassTrack.SeasonDays);
            _app.Pass.EnsureSeason();
            Assert.That(_app.Pass.Xp, Is.EqualTo(0));
            Assert.That(_app.Session.Meta.passFreeClaimed, Is.Empty);
            Assert.That(_app.Session.Meta.passPremiumClaimed, Is.Empty);
        }

        [Test, Description("Vérifie que les saisons de 42 jours s'enchaînent depuis la date de référence, sans trou ni chevauchement, et que le temps restant décroît jusqu'à la fin — parce que l'index de saison est la seule horloge du pass")]
        public void Les_saisons_s_enchainent_tous_les_42_jours()
        {
            var start = PassTrack.Epoch;
            Assert.That(PassTrack.SeasonIndex(start), Is.EqualTo(0));
            Assert.That(PassTrack.SeasonIndex(start.AddDays(41)), Is.EqualTo(0));
            Assert.That(PassTrack.SeasonIndex(start.AddDays(42)), Is.EqualTo(1));
            Assert.That(PassTrack.SeasonIndex(start.AddDays(-1)), Is.EqualTo(-1));
            _app.Time.LocalNow = start.AddDays(41).AddHours(12);
            Assert.That(_app.Pass.TimeLeft(), Is.EqualTo(TimeSpan.FromHours(12)));
        }

        [Test, Description("Vérifie que réclamer un palier verse aussi les Composants, les accélérateurs et les cosmétiques du lot — parce que le pass ne donne pas que des Crédits")]
        public void Reclamer_un_palier_verse_composants_accelerateurs_et_cosmetiques()
        {
            _app.Pass.AddXp(PassTrack.MaxXp);
            _app.Pass.GrantPremium();
            var all = _app.Pass.ClaimAll();
            Assert.That(all.IsSuccess);
            var m = _app.Session.Meta;
            Assert.That(m.speedUpTokens, Is.EqualTo(TestApp.TestPassRewards.Premium.Sum(d => d.SpeedUpMinutes)));
            Assert.That(m.cosmetics, Is.SupersetOf(new[] { "outfit.pass_vanguard", "outfit.pass_nightshift", "outfit.pass_dawn", "hq.pass_dawn", "dice.pass_amber", "pawn.pass_scout" }));
            int refined = m.components.Where(c => c.id.EndsWith(".Refined")).Sum(c => c.value);
            Assert.That(refined, Is.EqualTo(TestApp.TestPassRewards.Free.Concat(TestApp.TestPassRewards.Premium).SelectMany(d => d.Components).Where(c => c.Grade == ComponentGrade.Refined).Sum(c => c.Count)));
            Assert.That(Code(_app.Pass.ClaimAll()), Is.EqualTo("meta.not_ready"), "tout est réclamé");
        }

        [Test, Description("Vérifie qu'aucun palier des deux pistes ne donne de recette, de duel ni de point de tournoi — parce que le pass ne vend aucun avantage de compétition")]
        public void Aucun_palier_ne_donne_de_recette_ni_de_duel()
        {
            _app.Pass.AddXp(PassTrack.MaxXp);
            _app.Pass.GrantPremium();
            int recipes = _app.Session.Meta.recipes.Count;
            _app.Pass.ClaimAll();
            var m = _app.Session.Meta;
            Assert.That(m.recipes.Count, Is.EqualTo(recipes));
            Assert.That(m.arena.points, Is.EqualTo(0));
            Assert.That(m.arena.duelsToday, Is.EqualTo(0));
            Assert.That(m.cosmetics.Any(c => c.StartsWith("cosmetic:")), Is.False, "aucun cosmétique de tournoi");
            var cosmetics = TestApp.TestPassRewards.Free.Concat(TestApp.TestPassRewards.Premium).SelectMany(d => d.Cosmetics);
            Assert.That(cosmetics.Any(c => c.StartsWith("cosmetic:")), Is.False, "les cosmétiques de tournoi ne sont pas dans le pass");
        }

        [Test, Description("Vérifie qu'un point de pass est donné pour une fabrication (20), un boss de repaire (150) et un duel (30 gagné, 10 perdu) — parce que ces sources alimentent le pass")]
        public void Les_nouvelles_sources_donnent_les_points_de_la_spec()
        {
            Assert.That((PassXp.Craft, PassXp.Lair, PassXp.Duel(true), PassXp.Duel(false)), Is.EqualTo((20, 150, 30, 10)));
        }
    }
}
