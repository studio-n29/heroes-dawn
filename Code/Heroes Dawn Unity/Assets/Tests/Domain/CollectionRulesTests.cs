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

        [Test, Description("Vérifie que l'agent peut lancer sa 4e compétence (carte), qu'elle se recharge et le replace en fin de frise comme une compétence lourde — parce que la 4e compétence est une vraie action de combat")]
        public void FourthSkill_CanBeCast_AndCoolsDown()
        {
            var cards = new CardId?[] { new CardId("card_glass_wall"), new CardId("card_shackle") };
            var s = Sim(new[] { Ally("verre", cards: cards) }, new[] { "brute" });
            var verre = s.Unit(0);
            var brute = s.Unit(1);
            brute.Point = 0;
            verre.Point = verre.ActionPoint;
            Assert.That(s.CanUse(verre.Id, 3), Is.True);
            s.Submit(CombatCommand.UseSkill(verre.Id, 3, brute.Id));
            var events = s.Step(1);
            Assert.That(events.OfType<SkillUsedEvent>().Single().Skill.Value, Is.EqualTo("card_shackle"));
            Assert.That(Pct(verre), Is.EqualTo(0), "poids lourd : retour à 0 %");
            Assert.That(verre.Cooldowns[3], Is.GreaterThan(0));
            verre.Point = verre.ActionPoint;
            s.Step(1);
            Assert.That(s.CanUse(verre.Id, 3), Is.False, "toujours en recharge");
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
    }
}
