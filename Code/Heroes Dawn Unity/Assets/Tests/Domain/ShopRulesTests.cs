using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Shop;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    public sealed class ShopRulesTests
    {
        static readonly ShopWeights Weights = new ShopWeights(70, 25, 5);

        /// <summary>Contenu fictif : n recrues par rareté (stats et compétences d'une recrue du prototype).</summary>
        static List<MemberDef> Fake(int agents, int elites, int legends)
        {
            var proto = TestContent.Build().Members[0];
            var list = new List<MemberDef>();
            void Add(Rarity r, int n)
            {
                for (int i = 0; i < n; i++)
                    list.Add(new MemberDef(new MemberId(r + "_" + i), "k", proto.Family, proto.Role, r, proto.BaseStats, proto.Skills));
            }
            Add(Rarity.Agent, agents);
            Add(Rarity.Elite, elites);
            Add(Rarity.Legend, legends);
            return list;
        }

        static List<MemberId> DrawWith(ulong seed, IReadOnlyList<MemberDef> eligible, int slots = 3)
            => ShopDraw.Draw(new Pcg32(seed), eligible, Weights, slots);

        [Test, Description("Vérifie qu'à 8h59 on est encore sur la boutique de la veille — parce que la boutique se renouvelle à 9h00 heure locale (spec §3)")]
        public void Day_At0859_IsPreviousDay()
            => Assert.That(ShopDay.Of(new DateTime(2026, 10, 3, 8, 59, 0), 9), Is.EqualTo("2026-10-02"));

        [Test, Description("Vérifie qu'à 9h00 pile commence la boutique du jour — parce que 9h00 est l'heure du renouvellement (spec §3)")]
        public void Day_At0900_IsSameDay()
            => Assert.That(ShopDay.Of(new DateTime(2026, 10, 3, 9, 0, 0), 9), Is.EqualTo("2026-10-03"));

        [Test, Description("Vérifie que le jour du passage à l'heure d'été le renouvellement reste à 9h00 heure murale — parce que le calcul se fait sur l'heure locale affichée (spec §3)")]
        public void Day_DstSpringForward_ResetStaysAt9()
        {
            Assert.That(ShopDay.Of(new DateTime(2026, 3, 29, 8, 59, 0), 9), Is.EqualTo("2026-03-28"));
            Assert.That(ShopDay.Of(new DateTime(2026, 3, 29, 9, 0, 0), 9), Is.EqualTo("2026-03-29"));
        }

        [Test, Description("Vérifie que le prochain renouvellement est aujourd'hui 9h00 avant 9h, demain 9h00 après — parce qu'il alimente le compte à rebours (spec §5)")]
        public void NextReset_Before9_IsToday_After9_IsTomorrow()
        {
            Assert.That(ShopDay.NextReset(new DateTime(2026, 10, 3, 8, 0, 0), 9), Is.EqualTo(new DateTime(2026, 10, 3, 9, 0, 0)));
            Assert.That(ShopDay.NextReset(new DateTime(2026, 10, 3, 10, 0, 0), 9), Is.EqualTo(new DateTime(2026, 10, 4, 9, 0, 0)));
            Assert.That(ShopDay.NextReset(new DateTime(2026, 10, 3, 9, 0, 0), 9), Is.EqualTo(new DateTime(2026, 10, 4, 9, 0, 0)));
        }

        [Test, Description("Vérifie que la graine est identique pour les mêmes joueur, jour et tirage, et change avec le jour ou le tirage — parce qu'un serveur doit pouvoir refaire le tirage (approche A)")]
        public void Seed_SameInputs_Same_DifferentDayOrDraw_Different()
        {
            var s = ShopSeed.For("p1", "2026-10-03", 0);
            Assert.That(ShopSeed.For("p1", "2026-10-03", 0), Is.EqualTo(s));
            Assert.That(ShopSeed.For("p1", "2026-10-04", 0), Is.Not.EqualTo(s));
            Assert.That(ShopSeed.For("p1", "2026-10-03", 1), Is.Not.EqualTo(s));
            Assert.That(ShopSeed.For("p2", "2026-10-03", 0), Is.Not.EqualTo(s));
        }

        [Test, Description("Vérifie que deux tirages de même graine donnent les mêmes offres — parce que relancer le jeu ou vérifier côté serveur doit redonner la même boutique")]
        public void Draw_SameSeed_SameOffers()
        {
            var c = Fake(5, 5, 5);
            Assert.That(DrawWith(42, c), Is.EqualTo(DrawWith(42, c)));
        }

        [Test, Description("Vérifie qu'un tirage ne propose jamais deux fois le même héros ni un héros déjà possédé — parce que le GDD exclut les doublons")]
        public void Draw_NoDuplicates_NeverOwned()
        {
            var c = Fake(5, 5, 5);
            var owned = new HashSet<MemberId> { c[0].Id, c[6].Id };
            var eligible = ShopDraw.Eligible(c, owned.Contains, _ => true);
            for (ulong seed = 1; seed <= 1000; seed++)
            {
                var offers = DrawWith(seed, eligible);
                Assert.That(offers.Distinct().Count(), Is.EqualTo(offers.Count));
                Assert.That(offers.Any(owned.Contains), Is.False);
            }
        }

        [Test, Description("Vérifie qu'une rareté sans héros éligible n'est jamais tirée et que les 3 emplacements restent remplis — parce que son poids est réparti sur les autres raretés (spec §3)")]
        public void Draw_ExhaustedRarity_WeightsRenormalized()
        {
            var c = Fake(5, 5, 0);
            for (ulong seed = 1; seed <= 200; seed++)
            {
                var offers = DrawWith(seed, c);
                Assert.That(offers.Count, Is.EqualTo(3));
                Assert.That(offers.All(id => c.First(m => m.Id == id).Rarity != Rarity.Legend));
            }
        }

        [Test, Description("Vérifie que Verre n'entre dans la boutique qu'après la validation de la zone 1.1 — parce que les Légendes sont liées à l'histoire (GDD §Recrutement)")]
        public void Eligible_UnlockZone_RequiresCompletion()
        {
            var members = TestContent.Build().Members;
            var verre = new MemberId("verre");
            Assert.That(ShopDraw.Eligible(members, _ => false, _ => false).Any(m => m.Id == verre), Is.False);
            Assert.That(ShopDraw.Eligible(members, _ => false, z => z == PrototypeContent.Zone11).Any(m => m.Id == verre), Is.True);
        }

        [Test, Description("Vérifie qu'avec 2 héros éligibles la boutique n'a que 2 offres — parce qu'on ne propose pas de doublon pour compléter")]
        public void Draw_FewerThanThreeEligible_ReturnsFewer()
            => Assert.That(DrawWith(7, Fake(1, 1, 0)).Count, Is.EqualTo(2));

        [Test, Description("Vérifie qu'aucun héros éligible donne une boutique vide — parce que la collection de la ville est alors complète")]
        public void Draw_NoEligible_ReturnsEmpty()
            => Assert.That(DrawWith(7, new List<MemberDef>()), Is.Empty);

        [Test, Description("Vérifie que sur 10 000 tirages les raretés tombent à ±2 points de 70 / 25 / 5 — parce que ce sont les taux d'apparition validés")]
        public void Draw_Distribution_Matches70_25_5()
        {
            var c = Fake(5, 5, 5);
            var rng = new Pcg32(2026);
            var counts = new Dictionary<Rarity, int> { [Rarity.Agent] = 0, [Rarity.Elite] = 0, [Rarity.Legend] = 0 };
            const int n = 10000;
            for (int i = 0; i < n; i++)
            {
                var id = ShopDraw.Draw(rng, c, Weights, 1)[0];
                counts[c.First(m => m.Id == id).Rarity]++;
            }
            Assert.That(counts[Rarity.Agent] * 100.0 / n, Is.EqualTo(70).Within(2));
            Assert.That(counts[Rarity.Elite] * 100.0 / n, Is.EqualTo(25).Within(2));
            Assert.That(counts[Rarity.Legend] * 100.0 / n, Is.EqualTo(5).Within(2));
        }

        [Test, Description("Vérifie que le renouvellement est gratuit la première fois puis coûte 100, 200, 400 et 800 Crédits au plus — parce que ce sont les prix validés (spec §3)")]
        public void RerollCost_FreeThen100_200_400_800_Capped()
        {
            var b = new BalanceConfig();
            Assert.That(Enumerable.Range(0, 6).Select(i => ShopRules.RerollCost(i, b)), Is.EqualTo(new[] { 0, 100, 200, 400, 800, 800 }));
        }
    }
}
