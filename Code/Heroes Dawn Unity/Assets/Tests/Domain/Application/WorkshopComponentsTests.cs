using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Atelier de fabrication : coût en Composants, qualité minimale, catalyseur, étoiles, erreurs.</summary>
    public sealed class WorkshopComponentsTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code<T>(Result<T> r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie qu'une fabrication dépense les Composants et le catalyseur, puis ajoute l'objet avec la statistique orientée — parce que l'Atelier transforme des Composants en objet")]
        public void Fabriquer_depense_les_composants_et_le_catalyseur()
        {
            var s = _app.Session;
            CraftingOps.AddComponents(s, ComponentKind.Weapon, ComponentGrade.Common, 8);
            CraftingOps.AddCatalyst(s, CatalystKind.Energy, 2);
            var r = _app.Workshop.Craft("scrap_blade", ComponentGrade.Common, CatalystKind.Energy);
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(CraftingOps.Components(s, ComponentKind.Weapon, ComponentGrade.Common), Is.EqualTo(2));
            Assert.That(CraftingOps.Catalysts(s, CatalystKind.Energy), Is.EqualTo(1));
            Assert.That(CollectionOps.OwnedItems(s, r.Value.Item.Id), Is.EqualTo(1));
            Assert.That(r.Value.Stats.Atk, Is.GreaterThan(0));
            Assert.That(r.Value.Item.AtkPercent, Is.EqualTo(r.Value.Stats.Atk));
        }

        [Test, Description("Vérifie qu'une qualité de Composants inférieure à celle de la recette est refusée sans rien dépenser — parce qu'une recette Rare exige du Raffiné")]
        public void Une_qualite_trop_basse_est_refusee_sans_rien_depenser()
        {
            var s = _app.Session;
            CraftingOps.Learn(s, "plated_coat");
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Common, 20);
            var r = _app.Workshop.Craft("plated_coat", ComponentGrade.Common, null);
            Assert.That(Code(r), Is.EqualTo("meta.grade_too_low"));
            Assert.That(CraftingOps.Components(s, ComponentKind.Armor, ComponentGrade.Common), Is.EqualTo(20));
            Assert.That(s.Meta.crafted, Is.Empty);
        }

        [Test, Description("Vérifie que des Composants insuffisants sont refusés sans rien dépenser ni créer d'objet — parce qu'une fabrication refusée ne doit rien coûter")]
        public void Des_composants_insuffisants_sont_refuses()
        {
            var s = _app.Session;
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Common, 5);
            CraftingOps.AddCatalyst(s, CatalystKind.Metal, 1);
            var r = _app.Workshop.Craft("patch_vest", ComponentGrade.Common, CatalystKind.Metal);
            Assert.That(Code(r), Is.EqualTo("meta.components"));
            Assert.That(CraftingOps.Components(s, ComponentKind.Armor, ComponentGrade.Common), Is.EqualTo(5));
            Assert.That(CraftingOps.Catalysts(s, CatalystKind.Metal), Is.EqualTo(1));
            Assert.That(s.Meta.crafted, Is.Empty);
        }

        [Test, Description("Vérifie qu'un catalyseur absent du stock est refusé sans dépenser les Composants — parce que le refus ne doit rien coûter")]
        public void Un_catalyseur_absent_du_stock_est_refuse()
        {
            var s = _app.Session;
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Common, 6);
            var r = _app.Workshop.Craft("patch_vest", ComponentGrade.Common, CatalystKind.Bio);
            Assert.That(Code(r), Is.EqualTo("meta.no_catalyst"));
            Assert.That(CraftingOps.Components(s, ComponentKind.Armor, ComponentGrade.Common), Is.EqualTo(6));
            Assert.That(s.Meta.crafted, Is.Empty);
        }

        [Test, Description("Vérifie qu'une recette inconnue est refusée sans rien dépenser, même avec assez de Composants — parce qu'une recette doit d'abord être obtenue")]
        public void Une_recette_inconnue_est_refusee()
        {
            var s = _app.Session;
            CraftingOps.AddComponents(s, ComponentKind.Weapon, ComponentGrade.Refined, 30);
            Assert.That(Code(_app.Workshop.Craft("pulse_rifle", ComponentGrade.Refined, null)), Is.EqualTo("meta.unknown"));
            Assert.That(Code(_app.Workshop.Craft("nope", ComponentGrade.Common, null)), Is.EqualTo("meta.unknown"));
            Assert.That(CraftingOps.Components(s, ComponentKind.Weapon, ComponentGrade.Refined), Is.EqualTo(30));
        }

        [Test, Description("Vérifie que les étoiles suivent la qualité des Composants (Commun 1 à 3, Exceptionnel 4 à 6) sur 100 fabrications — parce que la qualité engagée fixe l'étoile de départ")]
        public void L_etoile_depend_du_grade()
        {
            var s = _app.Session;
            CraftingOps.Learn(s, "plated_coat");
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Common, 6 * 100);
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Exceptional, 8 * 100);
            for (int i = 0; i < 100; i++)
            {
                int common = _app.Workshop.Craft("patch_vest", ComponentGrade.Common, null).Value.Stars;
                Assert.That(common, Is.InRange(1, 3));
                int exceptional = _app.Workshop.Craft("plated_coat", ComponentGrade.Exceptional, null).Value.Stars;
                Assert.That(exceptional, Is.InRange(4, 6));
            }
        }

        [Test, Description("Vérifie que cinq fabrications donnent cinq identifiants distincts et que recharger le profil ne réutilise aucun numéro — parce que deux objets ne doivent jamais partager un identifiant")]
        public void Craft_UsesTheSavedCounter_NoDuplicateIds()
        {
            var s = _app.Session;
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Common, 60);
            var ids = Enumerable.Range(0, 5).Select(_ => _app.Workshop.Craft("patch_vest", ComponentGrade.Common, null).Value.Item.Id.Value).ToList();
            Assert.That(ids, Is.Unique);
            s.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(s.ToProfile(System.DateTime.UtcNow))));
            Assert.That(_app.Workshop.Craft("patch_vest", ComponentGrade.Common, null).Value.Item.Id.Value, Is.EqualTo("crafted_6"));
            Assert.That(s.Content.Collection.Crafted.Count, Is.EqualTo(6));
        }

        [Test, Description("Vérifie que la liste des recettes ne montre que les connues, avec la capacité à fabriquer par qualité et l'étoile de départ — parce que l'écran de l'Atelier s'appuie dessus")]
        public void Recipes_ShowAffordabilityByGrade()
        {
            var s = _app.Session;
            var views = _app.Workshop.Recipes();
            Assert.That(views.Select(v => v.Recipe.Id), Is.EqualTo(Recipes.Starter));
            Assert.That(views.All(v => !v.CanCraft(ComponentGrade.Common)), Is.True);
            CraftingOps.AddComponents(s, ComponentKind.Armor, ComponentGrade.Common, 6);
            var vest = _app.Workshop.Recipes().Single(v => v.Recipe.Id == "patch_vest");
            Assert.That(vest.CanCraft(ComponentGrade.Common), Is.True);
            Assert.That(vest.CanCraft(ComponentGrade.Refined), Is.False, "pas de Composants Raffinés");
            Assert.That((vest.Cost, vest.MinGrade, vest.BaseStars(ComponentGrade.Common)), Is.EqualTo((6, ComponentGrade.Common, 2)));
            Assert.That(RecipeView.StarOdds, Is.EqualTo(new[] { 25, 50, 25 }));
            CraftingOps.Learn(s, "pulse_rifle");
            Assert.That(_app.Workshop.Recipes().Count, Is.EqualTo(5));
        }

        [Test, Description("Vérifie qu'une fabrication annonce un changement de la zone Labo — parce que la page de l'Atelier doit se rafraîchir")]
        public void Craft_Publishes_LabArea()
        {
            var areas = new List<MetaArea>();
            using var sub = _app.Subscriber<MetaChanged>().Subscribe(m => areas.Add(m.Area));
            CraftingOps.AddComponents(_app.Session, ComponentKind.Armor, ComponentGrade.Common, 6);
            _app.Workshop.Craft("patch_vest", ComponentGrade.Common, null);
            Assert.That(areas, Does.Contain(MetaArea.Lab));
        }

        [Test, Description("Vérifie que GrantDrops verse Composants, catalyseur et recette appris — parce que les combats alimentent l'Atelier")]
        public void GrantDrops_verse_composants_catalyseur_et_recette()
        {
            var s = _app.Session;
            for (int i = 0; i < 30; i++) CraftingOps.GrantDrops(s, CombatRank.Boss, true);
            int comps = 0;
            for (int k = 0; k < ComponentKinds.Count; k++)
                for (int g = 0; g < ComponentKinds.GradeCount; g++) comps += CraftingOps.Components(s, (ComponentKind)k, (ComponentGrade)g);
            Assert.That(comps, Is.GreaterThan(0));
            Assert.That(Enumerable.Range(0, 4).Sum(k => CraftingOps.Catalysts(s, (CatalystKind)k)), Is.GreaterThan(0));
            Assert.That(Recipes.All.Count(r => CraftingOps.Knows(s, r.Id)), Is.GreaterThan(Recipes.Starter.Count));
        }
    }
}
