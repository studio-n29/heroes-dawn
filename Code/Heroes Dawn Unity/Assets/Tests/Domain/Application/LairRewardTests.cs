using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Récompenses des boss de repaire (bloc C de la 1.0.0) : Composants, catalyseur et recette, une fois par jour en entier puis réduites de moitié.</summary>
    public sealed class LairRewardTests
    {
        static LairDef Lair => Lairs.All[0];
        static LairBossDef Brick => Lair.Bosses.Single(b => b.Enemy == "brick");
        static LairBossDef Crocodile => Lair.Bosses.Single(b => b.Enemy == "crocodile");
        static LairBossDef Titan => Lair.Bosses.Single(b => b.Enemy == "bridge_titan");

        TestApp _app = null!;
        GameSession S => _app.Session;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        LairGrant Grant(LairBossDef boss, string day) => LairBook.GrantBossReward(S.Meta, S, Lair, boss, day);

        [Test, Description("Vérifie que la première victoire du jour sur un boss donne la récompense complète et la recette — parce que le repaire récompense d'abord la première victoire de la journée")]
        public void La_premiere_victoire_du_jour_donne_la_recompense_complete_et_la_recette()
        {
            Assert.That(CraftingOps.Knows(S, "plated_coat"), Is.False);
            var g = Grant(Brick, "2026-11-11");
            Assert.That(g.Reduced, Is.False);
            Assert.That(g.Grade, Is.EqualTo(ComponentGrade.Refined));
            Assert.That(g.Count, Is.EqualTo(6));
            Assert.That(g.Recipe, Is.EqualTo("plated_coat"));
            Assert.That(CraftingOps.Knows(S, "plated_coat"), Is.True);
            Assert.That(CraftingOps.Components(S, g.Kind, ComponentGrade.Refined), Is.EqualTo(6), "versés dans l'inventaire");

            var t = Grant(Titan, "2026-11-11");
            Assert.That(t.Count, Is.EqualTo(4));
            Assert.That(t.BonusCount, Is.EqualTo(1));
            Assert.That(t.Components, Is.EqualTo(5));
            Assert.That(CraftingOps.Components(S, t.Kind, ComponentGrade.Exceptional), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que la deuxième victoire du même jour donne la moitié des Composants (arrondie à l'inférieur, au moins 1) et pas de recette — parce que rejouer le repaire doit rester utile sans devenir une usine")]
        public void La_deuxieme_victoire_du_meme_jour_donne_la_moitie_et_pas_de_recette()
        {
            Grant(Brick, "2026-11-11");
            var g = Grant(Brick, "2026-11-11");
            Assert.That(g.Reduced, Is.True);
            Assert.That(g.Count, Is.EqualTo(3));
            Assert.That(g.Recipe, Is.Null);

            Grant(Titan, "2026-11-11");
            var t = Grant(Titan, "2026-11-11");
            Assert.That(t.Count, Is.EqualTo(2));
            Assert.That(t.BonusCount, Is.EqualTo(0), "1 / 2 arrondi à l'inférieur");

            var petit = new LairBossDef("n99", "brick", new LairRewardDef(ComponentKind.Weapon, ComponentGrade.Common, 1, 0));
            LairBook.GrantBossReward(S.Meta, S, Lair, petit, "2026-11-11");
            Assert.That(LairBook.GrantBossReward(S.Meta, S, Lair, petit, "2026-11-11").Count, Is.EqualTo(1), "au moins 1");
        }

        [Test, Description("Vérifie que la chance de catalyseur est divisée par deux après la première victoire du jour (15 % puis 7 % pour Brick, sur 400 tirages) — parce que la limite quotidienne porte aussi sur les catalyseurs")]
        public void La_chance_de_catalyseur_est_divisee_par_deux_apres_la_premiere_victoire()
        {
            int full = 0, reduced = 0;
            for (int d = 0; d < 400; d++) if (Grant(Brick, "2027-" + (d / 28 + 1).ToString("00") + "-" + (d % 28 + 1).ToString("00")).Catalyst.HasValue) full++;
            for (int i = 0; i < 400; i++) if (Grant(Brick, "2027-01-01").Catalyst.HasValue) reduced++;
            Assert.That(full / 4.0, Is.InRange(9.0, 21.0), "15 % attendus");
            Assert.That(reduced / 4.0, Is.InRange(3.0, 11.0), "7 % attendus");
            Assert.That(Enumerable.Range(0, 4).Sum(k => CraftingOps.Catalysts(S, (CatalystKind)k)), Is.EqualTo(full + reduced));
        }

        [Test, Description("Vérifie qu'un nouveau jour redonne la récompense complète, sans la recette déjà connue, et que chaque boss a sa propre limite — parce que la limite quotidienne est par boss et par jour")]
        public void Un_nouveau_jour_redonne_la_recompense_complete()
        {
            Grant(Brick, "2026-11-11");
            Assert.That(Grant(Brick, "2026-11-11").Reduced, Is.True);
            var g = Grant(Brick, "2026-11-12");
            Assert.That(g.Reduced, Is.False);
            Assert.That(g.Count, Is.EqualTo(6));
            Assert.That(g.Recipe, Is.Null, "recette déjà connue");
            Assert.That(Grant(Crocodile, "2026-11-12").Reduced, Is.False, "un autre boss garde sa propre limite");
        }

        [Test, Description("Vérifie qu'une sauvegarde sans données de récompense (profil d'avant le bloc C) est acceptée et que le jour de récompense survit à une sauvegarde — parce qu'un ancien profil ne doit pas planter et qu'on ne doit pas rejouer la même journée à l'infini")]
        public void Une_sauvegarde_sans_donnees_de_recompense_est_acceptee()
        {
            var old = JsonUtility.FromJson<MetaSave>("{}");
            Assert.That(old.lairRewardDay, Is.Not.Null);
            Assert.That(old.lairRewardDay, Is.Empty);
            Assert.DoesNotThrow(() => Grant(Brick, "2026-11-11"));

            Grant(Brick, "2026-11-11");
            var profile = JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(S.ToProfile(System.DateTime.UtcNow)));
            var restored = new TestApp();
            restored.SignInDirect("p1");
            restored.Session.ApplyProfile(profile);
            Assert.That(LairBook.GrantBossReward(restored.Session.Meta, restored.Session, Lair, Brick, "2026-11-11").Reduced, Is.True);
        }

        [Test, Description("Vérifie que deux profils identiques tirent les mêmes récompenses (type de Composants et catalyseurs) jour après jour — parce que le tirage dépend seulement du joueur, du boss, du jour et du compteur")]
        public void Le_tirage_est_deterministe()
        {
            var other = new TestApp();
            other.SignInDirect("p1");
            for (int d = 0; d < 30; d++)
            {
                var a = Grant(Titan, "2026-12-" + (d % 28 + 1).ToString("00"));
                var b = LairBook.GrantBossReward(other.Session.Meta, other.Session, Lair, Titan, "2026-12-" + (d % 28 + 1).ToString("00"));
                Assert.That(b.Kind, Is.EqualTo(a.Kind), "jour " + d);
                Assert.That(b.Catalyst, Is.EqualTo(a.Catalyst), "jour " + d);
                Assert.That(b.Reduced, Is.EqualTo(a.Reduced), "jour " + d);
            }
        }

        [Test, Description("Vérifie que les recettes de repaire ne tombent jamais des combats ordinaires (300 butins de boss) et que les récompenses de repaire désignent des recettes de source Repaire — parce qu'une recette de repaire ne s'obtient que dans son repaire")]
        public void Les_recettes_de_repaire_ne_tombent_pas_ailleurs()
        {
            for (int i = 0; i < 300; i++) CraftingOps.GrantDrops(S, CombatRank.Boss, true);
            foreach (var r in Recipes.All.Where(r => r.Source != RecipeSource.Adventure && r.Source != RecipeSource.Starter))
                Assert.That(CraftingOps.Knows(S, r.Id), Is.False, r.Id);
            foreach (var boss in Lair.Bosses)
            {
                Assert.That(boss.Reward, Is.Not.Null, boss.Enemy);
                Assert.That(Recipes.Find(boss.Reward!.RecipeId!)!.Source, Is.EqualTo(RecipeSource.Lair), boss.Enemy);
            }
        }
    }
}
