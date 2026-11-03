#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Langue des textes (L1 de la 0.15.0) : le réglage du profil choisit le dictionnaire, le français sert de repli.</summary>
    public sealed class LocLanguageTests
    {
        [TearDown]
        public void TearDown() => Loc.BindLanguage(() => "fr");

        [Test, Description("Vérifie que chaque clé anglaise existe aussi en français — parce qu'une clé orpheline n'apparaîtrait jamais dans le jeu")]
        public void EveryEnglishKeyExistsInFrench()
        {
            var orphans = EnglishKeys().Where(k => !Loc.Has(k)).ToList();
            Assert.That(orphans, Is.Empty);
        }

        [Test, Description("Vérifie qu'aucune traduction anglaise n'est vide ou ne ressemble à une clé brute — parce qu'un texte vide ou entre crochets se voit à l'écran")]
        public void EnglishValuesAreReadable()
        {
            var bad = EnglishKeys().Where(k => string.IsNullOrWhiteSpace(Loc.T(k)) || Loc.T(k).StartsWith("[", System.StringComparison.Ordinal)).ToList();
            Assert.That(bad, Is.Empty);
        }

        [Test, Description("Vérifie que la langue par défaut reste le français — parce qu'un profil sans réglage doit afficher le jeu comme avant")]
        public void DefaultLanguageIsFrench()
        {
            Loc.BindLanguage(() => "fr");
            Assert.That(Loc.Language, Is.EqualTo("fr"));
            Assert.That(Loc.T("nav.qg"), Is.EqualTo("QG"));
        }

        [Test, Description("Vérifie que le choix anglais change les textes traduits — parce que c'est le but du réglage « Langue des textes »")]
        public void EnglishLanguageSwitchesTranslatedTexts()
        {
            Loc.BindLanguage(() => "en");
            Assert.That(Loc.Language, Is.EqualTo("en"));
            Assert.That(Loc.T("nav.qg"), Is.EqualTo("HQ"));
            Assert.That(Loc.T("nav.shop"), Is.EqualTo("SHOP"));
        }

        [Test, Description("Vérifie qu'une clé sans traduction anglaise reprend le français — parce que les écrans se traduisent par lots et ne doivent pas afficher de clé brute")]
        public void UntranslatedKeyFallsBackToFrench()
        {
            Loc.BindLanguage(() => "en");
            Assert.That(Loc.EnglishKeys(), Does.Not.Contain("hub.recruit"));
            Assert.That(Loc.T("hub.recruit"), Is.EqualTo("RECRUTER · {0} PR"));
        }

        [Test, Description("Vérifie que le réglage « Langue des textes » est lu à chaque affichage — parce qu'un changement doit s'appliquer sans relancer le jeu")]
        public void LanguageFollowsBoundSetting()
        {
            var setting = "fr";
            Loc.BindLanguage(() => setting);
            Assert.That(Loc.T("nav.team"), Is.EqualTo("ÉQUIPE"));
            setting = "en";
            Assert.That(Loc.T("nav.team"), Is.EqualTo("TEAM"));
        }

        static IEnumerable<string> EnglishKeys() => Loc.EnglishKeys();
    }
}
