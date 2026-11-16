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
        static readonly ulong[] CardSeeds = { 7, 11 };
        static readonly string[] GoldenSkillPowerItems = { "neural_chip", "power_gauntlet" };

        /// <summary>Les 48 cartes du catalogue à l'enregistrement de la référence de P1 : les cartes ajoutées ensuite ont leur propre fichier de référence.</summary>
        static readonly string[] GoldenCardIds =
        {
            "card_bloodletting", "card_borrowed_mask", "card_bulwark_charge", "card_camouflage",
            "card_chain_lightning", "card_challenge", "card_concrete_rampart", "card_deep_anchor",
            "card_ember", "card_fresh_blood", "card_frost_wall", "card_glass_shards",
            "card_glass_wall", "card_guard_break", "card_guiding_echo", "card_healing_molt",
            "card_ice_carapace", "card_inferno", "card_living_bastion", "card_living_forge",
            "card_mark", "card_monolith", "card_neon_halo", "card_power_flux",
            "card_precise_shot", "card_precog", "card_premonition", "card_prism_burst",
            "card_ram_strike", "card_reflecting_prism", "card_regen_field", "card_rivet_armor",
            "card_second_wind", "card_shackle", "card_shared_pulse", "card_shared_watch",
            "card_side_step", "card_silent_takedown", "card_soothing_mist", "card_spark_rush",
            "card_steam_shield", "card_steel_skin", "card_storm", "card_support_cables",
            "card_telluric_fist", "card_time_rift", "card_time_veil", "card_tonic_current",
        };

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

        /// <summary>Niveau de l'équipe alliée par ennemi : 40 par défaut ; plus bas quand l'ennemi (niveau 10) mourrait avant son premier lancer. Les boss restent à 40 (phases).</summary>
        static readonly Dictionary<string, int> EnemyAllyLevels = new Dictionary<string, int>
        {
            { "thug", 10 }, { "brute", 30 }, { "street_pyro", 10 }, { "sniper", 10 }, { "amif_trooper", 20 }, { "amif_marksman", 10 },
            { "mos_guard", 20 }, { "mos_smuggler", 10 }, { "syn_racketeer", 30 }, { "syn_hacker", 10 }, { "syn_gunman", 10 },
            { "hrl_gangster", 30 }, { "hrl_torch", 10 }, { "hrl_roofer", 10 }, { "mnh_guard", 30 }, { "mnh_agent", 10 }, { "mnh_drone", 10 },
            { "kap_garde", 30 }, { "kap_convoyeur", 10 }, { "kap_technicien", 20 }, { "sad_guard", 30 }, { "sad_agent", 10 }, { "sad_operator", 20 },
            { "pr_kremlin_guard", 30 }, { "pr_technician", 10 }, { "pr_operator", 20 },
        };

        static CombatSimulation Enemy(EnemyId id, ulong seed)
        {
            var c = TestContent.Build();
            return Run(c, new CombatSetup(seed, CombatRank.Normal, Starter(EnemyAllyLevels.TryGetValue(id.Value, out var lv) ? lv : 40), new[] { new EnemyEntry(id, 10) }, true));
        }

        static CombatSimulation Duel()
        {
            var c = TestContent.Build();
            var theirs = new[] { "verre", "arc", "rempart", "seve" }.Select(m => new AllyEntry(new MemberId(m), 40, 0)).ToArray();
            var setup = new CombatSetup(1, CombatRank.Normal, Starter(40), Array.Empty<EnemyEntry>(), true).WithOpponentTeam(theirs, SideControl.Ai);
            return Run(c, setup);
        }

        static CombatSimulation DuelWithCards()
        {
            var c = TestContent.Build();
            AllyEntry[] Team(IEnumerable<string> members, int firstCard) => members.Select((m, i) =>
                new AllyEntry(new MemberId(m), 40, 0, 0, null, new CardId?[] { new CardId(GoldenCardIds[(firstCard + i) % GoldenCardIds.Length]) }, new[] { 6 })).ToArray();
            var setup = new CombatSetup(1, CombatRank.Normal, Team(LegacyRoster.StarterTeam, 0), Array.Empty<EnemyEntry>(), true)
                .WithOpponentTeam(Team(new[] { "verre", "arc", "rempart", "seve" }, 20), SideControl.Ai);
            return Run(c, setup);
        }

        public static IEnumerable<(string Key, Func<CombatSimulation> Build)> Scenarios()
        {
            var catalog = TestContent.Build();
            foreach (var cardId in GoldenCardIds)
            {
                var card = catalog.Collection.Cards.Single(c => c.Id.Value == cardId);
                foreach (var level in CardLevels)
                    foreach (var seed in CardSeeds)
                    {
                        var (cd, lv, sd) = (card, level, seed);
                        yield return ($"card:{cd.Id.Value}:L{lv}:s{sd}", () => Card(cd, lv, null, sd));
                    }
            }
            var ember = catalog.Collection.Cards.Single(c => c.Id.Value == "card_ember");
            foreach (var itemId in GoldenSkillPowerItems)
            {
                var it = catalog.Collection.Items.Single(i => i.Id.Value == itemId);
                yield return ($"item-skillpower:{it.Id.Value}:L6:s7", () => Card(ember, 6, it.Id, 7));
            }
            foreach (var enemy in catalog.Enemies)
                foreach (var seed in new ulong[] { 1, 2 })
                {
                    var (id, s) = (enemy.Id, seed);
                    yield return ($"enemy:{id.Value}:s{s}", () => Enemy(id, s));
                }
            yield return ("duel:starter:s1", Duel);
            yield return ("duel:cards:s1", DuelWithCards);
        }

        // 60 des 90 lignes "enemy:*" de combat-golden.txt (ennemis hrl_*, kap_*, mnh_*, pr_*, sad_*, syn_*)
        // viennent de contenu de quartiers non commité à l'enregistrement de la référence. Sans ce contenu,
        // ces lignes n'ont pas de scénario et ne sont simplement pas exercées : ne pas les supprimer.
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

        [Test, Description("Vérifie que les deux objets SkillPower du garde-fou donnent des empreintes différentes — parce que deux scénarios identiques ne protégeraient qu'un seul comportement")]
        public void SkillPowerItems_GiveDifferentDigests()
        {
            var digests = Scenarios().Where(s => s.Key.StartsWith("item-skillpower:", StringComparison.Ordinal)).Select(s => CombatDigest.Of(s.Build())).ToList();
            Assert.That(digests.Count, Is.EqualTo(GoldenSkillPowerItems.Length));
            Assert.That(digests.Distinct().Count(), Is.EqualTo(digests.Count), string.Join(", ", digests));
        }

        [Test, Description("Vérifie que chaque scénario d'ennemi voit au moins une compétence lancée par une unité ennemie — parce qu'un ennemi qui ne lance jamais sa compétence n'a aucune protection de non-régression")]
        public void EveryEnemyScenario_HasAnEnemyCast()
        {
            var never = new List<string>();
            foreach (var (key, build) in Scenarios().Where(s => s.Key.StartsWith("enemy:", StringComparison.Ordinal)))
            {
                var sim = build();
                if (!sim.Log.OfType<SkillUsedEvent>().Any(e => sim.Unit(e.Actor).Side == Side.Enemy)) never.Add(key);
            }
            Assert.That(never, Is.Empty, "scénarios sans compétence ennemie : " + string.Join(", ", never));
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

        [Test, Explicit("Réécrit combat-golden.txt : à lancer seulement pour (re)figer la référence"), Description("Vérifie que la référence combat-golden.txt est réécrite (uniquement si HEROES_RECORD_GOLDEN=1) — parce que la référence ne doit changer que sur décision explicite, jamais par un simple lancement de la classe")]
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
