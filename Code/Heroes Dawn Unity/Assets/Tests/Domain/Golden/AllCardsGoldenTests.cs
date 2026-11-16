#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>
    /// Combats de référence figés des 190 cartes mises en jeu après la première vague (Auto, graine 7, niveaux 1, 6 et 10) : leur empreinte est enregistrée dans
    /// <c>combat-golden-p4.txt</c>, fichier distinct de <c>combat-golden.txt</c> (48 cartes d'avant P1) et de <c>combat-golden-p1.txt</c> (première vague), qui ne bougent pas.
    /// </summary>
    public sealed class AllCardsGoldenTests
    {
        static readonly int[] Levels = { 1, 6, 10 };

        static string GoldenPath([CallerFilePath] string source = "") => Path.Combine(Path.GetDirectoryName(source)!, "combat-golden-p4.txt");

        /// <summary>Les 190 cartes du catalogue chargé qui ne sont ni des 48 cartes du code ni de la première vague, dans l'ordre du catalogue.</summary>
        internal static IReadOnlyList<SkillCardDef> NewCards(ContentCatalog catalog)
        {
            var old = new HashSet<string>(PrototypeCollection.LegacyCards().Select(c => c.Id.Value).Concat(FirstWaveTests.WaveCardIds));
            return catalog.Collection.Cards.Where(c => !old.Contains(c.Id.Value)).ToList();
        }

        /// <summary>Le scénario de la carte : à deux alliées pour les cartes alliées (<see cref="AllCardsTests.NeedsAlly"/>), sinon la vaillante seule ; mêmes combats que les garde-fous précédents.</summary>
        static CombatSimulation Build(SkillCardDef card, int level)
            => AllCardsTests.NeedsAlly(card) ? WaveGoldenTests.TwoAllyCard(card, level, 7) : CombatGoldenTests.Card(card, level, null, 7);

        static IEnumerable<(string Key, Func<CombatSimulation> Build)> Scenarios()
        {
            foreach (var card in NewCards(TestContent.Build()))
                foreach (var level in Levels)
                {
                    var (cd, lv) = (card, level);
                    yield return ($"all:{cd.Id.Value}:L{lv}:s7", () => Build(cd, lv));
                }
        }

        static Dictionary<string, string> ReadGolden()
        {
            var path = GoldenPath();
            Assert.That(File.Exists(path), Is.True, "combat-golden-p4.txt est absent : lancer RecordGoldenP4 avec HEROES_RECORD_GOLDEN_P4=1");
            return File.ReadAllLines(path).Where(l => l.Contains('=')).Select(l => l.Split('=')).ToDictionary(p => p[0], p => p[1]);
        }

        [Test, Description("Vérifie que chaque combat de référence des 190 cartes mises en jeu après la première vague donne l'empreinte enregistrée dans combat-golden-p4.txt, ni plus ni moins de lignes — parce que le comportement de ces cartes ne doit changer que par décision explicite")]
        public void AllCardsGolden_MatchesTheRecordedDigests()
        {
            var golden = ReadGolden();
            var failures = new List<string>();
            var keys = new HashSet<string>();
            foreach (var (key, build) in Scenarios())
            {
                keys.Add(key);
                if (!golden.TryGetValue(key, out var expected)) { failures.Add($"clé absente de la référence : {key}"); continue; }
                var actual = CombatDigest.Of(build());
                if (actual != expected) failures.Add($"{key} : attendu {expected}, obtenu {actual}");
            }
            foreach (var key in golden.Keys.Where(k => !keys.Contains(k))) failures.Add($"ligne de référence sans scénario : {key}");
            Assert.That(keys.Count, Is.EqualTo(570), "190 cartes x 3 niveaux");
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [Test, Explicit("Réécrit combat-golden-p4.txt : à lancer seulement pour (re)figer la référence des 190 cartes"), Description("Vérifie que la référence combat-golden-p4.txt est réécrite (uniquement si HEROES_RECORD_GOLDEN_P4=1) — parce que la référence ne doit changer que sur décision explicite, jamais par un simple lancement de la classe")]
        public void RecordGoldenP4()
        {
            // Le lanceur Unity exécute aussi les tests Explicit quand on cible la classe entière : garde-fou contre un écrasement involontaire.
            if (Environment.GetEnvironmentVariable("HEROES_RECORD_GOLDEN_P4") != "1") Assert.Ignore("Définir HEROES_RECORD_GOLDEN_P4=1 pour réécrire la référence.");
            var lines = Scenarios().Select(s => $"{s.Key}={CombatDigest.Of(s.Build())}").OrderBy(l => l, StringComparer.Ordinal).ToList();
            File.WriteAllText(GoldenPath(), string.Join("\n", lines) + "\n");
            TestContext.WriteLine($"{lines.Count} empreintes enregistrées");
        }
    }
}
