using System.Linq;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Composants, étoiles et catalogue des recettes de l'Atelier.</summary>
    public sealed class CraftingDefsTests
    {
        static readonly ItemType[] AllTypes = { ItemType.Weapon, ItemType.Armor, ItemType.Tool, ItemType.Implant };

        [Test, Description("Vérifie que le catalogue compte 12 recettes, trois par type d'objet (Commune, Rare, Épique) avec le coût en Composants et la qualité minimale de la spec — parce que chaque type d'objet doit offrir une progression de fabrication")]
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
                Assert.That((common.ComponentCost, common.MinGrade), Is.EqualTo((6, ComponentGrade.Common)), common.Id);
                Assert.That((rare.ComponentCost, rare.MinGrade), Is.EqualTo((8, ComponentGrade.Refined)), rare.Id);
                Assert.That((epic.ComponentCost, epic.MinGrade), Is.EqualTo((10, ComponentGrade.Refined)), epic.Id);
            }
        }

        [Test, Description("Vérifie que les fourchettes de stats suivent la spec (Commune 1 stat de 8 à 12, Rare 2 stats de 8 à 14, Épique 2 stats de 10 à 16 et un effet spécial) — parce que l'étoile multiplie ces fourchettes")]
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

        [Test, Description("Vérifie les multiplicateurs d'étoiles (80, 100, 115, 130, 150, 175 %), l'étoile de départ par qualité (2, 4, 5) et la correspondance type d'objet → type de Composant — parce que l'étoile change les stats tirées et que chaque type consomme son Composant")]
        public void StarPercents_AndComponentKinds()
        {
            Assert.That(Enumerable.Range(CraftStars.Min, 6).Select(CraftStars.Percent), Is.EqualTo(new[] { 80, 100, 115, 130, 150, 175 }));
            Assert.That(CraftStars.Base(ComponentGrade.Common), Is.EqualTo(2));
            Assert.That(CraftStars.Base(ComponentGrade.Refined), Is.EqualTo(4));
            Assert.That(CraftStars.Base(ComponentGrade.Exceptional), Is.EqualTo(5));
            Assert.That(ComponentKinds.Count, Is.EqualTo(4));
            Assert.That(ComponentKinds.For(ItemType.Weapon), Is.EqualTo(ComponentKind.Weapon));
            Assert.That(ComponentKinds.For(ItemType.Armor), Is.EqualTo(ComponentKind.Armor));
            Assert.That(ComponentKinds.For(ItemType.Tool), Is.EqualTo(ComponentKind.Tool));
            Assert.That(ComponentKinds.For(ItemType.Implant), Is.EqualTo(ComponentKind.Implant));
        }
    }
}
