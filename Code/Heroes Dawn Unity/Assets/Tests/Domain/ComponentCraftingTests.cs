using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Atelier de fabrication (bloc B de la 1.0.0) : étoiles, statistiques avec catalyseur, Composants laissés par les combats.</summary>
    public sealed class ComponentCraftingTests
    {
        static readonly IReadOnlyList<string> Unknown = Recipes.All.Skip(4).Select(r => r.Id).ToList();

        static IEnumerable<CraftDrops> Draws(CombatRank rank, bool boss, int n, IReadOnlyList<string>? unknown = null)
        {
            for (int i = 0; i < n; i++) yield return CraftingRules.RollDrops(new Pcg32(CollectionRules.Seed("p", "components", i)), rank, boss, unknown ?? Unknown);
        }

        static int Count(CraftDrops d, ComponentGrade grade)
            => Enumerable.Range(0, ComponentKinds.Count).Sum(k => d.Components[ComponentKinds.Index((ComponentKind)k, grade)]);

        [Test, Description("Vérifie que l'étoile de départ vaut 2 en Commun, 4 en Raffiné et 5 en Exceptionnel — parce que la qualité des Composants engagés fixe le niveau de départ de l'objet")]
        public void L_etoile_de_depart_suit_la_qualite()
        {
            Assert.That(CraftStars.Base(ComponentGrade.Common), Is.EqualTo(2));
            Assert.That(CraftStars.Base(ComponentGrade.Refined), Is.EqualTo(4));
            Assert.That(CraftStars.Base(ComponentGrade.Exceptional), Is.EqualTo(5));
        }

        [Test, Description("Vérifie que 10 000 tirages restent dans 1–3 (Commun), 3–5 (Raffiné) et 4–6 (Exceptionnel), et que le 6 n'apparaît qu'avec de l'Exceptionnel — parce que l'étoile est bornée de 1 à 6 et que le sommet se mérite")]
        public void RollStars_reste_entre_1_et_6_et_ne_donne_6_qu_avec_Exceptional()
        {
            var ranges = new[] { (ComponentGrade.Common, 1, 3), (ComponentGrade.Refined, 3, 5), (ComponentGrade.Exceptional, 4, 6) };
            foreach (var (grade, min, max) in ranges)
            {
                var rng = new Pcg32(7);
                var seen = new HashSet<int>();
                for (int i = 0; i < 10_000; i++)
                {
                    int stars = CraftingRules.RollStars(rng, grade);
                    Assert.That(stars, Is.InRange(min, max), grade.ToString());
                    seen.Add(stars);
                }
                Assert.That(seen.Contains(min) && seen.Contains(max), Is.True, grade + " : les bornes sortent");
            }
        }

        [Test, Description("Vérifie que le tirage −1 / 0 / +1 se répartit 25 / 50 / 25 % à ±2 points sur 10 000 tirages en Raffiné — parce que c'est la loi annoncée au joueur")]
        public void RollStars_repartit_25_50_25()
        {
            var rng = new Pcg32(11);
            var counts = new int[7];
            for (int i = 0; i < 10_000; i++) counts[CraftingRules.RollStars(rng, ComponentGrade.Refined)]++;
            Assert.That(counts[3] / 100.0, Is.EqualTo(25).Within(2));
            Assert.That(counts[4] / 100.0, Is.EqualTo(50).Within(2));
            Assert.That(counts[5] / 100.0, Is.EqualTo(25).Within(2));
        }

        [Test, Description("Vérifie les multiplicateurs 80, 100, 115, 130, 150 et 175 % pour les étoiles 1 à 6 — parce que l'étoile multiplie les statistiques tirées")]
        public void Percent_donne_80_100_115_130_150_175()
            => Assert.That(Enumerable.Range(1, 6).Select(CraftStars.Percent), Is.EqualTo(new[] { 80, 100, 115, 130, 150, 175 }));

        [Test, Description("Vérifie que chaque statistique d'une recette Rare à l'étoile 3 reste dans [9, 16] (plage 8–14 × 115 %, arrondie) et que les absentes valent 0 — parce que l'objet ne doit pas déborder de la plage annoncée")]
        public void RollStats_sans_catalyseur_reste_dans_la_plage_multipliee()
        {
            var recipe = Recipes.Find("plated_coat")!;   // PV + DEF, 8–14
            var rng = new Pcg32(3);
            for (int i = 0; i < 500; i++)
            {
                var s = CraftingRules.RollStats(rng, recipe, 3, null);
                Assert.That(s.Hp, Is.InRange(9, 16));
                Assert.That(s.Def, Is.InRange(9, 16));
                Assert.That((s.Atk, s.Spd), Is.EqualTo((0, 0)));
            }
        }

        [Test, Description("Vérifie qu'un catalyseur ajoute la statistique orientée absente de la recette, sans jamais dépasser trois statistiques — parce que l'objet garde au plus trois statistiques")]
        public void RollStats_avec_catalyseur_ajoute_la_stat_orientee_sans_depasser_3()
        {
            var recipe = Recipes.Find("plated_coat")!;   // PV + DEF
            var rng = new Pcg32(5);
            for (int i = 0; i < 300; i++)
            {
                var s = CraftingRules.RollStats(rng, recipe, 2, CatalystKind.Energy);
                Assert.That(s.Atk, Is.InRange(8, 14), "plage de la première statistique de la recette (8–14), ×100 %");
                Assert.That(new[] { s.Hp, s.Atk, s.Def, s.Spd }.Count(v => v > 0), Is.EqualTo(3));
            }
            foreach (var r in Recipes.All)
                foreach (CatalystKind c in System.Enum.GetValues(typeof(CatalystKind)))
                {
                    var s = CraftingRules.RollStats(new Pcg32(1), r, 4, c);
                    Assert.That(new[] { s.Hp, s.Atk, s.Def, s.Spd }.Count(v => v > 0), Is.LessThanOrEqualTo(3), r.Id + " " + c);
                }
        }

        [Test, Description("Vérifie qu'une statistique déjà présente dans la recette est tirée dans la moitié haute de sa plage avec le catalyseur correspondant (ATQ 8–12, étoile 2 : 10 à 12) — parce que le catalyseur renforce ce que la recette donne déjà")]
        public void Une_stat_deja_presente_est_tiree_dans_la_moitie_haute()
        {
            var recipe = Recipes.Find("scrap_blade")!;   // ATQ 8–12
            var rng = new Pcg32(9);
            for (int i = 0; i < 500; i++)
            {
                var s = CraftingRules.RollStats(rng, recipe, 2, CatalystKind.Energy);
                Assert.That(s.Atk, Is.InRange(10, 12));
            }
        }

        [Test, Description("Vérifie la correspondance Métal → DEF, Énergie → ATQ, Biologique → PV, Cristal → VIT — parce que chaque catalyseur oriente une statistique")]
        public void Chaque_catalyseur_oriente_une_statistique()
        {
            Assert.That(CraftStars.Oriented(CatalystKind.Metal), Is.EqualTo(RecipeStatKind.Def));
            Assert.That(CraftStars.Oriented(CatalystKind.Energy), Is.EqualTo(RecipeStatKind.Atk));
            Assert.That(CraftStars.Oriented(CatalystKind.Bio), Is.EqualTo(RecipeStatKind.Hp));
            Assert.That(CraftStars.Oriented(CatalystKind.Crystal), Is.EqualTo(RecipeStatKind.Spd));
        }

        [Test, Description("Vérifie que les combats Facile et Normal ne donnent que 1–2 puis 2–3 Composants Commun d'un seul type, sans catalyseur — parce que les petits combats donnent la matière de base")]
        public void RollDrops_Facile_et_Normal_ne_donnent_que_du_Commun()
        {
            foreach (var (rank, min, max) in new[] { (CombatRank.Easy, 1, 2), (CombatRank.Normal, 2, 3) })
                foreach (var d in Draws(rank, false, 300))
                {
                    Assert.That(Count(d, ComponentGrade.Common), Is.InRange(min, max), rank.ToString());
                    Assert.That(Count(d, ComponentGrade.Refined) + Count(d, ComponentGrade.Exceptional), Is.EqualTo(0));
                    Assert.That(d.Components.Count(x => x > 0), Is.EqualTo(1), "un seul type et une seule qualité");
                    Assert.That(d.Catalyst, Is.Null);
                }
            var kinds = Draws(CombatRank.Normal, false, 300).Select(d => System.Array.FindIndex(d.Components.ToArray(), x => x > 0) / ComponentKinds.GradeCount).Distinct();
            Assert.That(kinds.Count(), Is.EqualTo(4), "les quatre types sortent");
        }

        [Test, Description("Vérifie que l'Élite donne 3–4 Commun et 1–2 Raffiné, jamais d'Exceptionnel, avec un catalyseur environ 10 % du temps — parce que les élites récompensent mieux")]
        public void RollDrops_Elite_donne_du_Raffine()
        {
            var draws = Draws(CombatRank.Elite, false, 1000).ToList();
            Assert.That(draws.All(d => Count(d, ComponentGrade.Common) is >= 3 and <= 9 && Count(d, ComponentGrade.Refined) is >= 1 and <= 2), Is.True);
            Assert.That(draws.All(d => Count(d, ComponentGrade.Exceptional) == 0), Is.True);
            Assert.That(draws.Count(d => d.Catalyst.HasValue) / 10.0, Is.EqualTo(10).Within(3));
        }

        [Test, Description("Vérifie que le Boss donne 3–4 Raffiné et un Exceptionnel une fois sur deux, avec un catalyseur environ 30 % du temps — parce que seul le Boss donne de l'Exceptionnel")]
        public void RollDrops_Boss_donne_du_Raffine_et_parfois_de_l_Exceptionnel()
        {
            var draws = Draws(CombatRank.Boss, true, 1000).ToList();
            Assert.That(draws.All(d => Count(d, ComponentGrade.Refined) is >= 3 and <= 4), Is.True);
            Assert.That(draws.Count(d => Count(d, ComponentGrade.Exceptional) == 1) / 10.0, Is.EqualTo(50).Within(6));
            Assert.That(draws.Count(d => d.Catalyst.HasValue) / 10.0, Is.EqualTo(30).Within(5));
        }

        [Test, Description("Vérifie que les recettes tombent environ 25 % du temps en Élite et 60 % en Boss, jamais en Facile ou Normal, parmi les inconnues — parce que les recettes se méritent sur les combats difficiles")]
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

        [Test, Description("Vérifie que si toutes les recettes sont connues, aucune ne tombe et un tirage de recette réussi donne 5 Composants Commun de plus — parce qu'une recette en double doit rapporter quelque chose")]
        public void RollDrops_AllRecipesKnown_GivesFiveBonusComponents()
        {
            var none = new List<string>();
            var boss = Draws(CombatRank.Boss, true, 400, none).ToList();
            Assert.That(boss.All(d => d.NewRecipe == null), Is.True);
            Assert.That(boss.All(d => Count(d, ComponentGrade.Common) is 0 or 5), Is.True);
            Assert.That(boss.Any(d => Count(d, ComponentGrade.Common) == 5), Is.True, "le bonus apparaît");
            Assert.That(Draws(CombatRank.Easy, false, 300, none).All(d => Count(d, ComponentGrade.Common) is >= 1 and <= 2), Is.True, "pas de tirage de recette en Facile");
        }

        [Test, Description("Vérifie qu'une même graine donne le même tirage — parce que les sauvegardes et les tests doivent rejouer le même résultat")]
        public void RollDrops_IsDeterministic()
        {
            var a = CraftingRules.RollDrops(new Pcg32(42), CombatRank.Boss, true, Unknown);
            var b = CraftingRules.RollDrops(new Pcg32(42), CombatRank.Boss, true, Unknown);
            Assert.That(a.Components, Is.EqualTo(b.Components));
            Assert.That((a.Catalyst, a.NewRecipe), Is.EqualTo((b.Catalyst, b.NewRecipe)));
        }

        [Test, Description("Vérifie que les 12 recettes coûtent 6 Commun, 8 Raffiné ou 10 Raffiné selon leur rareté, que les 4 Communes sont de départ, que 3 viennent du repaire de l AMIF et que les 5 autres viennent de l aventure — parce que les tournois répartiront les dernières au bloc D")]
        public void Les_12_recettes_gardent_leurs_couts()
        {
            Assert.That(Recipes.All.Count, Is.EqualTo(12));
            foreach (var r in Recipes.All)
            {
                var expected = r.Rarity == ItemRarity.Common ? (6, ComponentGrade.Common) : r.Rarity == ItemRarity.Rare ? (8, ComponentGrade.Refined) : (10, ComponentGrade.Refined);
                Assert.That((r.ComponentCost, r.MinGrade), Is.EqualTo(expected), r.Id);
            }
            Assert.That(Recipes.All.Count(r => r.Source == RecipeSource.Starter), Is.EqualTo(4));
            Assert.That(Recipes.All.Count(r => r.Source == RecipeSource.Adventure), Is.EqualTo(5));
            Assert.That(Recipes.All.Where(r => r.Source == RecipeSource.Lair).Select(r => r.Id), Is.EquivalentTo(new[] { "plated_coat", "grapple_rig", "aegis_plate" }));
        }

        [Test, Description("Vérifie que le nom d'un objet fabriqué se relit (recette et étoiles) et qu'une clé invalide est refusée — parce que la présentation en tire le nom affiché")]
        public void Le_nom_d_un_objet_fabrique_se_relit()
        {
            Assert.That(CraftedItems.TryParseNameKey(CraftedItems.NameKey("pulse_rifle", 5), out var recipe, out int stars), Is.True);
            Assert.That((recipe, stars), Is.EqualTo(("pulse_rifle", 5)));
            Assert.That(CraftedItems.TryParseNameKey("craft.item.pulse_rifle.7", out _, out _), Is.False);
            Assert.That(CraftedItems.TryParseNameKey("craft.item.pulse_rifle.fine", out _, out _), Is.False);
        }
    }
}
