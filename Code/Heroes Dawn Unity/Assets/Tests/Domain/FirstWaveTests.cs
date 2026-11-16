#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Première vague de 12 cartes à conditions (phase P1, ★1) : elles sont dans le catalogue chargé, égales à leurs fiches, et se comportent comme leurs fiches le disent.</summary>
    public sealed class FirstWaveTests
    {
        /// <summary>Les 12 identifiants de la vague (ordre de mise-en-jeu.txt).</summary>
        internal static readonly string[] WaveCardIds =
        {
            "card_cinder_toss", "card_hot_iron", "card_static_jolt", "card_dead_weight", "card_lynx_eye", "card_designated_target",
            "card_running_start", "card_slipstream", "card_decoy", "card_shield_bash", "card_warm_compress", "card_emergency_balm",
        };

        static SkillCardDef Card(string id) => TestContent.Build().Collection.Cards.Single(c => c.Id.Value == id);

        /// <summary>Carte témoin sans condition : un seul effet de la valeur donnée, coût 0, mêmes règles de cible.</summary>
        static SkillCardDef Plain(SkillCardDef like, EffectType type, int value, TargetRule target, params ValueMod[] mods)
            => new SkillCardDef(new CardId("card_plain"), "cards.card_plain.name", like.Family, 1,
                                new SkillDef(new SkillId("card_plain"), SkillSlot.A3, 0, target, new[] { new EffectDef(type, value, mods: mods) }, 0));

        /// <summary>
        /// « verre » (unité 0) lance aussitôt la carte (énergie pleine), « givre » (unité 1) est son allié si <paramref name="withAlly"/>, puis viennent les ennemis.
        /// Tous les points d'action sont d'abord remis à zéro ; <paramref name="before"/> prépare le combat avant le lancer.
        /// </summary>
        static (CombatSimulation Sim, List<CombatEvent> Events) Cast(SkillCardDef card, string[] enemies, Action<CombatSimulation>? before = null, bool withAlly = false, ulong seed = 11, int target = -1)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var cards = col.Cards.Where(x => x.Id.Value != card.Id.Value).Concat(new[] { card }).ToList();
            var catalog = c.WithCollection(new CollectionDef(col.Items, cards, col.Boosters));
            var allies = new List<AllyEntry> { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { card.Id }) };
            if (withAlly) allies.Add(new AllyEntry(new MemberId("givre"), 5, 0));
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, allies.ToArray(),
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));
            foreach (var u in sim.Units) u.Point = 0;
            before?.Invoke(sim);
            var verre = sim.Unit(0);
            verre.Point = verre.ActionPoint;
            verre.Energy = verre.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(verre.Id, 2, target));
            return (sim, sim.Step(1).ToList());
        }

        static void Give(CombatUnit u, StatusType status) => u.Statuses.Add(new StatusInstance(status, 3, 0));

        static int FirstDamage(List<CombatEvent> events, int target) => events.OfType<DamageEvent>().First(d => d.Target == target).Amount;

        static int FirstHeal(List<CombatEvent> events, int target) => events.OfType<HealEvent>().First(h => h.Target == target).Amount;

        static readonly string[] One = { "brute" };

        // ------------------------------------------------------------------ Catalogue chargé

        [Test, Description("Vérifie que le catalogue chargé contient les 12 cartes de la vague, toutes ★1 et de phase P1 — parce que la mise en jeu doit les avoir ajoutées sans en oublier")]
        public void TheCatalogue_ContainsTheTwelveWaveCards()
        {
            var cards = TestContent.Build().Collection.Cards;
            Assert.That(WaveCardIds.Distinct().Count(), Is.EqualTo(12));
            foreach (var id in WaveCardIds)
            {
                var card = cards.SingleOrDefault(c => c.Id.Value == id);
                Assert.That(card, Is.Not.Null, id);
                Assert.That(card!.Stars, Is.EqualTo(1), id);
                Assert.That(card.Archetypes, Is.Not.Empty, id);
            }
        }

        [Test, Description("Vérifie que chaque carte de la vague se lance en combat automatique (niveaux 1, graine 7, énergie pleine) — parce qu'une carte jamais lancée ne se jouerait pas")]
        public void EveryWaveCard_IsCastInAuto()
        {
            var never = new List<string>();
            foreach (var id in WaveCardIds)
            {
                var card = Card(id);
                var allies = new List<AllyEntry> { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { card.Id }, new[] { 1 }) };
                if (WaveGoldenTests.TwoAllyCardIds.Contains(id)) allies.Add(new AllyEntry(new MemberId("givre"), 5, 0)); // cartes à cible alliée : un allié à viser (Sillage ne se lance pas seule)
                var sim = new CombatSimulation(TestContent.Build(), new CombatSetup(7, CombatRank.Normal, allies.ToArray(),
                    new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5) }, true));
                sim.Unit(0).Energy = sim.Unit(0).MaxEnergy;
                for (int i = 0; i < 3000 && !sim.IsOver; i++) sim.Step(1);
                if (!sim.Log.OfType<SkillUsedEvent>().Any(e => e.Skill == card.Skill.Id)) never.Add(id);
            }
            Assert.That(never, Is.Empty, "cartes jamais lancées : " + string.Join(", ", never));
        }

        // ------------------------------------------------------------------ Conditions et modificateurs

        [Test, Description("Vérifie que Fer chauffé inflige 100 % sur une cible saine et 100 % + 40 sur une cible qui brûle, comme deux cartes témoins sans condition (même graine, même famille) — parce que le +40 dépend du statut Brûlure de la cible")]
        public void HotIron_DealsMoreOnABurningTarget()
        {
            var hotIron = Card("card_hot_iron");
            int healthy = FirstDamage(Cast(hotIron, One).Events, 1);
            int burning = FirstDamage(Cast(hotIron, One, s => Give(s.Unit(1), StatusType.Burn)).Events, 1);
            Assert.That(healthy, Is.EqualTo(FirstDamage(Cast(Plain(hotIron, EffectType.Damage, 100, TargetRule.SingleEnemy), One).Events, 1)), "cible saine : comme 100 %");
            Assert.That(burning, Is.EqualTo(FirstDamage(Cast(Plain(hotIron, EffectType.Damage, 100, TargetRule.SingleEnemy, new ValueMod(40)), One).Events, 1)), "cible qui brûle : comme 100 % avec +40 sans condition");
            Assert.That(burning, Is.GreaterThan(healthy));
        }

        [Test, Description("Vérifie que Poids mort inflige 100 % sur une cible libre et 100 % + 45 sur une cible sous Entrave (Bind), comme deux cartes témoins (même graine, même famille) — parce que le +45 dépend du statut Bind de la cible")]
        public void DeadWeight_DealsMoreOnABoundTarget()
        {
            var deadWeight = Card("card_dead_weight");
            int free = FirstDamage(Cast(deadWeight, One).Events, 1);
            int bound = FirstDamage(Cast(deadWeight, One, s => Give(s.Unit(1), StatusType.Bind)).Events, 1);
            Assert.That(free, Is.EqualTo(FirstDamage(Cast(Plain(deadWeight, EffectType.Damage, 100, TargetRule.SingleEnemy), One).Events, 1)), "cible libre : comme 100 %");
            Assert.That(bound, Is.EqualTo(FirstDamage(Cast(Plain(deadWeight, EffectType.Damage, 100, TargetRule.SingleEnemy, new ValueMod(45)), One).Events, 1)), "cible entravée : comme 100 % avec +45 sans condition");
            Assert.That(bound, Is.GreaterThan(free));
        }

        [Test, Description("Vérifie que Poids mort vise l'ennemi qui porte Bind parmi deux ennemis — parce que sa cible est « 1E:Bind » (le porteur du statut)")]
        public void DeadWeight_TargetsTheBindHolder()
        {
            var (_, events) = Cast(Card("card_dead_weight"), new[] { "brute", "brute" }, s => Give(s.Unit(2), StatusType.Bind), target: 1);
            Assert.That(events.OfType<SkillUsedEvent>().First().Targets, Is.EqualTo(new[] { 2 }));
        }

        [Test, Description("Vérifie que Sillage vise l'allié le plus avancé sur la frise AUTRE que le lanceur, le fait avancer de 30 %, de 45 % si un allié a Haste, et ne vise personne quand le lanceur est seul — parce que sa cible est « 1A:prochain », que le lanceur agit déjà et que son bonus lit Haste")]
        public void Slipstream_PicksTheNextAllyAndReadsHaste()
        {
            var slipstream = Card("card_slipstream");
            var (_, plain) = Cast(slipstream, One, s => s.Unit(1).Point = 2, withAlly: true);
            Assert.That(plain.OfType<SkillUsedEvent>().First().Targets, Is.EqualTo(new[] { 1 }), "l'allié le plus avancé, pas le lanceur (déjà prêt)");
            var advanced = plain.OfType<AdvancedEvent>().Single();
            Assert.That((advanced.Unit, advanced.Percent), Is.EqualTo((1, 30)));
            var (_, hasted) = Cast(slipstream, One, s => { s.Unit(1).Point = 2; Give(s.Unit(0), StatusType.Haste); }, withAlly: true);
            Assert.That(hasted.OfType<AdvancedEvent>().Single().Percent, Is.EqualTo(45), "un allié a Haste : +15");
            var (_, alone) = Cast(slipstream, One);
            Assert.That(alone.OfType<SkillUsedEvent>().First().Targets, Is.Empty, "seul : personne à pousser");
            Assert.That(alone.OfType<AdvancedEvent>(), Is.Empty);
        }

        [Test, Description("Vérifie que Leurre donne Provocation 1 tour contre deux ennemis et 2 tours contre trois — parce que « ≥ 3 ennemis vivants » prolonge la Provocation")]
        public void Decoy_GivesALongerTauntAgainstThreeEnemies()
        {
            var decoy = Card("card_decoy");
            int Taunt(string[] enemies) => Cast(decoy, enemies).Sim.Unit(0).Find(StatusType.Taunt)!.TurnsLeft;
            Assert.That(Taunt(new[] { "brute", "brute" }), Is.EqualTo(1));
            Assert.That(Taunt(new[] { "brute", "brute", "brute" }), Is.EqualTo(2));
        }

        [Test, Description("Vérifie que Baume d'urgence soigne l'allié le plus blessé de 25 % + 2 par 10 % de PV manquants (max 5), comme une carte témoin — parce que son bonus lit les PV manquants de la cible")]
        public void EmergencyBalm_HealsMoreWhenTheAllyIsHurt()
        {
            var balm = Card("card_emergency_balm");
            void Hurt(CombatSimulation s) => s.Unit(1).Hp = s.Unit(1).MaxHp * 60 / 100; // 4 dixièmes manquants : +8
            var (_, events) = Cast(balm, One, Hurt, withAlly: true);
            Assert.That(events.OfType<SkillUsedEvent>().First().Targets, Is.EqualTo(new[] { 1 }), "l'allié le plus blessé");
            int healed = FirstHeal(events, 1);
            Assert.That(healed, Is.EqualTo(FirstHeal(Cast(Plain(balm, EffectType.Heal, 33, TargetRule.SingleAlly), One, Hurt, withAlly: true, target: 1).Events, 1)), "comme un soin de 33 %");
            Assert.That(healed, Is.GreaterThan(FirstHeal(Cast(Plain(balm, EffectType.Heal, 25, TargetRule.SingleAlly), One, Hurt, withAlly: true, target: 1).Events, 1)), "plus qu'un soin de 25 %");
        }

        // ------------------------------------------------------------------ Fiches

        static string CatalogueFolder => Path.GetFullPath(Path.Combine(UnityEngine.Application.dataPath, "..", "..", "docs", "catalogue"));

        static string Cond(Condition? c) => c == null ? "-" : $"{c.Kind}/{c.Status}/{c.Value}";

        static string Mod(ValueMod m) => $"{Cond(m.When)}|{m.Bonus}|{(m.Per.HasValue ? m.Per.Value.ToString() : "-")}|{m.MaxCount}";

        static string Fx(EffectDef e) => $"{e.Type}:{e.Value}:{e.Status}:{e.Duration}:{e.ChancePercent}:{Cond(e.When)}:[{string.Join(";", e.Mods.Select(Mod))}]";

        static string Describe(SkillCardDef c)
        {
            var s = c.Skill;
            return $"{c.Id.Value}|{c.NameKey}|{c.Family}|{c.Stars}|{c.Nature}|[{string.Join(",", c.Archetypes)}]|{c.Role}"
                 + $"|skill {s.Id.Value}/{s.Slot}/{s.Cooldown}/{s.Cost}/{s.Target}/{s.Pick}/{s.PickStatus}"
                 + $"|abilities {string.Join("+", s.Abilities.Select(a => a.Trigger + "{" + string.Join(",", a.Effects.Select(Fx)) + "}"))}"
                 + $"|tiers {string.Join("+", c.Tiers.Select(t => t.Level + "{" + string.Join(",", t.Effects.Select(Fx)) + "}"))}";
        }

        [Test, Description("Vérifie que les 12 cartes du catalogue chargé sont égales, champ par champ, à la conversion directe de leurs fiches Markdown — parce que cards.json ne doit jamais s'écarter des fiches")]
        public void WaveCards_MatchTheirSheets()
        {
            var rows = Directory.GetFiles(CatalogueFolder, "cartes-*.md").SelectMany(f => CatalogImporter.ReadTable(File.ReadAllText(f))).ToList();
            var loaded = TestContent.Build().Collection.Cards;
            foreach (var id in WaveCardIds)
            {
                var row = rows.Single(r => r.Id == id);
                var direct = ContentDataConverter.ToDef(CatalogImporter.ToRecord(row));
                Assert.That(Describe(loaded.Single(c => c.Id.Value == id)), Is.EqualTo(Describe(direct)), id);
            }
        }
    }
}
