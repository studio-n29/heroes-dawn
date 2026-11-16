#nullable enable
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Editor;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Garde-fou de l'export de cards.json : il ne doit jamais effacer les cartes mises en jeu par l'importeur.</summary>
    public sealed class ExportCardsMenuTests
    {
        static readonly System.Collections.Generic.IReadOnlyList<N29.HeroesDawn.Definitions.SkillCardDef> Code = PrototypeCollection.LegacyCards();

        [Test, Description("Vérifie que l'export accepte un fichier absent, vide ou identique aux cartes du code — parce que l'amorçage d'un projet neuf et la ré-écriture à l'identique ne perdent rien")]
        public void Export_IsAllowed_WhenNothingWouldBeLost()
        {
            Assert.That(ExportCardsMenu.RefusalReason(null, Code), Is.Null, "absent");
            Assert.That(ExportCardsMenu.RefusalReason("  ", Code), Is.Null, "vide");
            Assert.That(ExportCardsMenu.RefusalReason(CardDataLibrary.ToJson(Code), Code), Is.Null, "identique au code");
            Assert.That(ExportCardsMenu.RefusalReason(CardDataLibrary.ToJson(Code.Take(10).ToList()), Code), Is.Null, "sous-ensemble du code");
        }

        [Test, Description("Vérifie que l'export est refusé, avec les identifiants en cause, quand le fichier contient des cartes absentes du code — parce qu'il effacerait la vague mise en jeu")]
        public void Export_IsRefused_WhenTheFileHoldsCardsAbsentFromTheCode()
        {
            var all = PrototypeCollection.Build().Cards;
            Assume.That(all.Count, Is.GreaterThan(Code.Count), "le catalogue chargé contient la vague");
            var message = ExportCardsMenu.RefusalReason(CardDataLibrary.ToJson(all), Code);
            Assert.That(message, Is.Not.Null);
            var extra = all.Select(c => c.Id.Value).First(id => Code.All(c => c.Id.Value != id));
            Assert.That(message, Does.Contain(extra).And.Contain("refusé"));
        }

        [Test, Description("Vérifie que l'export est refusé quand le fichier existe mais est illisible — parce qu'écraser un fichier qu'on ne sait pas lire pourrait détruire du contenu")]
        public void Export_IsRefused_WhenTheFileIsUnreadable()
            => Assert.That(ExportCardsMenu.RefusalReason("{ pas du json", Code), Does.Contain("illisible"));
    }
}
