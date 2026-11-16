#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes générés des 250 cartes (les textes vivent dans la présentation, d'où ce fichier hors du dossier Domain, à côté de <c>AllCardsTests</c>).</summary>
    public sealed class AllCardsTextTests
    {
        [SetUp]
        public void French() => Loc.BindLanguage(() => "fr");

        [TearDown]
        public void Restore() => Loc.BindLanguage(() => "fr");

        /// <summary>Une clé de texte brute (« fx.damage », « reaction.trigger.EnemyDefeated »…) ou un trou de format (« {0} ») trahit un texte manquant.</summary>
        static readonly Regex RawKey = new Regex(@"\{\d\}|\b[a-z_]+\.[A-Za-z_]+(\.[A-Za-z_]+)*\b");

        [Test, Description("Vérifie que chacune des 250 cartes a en français un nom, une description de base et une ligne par palier (3, 6, 10), non vides, sans crochet (texte manquant « [clé] »), sans clé brute ni trou de format — parce que le grimoire affiche ces textes au joueur")]
        public void EveryCard_TextIsReadable()
        {
            var cards = PrototypeCollection.Build().Cards;
            Assert.That(cards.Count, Is.EqualTo(250));
            var bad = new List<string>();
            foreach (var c in cards)
            {
                var texts = new List<(string Where, string Text)> { ("nom", Loc.SkillName(c.Skill)), ("base", SkillText.Describe(c.Skill)) };
                foreach (var t in c.Tiers) texts.Add(("palier " + t.Level, string.Join(" · ", SkillText.Effects(t.Effects))));
                foreach (var (where, text) in texts)
                    if (string.IsNullOrWhiteSpace(text) || text.Contains("[") || text.Contains("]") || RawKey.IsMatch(text) || text == c.Id.Value)
                        bad.Add($"{c.Id.Value} {where} : « {text} »");
            }
            Assert.That(bad, Is.Empty, string.Join("\n", bad));
        }
    }
}
