using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Noms, étoiles et textes des objets fabriqués à l'Atelier.</summary>
    public sealed class CraftingTextTests
    {
        readonly LocLocalization _loc = new LocLocalization();

        static ItemDef Crafted(string recipeId, int stars) => CraftedItems.ToDef("crafted_1", Recipes.Find(recipeId)!, stars, new CraftedStats(10, 0, 0, 0));

        [Test, Description("Vérifie que le nom d'un objet fabriqué est celui de la recette suivi de ses étoiles entre parenthèses, et qu'un objet du catalogue garde son nom — parce que le joueur doit voir les étoiles de ce qu'il fabrique")]
        public void ItemName_OfACraftedItem_ShowsRecipeAndStars()
        {
            Assert.That(CollectionText.ItemName(_loc, Crafted("patch_vest", 3)), Is.EqualTo("Gilet rapiécé (★★★)"));
            Assert.That(CollectionText.ItemName(_loc, Crafted("scrap_blade", 6)), Is.EqualTo("Lame de fortune (★★★★★★)"));
            var vest = new ItemDef(new ItemId("test_vest"), "item.test", ItemRarity.Common, hp: 10);
            Assert.That(CollectionText.ItemName(_loc, vest), Is.EqualTo(_loc.T("item.test")));
        }

        [Test, Description("Vérifie que toutes les clés de texte de l'Atelier (12 recettes, 6 étoiles, 12 Composants, 4 catalyseurs, 3 qualités, nom composé, 3 erreurs) sont traduites — parce qu'une clé nue s'afficherait telle quelle à l'écran")]
        public void EveryCraftKey_IsTranslated()
        {
            var kinds = System.Enum.GetValues(typeof(ComponentKind)).Cast<ComponentKind>().ToList();
            var grades = System.Enum.GetValues(typeof(ComponentGrade)).Cast<ComponentGrade>().ToList();
            var keys = Recipes.All.Select(r => r.NameKey)
                .Concat(Enumerable.Range(CraftStars.Min, 6).Select(s => "craft.stars." + s))
                .Concat(kinds.SelectMany(k => grades.Select(g => "component." + k + "." + g)))
                .Concat(System.Enum.GetValues(typeof(CatalystKind)).Cast<CatalystKind>().Select(c => "catalyst." + c))
                .Concat(grades.Select(g => "grade." + g))
                .Concat(new[] { "craft.name", "errors.not_enough_components", "errors.grade_too_low", "errors.no_catalyst", "combat.craft_catalyst" });
            foreach (var key in keys) Assert.That(_loc.T(key), Is.Not.EqualTo(key), key);
        }

        [Test, Description("Vérifie qu'un objet fabriqué emprunte l'illustration d'un objet du catalogue de même type (et qu'un objet du catalogue garde la sienne) — parce que les objets fabriqués n'ont pas d'illustration propre")]
        public void ArtId_OfACraftedItem_IsACatalogueItemOfTheSameType()
        {
            var c = N29.HeroesDawn.Content.Prototype.PrototypeCollection.Build();
            var crafted = Crafted("patch_vest", 3);
            string art = CollectionText.ArtId(crafted, c);
            Assert.That(c.HasItem(new ItemId(art)), Is.True);
            Assert.That(c.Item(new ItemId(art)).Type, Is.EqualTo(ItemType.Armor));
            Assert.That(art, Does.Not.StartWith(CraftedItems.IdPrefix));
            var plain = c.Items[0];
            Assert.That(CollectionText.ArtId(plain, c), Is.EqualTo(plain.Id.Value));
        }

        [Test, Description("Vérifie que les étoiles 3 à 6 ont quatre couleurs distinctes et que les étoiles se lisent sur l'objet — parce que la couleur de l'étiquette distingue les objets fabriqués")]
        public void StarsColor_DiffersByTier_AndStarsAreReadFromTheItem()
        {
            var colors = Enumerable.Range(3, 4).Select(CollectionText.StarsColor).ToList();
            Assert.That(colors.Distinct().Count(), Is.EqualTo(4));
            Assert.That(CollectionText.StarsOf(Crafted("patch_vest", 5)), Is.EqualTo(5));
            Assert.That(CollectionText.StarsOf(new ItemDef(new ItemId("test_vest"), "item.test", ItemRarity.Common, hp: 10)), Is.Null);
            Assert.That(CollectionText.StarsText(_loc, 4), Is.EqualTo("★★★★"));
        }
    }
}
