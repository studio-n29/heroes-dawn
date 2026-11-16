using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Économie des doublons : exemplaires nécessaires pour monter une carte, praticable avec 250 cartes.</summary>
    public sealed class CardEconomyTests
    {
        [Test, Description("Vérifie que les exemplaires nécessaires aux niveaux 1 à 9 sont 1,1,1,2,2,2,3,3,3 (18 au total pour le niveau 10) — parce que l'ancienne table (25) était impraticable avec 250 cartes")]
        public void CopiesToUpgrade_Table()
        {
            var table = new int[9]; int total = 0;
            for (int l = 1; l <= 9; l++) { table[l - 1] = CardLevels.CopiesToUpgrade(l); total += table[l - 1]; }
            Assert.That(table, Is.EqualTo(new[] { 1, 1, 1, 2, 2, 2, 3, 3, 3 }));
            Assert.That(total, Is.EqualTo(18));
        }

        [Test, Description("Vérifie que trois doublons d'autres cartes de la même étoile valent un exemplaire — parce que c'est le taux de conversion de l'économie des doublons")]
        public void OtherCardsPerMissingCopy_IsThree()
        {
            Assert.That(CardLevels.OthersPerCopy, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que les Crédits d'amélioration restent 200 × niveau — parce que seule la monnaie des doublons change")]
        public void CreditsToUpgrade_Unchanged()
        {
            for (int l = 1; l <= 9; l++) Assert.That(CardLevels.CreditsToUpgrade(l), Is.EqualTo(200 * l));
        }
    }
}
