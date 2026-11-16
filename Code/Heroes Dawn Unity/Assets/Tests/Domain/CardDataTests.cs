#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Catalogue de cartes en données : cards.json doit redonner exactement les cartes du code, et le repli sur le code doit rester possible.</summary>
    public sealed class CardDataTests
    {
        const int MaxTicks = 3000;

        static string Cond(Condition? c) => c == null ? "-" : $"{c.Kind}/{c.Status}/{c.Value}";

        static string Mod(ValueMod m) => $"{Cond(m.When)}|{m.Bonus}|{(m.Per.HasValue ? m.Per.Value.ToString() : "-")}|{m.MaxCount}";

        static string Fx(EffectDef e) => $"{e.Type}:{e.Value}:{e.Status}:{e.Duration}:{e.ChancePercent}:{Cond(e.When)}:[{string.Join(";", e.Mods.Select(Mod))}]";

        /// <summary>Description complète d'une carte, champ par champ (EffectDef se compare par référence).</summary>
        static string Describe(SkillCardDef c)
        {
            var s = c.Skill;
            return $"{c.Id.Value}|{c.NameKey}|{c.Family}|{c.Stars}|{c.Nature}|[{string.Join(",", c.Archetypes)}]|{c.Role}"
                 + $"|skill {s.Id.Value}/{s.Slot}/{s.Cooldown}/{s.Cost}/{s.Target}/{s.Pick}/{s.PickStatus}"
                 + $"|abilities {string.Join("+", s.Abilities.Select(a => a.Trigger + "{" + string.Join(",", a.Effects.Select(Fx)) + "}"))}"
                 + $"|tiers {string.Join("+", c.Tiers.Select(t => t.Level + "{" + string.Join(",", t.Effects.Select(Fx)) + "}"))}";
        }

        static void AssertSameCards(IReadOnlyList<SkillCardDef> expected, IReadOnlyList<SkillCardDef> actual)
        {
            Assert.That(actual.Count, Is.EqualTo(expected.Count));
            for (int i = 0; i < expected.Count; i++) Assert.That(Describe(actual[i]), Is.EqualTo(Describe(expected[i])), expected[i].Id.Value);
        }

        [Test, Description("Vérifie que les 48 cartes du code, passées en JSON puis relues, sont égales champ par champ — parce que le fichier de données ne doit perdre aucun champ d'aucune carte")]
        public void RoundTrip_CodeCards_ThroughJson_AreEqual()
        {
            var legacy = PrototypeCollection.LegacyCards();
            Assert.That(legacy.Count, Is.EqualTo(48));
            AssertSameCards(legacy, CardDataLibrary.FromJson(CardDataLibrary.ToJson(legacy)));
        }

        [Test, Description("Vérifie qu'une carte portant condition, modificateurs à échelle, ciblage fin, deux archétypes et un palier conditionnel survit au JSON — parce que les cartes de la vague 1 en dépendront")]
        public void RoundTrip_RichCard_KeepsConditionsModsPickArchetypesAndTiers()
        {
            var effect = new EffectDef(EffectType.Damage, 100, StatusType.None, 0, 60,
                new Condition(ConditionKind.TargetHasStatus, StatusType.Burn),
                new[]
                {
                    new ValueMod(30, new Condition(ConditionKind.TargetHpBelow, StatusType.None, 50)),
                    new ValueMod(10, null, ScaleKind.TargetStatusCount, 4),
                });
            var tier = new EffectDef(EffectType.ApplyStatus, 0, StatusType.Mark, 2, 100, new Condition(ConditionKind.EnemiesAliveAtLeast, StatusType.None, 2));
            var skill = new SkillDef(new SkillId("card_rich"), SkillSlot.A3, 4, TargetRule.SingleEnemy, new[] { effect }, 35, TargetPick.StatusHolder, StatusType.Burn);
            var card = new SkillCardDef(new CardId("card_rich"), "cards.card_rich.name", Family.Elemental, 3, skill, CardNature.Offensive,
                new[] { new CardTier(3, tier) }, new[] { Archetype.Burn, Archetype.Hunt }, CardRole.Payoff);

            var back = CardDataLibrary.FromJson(CardDataLibrary.ToJson(new[] { card }));

            AssertSameCards(new[] { card }, back);
            Assert.That(back[0].Skill.Effects[0].Mods[1].Per, Is.EqualTo(ScaleKind.TargetStatusCount));
            Assert.That(back[0].Skill.Effects[0].Mods[0].Per, Is.Null);
            Assert.That(back[0].Skill.Effects[0].When!.Status, Is.EqualTo(StatusType.Burn));
        }

        [Test, Description("Vérifie que le fichier cards.json livré donne les 48 cartes du code, ni plus ni moins — parce que la source des cartes change sans changer le jeu")]
        public void Cards_FromTheJsonFile_EqualTheCodeCards()
        {
            var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Content/cards");
            Assert.That(asset, Is.Not.Null, "Resources/Content/cards.json est absent : lancer le menu d'export");
            AssertSameCards(PrototypeCollection.LegacyCards(), CardDataLibrary.FromJson(asset.text));
        }

        [Test, Description("Vérifie que le catalogue du prototype prend ses cartes dans cards.json — parce que PrototypeCollection.Build doit passer par le chargeur de données")]
        public void PrototypeBuild_UsesTheLoadedCards()
        {
            AssertSameCards(PrototypeCollection.LegacyCards(), PrototypeCollection.Build().Cards);
        }

        [Test, Description("Vérifie qu'une source JSON absente ou illisible redonne les cartes du code — parce que le jeu ne doit jamais démarrer sans catalogue de cartes")]
        public void MissingFile_FallsBackToCode()
        {
            var legacy = PrototypeCollection.LegacyCards();
            AssertSameCards(legacy, CardDataLibrary.Load(() => null, () => legacy));
            AssertSameCards(legacy, CardDataLibrary.Load(() => "", () => legacy));
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("cards"));
            AssertSameCards(legacy, CardDataLibrary.Load(() => "{ ceci n'est pas du json", () => legacy));
        }

        [Test, Description("Vérifie qu'une source JSON présente remplace les cartes du code — parce que le fichier de données est la source du catalogue")]
        public void PresentJson_ReplacesTheCodeCards()
        {
            var legacy = PrototypeCollection.LegacyCards();
            var two = legacy.Take(2).ToList();
            var loaded = CardDataLibrary.Load(() => CardDataLibrary.ToJson(two), () => legacy);
            AssertSameCards(two, loaded);
        }

        [Test, Description("Vérifie que WithCards remplace les cartes du catalogue en gardant objets et boosters — parce que les objets et boosters restent dans le code")]
        public void WithCards_ReplacesOnlyTheCards()
        {
            var cat = TestContent.Build();
            var one = cat.Collection.Cards.Take(1).ToList();
            var changed = cat.WithCards(one);
            Assert.That(changed.Collection.Cards.Count, Is.EqualTo(1));
            Assert.That(changed.Collection.Items, Is.SameAs(cat.Collection.Items));
            Assert.That(changed.Collection.Boosters, Is.SameAs(cat.Collection.Boosters));
        }

        static string PlayCard(ContentCatalog catalog, string cardId, int level, ulong seed)
        {
            var card = catalog.Collection.Cards.Single(c => c.Id.Value == cardId);
            var ally = new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { card.Id }, new[] { level });
            var setup = new CombatSetup(seed, CombatRank.Normal, new[] { ally },
                                        new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5) }, true);
            var sim = new CombatSimulation(catalog, setup);
            sim.Unit(0).Energy = sim.Unit(0).MaxEnergy;
            for (int i = 0; i < MaxTicks && !sim.IsOver; i++) sim.Step(1);
            return CombatDigest.Of(sim);
        }

        [Test, Description("Vérifie que trois combats de carte joués avec le catalogue issu du JSON ont les mêmes empreintes qu'avec le catalogue en code — parce que le passage aux données ne doit changer aucun combat")]
        public void JsonCatalog_PlaysTheSameCombats()
        {
            var legacy = PrototypeCollection.LegacyCards();
            var viaCode = TestContent.Build().WithCards(legacy);
            var viaJson = TestContent.Build().WithCards(CardDataLibrary.FromJson(CardDataLibrary.ToJson(legacy)));
            foreach (var (id, level, seed) in new[] { ("card_ember", 10, 7UL), ("card_silent_takedown", 6, 11UL), ("card_storm", 3, 7UL) })
                Assert.That(PlayCard(viaJson, id, level, seed), Is.EqualTo(PlayCard(viaCode, id, level, seed)), id);
        }
    }
}
