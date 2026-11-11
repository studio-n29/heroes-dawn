using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Points, plafonds et paliers de l'arène — bloc D de la 1.0.0.</summary>
    public sealed class ArenaRulesTests
    {
        [Test, Description("Vérifie qu'une victoire rapporte 10 points hors ligne et 20 en ligne — parce que le JcJ en ligne est plus risqué")]
        public void Victoire_hors_ligne_donne_10_en_ligne_20()
        {
            Assert.That(ArenaRules.PointsDelta(DuelMode.Offline, DuelOutcome.Win, 0), Is.EqualTo(10));
            Assert.That(ArenaRules.PointsDelta(DuelMode.Online, DuelOutcome.Win, 0), Is.EqualTo(20));
        }

        [Test, Description("Vérifie qu'une défaite ne retire rien — parce qu'on ne perd jamais de points en perdant")]
        public void Defaite_ne_retire_rien()
        {
            Assert.That(ArenaRules.PointsDelta(DuelMode.Offline, DuelOutcome.Loss, 40), Is.EqualTo(0));
            Assert.That(ArenaRules.PointsDelta(DuelMode.Online, DuelOutcome.Loss, 40), Is.EqualTo(0));
        }

        [Test, Description("Vérifie que quitter en ligne retire 10 points sans passer sous 0 — parce que le score ne peut pas être négatif")]
        public void Quitter_en_ligne_retire_10_sans_descendre_sous_0()
        {
            Assert.That(ArenaRules.PointsDelta(DuelMode.Online, DuelOutcome.Quit, 4), Is.EqualTo(-4));
            Assert.That(ArenaRules.PointsDelta(DuelMode.Online, DuelOutcome.Quit, 25), Is.EqualTo(-10));
            Assert.That(ArenaRules.PointsDelta(DuelMode.Online, DuelOutcome.Quit, 0), Is.EqualTo(0));
        }

        [Test, Description("Vérifie que quitter hors ligne ne change rien et ne compte pas comme un duel — parce que le duel hors ligne n'engage personne")]
        public void Quitter_hors_ligne_ne_change_rien_et_ne_compte_pas()
        {
            Assert.That(ArenaRules.PointsDelta(DuelMode.Offline, DuelOutcome.Quit, 30), Is.EqualTo(0));
            Assert.That(ArenaRules.CountsAsDuel(DuelMode.Offline, DuelOutcome.Quit), Is.False);
            Assert.That(ArenaRules.CountsAsDuel(DuelMode.Online, DuelOutcome.Quit), Is.True);
        }

        [Test, Description("Vérifie qu'un duel annulé ne change rien et ne compte pas — parce qu'une désynchronisation n'est la faute de personne")]
        public void Annule_ne_change_rien_et_ne_compte_pas()
        {
            foreach (var mode in new[] { DuelMode.Offline, DuelMode.Online })
            {
                Assert.That(ArenaRules.PointsDelta(mode, DuelOutcome.Cancelled, 30), Is.EqualTo(0));
                Assert.That(ArenaRules.CountsAsDuel(mode, DuelOutcome.Cancelled), Is.False);
            }
        }

        [Test, Description("Vérifie que le 11e duel du jour est refusé — parce que le plafond est de 10 duels par jour")]
        public void Le_11e_duel_du_jour_est_refuse()
        {
            Assert.That(ArenaRules.CanStartDuel(9), Is.True);
            Assert.That(ArenaRules.CanStartDuel(10), Is.False);
        }

        [Test, Description("Vérifie que les paliers sont à 50, 150, 300, 500 et 800 points — parce que la spec les fixe")]
        public void Les_paliers_sont_50_150_300_500_800()
        {
            Assert.That(ArenaRules.TierPoints, Is.EqualTo(new[] { 50, 150, 300, 500, 800 }));
            Assert.That(ArenaRules.TierReached(49), Is.EqualTo(0));
            Assert.That(ArenaRules.TierReached(50), Is.EqualTo(1));
            Assert.That(ArenaRules.TierReached(299), Is.EqualTo(2));
            Assert.That(ArenaRules.TierReached(800), Is.EqualTo(5));
            Assert.That(ArenaRules.TierReached(5000), Is.EqualTo(5));
        }
    }
}
