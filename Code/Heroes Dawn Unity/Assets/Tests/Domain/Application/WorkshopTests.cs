using System.Collections.Generic;
using System.Linq;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Fabrication d'objets au labo : coût, qualité tirée, expérience, erreurs.</summary>
    public sealed class WorkshopTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        static string Code<T>(Result<T> r) => r.Error?.Code ?? "ok";

        [Test, Description("Vérifie qu'une fabrication dépense les scraps, ajoute l'objet à l'inventaire et donne l'expérience prévue — parce que le labo doit transformer des scraps en objet")]
        public void Craft_SpendsScraps_AddsItem_GrantsXp()
        {
            CraftingOps.AddScraps(_app.Session, ScrapKind.Armor, 10);
            var r = _app.Workshop.Craft("patch_vest");
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(CraftingOps.Scraps(_app.Session, ScrapKind.Armor), Is.EqualTo(4));
            Assert.That(CollectionOps.OwnedItems(_app.Session, r.Value.Item.Id), Is.EqualTo(1));
            Assert.That(_app.Session.Content.Collection.HasItem(r.Value.Item.Id), Is.True);
            Assert.That(CraftingOps.Xp(_app.Session, ItemType.Armor), Is.EqualTo(CraftingRules.XpGain(1, 1)));
            Assert.That(r.Value.Item.HpPercent, Is.EqualTo(r.Value.Stats.Hp));
        }

        [Test, Description("Vérifie qu'avec trop peu de scraps rien n'est dépensé, aucun objet n'est créé et l'expérience ne bouge pas — parce qu'une fabrication refusée ne doit rien coûter")]
        public void Craft_Refused_WithoutEnoughScraps()
        {
            CraftingOps.AddScraps(_app.Session, ScrapKind.Armor, 5);
            var r = _app.Workshop.Craft("patch_vest");
            Assert.That(Code(r), Is.EqualTo("meta.scraps"));
            Assert.That(CraftingOps.Scraps(_app.Session, ScrapKind.Armor), Is.EqualTo(5));
            Assert.That(_app.Session.Meta.crafted, Is.Empty);
            Assert.That(CraftingOps.Xp(_app.Session, ItemType.Armor), Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'une recette inconnue est refusée sans rien dépenser, même avec assez de scraps et de Noyaux — parce qu'une recette doit d'abord être débloquée")]
        public void Craft_Refused_ForAnUnknownRecipe()
        {
            CraftingOps.AddScraps(_app.Session, ScrapKind.Weapon, 30);
            CraftingOps.AddScraps(_app.Session, ScrapKind.Core, 10);
            var r = _app.Workshop.Craft("pulse_rifle");
            Assert.That(Code(r), Is.EqualTo("meta.unknown"));
            Assert.That(Code(_app.Workshop.Craft("nope")), Is.EqualTo("meta.unknown"));
            Assert.That(CraftingOps.Scraps(_app.Session, ScrapKind.Weapon), Is.EqualTo(30));
            Assert.That(CraftingOps.Scraps(_app.Session, ScrapKind.Core), Is.EqualTo(10));
        }

        [Test, Description("Vérifie que les recettes Rares exigent des Noyaux en plus des scraps du type, et que le refus ne dépense rien — parce que les Noyaux sont le scrap rare commun à tous les types")]
        public void Craft_NeedsCoresForRareRecipes()
        {
            var s = _app.Session;
            CraftingOps.Learn(s, "plated_coat");
            CraftingOps.AddScraps(s, ScrapKind.Armor, 12);
            CraftingOps.AddScraps(s, ScrapKind.Core, 1);
            Assert.That(Code(_app.Workshop.Craft("plated_coat")), Is.EqualTo("meta.scraps"));
            Assert.That((CraftingOps.Scraps(s, ScrapKind.Armor), CraftingOps.Scraps(s, ScrapKind.Core)), Is.EqualTo((12, 1)));
            CraftingOps.AddScraps(s, ScrapKind.Core, 1);
            Assert.That(_app.Workshop.Craft("plated_coat").IsSuccess, Is.True);
            Assert.That((CraftingOps.Scraps(s, ScrapKind.Armor), CraftingOps.Scraps(s, ScrapKind.Core)), Is.EqualTo((0, 0)));
        }

        [Test, Description("Vérifie que la fabrication qui atteint le seuil d'expérience monte la compétence et le signale — parce que le joueur doit voir sa progression")]
        public void Craft_LevelsUp_WhenXpReachesTheThreshold()
        {
            CraftingOps.AddScraps(_app.Session, ScrapKind.Armor, 6);
            CraftingOps.AddXp(_app.Session, ItemType.Armor, 30);
            var r = _app.Workshop.Craft("patch_vest");
            Assert.That(r.Value.LeveledUp, Is.True);
            Assert.That(r.Value.NewSkill, Is.EqualTo(2));
            Assert.That(CraftingOps.Skill(_app.Session, ItemType.Armor), Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'au niveau maximum la fabrication fonctionne toujours, que la compétence reste à 10 et que la vue l'indique — parce qu'un joueur au sommet doit pouvoir continuer à fabriquer")]
        public void Craft_AtMaxSkill_KeepsWorking()
        {
            var s = _app.Session;
            CraftingOps.AddXp(s, ItemType.Armor, CraftingRules.XpForLevel(CraftingRules.MaxSkill));
            CraftingOps.AddScraps(s, ScrapKind.Armor, 6);
            var r = _app.Workshop.Craft("patch_vest");
            Assert.That(r.IsSuccess, Is.True);
            Assert.That(r.Value.LeveledUp, Is.False);
            Assert.That(CraftingOps.Skill(s, ItemType.Armor), Is.EqualTo(CraftingRules.MaxSkill));
            var view = _app.Workshop.Skills().Single(v => v.Type == ItemType.Armor);
            Assert.That((view.Level, view.Maxed), Is.EqualTo((CraftingRules.MaxSkill, true)));
        }

        [Test, Description("Vérifie que cinq fabrications donnent cinq identifiants distincts et que recharger le profil ne réutilise aucun numéro — parce que deux objets ne doivent jamais partager un identifiant")]
        public void Craft_UsesTheSavedCounter_NoDuplicateIds()
        {
            var s = _app.Session;
            CraftingOps.AddScraps(s, ScrapKind.Armor, 60);
            var ids = Enumerable.Range(0, 5).Select(_ => _app.Workshop.Craft("patch_vest").Value.Item.Id.Value).ToList();
            Assert.That(ids, Is.Unique);
            s.ApplyProfile(JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(s.ToProfile(System.DateTime.UtcNow))));
            Assert.That(_app.Workshop.Craft("patch_vest").Value.Item.Id.Value, Is.EqualTo("crafted_6"));
            Assert.That(s.Content.Collection.Crafted.Count, Is.EqualTo(6));
        }

        [Test, Description("Vérifie que la liste des recettes ne montre que les connues, avec les probabilités de qualité du niveau actuel et la capacité à payer — parce que l'écran du labo s'appuie dessus")]
        public void Recipes_ShowOddsAndAffordability()
        {
            var s = _app.Session;
            var views = _app.Workshop.Recipes();
            Assert.That(views.Select(v => v.Recipe.Id), Is.EqualTo(Recipes.Starter));
            Assert.That(views.All(v => !v.CanAfford), Is.True);
            var vest = views.Single(v => v.Recipe.Id == "patch_vest");
            Assert.That(vest.Odds, Is.EqualTo(CraftingRules.QualityOdds(1, 1)));
            CraftingOps.AddScraps(s, ScrapKind.Armor, 6);
            Assert.That(_app.Workshop.Recipes().Single(v => v.Recipe.Id == "patch_vest").CanAfford, Is.True);
            CraftingOps.Learn(s, "pulse_rifle");
            Assert.That(_app.Workshop.Recipes().Count, Is.EqualTo(5));
        }

        [Test, Description("Vérifie qu'une fabrication annonce un changement de la zone Labo — parce que la page du labo doit se rafraîchir")]
        public void Craft_Publishes_LabArea()
        {
            var areas = new List<MetaArea>();
            using var sub = _app.Subscriber<MetaChanged>().Subscribe(m => areas.Add(m.Area));
            CraftingOps.AddScraps(_app.Session, ScrapKind.Armor, 6);
            _app.Workshop.Craft("patch_vest");
            Assert.That(areas, Does.Contain(MetaArea.Lab));
        }
    }
}
