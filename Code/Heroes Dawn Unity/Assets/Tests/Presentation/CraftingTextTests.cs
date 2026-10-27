using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Noms, qualités et textes des objets fabriqués au labo (lot E de la 0.14.0).</summary>
    public sealed class CraftingTextTests
    {
        readonly LocLocalization _loc = new LocLocalization();

        static ItemDef Crafted(string recipeId, CraftQuality q) => CraftedItems.ToDef("crafted_1", Recipes.Find(recipeId)!, q, new CraftedStats(10, 0, 0, 0));

        [Test, Description("Vérifie que le nom d'un objet fabriqué est celui de la recette suivi de sa qualité entre parenthèses, et qu'un objet du catalogue garde son nom — parce que le joueur doit voir la qualité de ce qu'il fabrique")]
        public void ItemName_OfACraftedItem_ShowsRecipeAndQuality()
        {
            Assert.That(CollectionText.ItemName(_loc, Crafted("patch_vest", CraftQuality.Fine)), Is.EqualTo("Gilet rapiécé (Soigné)"));
            Assert.That(CollectionText.ItemName(_loc, Crafted("scrap_blade", CraftQuality.Masterwork)), Is.EqualTo("Lame de fortune (Chef-d'œuvre)"));
            var vest = new ItemDef(new ItemId("test_vest"), "item.test", ItemRarity.Common, hp: 10);
            Assert.That(CollectionText.ItemName(_loc, vest), Is.EqualTo(_loc.T("item.test")));
        }

        [Test, Description("Vérifie que toutes les clés de texte de la fabrication (12 recettes, 5 qualités, 5 scraps, nom composé, erreur) sont traduites — parce qu'une clé nue s'afficherait telle quelle à l'écran")]
        public void EveryCraftKey_IsTranslated()
        {
            var keys = Recipes.All.Select(r => r.NameKey)
                .Concat(System.Enum.GetValues(typeof(CraftQuality)).Cast<CraftQuality>().Select(q => "craft.quality." + q.ToString().ToLowerInvariant()))
                .Concat(System.Enum.GetValues(typeof(ScrapKind)).Cast<ScrapKind>().Select(k => "scrap." + k))
                .Concat(new[] { "craft.name", "errors.not_enough_scraps" });
            foreach (var key in keys) Assert.That(_loc.T(key), Is.Not.EqualTo(key), key);
            Assert.That(_loc.T("scrap.Core"), Is.EqualTo("Noyaux"));
        }

        [Test, Description("Vérifie que les cinq qualités ont cinq couleurs distinctes et que la qualité se lit sur l'objet — parce que la couleur de l'étiquette distingue les objets fabriqués")]
        public void QualityColor_DiffersByTier_AndQualityIsReadFromTheItem()
        {
            var colors = System.Enum.GetValues(typeof(CraftQuality)).Cast<CraftQuality>().Select(CollectionText.QualityColor).ToList();
            Assert.That(colors.Distinct().Count(), Is.EqualTo(5));
            Assert.That(CollectionText.QualityOf(Crafted("patch_vest", CraftQuality.Superior)), Is.EqualTo(CraftQuality.Superior));
            Assert.That(CollectionText.QualityOf(new ItemDef(new ItemId("test_vest"), "item.test", ItemRarity.Common, hp: 10)), Is.Null);
            Assert.That(CollectionText.QualityName(_loc, CraftQuality.Rough), Is.EqualTo("Grossier"));
        }
    }
}
