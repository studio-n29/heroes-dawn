using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Scraps et recettes laissés par les combats gagnés.</summary>
    public sealed class CraftingDropTests
    {
        static readonly IReadOnlyList<string> Unknown = Recipes.All.Skip(4).Select(r => r.Id).ToList();

        static int Types(CraftDrops d) => d.Scraps.Take(4).Sum();
        static int Cores(CraftDrops d) => d.Scraps[(int)ScrapKind.Core];

        static IEnumerable<CraftDrops> Draws(CombatRank rank, bool boss, int n, IReadOnlyList<string>? unknown = null)
        {
            for (int i = 0; i < n; i++) yield return CraftingRules.RollDrops(new Pcg32(CollectionRules.Seed("p", "scrap", i)), rank, boss, unknown ?? Unknown);
        }

        [Test, Description("Vérifie que la quantité de scraps suit le rang du combat (Facile 1-2, Normal 2-3, Élite 4-6, Boss 4-6) et qu'un seul type reçoit les scraps — parce que les combats difficiles récompensent plus")]
        public void RollDrops_ScrapCountsFollowTheRank()
        {
            var expected = new[] { (CombatRank.Easy, false, 1, 2), (CombatRank.Normal, false, 2, 3), (CombatRank.Elite, false, 4, 6), (CombatRank.Boss, true, 4, 6) };
            foreach (var (rank, boss, min, max) in expected)
                foreach (var d in Draws(rank, boss, 300))
                {
                    Assert.That(Types(d), Is.InRange(min, max), rank.ToString());
                    Assert.That(d.Scraps.Take(4).Count(x => x > 0), Is.EqualTo(1), "un seul type par tirage");
                }
            var kinds = Draws(CombatRank.Normal, false, 300).Select(d => System.Array.FindIndex(d.Scraps.ToArray(), x => x > 0)).Distinct().ToList();
            Assert.That(kinds.Count, Is.EqualTo(4), "les quatre types sortent");
        }

        [Test, Description("Vérifie que les Noyaux ne tombent qu'en Élite (une fois sur deux) et en Boss (toujours 2) — parce que le Noyau est le scrap rare")]
        public void RollDrops_CoresOnlyForEliteAndBoss()
        {
            Assert.That(Draws(CombatRank.Easy, false, 300).All(d => Cores(d) == 0), Is.True);
            Assert.That(Draws(CombatRank.Normal, false, 300).All(d => Cores(d) == 0), Is.True);
            Assert.That(Draws(CombatRank.Boss, true, 300).All(d => Cores(d) == 2), Is.True);
            int withCore = Draws(CombatRank.Elite, false, 600).Count(d => Cores(d) == 1);
            Assert.That(withCore * 100.0 / 600, Is.EqualTo(50).Within(8));
            Assert.That(Draws(CombatRank.Elite, false, 600).All(d => Cores(d) <= 1), Is.True);
        }

        [Test, Description("Vérifie que les recettes tombent environ 25 % du temps en Élite et 60 % en Boss, jamais en Facile ou Normal, et qu'elles sont tirées parmi les inconnues — parce que les recettes se méritent sur les combats difficiles")]
        public void RollDrops_RecipeChances()
        {
            Assert.That(Draws(CombatRank.Easy, false, 300).All(d => d.NewRecipe == null), Is.True);
            Assert.That(Draws(CombatRank.Normal, false, 300).All(d => d.NewRecipe == null), Is.True);
            int elite = Draws(CombatRank.Elite, false, 800).Count(d => d.NewRecipe != null);
            int boss = Draws(CombatRank.Boss, true, 800).Count(d => d.NewRecipe != null);
            Assert.That(elite * 100.0 / 800, Is.EqualTo(25).Within(6));
            Assert.That(boss * 100.0 / 800, Is.EqualTo(60).Within(6));
            Assert.That(Draws(CombatRank.Boss, true, 300).Where(d => d.NewRecipe != null).All(d => Unknown.Contains(d.NewRecipe!)), Is.True);
        }

        [Test, Description("Vérifie que si toutes les recettes sont connues, aucune recette ne tombe et un tirage de recette réussi donne 5 scraps de plus du même type — parce qu'une recette en double doit rapporter quelque chose")]
        public void RollDrops_AllRecipesKnown_GivesFiveBonusScraps()
        {
            var none = new List<string>();
            var boss = Draws(CombatRank.Boss, true, 400, none).ToList();
            Assert.That(boss.All(d => d.NewRecipe == null), Is.True);
            Assert.That(boss.All(d => Types(d) is >= 4 and <= 6 or >= 9 and <= 11), Is.True);
            Assert.That(boss.Any(d => Types(d) >= 9), Is.True, "le bonus apparaît");
            Assert.That(boss.Any(d => Types(d) <= 6), Is.True);
            Assert.That(Draws(CombatRank.Easy, false, 300, none).All(d => Types(d) is >= 1 and <= 2), Is.True, "pas de tirage de recette en Facile");
        }

        [Test, Description("Vérifie qu'une même graine donne le même tirage — parce que les sauvegardes et les tests doivent rejouer le même résultat")]
        public void RollDrops_IsDeterministic()
        {
            var a = CraftingRules.RollDrops(new Pcg32(42), CombatRank.Boss, true, Unknown);
            var b = CraftingRules.RollDrops(new Pcg32(42), CombatRank.Boss, true, Unknown);
            Assert.That(a.Scraps, Is.EqualTo(b.Scraps));
            Assert.That(a.NewRecipe, Is.EqualTo(b.NewRecipe));
        }
    }
}
