#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Mécaniques des paliers de cartes (lot C1) : vol de vie, exécution, purge, rebond, jouées par une carte de test en vrai combat.</summary>
    public sealed class CardMechanicsTests
    {
        /// <summary>
        /// Combat où « verre » (unité 0) lance aussitôt une carte de test Matière (coût 0) faite de ces effets sur <paramref name="targetUnit"/> ;
        /// les ennemis suivent (unités 1, 2…). <paramref name="before"/> prépare les unités avant le lancer.
        /// </summary>
        static CombatSimulation Cast(TargetRule target, EffectDef[] effects, string[] enemies, out List<CombatEvent> events,
                                     Action<CombatSimulation>? before = null, int targetUnit = 1, ulong seed = 11)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var card = new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, ItemRarity.Common,
                                        new SkillDef(new SkillId("card_test"), SkillSlot.A3, 0, target, effects, 0));
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var ally = new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { new CardId("card_test") });
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, new[] { ally },
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));
            foreach (var u in sim.Units) u.Point = 0;
            before?.Invoke(sim);
            var verre = sim.Unit(0);
            verre.Point = verre.ActionPoint;
            verre.Energy = verre.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(verre.Id, 2, targetUnit));
            events = sim.Step(1).ToList();
            return sim;
        }

        static EffectDef Fx(EffectType t, int v, StatusType s = StatusType.None, int d = 0) => new EffectDef(t, v, s, d);

        static IEnumerable<HealEvent> HealsOn(IEnumerable<CombatEvent> e, int unit) => e.OfType<HealEvent>().Where(h => h.Target == unit);

        // ------------------------------------------------------------------ Vol de vie

        [Test, Description("Vérifie que le vol de vie rend au lanceur le pourcentage des dégâts qui ont traversé le bouclier de la cible (pas de la partie absorbée) — parce qu'on ne vole que la vie réellement retirée")]
        public void Lifesteal_HealsThePartOfDamageThatGotThrough()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 200), Fx(EffectType.Lifesteal, 50) }, new[] { "brute" }, out var events, s =>
            {
                s.Unit(0).Hp = s.Unit(0).MaxHp / 4;
                s.Unit(1).Statuses.Add(new StatusInstance(StatusType.Shield, 2, 20));
            });
            var dmg = events.OfType<DamageEvent>().Single(d => d.Target == 1);
            Assert.That(events.OfType<ShieldAbsorbedEvent>().Any(), Is.True, "le bouclier a absorbé une partie");
            Assert.That(HealsOn(events, 0).Single().Amount, Is.EqualTo(dmg.Amount * 50 / 100));
        }

        [Test, Description("Vérifie que le vol de vie ne soigne pas au-delà des PV max, et ne fait rien sur une compétence sans dégâts — parce qu'un vol de vie sans coup ne doit rien rendre")]
        public void Lifesteal_IsCappedByMaxHp_AndNothingWithoutDamage()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Lifesteal, 50) }, new[] { "brute" }, out var full);
            Assert.That(HealsOn(full, 0).Sum(h => h.Amount), Is.EqualTo(0), "déjà à PV max");
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2), Fx(EffectType.Lifesteal, 50) }, new[] { "brute" }, out var none,
                 s => s.Unit(0).Hp = s.Unit(0).MaxHp / 4);
            Assert.That(HealsOn(none, 0), Is.Empty);
        }

        [Test, Description("Vérifie qu'une compétence de zone avec vol de vie soigne le lanceur une fois par ennemi touché — parce que le vol de vie compte cible par cible")]
        public void Lifesteal_OnAoe_HealsOncePerTarget()
        {
            Cast(TargetRule.AllEnemies, new[] { Fx(EffectType.Damage, 80), Fx(EffectType.Lifesteal, 20) }, new[] { "brute", "brute" }, out var events,
                 s => s.Unit(0).Hp = s.Unit(0).MaxHp / 4);
            Assert.That(HealsOn(events, 0).Count(), Is.EqualTo(2));
        }

        // ------------------------------------------------------------------ Exécution

        static int ExecuteDamage(int hpPercent, bool executeFirst, out List<CombatEvent> events)
        {
            var fx = executeFirst
                ? new[] { Fx(EffectType.Execute, 50), Fx(EffectType.Damage, 100) }
                : new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Execute, 50) };
            Cast(TargetRule.SingleEnemy, fx, new[] { "brute" }, out events, s => s.Unit(1).Hp = s.Unit(1).MaxHp * hpPercent / 100);
            return events.OfType<DamageEvent>().Single(d => d.Target == 1).Amount;
        }

        [Test, Description("Vérifie que l'exécution augmente les dégâts de 50 % sous 30 % des PV de la cible (et pas au-dessus), où qu'elle soit placée dans la liste — parce que le bonus vise les ennemis presque vaincus")]
        public void Execute_AppliesBelowThreshold_WhereverItIsInTheList()
        {
            int normal = ExecuteDamage(31, false, out var above);
            Assert.That(above.OfType<ExecutedEvent>(), Is.Empty);
            foreach (bool first in new[] { true, false })
            {
                int boosted = ExecuteDamage(29, first, out var below);
                Assert.That(boosted, Is.EqualTo(normal * 150 / 100), first ? "exécution avant les dégâts" : "exécution après les dégâts");
                Assert.That(below.OfType<ExecutedEvent>().Single().Target, Is.EqualTo(1));
            }
        }

        // ------------------------------------------------------------------ Purge

        static void Give(CombatUnit u, params StatusType[] statuses) { foreach (var st in statuses) u.Statuses.Add(new StatusInstance(st, 3, st == StatusType.Shield ? 50 : 0)); }

        [Test, Description("Vérifie que la purge sur un allié retire ses altérations négatives, la plus récente d'abord, et laisse ses bonus — parce qu'une purge soigne sans retirer les bienfaits")]
        public void Purge_OnAlly_RemovesNegativeStatuses_NewestFirst()
        {
            var sim = Cast(TargetRule.SingleAlly, new[] { Fx(EffectType.Purge, 1) }, new[] { "brute" }, out var events,
                           s => Give(s.Unit(0), StatusType.Burn, StatusType.Vulnerable, StatusType.Haste), targetUnit: 0);
            var verre = sim.Unit(0);
            Assert.That(verre.Has(StatusType.Vulnerable), Is.False);
            Assert.That(verre.Has(StatusType.Burn) && verre.Has(StatusType.Haste), Is.True);
            Assert.That(events.OfType<StatusExpiredEvent>().Any(e => e.Target == 0 && e.Status == StatusType.Vulnerable), Is.True);
            Assert.That(events.OfType<PurgedEvent>().Single().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'une purge totale sur un ennemi retire tous ses bonus mais pas ses altérations, et ne signale rien quand il n'y a rien à retirer — parce que la purge ennemie vise ses protections")]
        public void Purge_Zero_RemovesAll_OnEnemyOnlyPositives()
        {
            var sim = Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Purge, 0) }, new[] { "brute" }, out var events,
                           s => Give(s.Unit(1), StatusType.Shield, StatusType.Haste, StatusType.Burn));
            var brute = sim.Unit(1);
            Assert.That(brute.Has(StatusType.Shield) || brute.Has(StatusType.Haste), Is.False);
            Assert.That(brute.Has(StatusType.Burn), Is.True);
            Assert.That(events.OfType<PurgedEvent>().Single().Count, Is.EqualTo(2));
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Purge, 0) }, new[] { "brute" }, out var nothing);
            Assert.That(nothing.OfType<PurgedEvent>(), Is.Empty);
        }

        [Test, Description("Vérifie qu'un bouclier purgé n'absorbe plus les dégâts qui suivent dans la même compétence — parce que la purge doit vraiment ouvrir la défense")]
        public void Purge_OfAShield_LetsTheNextDamageThrough()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Purge, 0), Fx(EffectType.Damage, 100) }, new[] { "brute" }, out var events,
                 s => Give(s.Unit(1), StatusType.Shield));
            Assert.That(events.OfType<ShieldAbsorbedEvent>(), Is.Empty);
            Assert.That(events.OfType<DamageEvent>().Any(d => d.Target == 1), Is.True);
        }

        // ------------------------------------------------------------------ Rebond

        [Test, Description("Vérifie que le rebond touche un autre ennemi vivant après la cible (rebond annoncé, puis dégâts sur lui) — parce que le rebond prolonge le coup sur un second ennemi")]
        public void Bounce_HitsAnotherLivingEnemy()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Bounce, 50) }, new[] { "brute", "brute", "brute" }, out var events);
            var bounce = events.OfType<BounceEvent>().Single();
            Assert.That(bounce.From, Is.EqualTo(1));
            Assert.That(bounce.To, Is.Not.EqualTo(1));
            int at = events.IndexOf(bounce);
            Assert.That(events.Skip(at).OfType<DamageEvent>().Any(d => d.Target == bounce.To && d.Amount > 0), Is.True);
        }

        [Test, Description("Vérifie que le rebond part même quand le coup principal vainc la cible — parce que le rebond prolonge le coup après la cible, qu'elle tombe ou non")]
        public void Bounce_StillFires_WhenTheFirstHitDefeatsTheTarget()
        {
            Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Bounce, 50) }, new[] { "brute", "brute" }, out var events,
                 s => s.Unit(1).Hp = 1);
            Assert.That(events.OfType<UnitDefeatedEvent>().Any(d => d.Unit == 1), Is.True);
            Assert.That(events.OfType<BounceEvent>().Single().To, Is.EqualTo(2));
        }

        // ------------------------------------------------------------------ Vraies cartes (ordre base puis paliers)

        static SkillDef Real(string card, int level)
            => CardLevels.SkillAt(N29.HeroesDawn.Content.Prototype.PrototypeCollection.Build().Card(new CardId(card)), level);

        [Test, Description("Vérifie que Braise niveau 10 (dégâts, Brûlure, rebond…) rebondit quand son coup vainc la cible, malgré le statut placé entre les dégâts et le rebond — parce que les paliers s'ajoutent après les effets de base dans les vraies cartes")]
        public void RealCard_Ember10_BouncesOnAKillingBlow()
        {
            var ember = Real("card_ember", 10);
            Cast(ember.Target, ember.Effects.ToArray(), new[] { "brute", "brute" }, out var events, s => s.Unit(1).Hp = 1);
            Assert.That(events.OfType<UnitDefeatedEvent>().Any(d => d.Unit == 1), Is.True);
            Assert.That(events.OfType<BounceEvent>().Single().To, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que Brasier niveau 3 (dégâts de zone, Brûlure, vol de vie) soigne le lanceur sur un ennemi qu'il vainc — parce que le vol de vie compte le coup fatal même avec un statut entre les deux")]
        public void RealCard_Inferno3_StealsLifeOnAKillingBlow()
        {
            var inferno = Real("card_inferno", 3);
            Cast(inferno.Target, inferno.Effects.ToArray(), new[] { "brute" }, out var events, s =>
            {
                s.Unit(0).Hp = s.Unit(0).MaxHp / 4;
                s.Unit(1).Hp = 1;
            });
            Assert.That(events.OfType<UnitDefeatedEvent>().Any(d => d.Unit == 1), Is.True);
            Assert.That(HealsOn(events, 0).Single().Amount, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie que, pour chaque famille, un combat en mode Auto où deux agents portent au niveau 10 une nouvelle carte offensive et une nouvelle carte d'équipe se termine et se rejoue à l'identique — parce que les 36 cartes doivent jouer sans erreur et rester vérifiables par le serveur")]
        public void AutoCombat_WithNewCards_EndsAndReplays()
        {
            var cards = new Dictionary<Family, string[]>
            {
                [Family.Matter] = new[] { "card_glass_shards", "card_healing_molt" },
                [Family.Elemental] = new[] { "card_chain_lightning", "card_soothing_mist" },
                [Family.PhysicalBio] = new[] { "card_bloodletting", "card_shared_pulse" },
                [Family.Perception] = new[] { "card_guard_break", "card_time_veil" },
            };
            var c = TestContent.Build();
            foreach (var kv in cards)
            {
                var members = c.Members.Where(m => m.Family == kv.Key).Take(2).ToList();
                Assume.That(members.Count, Is.EqualTo(2), kv.Key.ToString());
                var allies = members.Select(m => new AllyEntry(m.Id, 10, 0, 0, null,
                    new CardId?[] { new CardId(kv.Value[0]), new CardId(kv.Value[1]) }, new[] { 10, 10 })).ToArray();
                var setup = new CombatSetup(31UL, CombatRank.Normal, allies,
                    new[] { new EnemyEntry(new EnemyId("brute"), 8), new EnemyEntry(new EnemyId("brute"), 8), new EnemyEntry(new EnemyId("brute"), 8) }, true);
                var sim = new CombatSimulation(c, setup);
                int guard = 0;
                while (!sim.IsOver && guard++ < 40000) sim.Step(1);
                Assert.That(sim.IsOver, Is.True, kv.Key + " : le combat se termine");
                Assert.That(CombatReplay.Run(c, setup, sim.InputLog).StateHash, Is.EqualTo(sim.StateHash()), kv.Key + " : rejeu identique");
            }
        }

        [Test, Description("Vérifie que chacune des 48 cartes, équipée en carte 1 (3e place) au niveau 10 par un agent de sa famille, est bien lancée par l'IA en mode Auto, sans erreur — parce que chaque liste d'effets doit réellement tourner dans le moteur")]
        public void AutoMode_CastsEveryCard()
        {
            var c = TestContent.Build();
            var never = new List<string>();
            foreach (var card in c.Collection.Cards)
            {
                var member = c.Members.First(m => m.Family == card.Family);
                var ally = new AllyEntry(member.Id, 10, 0, 0, null, new CardId?[] { card.Id }, new[] { 10 });
                var setup = new CombatSetup(17UL, CombatRank.Normal, new[] { ally },
                    new[] { new EnemyEntry(new EnemyId("brute"), 8), new EnemyEntry(new EnemyId("brute"), 8) }, true);
                var sim = new CombatSimulation(c, setup);
                sim.Unit(0).Energy = sim.Unit(0).MaxEnergy; // énergie pleine : les Légendaires (70) sont lançables avant la fin d'un combat court
                bool cast = false;
                int steps = 0, maxEnergy = 0;
                for (; steps < 20000 && !cast && !sim.IsOver; steps++)
                {
                    cast = sim.Step(1).OfType<SkillUsedEvent>().Any(e => e.Actor == 0 && e.Skill.Value == card.Id.Value);
                    maxEnergy = Math.Max(maxEnergy, sim.Unit(0).Energy);
                }
                if (!cast) never.Add(card.Id.Value + " (coût " + card.Skill.Cost + ", énergie max " + maxEnergy + ", " + steps + " pas, fini " + sim.IsOver + ")");
            }
            Assert.That(never, Is.Empty);
        }

        static string Trace(IEnumerable<CombatEvent> e) => string.Join("|", e.Select(x => x.GetType().Name + ":" + x.Tick));

        [Test, Description("Vérifie que le rebond ne fait rien (et ne tire rien au sort) sans autre ennemi ou sans effet de dégâts : la suite du combat est identique à celle d'une compétence sans rebond — parce que le combat doit rester rejouable à l'identique")]
        public void Bounce_DoesNothing_WithoutAnotherEnemyOrWithoutDamage()
        {
            foreach (var (with, without) in new[]
            {
                (new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Bounce, 50) }, new[] { Fx(EffectType.Damage, 100) }),
                (new[] { Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2), Fx(EffectType.Bounce, 50) }, new[] { Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2) }),
            })
            {
                var a = Cast(TargetRule.SingleEnemy, with, new[] { "brute" }, out var ea);
                var b = Cast(TargetRule.SingleEnemy, without, new[] { "brute" }, out var eb);
                Assert.That(ea.OfType<BounceEvent>(), Is.Empty);
                for (int i = 0; i < 200; i++) { ea.AddRange(a.Step(1)); eb.AddRange(b.Step(1)); }
                Assert.That(Trace(ea), Is.EqualTo(Trace(eb)));
            }
        }

        [Test, Description("Vérifie que le rebond ne choisit jamais un ennemi vaincu et qu'un ennemi en Esquive l'esquive — parce que le rebond suit les règles normales d'un coup")]
        public void Bounce_NeverTargetsADefeatedEnemy_AndRespectsEvade()
        {
            for (ulong seed = 1; seed <= 8; seed++)
            {
                Cast(TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Bounce, 50) }, new[] { "brute", "brute", "brute" }, out var events,
                     s => { s.Unit(2).Hp = 0; Give(s.Unit(3), StatusType.Evade); }, seed: seed);
                var bounce = events.OfType<BounceEvent>().Single();
                Assert.That(bounce.To, Is.EqualTo(3), "graine " + seed);
                Assert.That(events.Skip(events.IndexOf(bounce)).OfType<MissEvent>().Any(m => m.Target == 3), Is.True, "graine " + seed);
            }
        }

        [Test, Description("Vérifie qu'un combat complet où une carte rebondit se rejoue à l'identique depuis son journal de commandes — parce que la vérification serveur repose sur le rejeu")]
        public void Simulation_WithBounce_MatchesItsVerification()
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var card = new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, ItemRarity.Common,
                new SkillDef(new SkillId("card_test"), SkillSlot.A3, 0, TargetRule.SingleEnemy, new[] { Fx(EffectType.Damage, 100), Fx(EffectType.Bounce, 60) }, 0));
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var setup = new CombatSetup(21UL, CombatRank.Normal,
                new[] { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { new CardId("card_test") }) },
                new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("brute"), 5) }, false);
            var sim = new CombatSimulation(catalog, setup);
            int guard = 0, bounces = 0;
            while (!sim.IsOver && guard++ < 20000)
            {
                foreach (var u in sim.Units.Where(x => x.Side == Side.Ally && x.IsReady))
                    sim.Submit(CombatCommand.UseSkill(u.Id, sim.CanUse(u.Id, 2) ? 2 : 0, -1));
                bounces += sim.Step(1).OfType<BounceEvent>().Count();
            }
            Assert.That(bounces, Is.GreaterThan(0), "la carte a rebondi");
            Assert.That(CombatReplay.Run(catalog, setup, sim.InputLog).StateHash, Is.EqualTo(sim.StateHash()));
        }
    }
}
