#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using N29.HeroesDawn.Core;
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

        /// <summary>
        /// Cartes à cible alliée ou « Soi » : leur scénario d'une seule alliée ne leur donne aucun allié à viser (Sillage ne se lance même plus en Auto), donc elles
        /// ont aussi un scénario à deux alliés (clés <c>wave2:</c>), où leurs paliers peuvent s'exprimer.
        /// </summary>
        internal static readonly string[] TwoAllyCardIds = { "card_slipstream", "card_emergency_balm", "card_warm_compress", "card_running_start", "card_decoy" };

        /// <summary>Combat de référence à deux alliés : « verre » équipé de la carte au niveau donné, « givre » (plus tard sur la frise, sans carte), contre deux brutes, Auto.</summary>
        internal static CombatSimulation TwoAllyCard(SkillCardDef card, int level, ulong seed)
        {
            var allies = new[]
            {
                new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { card.Id }, new[] { level }),
                new AllyEntry(new MemberId("givre"), 5, 0),
            };
            var setup = new CombatSetup(seed, CombatRank.Normal, allies,
                                        new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5) }, true);
            var sim = new CombatSimulation(TestContent.Build(), setup);
            sim.Unit(0).Energy = sim.Unit(0).MaxEnergy;
            // Sillage : son palier 10 (Purge 1) ne s'exprime que sur un allié affligé ; « givre » brûle donc dès le départ.
            if (card.Id.Value == "card_slipstream") sim.Unit(1).Statuses.Add(new StatusInstance(StatusType.Burn, 3, 0));
            for (int i = 0; i < 3000 && !sim.IsOver; i++) sim.Step(1);
            return sim;
        }

        /// <summary>Le scénario le plus parlant de la carte à ce niveau : à deux alliés pour les cartes à cible alliée, sinon la vaillante seule.</summary>
        static CombatSimulation Best(SkillCardDef card, int level)
            => TwoAllyCardIds.Contains(card.Id.Value) ? TwoAllyCard(card, level, 7) : CombatGoldenTests.Card(card, level, null, 7);

        static IEnumerable<(string Key, Func<CombatSimulation> Build)> Scenarios()
        {
            var catalog = TestContent.Build();
            foreach (var id in FirstWaveTests.WaveCardIds)
            {
                var card = catalog.Collection.Cards.Single(c => c.Id.Value == id);
                foreach (var level in Levels)
                {
                    var (cd, lv) = (card, level);
                    // Sillage seul n'a personne à faire avancer : l'IA ne le lance plus, un scénario à une alliée ne le testerait pas.
                    if (id != "card_slipstream") yield return ($"wave:{id}:L{lv}:s7", () => CombatGoldenTests.Card(cd, lv, null, 7));
                    if (TwoAllyCardIds.Contains(id)) yield return ($"wave2:{id}:L{lv}:s7", () => TwoAllyCard(cd, lv, 7));
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
            Assert.That(keys.Count, Is.EqualTo(48), "11 cartes seules x 3 niveaux + 5 cartes à deux alliées x 3 niveaux");
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [Test, Description("Vérifie que pour chacune des 12 cartes de la vague les empreintes de combat aux niveaux 1, 6 et 10 sont deux à deux différentes — parce que chaque palier (3, 6, 10) doit réellement agir dans le scénario de la carte, sinon il est inerte")]
        public void EveryWaveTier_ChangesTheDigest()
        {
            var catalog = TestContent.Build();
            var inert = new List<string>();
            foreach (var id in FirstWaveTests.WaveCardIds)
            {
                var card = catalog.Collection.Cards.Single(c => c.Id.Value == id);
                var digests = Levels.Select(l => CombatDigest.Of(Best(card, l))).ToArray();
                if (digests[0] == digests[1]) inert.Add($"{id} : niveaux 1 et 6 identiques (paliers 3 et 6 inertes)");
                if (digests[1] == digests[2]) inert.Add($"{id} : niveaux 6 et 10 identiques (palier 10 inerte)");
                if (digests[0] == digests[2]) inert.Add($"{id} : niveaux 1 et 10 identiques");
            }
            Assert.That(inert, Is.Empty, string.Join("\n", inert));
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
