#nullable enable
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Editor;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Noms français et anglais des cartes du catalogue (fiches <c>cartes-*.md</c>) et visuel de repli garanti.</summary>
    public sealed class CardNamesTests
    {
        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));

        static IReadOnlyList<CatalogRow> Rows() => Directory.GetFiles(CatalogueFolder, "cartes-*.md").OrderBy(f => f, System.StringComparer.Ordinal)
            .SelectMany(f => CatalogImporter.ReadTable(File.ReadAllText(f, Encoding.UTF8))).ToList();

        static string CommonFolder => Path.Combine(UnityEngine.Application.dataPath, "_Project", "Presentation", "Common");

        static string Normalize(string s) => s.Replace("\r\n", "\n");

        [TearDown]
        public void Restore() => Loc.BindLanguage(() => "fr");

        /// <summary>Noms français déjà écrits à la main dans LocCollection.cs (source de vérité pour ces cartes).</summary>
        static Dictionary<string, string> HandWrittenFrench()
        {
            var text = File.ReadAllText(Path.Combine(CommonFolder, "LocCollection.cs"), Encoding.UTF8);
            return Regex.Matches(text, @"d\[""skills\.(card_[A-Za-z0-9_]+)""\]\s*=\s*""((?:[^""\\]|\\.)*)""")
                .Cast<Match>().ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value.Replace("\\\"", "\"").Replace("\\\\", "\\"));
        }

        [Test, Description("Vérifie que chaque carte des fiches a en français le nom de la colonne Nom — parce que le grimoire affiche Loc.T(skills.<id>)")]
        public void EveryCard_HasAFrenchName_EqualToItsSheet()
        {
            Loc.BindLanguage(() => "fr");
            var rows = Rows();
            Assert.That(rows.Count, Is.GreaterThanOrEqualTo(60));
            foreach (var r in rows)
            {
                string key = "skills." + r.Id;
                Assert.That(Loc.Has(key), Is.True, r.Id);
                Assert.That(Loc.T(key), Is.EqualTo(r.Nom), r.Id);
            }
        }

        [Test, Description("Vérifie que chaque carte des fiches a en anglais le nom de la colonne Name quand la langue est « en » — parce que le joueur anglophone doit lire les noms des fiches")]
        public void EveryCard_HasAnEnglishName_EqualToItsSheet()
        {
            Loc.BindLanguage(() => "en");
            foreach (var r in Rows())
            {
                Assert.That(Loc.HasEnglish("skills." + r.Id), Is.True, r.Id);
                Assert.That(Loc.T("skills." + r.Id), Is.EqualTo(r.Name), r.Id);
            }
        }

        [Test, Description("Vérifie qu'une clé sans traduction anglaise renvoie le texte français en langue « en » — parce que le repli vers le français ne doit pas disparaître avec les noms de cartes")]
        public void English_FallsBackToFrench_WhenMissing()
        {
            const string key = "skills.card_ember_inexistante_en";
            Loc.Override(new[] { new KeyValuePair<string, string>(key, "Texte français") });
            Loc.BindLanguage(() => "en");
            Assert.That(Loc.HasEnglish(key), Is.False);
            Assert.That(Loc.T(key), Is.EqualTo("Texte français"));
            Loc.BindLanguage(() => "fr");
            Assert.That(Loc.T("skills.card_ember"), Is.EqualTo("Braise"));
        }

        [Test, Description("Vérifie qu'aucun identifiant de carte n'est défini à la fois dans LocCollection.cs et dans LocCollectionNames.cs — parce qu'un doublon d'indexeur écraserait silencieusement un nom")]
        public void NoDuplicateNameKeys()
        {
            var generated = File.ReadAllText(Path.Combine(CommonFolder, "LocCollectionNames.cs"), Encoding.UTF8);
            var ids = Regex.Matches(generated, @"d\[""skills\.([A-Za-z0-9_]+)""\]").Cast<Match>().Select(m => m.Groups[1].Value).ToList();
            Assert.That(ids.Count, Is.GreaterThan(0));
            Assert.That(ids.Distinct().Count(), Is.EqualTo(ids.Count), "doublon dans le fichier généré");
            var hand = HandWrittenFrench().Keys;
            Assert.That(ids.Intersect(hand).ToList(), Is.Empty);
        }

        [Test, Description("Vérifie que chaque carte du catalogue obtient un sprite non nul de AgentArt.Card (image propre ou visuel de repli), stable d'un appel à l'autre — parce qu'aucune carte ne doit s'afficher sans visuel")]
        public void EveryCardHasAnArtSprite_EvenWithoutItsOwnImage()
        {
            foreach (var r in Rows())
            {
                var s = AgentArt.Card(r.Id);
                Assert.That(s != null, Is.True, r.Id);
                Assert.That(AgentArt.Card(r.Id), Is.SameAs(s), r.Id + " : le visuel doit être déterministe");
            }
            Assert.That(AgentArt.Card("card_sans_image_a") != null, Is.True);
            Assert.That(AgentArt.Card("card_sans_image_b") != null, Is.True);
        }

        [Test, Description("Vérifie que la régénération en mémoire depuis les fiches redonne exactement les deux fichiers générés commités — parce qu'ils ne doivent jamais être édités à la main ni rester en retard sur les fiches")]
        public void GeneratedFilesAreUpToDate()
        {
            var (fr, en) = GenerateCardNamesMenu.Generate(Rows());
            Assert.That(Normalize(File.ReadAllText(Path.Combine(CommonFolder, "LocCollectionNames.cs"), Encoding.UTF8)), Is.EqualTo(fr));
            Assert.That(Normalize(File.ReadAllText(Path.Combine(CommonFolder, "LocCollectionNamesEn.cs"), Encoding.UTF8)), Is.EqualTo(en));
        }

        [Test, Description("Vérifie que Generate échappe les guillemets et ignore les identifiants déjà définis à la main — parce que le fichier généré doit compiler sans doublon de clé")]
        public void Generate_EscapesQuotes_AndSkipsKnownIds()
        {
            var rows = new List<CatalogRow>
            {
                new CatalogRow { Id = "card_a", Nom = "Le \"Roi\"", Name = "The \"King\"" },
                new CatalogRow { Id = "card_b", Nom = "Déjà là", Name = "Already" },
            };
            var (fr, en) = GenerateCardNamesMenu.Generate(rows, new HashSet<string> { "card_b" });
            Assert.That(fr, Does.Contain("d[\"skills.card_a\"] = \"Le \\\"Roi\\\"\";"));
            Assert.That(fr, Does.Not.Contain("card_b"));
            Assert.That(en, Does.Contain("d[\"skills.card_b\"] = \"Already\";"));
            Assert.That(en, Does.Contain("d[\"skills.card_a\"] = \"The \\\"King\\\"\";"));
        }
    }
}
