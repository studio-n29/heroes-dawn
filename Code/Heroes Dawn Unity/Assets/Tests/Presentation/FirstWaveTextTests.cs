#nullable enable
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes générés des cartes de la première vague : le joueur doit lire leur condition (les textes vivent dans la présentation, d'où ce fichier hors du dossier Domain).</summary>
    public sealed class FirstWaveTextTests
    {
        static SkillCardDef Card(string id) => PrototypeCollection.Build().Card(new CardId(id));

        [Test, Description("Vérifie que la description générée de Fer chauffé et de Poids mort contient leur condition (+40 % si la cible subit Brûlure, +45 % si la cible subit Entrave) — parce que le joueur doit lire quand le bonus s'applique")]
        public void WaveCard_TextsMentionTheirConditions()
        {
            var hotIron = SkillText.Describe(Card("card_hot_iron").Skill);
            Assert.That(hotIron, Does.Contain("+40 % si la cible subit Brûlure"));
            var deadWeight = SkillText.Describe(Card("card_dead_weight").Skill);
            Assert.That(deadWeight, Does.Contain("+45 % si la cible subit Entrave"));
            Assert.That(deadWeight, Does.Contain("Cible : porteur de Entrave"), "le ciblage fin est aussi annoncé");
        }
    }
}
