using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Tirage de la qualité et des statistiques d'un objet fabriqué, progression de la compétence de création.</summary>
    public sealed class CraftingRulesTests
    {
        static int Stat(CraftedStats s, RecipeStatKind k) => k == RecipeStatKind.Hp ? s.Hp : k == RecipeStatKind.Atk ? s.Atk : k == RecipeStatKind.Def ? s.Def : s.Spd;

        [Test, Description("Vérifie que les cinq probabilités de qualité font toujours 100 % — parce qu'un tirage doit toujours tomber sur un palier")]
        public void Odds_SumTo100_ForEveryEdge()
        {
            for (int skill = 1; skill <= 10; skill++)
                for (int difficulty = 1; difficulty <= 10; difficulty++)
                {
                    var odds = CraftingRules.QualityOdds(skill, difficulty);
                    Assert.That(odds.Count, Is.EqualTo(5));
                    Assert.That(odds.Sum(), Is.EqualTo(100), $"compétence {skill}, difficulté {difficulty}");
                }
        }

        [Test, Description("Vérifie que les probabilités reprennent la table de la spec pour chaque tranche d'écart compétence − difficulté — parce que l'équilibrage de la fabrication repose sur cette table")]
        public void Odds_ReadFromTheSpecTable()
        {
            Assert.That(CraftingRules.QualityOdds(1, 7), Is.EqualTo(new[] { 60, 35, 5, 0, 0 }), "écart −6");
            Assert.That(CraftingRules.QualityOdds(3, 5), Is.EqualTo(new[] { 35, 50, 13, 2, 0 }), "écart −2");
            Assert.That(CraftingRules.QualityOdds(4, 4), Is.EqualTo(new[] { 15, 50, 25, 9, 1 }), "écart 0");
            Assert.That(CraftingRules.QualityOdds(5, 4), Is.EqualTo(new[] { 5, 35, 35, 20, 5 }), "écart +1");
            Assert.That(CraftingRules.QualityOdds(8, 4), Is.EqualTo(new[] { 0, 15, 40, 30, 15 }), "écart +4");
            Assert.That(CraftingRules.QualityOdds(9, 4), Is.EqualTo(new[] { 0, 5, 30, 40, 25 }), "écart +5");
        }

        [Test, Description("Vérifie que monter en compétence ne baisse jamais la chance d'obtenir Soigné ou mieux — parce que progresser doit toujours améliorer les résultats")]
        public void Odds_NeverHurtHigherTiers_AsSkillRises()
        {
            for (int difficulty = 1; difficulty <= 10; difficulty++)
            {
                int previous = -1;
                for (int skill = 1; skill <= 10; skill++)
                {
                    int fineOrBetter = CraftingRules.QualityOdds(skill, difficulty).Skip(2).Sum();
                    Assert.That(fineOrBetter, Is.GreaterThanOrEqualTo(previous), $"difficulté {difficulty}, compétence {skill}");
                    previous = fineOrBetter;
                }
            }
        }

        [Test, Description("Vérifie que le tirage de qualité suit les probabilités (écart 0 : Chef-d'œuvre possible ; écart −3 : jamais Supérieur ni Chef-d'œuvre) — parce que le hasard doit respecter la table")]
        public void RollQuality_FollowsTheOdds()
        {
            var rng = new Pcg32(7);
            var counts = new int[5];
            const int n = 4000;
            for (int i = 0; i < n; i++) counts[(int)CraftingRules.RollQuality(rng, 4, 4)]++;
            var expected = new[] { 15, 50, 25, 9, 1 };
            for (int q = 0; q < 5; q++) Assert.That(counts[q] * 100.0 / n, Is.EqualTo(expected[q]).Within(3), "palier " + q);
            Assert.That(counts[4], Is.GreaterThan(0), "le Chef-d'œuvre existe");

            for (int i = 0; i < 2000; i++)
            {
                var q = CraftingRules.RollQuality(rng, 1, 4);
                Assert.That(q, Is.LessThan(CraftQuality.Superior));
            }
        }

        [Test, Description("Vérifie que chaque stat tirée reste dans sa fourchette multipliée par la qualité (au moins 1) et que les stats absentes de la recette valent 0 — parce que les objets fabriqués ne doivent pas déborder de la fourchette annoncée")]
        public void RollStats_StayInRangeTimesQuality()
        {
            var rng = new Pcg32(11);
            foreach (var recipe in Recipes.All)
                foreach (CraftQuality quality in System.Enum.GetValues(typeof(CraftQuality)))
                {
                    int p = CraftQualities.Percent(quality);
                    for (int i = 0; i < 200; i++)
                    {
                        var stats = CraftingRules.RollStats(rng, recipe, quality);
                        foreach (RecipeStatKind k in System.Enum.GetValues(typeof(RecipeStatKind)))
                        {
                            var def = recipe.Stats.Where(s => s.Stat == k).Cast<RecipeStat?>().FirstOrDefault();
                            int value = Stat(stats, k);
                            if (def == null) { Assert.That(value, Is.EqualTo(0), recipe.Id + " " + k); continue; }
                            int lo = System.Math.Max(1, (def.Value.Min * p + 50) / 100), hi = System.Math.Max(1, (def.Value.Max * p + 50) / 100);
                            Assert.That(value, Is.InRange(lo, hi), recipe.Id + " " + k + " " + quality);
                        }
                    }
                }
        }

        [Test, Description("Vérifie les seuils d'expérience (niveau 2 à 40, niveau 3 à 120, niveau 10 à 1800, plafond 10) — parce que la compétence monte de 40 × n par niveau")]
        public void Skill_XpThresholds()
        {
            Assert.That(CraftingRules.SkillLevel(0), Is.EqualTo(1));
            Assert.That(CraftingRules.SkillLevel(39), Is.EqualTo(1));
            Assert.That(CraftingRules.SkillLevel(40), Is.EqualTo(2));
            Assert.That(CraftingRules.SkillLevel(120), Is.EqualTo(3));
            Assert.That(CraftingRules.SkillLevel(1_000_000), Is.EqualTo(CraftingRules.MaxSkill));
            Assert.That(CraftingRules.XpForLevel(10), Is.EqualTo(1800));
            Assert.That(CraftingRules.XpForLevel(1), Is.EqualTo(0));
        }

        [Test, Description("Vérifie le gain d'expérience (10 + 4 × difficulté tant que l'écart ne dépasse pas +2, sinon 5) — parce que fabriquer des recettes trop faciles ne doit presque plus rien rapporter")]
        public void XpGain_FullThenFlat()
        {
            Assert.That(CraftingRules.XpGain(1, 7), Is.EqualTo(38));
            Assert.That(CraftingRules.XpGain(3, 1), Is.EqualTo(14));
            Assert.That(CraftingRules.XpGain(5, 1), Is.EqualTo(5));
        }
    }
}
