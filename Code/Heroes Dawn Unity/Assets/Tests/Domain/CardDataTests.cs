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

        [Test, Description("Vérifie que le fichier cards.json livré commence par les 48 cartes du code, à l identique et dans le même ordre, suivies des 12 cartes de la première vague — parce que la source des cartes change sans changer les cartes d avant P1")]
        public void Cards_FromTheJsonFile_EqualTheCodeCards()
        {
            var asset = UnityEngine.Resources.Load<UnityEngine.TextAsset>("Content/cards");
            Assert.That(asset, Is.Not.Null, "Resources/Content/cards.json est absent : lancer le menu d'export");
            var fromFile = CardDataLibrary.FromJson(asset.text);
            Assert.That(fromFile.Count, Is.EqualTo(60));
            AssertSameCards(PrototypeCollection.LegacyCards(), fromFile.Take(48).ToList());
        }

        [Test, Description("Vérifie que le catalogue du prototype prend ses cartes dans cards.json (ses 48 premières sont celles du code) — parce que PrototypeCollection.Build doit passer par le chargeur de données")]
        public void PrototypeBuild_UsesTheLoadedCards()
        {
            AssertSameCards(PrototypeCollection.LegacyCards(), PrototypeCollection.Build().Cards.Take(48).ToList());
        }

        [Test, Description("Vérifie qu'une source JSON absente ou illisible redonne les cartes du code — parce que le jeu ne doit jamais démarrer sans catalogue de cartes")]
        public void MissingFile_FallsBackToCode()
        {
            var legacy = PrototypeCollection.LegacyCards();
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("cards.json"));
            AssertSameCards(legacy, CardDataLibrary.Load(() => null, () => legacy));
            UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("cards.json"));
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

        // ---- Validation du chargeur de cartes ----

        static SkillCardDef Card(string id, Family family = Family.Matter, EffectDef? effect = null, CardRole role = CardRole.Basic, Archetype[]? archetypes = null)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", family, 1,
                                new SkillDef(new SkillId(id), SkillSlot.A3, 4, TargetRule.SingleEnemy, new[] { effect ?? new EffectDef(EffectType.Damage, 100) }, 40),
                                CardNature.Offensive, null, archetypes ?? new Archetype[0], role);

        const string OldRecordJson = "{\"cards\":[{\"id\":\"card_old\",\"family\":0,\"stars\":1,\"nature\":0,\"role\":0,"
            + "\"skill\":{\"id\":\"card_old\",\"slot\":2,\"cost\":40,\"cooldown\":4,\"target\":0,\"effects\":[{\"type\":0,\"value\":100,\"status\":0,\"duration\":0,\"chance\":100}]},"
            + "\"tiers\":[{\"level\":3,\"effects\":[{\"type\":0,\"value\":20,\"status\":0,\"duration\":0,\"chance\":100}]}]}]}";

        static void ExpectCardsWarning(string pattern = "cards.json")
            => UnityEngine.TestTools.LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex(pattern));

        [Test, Description("Vérifie qu'un ancien enregistrement sans les champs de condition, de modificateurs ni de ciblage fin se relit en effet sans condition ni modificateur — parce que les fichiers cards.json déjà écrits doivent rester lisibles")]
        public void OldRecord_WithoutTheNewFields_ReadsAsAnUnconditionalEffect()
        {
            var cards = CardDataLibrary.FromJson(OldRecordJson);
            Assert.That(cards.Count, Is.EqualTo(1));
            var fx = cards[0].Skill.Effects[0];
            Assert.That(fx.When, Is.Null);
            Assert.That(fx.Mods, Is.Empty);
            Assert.That(cards[0].Skill.Pick, Is.EqualTo(TargetPick.Default));
            Assert.That(cards[0].Tiers[0].Effects[0].When, Is.Null);
            Assert.That(cards[0].Tiers[0].Effects[0].Mods, Is.Empty);
            Assert.That(CardDataLibrary.Validate(cards), Is.Empty);
        }

        [Test, Description("Vérifie que {\"cards\":[]} avertit puis retombe sur les cartes du code — parce qu'un catalogue vide rendrait le jeu injouable")]
        public void EmptyCardList_WarnsThenFallsBack()
        {
            var legacy = PrototypeCollection.LegacyCards();
            ExpectCardsWarning();
            AssertSameCards(legacy, CardDataLibrary.Load(() => "{\"cards\":[]}", () => legacy));
        }

        [Test, Description("Vérifie qu'un fichier cards.json absent ou vide avertit avant le repli sur le code — parce qu'un repli silencieux en build cacherait un catalogue manquant")]
        public void AbsentOrBlankFile_WarnsBeforeFallingBack()
        {
            var legacy = PrototypeCollection.LegacyCards();
            ExpectCardsWarning();
            AssertSameCards(legacy, CardDataLibrary.Load(() => null, () => legacy));
            ExpectCardsWarning();
            AssertSameCards(legacy, CardDataLibrary.Load(() => "  ", () => legacy));
        }

        [Test, Description("Vérifie que Validate signale un identifiant vide et un identifiant en double, et que Load se replie sur le code avec un avertissement par erreur — parce que deux cartes de même id écraseraient l'une l'autre")]
        public void DuplicateOrEmptyId_IsReportedAndFallsBack()
        {
            var errors = CardDataLibrary.Validate(new[] { Card("card_a"), Card("card_a"), Card("") });
            Assert.That(errors.Count, Is.EqualTo(2));
            Assert.That(errors.Any(e => e.Contains("card_a")), Is.True);

            var legacy = PrototypeCollection.LegacyCards();
            var json = CardDataLibrary.ToJson(new[] { Card("card_a"), Card("card_a") });
            ExpectCardsWarning("card_a");
            AssertSameCards(legacy, CardDataLibrary.Load(() => json, () => legacy));
        }

        [Test, Description("Vérifie que Validate signale une valeur d'enum hors plage (famille, rôle, archétype, type d'effet, statut, condition, échelle, ciblage) et que Load se replie — parce qu'une valeur inconnue serait ignorée en silence par le moteur")]
        public void OutOfRangeEnum_IsReportedAndFallsBack()
        {
            Assert.That(CardDataLibrary.Validate(new[] { Card("c1", (Family)9) }), Has.Count.EqualTo(1));
            Assert.That(CardDataLibrary.Validate(new[] { Card("c2", role: (CardRole)9) }), Has.Count.EqualTo(1));
            Assert.That(CardDataLibrary.Validate(new[] { Card("c3", archetypes: new[] { (Archetype)9 }) }), Has.Count.EqualTo(1));
            Assert.That(CardDataLibrary.Validate(new[] { Card("c4", effect: new EffectDef((EffectType)99, 10)) }), Has.Count.EqualTo(1));
            Assert.That(CardDataLibrary.Validate(new[] { Card("c5", effect: new EffectDef(EffectType.ApplyStatus, 0, (StatusType)99, 2)) }), Has.Count.EqualTo(1));
            Assert.That(CardDataLibrary.Validate(new[] { Card("c6", effect: new EffectDef(EffectType.Damage, 10, StatusType.None, 0, 100, new Condition((ConditionKind)99))) }), Has.Count.EqualTo(1));
            Assert.That(CardDataLibrary.Validate(new[] { Card("c7", effect: new EffectDef(EffectType.Damage, 10, StatusType.None, 0, 100, null, new[] { new ValueMod(5, null, (ScaleKind)99, 2) })) }), Has.Count.EqualTo(1));
            var badSkill = new SkillCardDef(new CardId("c8"), "cards.c8.name", Family.Matter, 1,
                new SkillDef(new SkillId("c8"), SkillSlot.A3, 4, (TargetRule)9, new[] { new EffectDef(EffectType.Damage, 10) }, 40, (TargetPick)9));
            Assert.That(CardDataLibrary.Validate(new[] { badSkill }).Count, Is.EqualTo(2));
            Assert.That(CardDataLibrary.Validate(new[] { Card("ok", Family.Elemental, null, CardRole.Payoff, new[] { Archetype.Burn }) }), Is.Empty);

            var legacy = PrototypeCollection.LegacyCards();
            var json = CardDataLibrary.ToJson(new[] { Card("card_bad", (Family)9) });
            ExpectCardsWarning("card_bad");
            AssertSameCards(legacy, CardDataLibrary.Load(() => json, () => legacy));
        }

        static SkillCardDef WithTiers(string id, params CardTier[] tiers)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", Family.Matter, 1,
                                new SkillDef(new SkillId(id), SkillSlot.A3, 4, TargetRule.SingleEnemy, new[] { new EffectDef(EffectType.Damage, 100) }, 40),
                                CardNature.Offensive, tiers, new Archetype[0], CardRole.Basic);

        [Test, Description("Vérifie que Validate refuse une condition sur Lifesteal, Bounce, Execute ou Purge (en base comme en palier) mais l'accepte sur Damage — parce que le moteur ne lit pas la condition de ces effets de la même façon et l'ignorerait ou la lirait mal")]
        public void Validate_RejectsAConditionOnUnsupportedEffects()
        {
            var cond = new Condition(ConditionKind.TargetHasStatus, StatusType.Burn);
            foreach (var type in new[] { EffectType.Lifesteal, EffectType.Bounce, EffectType.Execute, EffectType.Purge })
            {
                Assert.That(CardDataLibrary.Validate(new[] { Card("c_" + type, effect: new EffectDef(type, 10, StatusType.None, 0, 100, cond)) }), Has.Count.EqualTo(1), "base " + type);
                Assert.That(CardDataLibrary.Validate(new[] { WithTiers("t_" + type, new CardTier(3, new EffectDef(type, 10, StatusType.None, 0, 100, cond))) }), Has.Count.EqualTo(1), "palier " + type);
            }
            Assert.That(CardDataLibrary.Validate(new[] { Card("c_dmg", effect: new EffectDef(EffectType.Damage, 10, StatusType.None, 0, 100, cond)) }), Is.Empty);
        }

        [Test, Description("Vérifie que Validate refuse un modificateur de valeur sur un type autre que Damage, Heal, Shield, Advance, PushBack, GaugeDelta — parce que le moteur ne lit les modificateurs que sur ces effets")]
        public void Validate_RejectsAModifierOnUnsupportedEffects()
        {
            var mod = new[] { new ValueMod(5) };
            foreach (var type in new[] { EffectType.ApplyStatus, EffectType.Lifesteal, EffectType.Purge, EffectType.Bounce, EffectType.Execute })
                Assert.That(CardDataLibrary.Validate(new[] { Card("c_" + type, effect: new EffectDef(type, 10, StatusType.None, 0, 100, null, mod)) }), Has.Count.EqualTo(1), type.ToString());
            foreach (var type in new[] { EffectType.Damage, EffectType.Heal, EffectType.Shield, EffectType.Advance, EffectType.PushBack, EffectType.GaugeDelta })
                Assert.That(CardDataLibrary.Validate(new[] { Card("ok_" + type, effect: new EffectDef(type, 10, StatusType.None, 0, 100, null, mod)) }), Is.Empty, type.ToString());
        }

        [Test, Description("Vérifie que Validate refuse un maxCount non nul sur un modificateur sans échelle — parce que seul un modificateur à échelle plafonne un compte")]
        public void Validate_RejectsAMaxCountWithoutAScale()
        {
            var bad = new EffectDef(EffectType.Damage, 10, StatusType.None, 0, 100, null, new[] { new ValueMod(5, null, null, 3) });
            Assert.That(CardDataLibrary.Validate(new[] { Card("c_max", effect: bad) }), Has.Count.EqualTo(1));
            var scaled = new EffectDef(EffectType.Damage, 10, StatusType.None, 0, 100, null, new[] { new ValueMod(5, null, ScaleKind.EnemiesAlive, 3) });
            Assert.That(CardDataLibrary.Validate(new[] { Card("c_ok", effect: scaled) }), Is.Empty);
            Assert.Throws<System.ArgumentException>(() => new ValueMod(5, null, ScaleKind.EnemiesAlive, 0), "une échelle sans plafond ne se construit même pas");
        }

        [Test, Description("Vérifie que Validate refuse un palier de niveau autre que 3, 6 ou 10 et un niveau répété — parce que les paliers se débloquent à ces seuls niveaux et un doublon s'appliquerait deux fois")]
        public void Validate_RejectsBadTierLevels()
        {
            var fx = new EffectDef(EffectType.Damage, 10);
            Assert.That(CardDataLibrary.Validate(new[] { WithTiers("t_ok", new CardTier(3, fx), new CardTier(6, fx), new CardTier(10, fx)) }), Is.Empty);
            Assert.That(CardDataLibrary.Validate(new[] { WithTiers("t_4", new CardTier(4, fx)) }), Has.Count.EqualTo(1), "niveau 4");
            Assert.That(CardDataLibrary.Validate(new[] { WithTiers("t_0", new CardTier(0, fx)) }), Has.Count.EqualTo(1), "niveau 0");
            Assert.That(CardDataLibrary.Validate(new[] { WithTiers("t_dup", new CardTier(3, fx), new CardTier(3, fx)) }), Has.Count.EqualTo(1), "niveau répété");
        }

        [Test, Description("Vérifie que Validate accepte les cartes du code et refuse une liste vide — parce que le catalogue livré doit toujours passer la validation et qu'un catalogue vide est une erreur")]
        public void Validate_AcceptsTheCodeCards_AndRejectsAnEmptyList()
        {
            Assert.That(CardDataLibrary.Validate(PrototypeCollection.LegacyCards()), Is.Empty);
            Assert.That(CardDataLibrary.Validate(new SkillCardDef[0]), Has.Count.EqualTo(1));
        }

        static string EnumTable<T>() where T : struct, System.Enum
            => string.Join(",", System.Enum.GetValues(typeof(T)).Cast<T>().Select(v => v + "=" + System.Convert.ToInt32(v)));

        [Test, Description("Vérifie la table nom → valeur entière de chaque enum sérialisé dans cards.json — parce que renuméroter une valeur changerait en silence le sens de toutes les cartes déjà écrites")]
        public void SerializedEnums_AreStable()
        {
            // Les enums sont sérialisés en entiers dans cards.json : n'ajouter une valeur qu'en fin d'enum.
            Assert.That(EnumTable<StatusType>(), Is.EqualTo("None=0,Burn=1,Freeze=2,Shock=3,Corrosion=4,Blind=5,Vulnerable=6,Bind=7,Mark=8,Taunt=9,Shield=10,Regen=11,Haste=12,Evade=13,Anchor=14"));
            Assert.That(EnumTable<EffectType>(), Is.EqualTo("Damage=0,Heal=1,ApplyStatus=2,GaugeDelta=3,Shield=4,PushBack=5,Advance=6,Lifesteal=7,Purge=8,Bounce=9,Execute=10,ArmReaction=11"));
            Assert.That(EnumTable<ReactionTrigger>(), Is.EqualTo("EnemyReceivesStatus=0,AllyReceivesStatus=1,EnemyDefeated=2,AllyDefeated=3,CarrierHit=4,CarrierCrit=5"));
            Assert.That(EnumTable<TargetRule>(), Is.EqualTo("SingleEnemy=0,AllEnemies=1,SingleAlly=2,AllAllies=3,Self=4"));
            Assert.That(EnumTable<TargetPick>(), Is.EqualTo("Default=0,LowestHp=1,HighestHp=2,StatusHolder=3,NextToAct=4,Random=5,TwoRandom=6"));
            Assert.That(EnumTable<ConditionKind>(), Is.EqualTo("TargetHasStatus=0,TargetLacksStatus=1,TargetHpBelow=2,TargetHpAbove=3,CasterHasStatus=4,CasterHpBelow=5,AnyAllyHasStatus=6,TargetStatusCountAtLeast=7,TargetPlannedAction=8,EnemiesAliveAtLeast=9,EnemiesAliveAtMost=10"));
            Assert.That(EnumTable<ScaleKind>(), Is.EqualTo("TargetStatusCount=0,EnemiesAlive=1,AlliesAlive=2,TargetMissingHpTenths=3,CasterMissingHpTenths=4"));
            Assert.That(EnumTable<Archetype>(), Is.EqualTo("Burn=0,Restrain=1,Hunt=2,Tempo=3,Bulwark=4,Renewal=5"));
            Assert.That(EnumTable<CardRole>(), Is.EqualTo("Primer=0,Payoff=1,Bridge=2,Basic=3"));
            Assert.That(EnumTable<Family>(), Is.EqualTo("Matter=0,Elemental=1,PhysicalBio=2,Perception=3"));
            Assert.That(EnumTable<CardNature>(), Is.EqualTo("Offensive=0,Defensive=1,Support=2,Control=3"));
            Assert.That(EnumTable<SkillSlot>(), Is.EqualTo("A1=0,A2=1,A3=2,Passive=3,A4=4"));
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
