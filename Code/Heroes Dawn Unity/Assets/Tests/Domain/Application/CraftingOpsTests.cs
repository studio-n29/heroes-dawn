using System.Linq;
using N29.HeroesDawn.Application.Features.Collection;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Composants, catalyseurs, recettes connues et objets fabriqués : stockage dans le profil.</summary>
    public sealed class CraftingOpsTests
    {
        static MemberId Verre => new MemberId("verre");
        TestApp _app = null!;
        GameSession S => _app.Session;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Member(Verre), 5));
        }

        ItemDef Craft(string recipeId = "patch_vest", int stars = 3, int hp = 11)
            => CraftingOps.AddCrafted(S, Recipes.Find(recipeId)!, stars, new CraftedStats(hp, 0, 0, 0));

        static ProfileData RoundTrip(ProfileData p) => JsonUtility.FromJson<ProfileData>(JsonUtility.ToJson(p));

        [Test, Description("Vérifie que les quatre recettes de départ sont connues, pas les autres, et qu'apprendre deux fois une recette ne la double pas — parce que le labo doit être utilisable dès le début")]
        public void StarterRecipes_AreKnown_OthersAreNot()
        {
            foreach (var id in Recipes.Starter) Assert.That(CraftingOps.Knows(S, id), Is.True, id);
            Assert.That(CraftingOps.Knows(S, "pulse_rifle"), Is.False);
            CraftingOps.Learn(S, "pulse_rifle");
            CraftingOps.Learn(S, "pulse_rifle");
            Assert.That(CraftingOps.Knows(S, "pulse_rifle"), Is.True);
            Assert.That(S.Meta.recipes.Count(r => r == "pulse_rifle"), Is.EqualTo(1));
            CraftingOps.Learn(S, "scrap_blade");
            Assert.That(S.Meta.recipes, Does.Not.Contain("scrap_blade"), "les recettes de départ ne s'écrivent pas dans la sauvegarde");
        }

        [Test, Description("Vérifie qu'on peut ajouter et dépenser des Composants par type et par qualité, et qu'une dépense trop grande est refusée sans rien retirer — parce qu'un compteur ne doit jamais devenir négatif")]
        public void Components_AddSpendAndRefuse()
        {
            CraftingOps.AddComponents(S, ComponentKind.Armor, ComponentGrade.Common, 6);
            CraftingOps.AddComponents(S, ComponentKind.Armor, ComponentGrade.Refined, 2);
            Assert.That(CraftingOps.Components(S, ComponentKind.Armor, ComponentGrade.Common), Is.EqualTo(6));
            Assert.That(CraftingOps.TrySpendComponents(S, ComponentKind.Armor, ComponentGrade.Common, 6), Is.True);
            Assert.That(CraftingOps.Components(S, ComponentKind.Armor, ComponentGrade.Common), Is.EqualTo(0));
            Assert.That(CraftingOps.TrySpendComponents(S, ComponentKind.Armor, ComponentGrade.Refined, 3), Is.False);
            Assert.That(CraftingOps.Components(S, ComponentKind.Armor, ComponentGrade.Refined), Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'on peut ajouter et dépenser des catalyseurs, sans passer sous zéro — parce qu'un catalyseur absent du stock ne doit pas être dépensé")]
        public void Catalysts_AddAndStayPositive()
        {
            CraftingOps.AddCatalyst(S, CatalystKind.Bio, 2);
            CraftingOps.AddCatalyst(S, CatalystKind.Bio, -5);
            Assert.That(CraftingOps.Catalysts(S, CatalystKind.Bio), Is.EqualTo(0));
            Assert.That(CraftingOps.Catalysts(S, CatalystKind.Metal), Is.EqualTo(0));
        }

        [Test, Description("Vérifie que chaque objet fabriqué reçoit un identifiant crafted_N distinct, est possédé en un exemplaire et reconnu par le catalogue — parce que les objets fabriqués sont des exemplaires uniques")]
        public void AddCrafted_NumbersAndStoresTheItem()
        {
            var a = Craft();
            var b = Craft("aegis_plate", 5, 20);
            Assert.That(a.Id.Value, Is.EqualTo("crafted_1"));
            Assert.That(b.Id.Value, Is.EqualTo("crafted_2"));
            Assert.That(CollectionOps.OwnedItems(S, a.Id), Is.EqualTo(1));
            Assert.That(CollectionOps.OwnedItems(S, b.Id), Is.EqualTo(1));
            Assert.That(S.Content.Collection.HasItem(a.Id), Is.True);
            Assert.That(S.Meta.crafted.Count, Is.EqualTo(2));
            Assert.That((S.Meta.crafted[1].recipeId, S.Meta.crafted[1].stars, S.Meta.crafted[1].hp), Is.EqualTo(("aegis_plate", 5, 20)));
        }

        [Test, Description("Vérifie qu'un objet fabriqué équipé sur un agent est retrouvé, avec ses stats, après sauvegarde et chargement dans une session neuve — parce que la progression ne doit pas se perdre")]
        public void CraftedItem_EquippedSurvivesSaveAndReload()
        {
            var item = Craft("patch_vest", 4, 13);
            Assert.That(_app.Collection.EquipItem(Verre, item.Id).IsSuccess);
            var restored = new GameSession(TestContent.Build());
            restored.ApplyProfile(RoundTrip(S.ToProfile(System.DateTime.UtcNow)));
            Assert.That(restored.Find(Verre)!.Items, Is.EqualTo(new[] { item.Id }));
            var def = restored.Content.Collection.Item(item.Id);
            Assert.That((def.HpPercent, def.Rarity, def.Type), Is.EqualTo((13, ItemRarity.Common, ItemType.Armor)));
            Assert.That(def.NameKey, Is.EqualTo(item.NameKey));
            Assert.That(CollectionOps.OwnedItems(restored, item.Id), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un identifiant crafted_N resté dans le catalogue du profil précédent ne s'équipe pas quand le profil chargé ne le possède pas — parce qu'un autre joueur ne doit pas hériter des objets du précédent")]
        public void StaleCraftedId_DoesNotEquipInAnotherProfile()
        {
            Craft();
            var other = RoundTrip(S.ToProfile(System.DateTime.UtcNow));
            other.meta.crafted.Clear();
            other.meta.craftedNext = 0;
            other.team.Single(m => m.id == Verre.Value).items = new System.Collections.Generic.List<string> { "crafted_1" };
            S.ApplyProfile(other);
            Assert.That(S.Content.Collection.HasItem(new ItemId("crafted_1")), Is.False);
            Assert.That(S.Find(Verre)!.Items, Is.Empty);
        }

        [Test, Description("Vérifie qu'un profil sans champs de fabrication (ancienne sauvegarde) se charge, connaît les quatre recettes de départ et n'a aucun objet fabriqué — parce que les anciennes sauvegardes doivent rester valides")]
        public void OldSave_WithoutCraftingFields_Loads()
        {
            var data = RoundTrip(S.ToProfile(System.DateTime.UtcNow));
            data.meta = new MetaSave();
            var s = new GameSession(TestContent.Build());
            Assert.DoesNotThrow(() => s.ApplyProfile(data));
            foreach (var id in Recipes.Starter) Assert.That(CraftingOps.Knows(s, id), Is.True);
            Assert.That(s.Content.Collection.Crafted, Is.Empty);
            Assert.That(s.Meta.crafted, Is.Empty);
            Assert.That(CraftingOps.Components(s, ComponentKind.Weapon, ComponentGrade.Common), Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un compteur craftedNext en retard sur les objets enregistrés est remonté au chargement — parce que deux objets ne doivent jamais partager un identifiant, même avec une sauvegarde incohérente")]
        public void RegisterSaved_RaisesALateCounter()
        {
            Craft(); Craft();
            S.Meta.craftedNext = 0;
            CraftingOps.RegisterSaved(S);
            Assert.That(S.Meta.craftedNext, Is.EqualTo(2));
            Assert.That(Craft().Id.Value, Is.EqualTo("crafted_3"));
            Assert.That(S.Meta.crafted.Select(c => c.id), Is.Unique);
        }

        [Test, Description("Vérifie que GrantDrops verse les Composants et catalyseurs tirés, apprend la recette obtenue et fait avancer le compteur de tirages — parce que le tirage dépend du joueur et du nombre de tirages")]
        public void GrantDrops_UpdatesTheSave()
        {
            int learned = 0, catalysts = 0;
            for (int i = 1; i <= 40; i++)
            {
                var d = CraftingOps.GrantDrops(S, CombatRank.Boss, true);
                Assert.That(S.Meta.scrapDrops, Is.EqualTo(i));
                if (d.NewRecipe != null) { learned++; Assert.That(CraftingOps.Knows(S, d.NewRecipe), Is.True); }
                if (d.Catalyst.HasValue) catalysts++;
            }
            Assert.That(learned, Is.GreaterThan(0));
            int refined = Enumerable.Range(0, 4).Sum(k => CraftingOps.Components(S, (ComponentKind)k, ComponentGrade.Refined));
            Assert.That(refined, Is.GreaterThanOrEqualTo(40 * 3), "3 à 4 Composants Raffinés par boss");
            int owned = Enumerable.Range(0, 4).Sum(k => CraftingOps.Catalysts(S, (CatalystKind)k));
            Assert.That(owned, Is.EqualTo(catalysts));
            Assert.That(catalysts, Is.GreaterThan(0));
        }
    }
}
