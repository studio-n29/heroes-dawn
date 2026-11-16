#nullable enable
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Étoiles des cartes (1 à 5) et boosters — bloc A de la 1.0.0.</summary>
    public sealed class CardStarsTests
    {
        static readonly CollectionDef Defs = PrototypeCollection.Build();

        [Test, Description("Vérifie que les 60 cartes se répartissent en 24 ★1, 12 ★2, 12 ★3, 8 ★4 et 4 ★5 — parce que les 12 cartes ★1 de la première vague de P1 s ajoutent à la répartition validée de la spec du bloc A (12/12/12/8/4)")]
        public void La_repartition_des_60_cartes_est_24_12_12_8_4()
        {
            Assert.That(Defs.Cards.Count, Is.EqualTo(60));
            Assert.That(Enumerable.Range(1, 5).Select(s => Defs.Cards.Count(c => c.Stars == s)), Is.EqualTo(new[] { 24, 12, 12, 8, 4 }));
        }

        [Test, Description("Vérifie que des cartes témoins ont l'étoile de la table (card_monolith 5, card_inferno 4, card_shackle 3, card_borrowed_mask 2, card_ember 2, card_concrete_rampart 1) — parce que la table redistribue les anciennes raretés")]
        public void Chaque_carte_a_l_etoile_de_la_table()
        {
            int Of(string id) => Defs.Cards.Single(c => c.Id.Value == id).Stars;
            Assert.That(Of("card_monolith"), Is.EqualTo(5));
            Assert.That(Of("card_inferno"), Is.EqualTo(4));
            Assert.That(Of("card_shackle"), Is.EqualTo(3));
            Assert.That(Of("card_borrowed_mask"), Is.EqualTo(2));
            Assert.That(Of("card_ember"), Is.EqualTo(2));
            Assert.That(Of("card_concrete_rampart"), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que sur 10 000 tirages les étoiles tombent à ±2 points de 50 / 28 / 15 / 6 / 1 — parce que ce sont les taux validés du booster ordinaire")]
        public void RollStars_respecte_les_poids()
        {
            var rng = new Pcg32(2026);
            var counts = new int[6];
            const int n = 10000;
            for (int i = 0; i < n; i++) counts[CollectionRules.RollStars(rng, BoosterDef.StandardWeights)]++;
            var expected = new[] { 50, 28, 15, 6, 1 };
            for (int s = 1; s <= 5; s++) Assert.That(counts[s] * 100.0 / n, Is.EqualTo(expected[s - 1]).Within(2), "★" + s);
        }

        [Test, Description("Vérifie qu'une étoile absente du jeu retombe sur l'étoile inférieure — parce qu'un booster ne doit jamais échouer faute de carte ★5")]
        public void Une_etoile_absente_retombe_sur_l_etoile_inferieure()
        {
            var sans5 = new CollectionDef(Defs.Items, Defs.Cards.Where(c => c.Stars < 5).ToList(), new[] { new BoosterDef("t", "t", 3, new[] { 0, 0, 0, 0, 100 }, 0, 0) });
            var o = CollectionRules.OpenBooster(sans5, sans5.Booster("t"), 7UL, 0);
            Assert.That(o.Cards.Count, Is.EqualTo(3));
            Assert.That(o.Cards.All(id => sans5.Card(id).Stars == 4), Is.True, "★5 tiré mais absent : une carte ★4");
        }

        [Test, Description("Vérifie que la pitié garantit une carte ★3 ou mieux au bout de N boosters sans — parce que la malchance ne doit pas durer")]
        public void La_pitie_garantit_une_carte_3_etoiles_ou_mieux()
        {
            var faible = new BoosterDef("t", "t", 3, new[] { 100, 0, 0, 0, 0 }, 2, 0);
            var defs = new CollectionDef(Defs.Items, Defs.Cards, new[] { faible });
            var premier = CollectionRules.OpenBooster(defs, faible, 11UL, 0);
            Assert.That(premier.Cards.Any(id => defs.Card(id).Stars >= 3), Is.False);
            var second = CollectionRules.OpenBooster(defs, faible, 12UL, premier.Pity);
            Assert.That(second.Cards.Any(id => defs.Card(id).Stars >= 3), Is.True);
            Assert.That(second.Pity, Is.EqualTo(0));
        }
    }
}
