using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Objets, cartes de compétences et boosters (plan 2026-10-10, sous-projets L et M) : règles du domaine et effets en combat.</summary>
    public sealed class CollectionRulesTests
    {
        static readonly ContentCatalog C = TestContent.Build();
        static CollectionDef Defs => C.Collection;

        static CombatSimulation Sim(AllyEntry[] allies, string[] enemies, ulong seed = 11)
            => new CombatSimulation(C, new CombatSetup(seed, CombatRank.Normal, allies,
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));

        static AllyEntry Ally(string id, ItemId[]? items = null, CardId?[]? cards = null)
            => new AllyEntry(new MemberId(id), 5, 0, 0, items, cards);

        static int Pct(CombatUnit u) => (int)System.Math.Round(u.Point * 100.0 / u.ActionPoint);

        // ------------------------------------------------------------------ Contenu

        [Test, Description("Vérifie que le prototype propose des objets de chaque rareté, trois cartes par catégorie d'agent au moins et un booster vendu — parce que la collection doit être jouable dès le départ")]
        public void PrototypeCollection_IsComplete()
        {
            foreach (ItemRarity r in System.Enum.GetValues(typeof(ItemRarity))) Assert.That(Defs.Items.Any(i => i.Rarity == r), Is.True, "objet " + r);
            foreach (Family f in System.Enum.GetValues(typeof(Family))) Assert.That(Defs.Cards.Count(c => c.Family == f), Is.GreaterThanOrEqualTo(3), "cartes " + f);
            Assert.That(Defs.Booster(BoosterDef.StandardId).PricePr, Is.GreaterThan(0));
            Assert.That(Defs.Items.Where(i => i.Price > 0).Count(), Is.GreaterThanOrEqualTo(CollectionRules.MaxItems * 2));
        }

        // ------------------------------------------------------------------ Objets

        [Test, Description("Vérifie que les bonus de statistiques d'un objet s'appliquent à l'unité en combat — parce que l'équipement doit renforcer l'agent")]
        public void Items_BoostTheCombatStats()
        {
            var plain = Sim(new[] { Ally("ancre") }, new[] { "brute" }).Unit(0);
            var kitted = Sim(new[] { Ally("ancre", new[] { new ItemId("kevlar_vest"), new ItemId("steel_knuckles") }) }, new[] { "brute" }).Unit(0);
            Assert.That(kitted.MaxHp, Is.EqualTo(plain.MaxHp * 110 / 100).Within(1));
            Assert.That(kitted.Atk, Is.EqualTo(plain.Atk * 108 / 100).Within(1));
            Assert.That(kitted.Def, Is.EqualTo(plain.Def));
        }

        [Test, Description("Vérifie que les effets spéciaux des objets agissent : départ avancé sur la frise, bouclier d'ouverture, chances de critique — parce qu'un objet rare doit changer la façon de jouer")]
        public void ItemPerks_AreApplied()
        {
            var shard = Sim(new[] { Ally("ancre", new[] { new ItemId("time_shard") }) }, new[] { "brute" }).Unit(0);
            var baseline = Sim(new[] { Ally("ancre") }, new[] { "brute" }).Unit(0);
            Assert.That(shard.StartPointBonus, Is.EqualTo(3));
            Assert.That(shard.Point, Is.GreaterThan(baseline.Point));
            var core = Sim(new[] { Ally("ancre", new[] { new ItemId("phoenix_core") }) }, new[] { "brute" }).Unit(0);
            var shield = core.Statuses.Single(s => s.Type == StatusType.Shield);
            Assert.That(shield.Value, Is.EqualTo(core.MaxHp * 20 / 100));
            var gauntlets = Sim(new[] { Ally("ancre", new[] { new ItemId("titan_gauntlets") }) }, new[] { "brute" }).Unit(0);
            Assert.That(gauntlets.CritBonusPercent, Is.EqualTo(8));
        }

        [Test, Description("Vérifie qu'un objet inconnu est ignoré et que le combat reste identique sans objet — parce qu'une sauvegarde ne doit pas casser un combat")]
        public void UnknownItems_AreIgnored_AndNoItemMeansNoChange()
        {
            var a = Sim(new[] { Ally("ancre", new[] { new ItemId("nope") }) }, new[] { "brute" }).Unit(0);
            var b = Sim(new[] { Ally("ancre") }, new[] { "brute" }).Unit(0);
            Assert.That(a.MaxHp, Is.EqualTo(b.MaxHp));
            Assert.That(a.Point, Is.EqualTo(b.Point));
        }

        // ------------------------------------------------------------------ Cartes

        [Test, Description("Vérifie qu'une carte équipée en emplacement 3 remplace la compétence lourde de l'agent et qu'une seconde carte ajoute un 4e emplacement — parce que les compétences 3 et 4 viennent des cartes")]
        public void Cards_ReplaceThirdSkill_AndAddAFourth()
        {
            var cards = new CardId?[] { new CardId("card_glass_wall"), new CardId("card_shackle") };
            var u = Sim(new[] { Ally("verre", cards: cards) }, new[] { "brute" }).Unit(0);
            Assert.That(u.Skills.Count, Is.EqualTo(4));
            Assert.That(u.Skills[2].Id.Value, Is.EqualTo("card_glass_wall"));
            Assert.That(u.Skills[3].Id.Value, Is.EqualTo("card_shackle"));
            Assert.That(CombatSimulation.ActiveSkillCount(u), Is.EqualTo(4));
            var plain = Sim(new[] { Ally("verre") }, new[] { "brute" }).Unit(0);
            Assert.That(plain.Skills.Count, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'une carte d'une autre catégorie n'est pas équipée en combat — parce qu'une carte correspond à une catégorie d'agent")]
        public void Cards_OfAnotherFamily_AreIgnored()
        {
            var u = Sim(new[] { Ally("verre", cards: new CardId?[] { new CardId("card_ember") }) }, new[] { "brute" }).Unit(0);
            Assert.That(u.Skills.Count, Is.EqualTo(3));
            Assert.That(u.Skills[2].Id.Value, Is.EqualTo("verre_a3"));
        }

        [Test, Description("Vérifie que l'agent peut lancer sa 4e compétence (carte), qu elle coûte de l énergie et le replace en fin de frise comme une compétence lourde — parce que la 4e compétence est une vraie action de combat")]
        public void FourthSkill_CanBeCast_AndCostsEnergy()
        {
            var cards = new CardId?[] { new CardId("card_glass_wall"), new CardId("card_shackle") };
            var s = Sim(new[] { Ally("verre", cards: cards) }, new[] { "brute" });
            var verre = s.Unit(0);
            var brute = s.Unit(1);
            brute.Point = 0;
            verre.Point = verre.ActionPoint;
            verre.Energy = verre.MaxEnergy;
            Assert.That(s.CanUse(verre.Id, 3), Is.True);
            s.Submit(CombatCommand.UseSkill(verre.Id, 3, brute.Id));
            var events = s.Step(1);
            Assert.That(events.OfType<SkillUsedEvent>().Single().Skill.Value, Is.EqualTo("card_shackle"));
            Assert.That(Pct(verre), Is.EqualTo(0), "poids lourd : retour à 0 %");
            Assert.That(verre.Energy, Is.LessThan(verre.MaxEnergy), "la compétence a coûté de l énergie");
            verre.Energy = 0;
            verre.Point = verre.ActionPoint;
            s.Step(1);
            Assert.That(s.CanUse(verre.Id, 3), Is.False, "plus assez d énergie");
        }

        [Test, Description("Vérifie que les compétences de l'IA ennemie et des alliés automatiques restent dans leurs emplacements existants avec la 4e compétence — parce que le mode Auto doit pouvoir lancer une carte sans erreur")]
        public void AutoMode_RunsWithCards_Deterministically()
        {
            string Run()
            {
                var cards = new CardId?[] { new CardId("card_glass_wall"), new CardId("card_shackle") };
                var s = new CombatSimulation(C, new CombatSetup(21, CombatRank.Normal, new[] { Ally("verre", cards: cards), Ally("ancre") },
                    new[] { new EnemyEntry(new EnemyId("thug"), 5) }, true));
                var log = new System.Text.StringBuilder();
                for (int i = 0; i < 4000 && !s.IsOver; i++) foreach (var e in s.Step(1)) if (e is SkillUsedEvent su) log.Append(su.Skill.Value).Append(';');
                return log + "|" + s.IsOver;
            }
            var first = Run();
            Assert.That(first, Does.EndWith("True"), "le combat se termine");
            Assert.That(Run(), Is.EqualTo(first));
        }

        // ------------------------------------------------------------------ Tirages

        [Test, Description("Vérifie que la rareté tirée suit les poids et que le tirage est déterministe — parce que les boosters doivent être reproductibles par un serveur")]
        public void RarityRoll_FollowsWeights_Deterministically()
        {
            var rng = new Pcg32(5);
            var counts = new int[4];
            for (int i = 0; i < 4000; i++) counts[(int)CollectionRules.RollRarity(rng, new[] { 70, 22, 7, 1 })]++;
            Assert.That(counts[0], Is.GreaterThan(counts[1]));
            Assert.That(counts[1], Is.GreaterThan(counts[2]));
            Assert.That(counts[2], Is.GreaterThan(counts[3]));
            Assert.That(counts[0], Is.InRange(2600, 3000));
            var a = new Pcg32(9); var b = new Pcg32(9);
            for (int i = 0; i < 50; i++) Assert.That(CollectionRules.RollRarity(a, new[] { 50, 50, 0, 0 }), Is.EqualTo(CollectionRules.RollRarity(b, new[] { 50, 50, 0, 0 })));
        }

        [Test, Description("Vérifie qu'un booster tire le bon nombre de cartes, de façon déterministe — parce que la graine fixe le résultat")]
        public void Booster_DrawsItsCards_Deterministically()
        {
            var booster = Defs.Booster(BoosterDef.StandardId);
            var a = CollectionRules.OpenBooster(Defs, booster, 123UL, 0);
            var b = CollectionRules.OpenBooster(Defs, booster, 123UL, 0);
            Assert.That(a.Cards.Count, Is.EqualTo(booster.Cards));
            Assert.That(a.Cards.Select(c => c.Value), Is.EqualTo(b.Cards.Select(c => c.Value)));
            Assert.That(a.Cards.All(c => Defs.HasCard(c)), Is.True);
        }

        [Test, Description("Vérifie que la pitié garantit une carte Rare ou mieux au bout de N boosters sans — parce que la malchance ne doit pas durer")]
        public void Booster_Pity_GuaranteesARareCard()
        {
            var commonOnly = new BoosterDef("test", "boosters.test", 3, new[] { 100, 0, 0, 0 }, 3, 0);
            var defs = new CollectionDef(Defs.Items, Defs.Cards, new[] { commonOnly });
            int pity = 0;
            bool guaranteed = false;
            for (int i = 0; i < 3; i++)
            {
                var o = CollectionRules.OpenBooster(defs, commonOnly, (ulong)(100 + i), pity);
                bool hasRare = o.Cards.Any(c => defs.Card(c).Rarity >= ItemRarity.Rare);
                if (i < 2) { Assert.That(hasRare, Is.False, "booster " + i); Assert.That(o.Pity, Is.EqualTo(i + 1)); }
                else { guaranteed = hasRare; Assert.That(o.Pity, Is.EqualTo(0), "pitié remise à zéro"); }
                pity = o.Pity;
            }
            Assert.That(guaranteed, Is.True);
        }

        [Test, Description("Vérifie que les offres d'objets du jour sont les mêmes pour un même jour, changent d'un jour à l'autre et ne contiennent que des objets vendus — parce que la boutique tourne chaque jour")]
        public void DailyItemOffers_RotateByDay()
        {
            var d1 = CollectionRules.DailyItemOffers(Defs, "2026-10-10", 4);
            var d1b = CollectionRules.DailyItemOffers(Defs, "2026-10-10", 4);
            Assert.That(d1.Select(i => i.Id.Value), Is.EqualTo(d1b.Select(i => i.Id.Value)));
            Assert.That(d1.Select(i => i.Id.Value).Distinct().Count(), Is.EqualTo(4));
            Assert.That(d1.All(i => i.Price > 0), Is.True);
            var days = Enumerable.Range(11, 10).Select(d => string.Join(",", CollectionRules.DailyItemOffers(Defs, "2026-10-" + d, 4).Select(i => i.Id.Value))).Distinct().Count();
            Assert.That(days, Is.GreaterThan(3), "les offres changent d'un jour à l'autre");
        }

        // ------------------------------------------------------------------ Catégories (lot B 0.14.0)

        static SkillDef Skill(TargetRule target, params EffectDef[] fx) => new SkillDef(new SkillId("test"), SkillSlot.A3, 5, target, fx);

        [TestCase(EffectType.Damage, 100, StatusType.None, TargetRule.SingleEnemy, CardNature.Offensive)]
        [TestCase(EffectType.ApplyStatus, 0, StatusType.Bind, TargetRule.SingleEnemy, CardNature.Offensive)]
        [TestCase(EffectType.PushBack, 25, StatusType.None, TargetRule.AllEnemies, CardNature.Offensive)]
        [TestCase(EffectType.GaugeDelta, -20, StatusType.None, TargetRule.SingleEnemy, CardNature.Offensive)]
        [TestCase(EffectType.Shield, 15, StatusType.Shield, TargetRule.AllAllies, CardNature.Defensive)]
        [TestCase(EffectType.Heal, 35, StatusType.None, TargetRule.SingleAlly, CardNature.Support)]
        [TestCase(EffectType.ApplyStatus, 0, StatusType.Haste, TargetRule.Self, CardNature.Support)]
        [TestCase(EffectType.Advance, 50, StatusType.None, TargetRule.SingleAlly, CardNature.Support)]
        [TestCase(EffectType.GaugeDelta, 20, StatusType.None, TargetRule.AllAllies, CardNature.Support)]
        [Description("Vérifie que la nature déduite d'une carte suit son effet de base et sa cible (bouclier : défensive ; soin ou aide à un allié : soutien ; action sur l'ennemi : offensive) — parce que cette déduction pré-remplit la nature de chaque carte")]
        public void CardNatures_Of_FollowsTheBaseEffectAndTarget(EffectType type, int value, StatusType status, TargetRule target, CardNature expected)
        {
            Assert.That(CardNatures.Of(Skill(target, new EffectDef(type, value, status, 2))), Is.EqualTo(expected));
        }

        [Test, Description("Vérifie qu'une compétence sans effet est classée offensive — parce que la déduction doit toujours rendre une nature")]
        public void CardNatures_Of_NoEffect_IsOffensive()
        {
            Assert.That(CardNatures.Of(Skill(TargetRule.SingleEnemy)), Is.EqualTo(CardNature.Offensive));
        }

        [Test, Description("Vérifie qu'une carte garde la nature saisie, et prend la nature déduite quand rien n'est saisi — parce que la nature est un champ saisi, pré-rempli par la déduction")]
        public void SkillCardDef_Nature_IsTheGivenOneOrTheDeducedOne()
        {
            var hit = Skill(TargetRule.SingleEnemy, new EffectDef(EffectType.Damage, 100));
            Assert.That(new SkillCardDef(new CardId("a"), "a", Family.Matter, ItemRarity.Common, hit).Nature, Is.EqualTo(CardNature.Offensive));
            Assert.That(new SkillCardDef(new CardId("b"), "b", Family.Matter, ItemRarity.Common, hit, CardNature.Support).Nature, Is.EqualTo(CardNature.Support));
        }

        // ------------------------------------------------------------------ Paliers (lot C1)

        static readonly SkillCardDef[] Catalog = PrototypeCollection.Build().Cards.ToArray();

        [Test, Description("Vérifie que chaque carte du catalogue a exactement trois paliers, aux niveaux 3, 6 et 10, chacun avec au moins un effet — parce que les effets secondaires se débloquent à ces paliers")]
        public void EveryCard_HasThreeTiers_At3_6_10()
        {
            foreach (var c in Catalog)
            {
                Assert.That(c.Tiers.Select(t => t.Level), Is.EqualTo(new[] { 3, 6, 10 }), c.Id.Value);
                Assert.That(c.Tiers.All(t => t.Effects.Count > 0), Is.True, c.Id.Value);
            }
        }

        // ------------------------------------------------------------------ Catalogue de 48 cartes (lot C2)

        [Test, Description("Vérifie que le catalogue compte 48 cartes aux identifiants uniques, 12 par famille et 4 par nature dans chaque famille — parce que le deck building demande un vrai choix dans chaque case")]
        public void Catalog_Has48Cards_12PerFamily_4PerNature()
        {
            Assert.That(Catalog.Length, Is.EqualTo(48));
            Assert.That(Catalog.Select(c => c.Id.Value).Distinct().Count(), Is.EqualTo(48));
            foreach (Family f in System.Enum.GetValues(typeof(Family)))
            {
                var fam = Catalog.Where(c => c.Family == f).ToList();
                Assert.That(fam.Count, Is.EqualTo(12), f.ToString());
                foreach (CardNature n in System.Enum.GetValues(typeof(CardNature)))
                    Assert.That(fam.Count(c => c.Nature == n), Is.EqualTo(4), f + " " + n);
            }
        }

        [Test, Description("Vérifie que chaque famille a 5 cartes Communes, 4 Rares, 2 Épiques et 1 Légendaire — parce que la répartition suit les taux des boosters")]
        public void Catalog_Rarities_Are5_4_2_1_PerFamily()
        {
            foreach (Family f in System.Enum.GetValues(typeof(Family)))
            {
                var fam = Catalog.Where(c => c.Family == f).ToList();
                Assert.That(new[] { ItemRarity.Common, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary }.Select(r => fam.Count(c => c.Rarity == r)),
                            Is.EqualTo(new[] { 5, 4, 2, 1 }), f.ToString());
            }
        }

        [Test, Description("Vérifie que la nature saisie de chaque carte est celle déduite de son effet de base — parce que la spec du catalogue l'exige pour les 48 cartes")]
        public void EveryCard_NatureEqualsItsDeduction()
        {
            foreach (var c in Catalog) Assert.That(c.Nature, Is.EqualTo(CardNatures.Of(c.Skill)), c.Id.Value);
        }

        [Test, Description("Vérifie qu'aucune carte lancée sur soi ne compte sur un élan ou un gain de jauge — parce que l'élan ignore le lanceur et l'effet serait perdu")]
        public void SelfCards_UseNoAdvance()
        {
            foreach (var c in Catalog.Where(c => c.Skill.Target == TargetRule.Self))
                Assert.That(c.Skill.Effects.Concat(c.Tiers.SelectMany(t => t.Effects)).Any(e => e.Type == EffectType.Advance || e.Type == EffectType.GaugeDelta),
                            Is.False, c.Id.Value);
        }

        [Test, Description("Vérifie quelques lignes de la table de la spec (Foudre en chaîne, Flux électrique, Faille de garde, Armure de rivets) — parce que le contenu doit suivre la table validée")]
        public void NewCards_MatchTheSpec()
        {
            SkillCardDef Card(string id) => Catalog.Single(c => c.Id.Value == id);
            var chain = Card("card_chain_lightning");
            Assert.That((chain.Rarity, chain.Skill.Target, chain.Skill.Cooldown), Is.EqualTo((ItemRarity.Legendary, TargetRule.SingleEnemy, 7)));
            Assert.That(chain.Skill.Effects.Select(e => (e.Type, e.Value, e.Status, e.ChancePercent)),
                        Is.EqualTo(new[] { (EffectType.Damage, 160, StatusType.None, 100), (EffectType.ApplyStatus, 0, StatusType.Shock, 40) }));
            Assert.That(chain.Tiers[1].Effects[0].Status, Is.EqualTo(StatusType.Vulnerable));
            var flux = Card("card_power_flux");
            Assert.That((flux.Skill.Target, flux.Skill.Effects[0].Type, flux.Skill.Effects[0].Value), Is.EqualTo((TargetRule.AllAllies, EffectType.GaugeDelta, 100)));
            Assert.That(Card("card_guard_break").Skill.Effects.Select(e => e.Type),
                        Is.EqualTo(new[] { EffectType.Damage, EffectType.Purge, EffectType.ApplyStatus }));
            var rivets = Card("card_rivet_armor");
            Assert.That((rivets.Skill.Target, rivets.Tiers[0].Effects[0].Status), Is.EqualTo((TargetRule.Self, StatusType.Taunt)));
        }

        static readonly StatusType[] AllyStatuses = { StatusType.Shield, StatusType.Regen, StatusType.Haste, StatusType.Evade, StatusType.Anchor, StatusType.Taunt };

        [Test, Description("Vérifie qu'aucune carte visant un ennemi ne porte d'effet allié (soin, bouclier, élan, bonus) dans sa base ou ses paliers — parce qu'un effet s'applique aux cibles de la carte et irait renforcer l'ennemi")]
        public void EnemyCards_CarryNoAllyEffects()
        {
            foreach (var c in Catalog.Where(c => c.Skill.Target == TargetRule.SingleEnemy || c.Skill.Target == TargetRule.AllEnemies))
                foreach (var fx in c.Skill.Effects.Concat(c.Tiers.SelectMany(t => t.Effects)))
                {
                    bool ally = fx.Type == EffectType.Heal || fx.Type == EffectType.Shield || fx.Type == EffectType.Advance
                             || fx.Type == EffectType.ApplyStatus && AllyStatuses.Contains(fx.Status);
                    Assert.That(ally, Is.False, c.Id.Value + " : " + fx.Type + " " + fx.Status);
                }
        }

        [Test, Description("Vérifie que le rebond n'apparaît que sur des cartes qui infligent des dégâts — parce qu'un rebond sans dégâts ne fait rien")]
        public void Bounce_OnlyOnDamagingCards()
        {
            foreach (var c in Catalog.Where(c => c.Tiers.SelectMany(t => t.Effects).Any(e => e.Type == EffectType.Bounce)))
                Assert.That(c.Skill.Effects.Any(e => e.Type == EffectType.Damage), Is.True, c.Id.Value);
        }

        [Test, Description("Vérifie quelques paliers de la table de la spec (Braise niv. 6 : rebond 50 % ; Second souffle niv. 3 : purge totale ; Brasier niv. 10 : exécution +40 % ; Mur de verre niv. 6 : Ancrage 1 t) — parce que le contenu doit suivre la table validée")]
        public void TierTable_MatchesTheSpec()
        {
            EffectDef At(string card, int level) => Catalog.Single(c => c.Id.Value == card).Tiers.Single(t => t.Level == level).Effects[0];
            Assert.That((At("card_ember", 6).Type, At("card_ember", 6).Value), Is.EqualTo((EffectType.Bounce, 50)));
            Assert.That((At("card_second_wind", 3).Type, At("card_second_wind", 3).Value), Is.EqualTo((EffectType.Purge, 0)));
            Assert.That((At("card_inferno", 10).Type, At("card_inferno", 10).Value), Is.EqualTo((EffectType.Execute, 40)));
            var anchor = At("card_glass_wall", 6);
            Assert.That((anchor.Type, anchor.Status, anchor.Duration), Is.EqualTo((EffectType.ApplyStatus, StatusType.Anchor, 1)));
        }

        [Test, Description("Vérifie que le type déduit d'un objet suit d'abord son effet spécial (puissance : implant, critique : arme, bouclier d'ouverture : armure, jauge : outil) — parce que l'effet dit à quoi sert l'objet, même quand une autre stat est plus forte")]
        public void ItemTypes_Of_UsesThePerkFirst()
        {
            Assert.That(ItemTypes.Of(ItemPerk.SkillPower, 0, 4, 0, 0), Is.EqualTo(ItemType.Implant));
            Assert.That(ItemTypes.Of(ItemPerk.CritChance, 0, 12, 0, 0), Is.EqualTo(ItemType.Weapon));
            Assert.That(ItemTypes.Of(ItemPerk.OpeningShield, 15, 10, 0, 0), Is.EqualTo(ItemType.Armor), "comme le Cœur de Phénix");
            Assert.That(ItemTypes.Of(ItemPerk.StartGauge, 0, 0, 0, 8), Is.EqualTo(ItemType.Tool));
        }

        [Test, Description("Vérifie que, sans effet spécial, le type suit la stat la plus forte (ATQ : arme, PV ou DEF : armure, VIT : outil), avec l'ATQ puis PV/DEF en cas d'égalité — parce que tout objet doit avoir un type sans ambiguïté")]
        public void ItemTypes_Of_UsesTheStrongestStatWithoutPerk()
        {
            Assert.That(ItemTypes.Of(ItemPerk.None, 0, 8, 0, 0), Is.EqualTo(ItemType.Weapon));
            Assert.That(ItemTypes.Of(ItemPerk.None, 10, 0, 0, 0), Is.EqualTo(ItemType.Armor));
            Assert.That(ItemTypes.Of(ItemPerk.None, 0, 0, 10, 0), Is.EqualTo(ItemType.Armor));
            Assert.That(ItemTypes.Of(ItemPerk.None, 0, 0, 0, 6), Is.EqualTo(ItemType.Tool));
            Assert.That(ItemTypes.Of(ItemPerk.None, 0, 10, 10, 0), Is.EqualTo(ItemType.Weapon), "égalité ATQ / DEF");
            Assert.That(ItemTypes.Of(ItemPerk.None, 8, 0, 0, 8), Is.EqualTo(ItemType.Armor), "égalité PV / VIT");
            Assert.That(ItemTypes.Of(ItemPerk.None, 0, 0, 0, 0), Is.EqualTo(ItemType.Weapon), "aucune stat");
        }

        [Test, Description("Vérifie qu'un objet garde le type saisi et prend le type déduit quand rien n'est saisi — parce que le type est un champ saisi, pré-rempli par la déduction")]
        public void ItemDef_Type_IsTheGivenOneOrTheDeducedOne()
        {
            Assert.That(new ItemDef(new ItemId("a"), "a", ItemRarity.Common, def: 10).Type, Is.EqualTo(ItemType.Armor));
            Assert.That(new ItemDef(new ItemId("b"), "b", ItemRarity.Common, def: 10, type: ItemType.Tool).Type, Is.EqualTo(ItemType.Tool));
        }

        [Test, Description("Vérifie qu'un agent ne peut pas porter deux objets du même type, mais deux types différents — parce que les deux emplacements doivent forcer de vrais choix")]
        public void CanEquip_RefusesASecondItemOfTheSameType()
        {
            var armor = new ItemDef(new ItemId("a1"), "a1", ItemRarity.Common, hp: 10);
            var armor2 = new ItemDef(new ItemId("a2"), "a2", ItemRarity.Common, def: 10);
            var weapon = new ItemDef(new ItemId("w"), "w", ItemRarity.Common, atk: 8);
            Assert.That(CollectionRules.CanEquip(armor2, new[] { armor }), Is.False);
            Assert.That(CollectionRules.CanEquip(armor2, new[] { weapon }), Is.True);
            Assert.That(CollectionRules.CanEquip(armor2, new ItemDef[0]), Is.True);
        }
    }
}
