#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class LocIdentityTests
    {
        [TearDown]
        public void Restore() => Loc.BindLanguage(() => "fr");

        [TestCase("fr"), TestCase("en")]
        [Description("Vérifie qu'aucun texte localisé du jeu ne contient « Aube tactique » — parce que c'est un nom de travail de la direction artistique, jamais visible du joueur (spec refonte UI, règle d'identité)")]
        public void NoLocalizedText_ContainsTheWorkingArtDirectionName(string language)
        {
            Loc.BindLanguage(() => language);
            var keys = Loc.AllKeys().Concat(Loc.EnglishKeys()).Distinct().ToList();
            var offenders = new List<string>();
            foreach (var key in keys)
                if (Loc.T(key).IndexOf("aube tactique", System.StringComparison.OrdinalIgnoreCase) >= 0) offenders.Add(key);
            Assert.That(keys.Count, Is.GreaterThan(100), "le dictionnaire est bien parcouru");
            Assert.That(offenders, Is.Empty, string.Join(", ", offenders));
        }
    }
}
