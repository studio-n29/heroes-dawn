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
    /// Combats de référence figés (Auto, graine fixe) : leur empreinte est enregistrée dans <c>combat-golden.txt</c> avant la refonte des capacités
    /// et doit rester identique après chaque étape qui ne change aucune règle.
    /// </summary>
    public sealed class CombatGoldenTests
    {
        const int MaxTicks = 3000;
        static readonly int[] CardLevels = { 1, 3, 6, 10 };

        static string GoldenPath([CallerFilePath] string source = "") => Path.Combine(Path.GetDirectoryName(source)!, "combat-golden.txt");

        static CombatSimulation Run(ContentCatalog catalog, CombatSetup setup, Action<CombatSimulation>? before = null)
        {
            var sim = new CombatSimulation(catalog, setup);
            before?.Invoke(sim);
            for (int i = 0; i < MaxTicks && !sim.IsOver; i++) sim.Step(1);
            return sim;
        }

        static CombatSimulation Card(SkillCardDef card, int level, ItemId? item, ulong seed)
        {
            var c = TestContent.Build();
            var ally = new AllyEntry(new MemberId("verre"), 5, 0, 0, item.HasValue ? new[] { item.Value } : null,
                                     new CardId?[] { card.Id }, new[] { level });
            var setup = new CombatSetup(seed, CombatRank.Normal, new[] { ally },
                                        new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5) }, true);
            return Run(c, setup, s => s.Unit(0).Energy = s.Unit(0).MaxEnergy); // énergie pleine : la carte (coût jusqu à 70) part dès le premier tour, comme dans CardMechanicsTests.Cast
        }

        static AllyEntry[] Starter(int level) => LegacyRoster.StarterTeam.Select(m => new AllyEntry(new MemberId(m), level, 0)).ToArray();

        static CombatSimulation Enemy(EnemyId id, ulong seed)
        {
            var c = TestContent.Build();
            return Run(c, new CombatSetup(seed, CombatRank.Normal, Starter(40), new[] { new EnemyEntry(id, 10) }, true));
        }

        static CombatSimulation Duel()
        {
            var c = TestContent.Build();
            var theirs = new[] { "verre", "arc", "rempart", "seve" }.Select(m => new AllyEntry(new MemberId(m), 40, 0)).ToArray();
            var setup = new CombatSetup(1, CombatRank.Normal, Starter(40), Array.Empty<EnemyEntry>(), true).WithOpponentTeam(theirs, SideControl.Ai);
            return Run(c, setup);
        }

        public static IEnumerable<(string Key, Func<CombatSimulation> Build)> Scenarios()
        {
            var catalog = TestContent.Build();
            foreach (var card in catalog.Collection.Cards)
                foreach (var level in CardLevels)
                {
                    var (cd, lv) = (card, level);
                    yield return ($"card:{cd.Id.Value}:L{lv}:s7", () => Card(cd, lv, null, 7));
                }
            var firstCard = catalog.Collection.Cards[0];
            foreach (var item in catalog.Collection.Items.Where(i => i.Perk == ItemPerk.SkillPower))
            {
                var it = item;
                yield return ($"item-skillpower:{it.Id.Value}:L6:s7", () => Card(firstCard, 6, it.Id, 7));
            }
            foreach (var enemy in catalog.Enemies)
                foreach (var seed in new ulong[] { 1, 2 })
                {
                    var (id, s) = (enemy.Id, seed);
                    yield return ($"enemy:{id.Value}:s{s}", () => Enemy(id, s));
                }
            yield return ("duel:starter:s1", Duel);
        }

        static Dictionary<string, string> ReadGolden()
        {
            var path = GoldenPath();
            Assert.That(File.Exists(path), Is.True, "combat-golden.txt est absent : lancer RecordGolden");
            return File.ReadAllLines(path).Where(l => l.Contains('=')).Select(l => l.Split('=')).ToDictionary(p => p[0], p => p[1]);
        }

        [Test, Description("Vérifie que chaque combat de référence donne l'empreinte enregistrée dans combat-golden.txt — parce que la refonte des capacités ne doit changer aucun résultat de combat")]
        public void Golden_MatchesTheRecordedDigests()
        {
            var golden = ReadGolden();
            var failures = new List<string>();
            int count = 0;
            foreach (var (key, build) in Scenarios())
            {
                count++;
                if (!golden.TryGetValue(key, out var expected)) { failures.Add($"clé absente de la référence : {key}"); continue; }
                var actual = CombatDigest.Of(build());
                if (actual != expected) failures.Add($"{key} : attendu {expected}, obtenu {actual}");
            }
            Assert.That(count, Is.GreaterThan(0));
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        [Test, Description("Vérifie qu'un même scénario construit deux fois donne la même empreinte — parce que sans cela la référence figée ne prouverait rien")]
        public void Digest_IsStableAcrossTwoRuns()
        {
            foreach (var (key, build) in Scenarios().Where((_, i) => i % 7 == 0))
                Assert.That(CombatDigest.Of(build()), Is.EqualTo(CombatDigest.Of(build())), key);
        }

        [Test, Description("Vérifie que chaque scénario de carte lance réellement la carte au moins une fois — parce qu'une carte jamais lancée n'aurait aucune protection de non-régression")]
        public void EveryCardScenario_CastsTheCardAtLeastOnce()
        {
            var catalog = TestContent.Build();
            var never = new List<string>();
            foreach (var (key, build) in Scenarios().Where(s => s.Key.StartsWith("card:", StringComparison.Ordinal)))
            {
                var cardId = key.Split(':')[1];
                var skill = catalog.Collection.Cards.Single(c => c.Id.Value == cardId).Skill.Id;
                if (!build().Log.OfType<SkillUsedEvent>().Any(e => e.Skill == skill)) never.Add(key);
            }
            Assert.That(never, Is.Empty, "cartes jamais lancées : " + string.Join(", ", never));
        }

        [Test, Description("Vérifie que chaque boss du catalogue change de phase dans son scénario de référence, et qu'au moins un remplace son kit — parce que les changements de kit de boss sont un risque nommé de la refonte")]
        public void BossScenarios_CrossPhaseThresholds()
        {
            var catalog = TestContent.Build();
            var noPhase = new List<string>();
            bool kitChanged = false;
            foreach (var (key, build) in Scenarios().Where(s => s.Key.StartsWith("enemy:", StringComparison.Ordinal) && s.Key.EndsWith(":s1", StringComparison.Ordinal)))
            {
                var id = key.Split(':')[1];
                var phases = build().Log.OfType<BossPhaseChangedEvent>().ToList();
                kitChanged |= phases.Any(p => p.KitChanged);
                if (catalog.Enemies.Single(e => e.Id.Value == id).IsBoss && phases.Count == 0) noPhase.Add(key);
                if (phases.Count > 0) TestContext.WriteLine($"{key} : {phases.Count} changement(s) de phase, kit changé = {phases.Any(p => p.KitChanged)}");
            }
            Assert.That(noPhase, Is.Empty, "boss sans changement de phase : " + string.Join(", ", noPhase));
            Assert.That(kitChanged, Is.True, "aucun boss ne change de kit");
        }

        [Test, Explicit("Réécrit combat-golden.txt : à lancer seulement pour (re)figer la référence")]
        public void RecordGolden()
        {
            // Le lanceur Unity exécute aussi les tests Explicit quand on cible la classe entière : garde-fou contre un écrasement involontaire.
            if (Environment.GetEnvironmentVariable("HEROES_RECORD_GOLDEN") != "1") Assert.Ignore("Définir HEROES_RECORD_GOLDEN=1 pour réécrire la référence.");
            var lines = Scenarios().Select(s => $"{s.Key}={CombatDigest.Of(s.Build())}").OrderBy(l => l, StringComparer.Ordinal).ToList();
            File.WriteAllText(GoldenPath(), string.Join("\n", lines) + "\n");
            TestContext.WriteLine($"{lines.Count} empreintes enregistrées");
        }
    }
}
