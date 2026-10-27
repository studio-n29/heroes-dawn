using System.Linq;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Scraps, qualités et catalogue des recettes de fabrication du labo.</summary>
    public sealed class CraftingDefsTests
    {
        static readonly ItemType[] AllTypes = { ItemType.Weapon, ItemType.Armor, ItemType.Tool, ItemType.Implant };

        [Test, Description("Vérifie que le catalogue compte 12 recettes, trois par type d'objet (Commune, Rare, Épique) avec la difficulté et le coût de la spec — parce que chaque type d'objet doit offrir une progression de fabrication")]
        public void Catalog_Has12Recipes_3PerType_OneOfEachRarity()
        {
            Assert.That(Recipes.All.Count, Is.EqualTo(12));
            Assert.That(Recipes.All.Select(r => r.Id), Is.Unique);
            foreach (var type in AllTypes)
            {
                var ofType = Recipes.All.Where(r => r.Type == type).ToList();
                Assert.That(ofType.Count, Is.EqualTo(3), type.ToString());
                var common = ofType.Single(r => r.Rarity == ItemRarity.Common);
                var rare = ofType.Single(r => r.Rarity == ItemRarity.Rare);
                var epic = ofType.Single(r => r.Rarity == ItemRarity.Epic);
                Assert.That((common.Difficulty, common.ScrapCost, common.CoreCost), Is.EqualTo((1, 6, 0)), common.Id);
                Assert.That((rare.Difficulty, rare.ScrapCost, rare.CoreCost), Is.EqualTo((4, 12, 2)), rare.Id);
                Assert.That((epic.Difficulty, epic.ScrapCost, epic.CoreCost), Is.EqualTo((7, 20, 5)), epic.Id);
            }
        }

        [Test, Description("Vérifie que les fourchettes de stats suivent la spec (Commune 1 stat de 8 à 12, Rare 2 stats de 8 à 14, Épique 2 stats de 10 à 16 et un effet spécial) — parce que la qualité multiplie ces fourchettes")]
        public void StatRanges_MatchTheSpec()
        {
            foreach (var r in Recipes.All)
            {
                int count = r.Rarity == ItemRarity.Common ? 1 : 2;
                int min = r.Rarity == ItemRarity.Epic ? 10 : 8;
                int max = r.Rarity == ItemRarity.Common ? 12 : r.Rarity == ItemRarity.Rare ? 14 : 16;
                Assert.That(r.Stats.Count, Is.EqualTo(count), r.Id);
                Assert.That(r.Stats.Select(s => s.Stat), Is.Unique, r.Id);
                foreach (var s in r.Stats) Assert.That((s.Min, s.Max), Is.EqualTo((min, max)), r.Id);
                if (r.Rarity == ItemRarity.Epic) { Assert.That(r.Perk, Is.Not.EqualTo(ItemPerk.None), r.Id); Assert.That(r.PerkValue, Is.GreaterThan(0), r.Id); }
                else { Assert.That(r.Perk, Is.EqualTo(ItemPerk.None), r.Id); Assert.That(r.PerkValue, Is.EqualTo(0), r.Id); }
            }
        }

        [Test, Description("Vérifie que les recettes de départ sont les quatre recettes Communes, une par type — parce que le labo doit être utilisable dès le début")]
        public void Starter_IsTheFourCommonRecipes()
        {
            Assert.That(Recipes.Starter.Count, Is.EqualTo(4));
            var starters = Recipes.Starter.Select(id => Recipes.Find(id)!).ToList();
            Assert.That(starters.All(r => r.Rarity == ItemRarity.Common), Is.True);
            Assert.That(starters.Select(r => r.Type), Is.EquivalentTo(AllTypes));
            Assert.That(Recipes.Find("pulse_rifle"), Is.Not.Null);
            Assert.That(Recipes.Find("nope"), Is.Null);
        }

        [Test, Description("Vérifie les multiplicateurs de qualité (80, 100, 115, 130, 150 %) et la correspondance type d'objet → scrap — parce que la qualité change les stats tirées et que chaque type consomme son scrap")]
        public void QualityPercents_AndScrapKinds()
        {
            Assert.That(System.Enum.GetValues(typeof(CraftQuality)).Cast<CraftQuality>().Select(CraftQualities.Percent), Is.EqualTo(new[] { 80, 100, 115, 130, 150 }));
            Assert.That(ScrapKinds.Count, Is.EqualTo(5));
            Assert.That(ScrapKinds.For(ItemType.Weapon), Is.EqualTo(ScrapKind.Weapon));
            Assert.That(ScrapKinds.For(ItemType.Armor), Is.EqualTo(ScrapKind.Armor));
            Assert.That(ScrapKinds.For(ItemType.Tool), Is.EqualTo(ScrapKind.Tool));
            Assert.That(ScrapKinds.For(ItemType.Implant), Is.EqualTo(ScrapKind.Implant));
        }
    }
}
