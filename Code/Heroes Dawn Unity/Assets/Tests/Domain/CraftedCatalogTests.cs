using System;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Objets fabriqués : exemplaires enregistrés à part dans le catalogue de la session.</summary>
    public sealed class CraftedCatalogTests
    {
        static CollectionDef Fresh()
        {
            var c = TestContent.Build().Collection;
            return new CollectionDef(c.Items, c.Cards, c.Boosters);
        }

        static ItemDef Crafted(int n, string recipeId = "patch_vest", CraftQuality q = CraftQuality.Fine)
            => CraftedItems.ToDef(CraftedItems.IdPrefix + n, Recipes.Find(recipeId)!, q, new CraftedStats(11, 0, 0, 0));

        [Test, Description("Vérifie qu'un objet enregistré est retrouvé par HasItem/Item et listé dans AllItems mais pas dans le catalogue Items — parce que les objets fabriqués s'équipent comme les autres sans entrer dans la boutique")]
        public void Registered_ItemIsFound_ButNotInTheCatalogue()
        {
            var defs = Fresh();
            var item = Crafted(1);
            defs.Register(item);
            Assert.That(defs.HasItem(item.Id), Is.True);
            Assert.That(defs.Item(item.Id), Is.SameAs(item));
            Assert.That(defs.AllItems, Does.Contain(item));
            Assert.That(defs.Crafted, Is.EqualTo(new[] { item }));
            Assert.That(defs.Items.Contains(item), Is.False);
        }

        [Test, Description("Vérifie que l'instance partagée CollectionDef.Empty refuse l'enregistrement — parce qu'elle est commune à tout le jeu et ne doit jamais être modifiée")]
        public void Register_RefusedOnEmpty()
        {
            Assert.Throws<InvalidOperationException>(() => CollectionDef.Empty.Register(Crafted(1)));
        }

        [Test, Description("Vérifie que ClearCrafted retire les objets fabriqués sans toucher au catalogue — parce qu'un autre profil ne doit pas hériter des objets du précédent")]
        public void ClearCrafted_RemovesThem()
        {
            var defs = Fresh();
            int catalogue = defs.Items.Count;
            defs.Register(Crafted(1));
            defs.ClearCrafted();
            Assert.That(defs.HasItem(new ItemId("crafted_1")), Is.False);
            Assert.That(defs.Crafted, Is.Empty);
            Assert.That(defs.Items.Count, Is.EqualTo(catalogue));
        }

        [Test, Description("Vérifie que deux enregistrements du même identifiant ne laissent qu'une entrée, la seconde l'emportant — parce que le rechargement d'un profil réenregistre les mêmes objets")]
        public void Register_SameIdReplaces()
        {
            var defs = Fresh();
            defs.Register(Crafted(1, "patch_vest", CraftQuality.Rough));
            var second = Crafted(1, "patch_vest", CraftQuality.Masterwork);
            defs.Register(second);
            Assert.That(defs.Crafted.Count, Is.EqualTo(1));
            Assert.That(defs.Item(second.Id).NameKey, Is.EqualTo(second.NameKey));
        }

        [Test, Description("Vérifie que ni les drops ni les offres de la boutique du jour ne proposent jamais un objet fabriqué — parce que ces objets viennent seulement du labo")]
        public void ShopAndDrops_NeverOfferCrafted()
        {
            var defs = Fresh();
            for (int i = 1; i <= 5; i++) defs.Register(Crafted(i, i % 2 == 0 ? "aegis_plate" : "patch_vest", CraftQuality.Masterwork));
            for (int i = 0; i < 300; i++)
                Assert.That(CollectionRules.RollItem(defs, new Pcg32(CollectionRules.Seed("p", "drop", i)))!.Id.Value, Does.Not.StartWith(CraftedItems.IdPrefix));
            for (int day = 1; day <= 30; day++)
                foreach (var offer in CollectionRules.DailyItemOffers(defs, "2026-11-" + day.ToString("00"), 4))
                    Assert.That(offer.Id.Value, Does.Not.StartWith(CraftedItems.IdPrefix));
        }

        [Test, Description("Vérifie que la définition copie le type, la rareté, l'effet spécial de la recette et les stats tirées, et que la clé de nom fait l'aller-retour — parce que l'objet doit se comporter comme sa recette")]
        public void ToDef_CopiesRecipeAndRolledStats()
        {
            var recipe = Recipes.Find("aegis_plate")!;
            var def = CraftedItems.ToDef("crafted_7", recipe, CraftQuality.Superior, new CraftedStats(15, 0, 12, 0));
            Assert.That(def.Id.Value, Is.EqualTo("crafted_7"));
            Assert.That((def.Type, def.Rarity, def.Perk, def.PerkValue), Is.EqualTo((ItemType.Armor, ItemRarity.Epic, ItemPerk.OpeningShield, 10)));
            Assert.That((def.HpPercent, def.AtkPercent, def.DefPercent, def.SpdPercent), Is.EqualTo((15, 0, 12, 0)));
            Assert.That(def.Price, Is.EqualTo(0));
            Assert.That(CraftedItems.TryParseNameKey(def.NameKey, out var recipeId, out var quality), Is.True);
            Assert.That((recipeId, quality), Is.EqualTo(("aegis_plate", CraftQuality.Superior)));
            Assert.That(CraftedItems.TryParseNameKey("item.kevlar_vest", out _, out _), Is.False);
        }
    }
}
