#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>
    /// Combats de référence figés des cartes de la première vague (Auto, graine 7) : leur empreinte est enregistrée dans <c>combat-golden-p1.txt</c>,
    /// fichier distinct de <c>combat-golden.txt</c> (qui, lui, ne couvre que les 48 cartes d'avant P1 et ne bouge jamais).
    /// </summary>
    public sealed class WaveGoldenTests
    {
        static readonly int[] Levels = { 1, 6, 10 };

        static string GoldenPath([CallerFilePath] string source = "") => Path.Combine(Path.GetDirectoryName(source)!, "combat-golden-p1.txt");

        static IEnumerable<(string Key, Func<CombatSimulation> Build)> Scenarios()
        {
            var catalog = TestContent.Build();
            foreach (var id in FirstWaveTests.WaveCardIds)
            {
                var card = catalog.Collection.Cards.Single(c => c.Id.Value == id);
                foreach (var level in Levels)
                {
                    var (cd, lv) = (card, level);
                    yield return ($"wave:{id}:L{lv}:s7", () => CombatGoldenTests.Card(cd, lv, null, 7));
                }
            }
        }

        static Dictionary<string, string> ReadGolden()
        {
            var path = GoldenPath();
            Assert.That(File.Exists(path), Is.True, "combat-golden-p1.txt est absent : lancer RecordGoldenP1 avec HEROES_RECORD_GOLDEN_P1=1");
            return File.ReadAllLines(path).Where(l => l.Contains('=')).Select(l => l.Split('=')).ToDictionary(p => p[0], p => p[1]);
        }

        [Test, Description("Vérifie que chaque combat de carte de la vague donne l'empreinte enregistrée dans combat-golden-p1.txt, ni plus ni moins de lignes — parce que le comportement des 12 nouvelles cartes ne doit changer que par décision explicite")]
        public void WaveGolden_MatchesTheRecordedDigests()
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
            Assert.That(keys.Count, Is.EqualTo(36));
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [Test, Description("Vérifie que chaque scénario de la vague lance réellement la carte au moins une fois — parce qu'une carte jamais lancée n'aurait aucune protection de non-régression")]
        public void EveryWaveScenario_CastsTheCardAtLeastOnce()
        {
            var catalog = TestContent.Build();
            var never = new List<string>();
            foreach (var (key, build) in Scenarios())
            {
                var skill = catalog.Collection.Cards.Single(c => c.Id.Value == key.Split(':')[1]).Skill.Id;
                if (!build().Log.OfType<SkillUsedEvent>().Any(e => e.Skill == skill)) never.Add(key);
            }
            Assert.That(never, Is.Empty, "cartes jamais lancées : " + string.Join(", ", never));
        }

        [Test, Explicit("Réécrit combat-golden-p1.txt : à lancer seulement pour (re)figer la référence de la vague"), Description("Vérifie que la référence combat-golden-p1.txt est réécrite (uniquement si HEROES_RECORD_GOLDEN_P1=1) — parce que la référence ne doit changer que sur décision explicite, jamais par un simple lancement de la classe")]
        public void RecordGoldenP1()
        {
            // Le lanceur Unity exécute aussi les tests Explicit quand on cible la classe entière : garde-fou contre un écrasement involontaire.
            if (Environment.GetEnvironmentVariable("HEROES_RECORD_GOLDEN_P1") != "1") Assert.Ignore("Définir HEROES_RECORD_GOLDEN_P1=1 pour réécrire la référence.");
            var lines = Scenarios().Select(s => $"{s.Key}={CombatDigest.Of(s.Build())}").OrderBy(l => l, StringComparer.Ordinal).ToList();
            File.WriteAllText(GoldenPath(), string.Join("\n", lines) + "\n");
            TestContext.WriteLine($"{lines.Count} empreintes enregistrées");
        }
    }
}
