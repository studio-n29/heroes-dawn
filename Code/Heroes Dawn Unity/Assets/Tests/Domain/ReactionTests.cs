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
    /// <summary>Réactions armées par les cartes (P2) : armement sur le lanceur, déclencheurs, cibles, durées, file sans récursion, aucun effet sans réaction.</summary>
    public sealed class ReactionTests
    {
        static readonly string[] AllyIds = { "verre", "givre", "ancre" };

        static SkillCardDef Card(string id, TargetRule target, params EffectDef[] effects)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", Family.Matter, 1, new SkillDef(new SkillId(id), SkillSlot.A3, 0, target, effects, 0));

        static EffectDef Fx(EffectType t, int v, StatusType s = StatusType.None, int d = 0) => new EffectDef(t, v, s, d);

        static ReactionDef React(ReactionTrigger trigger, EffectDef effect, StatusType status = StatusType.None, int turns = 3,
                                 TargetRule target = TargetRule.Self, TargetPick pick = TargetPick.Default, bool onTrigger = false)
            => new ReactionDef(trigger, status, turns, effect, target, pick, StatusType.None, onTrigger);

        static EffectDef Arm(ReactionDef r) => new EffectDef(EffectType.ArmReaction, 0, reaction: r);

        /// <summary>
        /// Combat manuel : un allié par liste de cartes (« verre » = 0, « givre » = 1, « ancre » = 2 ; cartes aux emplacements 2, 3…), puis les ennemis.
        /// Toutes les unités partent au point 0 : rien ne bouge tant qu'on ne fait pas lancer une carte.
        /// </summary>
        static CombatSimulation Fight(SkillCardDef[][] cardsPerAlly, string[] enemies, ulong seed = 11, int cardLevel = 1, bool auto = false)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var all = cardsPerAlly.SelectMany(x => x).ToList();
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(all).ToList(), col.Boosters));
            var allies = cardsPerAlly.Select((cards, i) => new AllyEntry(new MemberId(AllyIds[i]), 5, 0, 0, null,
                cards.Select(k => (CardId?)k.Id).ToArray(), cards.Select(_ => cardLevel).ToArray())).ToArray();
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, allies,
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), auto));
            foreach (var u in sim.Units) u.Point = 0;
            return sim;
        }

        /// <summary>L'unité lance sa carte de l'emplacement donné (2 = première carte) sur la cible voulue ; renvoie les événements du pas.</summary>
        static List<CombatEvent> Cast(CombatSimulation sim, int actor, int slot = 2, int target = -1)
        {
            var u = sim.Unit(actor);
            u.Point = u.ActionPoint;
            u.Energy = u.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(actor, slot, target));
            var events = sim.Step(1).ToList();
            Assert.That(events.OfType<SkillUsedEvent>().Any(e => e.Actor == actor), Is.True, "la carte de l'unité " + actor + " est lancée");
            return events;
        }

        static List<ReactionTriggeredEvent> Triggered(IEnumerable<CombatEvent> events) => events.OfType<ReactionTriggeredEvent>().ToList();

        static readonly string[] One = { "brute" };
        static readonly string[] Two = { "brute", "brute" };
        static readonly string[] Three = { "brute", "brute", "brute" };

        // ------------------------------------------------------------------ Armement

        [Test, Description("Vérifie qu'ArmReaction arme la réaction sur le lanceur (pas sur la cible de la carte), avec sa durée en tours, et le journalise — parce que la réaction appartient au porteur de la carte")]
        public void ArmReaction_ArmsTheCaster_WithTurns()
        {
            var r = React(ReactionTrigger.EnemyDefeated, Fx(EffectType.Heal, 10), turns: 3);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 50), Arm(r)) } }, One);
            var events = Cast(sim, 0, target: 1);
            Assert.That(sim.Unit(0).Reactions.Count, Is.EqualTo(1), "une réaction sur le lanceur");
            Assert.That(sim.Unit(0).Reactions[0].Def, Is.SameAs(r));
            Assert.That(sim.Unit(0).Reactions[0].TurnsLeft, Is.EqualTo(3));
            Assert.That(sim.Unit(1).Reactions, Is.Empty, "rien sur la cible");
            var armed = events.OfType<ReactionArmedEvent>().Single();
            Assert.That((armed.Carrier, armed.Trigger, armed.Turns), Is.EqualTo((0, ReactionTrigger.EnemyDefeated, 3)));
            Assert.That(Triggered(events), Is.Empty, "aucun déclenchement sans ennemi vaincu");
        }

        [Test, Description("Vérifie que réarmer la même réaction remet sa durée à Turns sans doublon ni changement de rang, alors qu'une autre définition s'ajoute — parce qu'une carte relancée prolonge sa réaction au lieu de la cumuler")]
        public void ReArming_RefreshesTheExistingReaction()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10), turns: 3);
            var other = React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10), turns: 2);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)), Card("card_r1", TargetRule.Self, Arm(other)) } }, One);
            Cast(sim, 0);
            var first = sim.Unit(0).Reactions.Single();
            int order = first.Order;
            first.TurnsLeft = 1;
            Cast(sim, 0);
            Assert.That(sim.Unit(0).Reactions.Count, Is.EqualTo(1), "pas de doublon");
            Assert.That(sim.Unit(0).Reactions[0].TurnsLeft, Is.EqualTo(3), "durée remise à Turns");
            Assert.That(sim.Unit(0).Reactions[0].Order, Is.EqualTo(order), "même rang d'armement");
            Cast(sim, 0, slot: 3);
            Assert.That(sim.Unit(0).Reactions.Select(x => x.Def), Is.EqualTo(new[] { r, other }), "une autre définition s'ajoute après");
        }

        // ------------------------------------------------------------------ Déclencheurs

        [Test, Description("Vérifie qu'« un ennemi reçoit Brûlure » ne se déclenche que pour la Brûlure posée sur un ennemi (ni le Gel, ni une Brûlure sur un allié), et frappe l'unité déclenchante — parce que le déclencheur nomme un statut et un camp")]
        public void EnemyReceivesStatus_TriggersOnlyForThatStatus()
        {
            var r = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 40), StatusType.Burn, onTrigger: true);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_freeze", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Freeze, 2)), Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
                new[] { Card("card_burn_ally", TargetRule.SingleAlly, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, Two);
            Cast(sim, 0);
            Assert.That(Triggered(Cast(sim, 1, slot: 2, target: 3)), Is.Empty, "Gel : autre statut");
            Assert.That(Triggered(Cast(sim, 2, slot: 2, target: 1)), Is.Empty, "Brûlure sur un allié : autre camp");
            var events = Cast(sim, 1, slot: 3, target: 3);
            var t = Triggered(events).Single();
            Assert.That((t.Carrier, t.Trigger, t.TriggerUnit), Is.EqualTo((0, ReactionTrigger.EnemyReceivesStatus, 3)));
            Assert.That(t.Targets, Is.EqualTo(new[] { 3 }), "l'unité déclenchante est visée");
            int at = events.IndexOf(t);
            Assert.That(events.Skip(at).OfType<DamageEvent>().Any(d => d.Target == 3), Is.True, "l'effet de la réaction suit son déclenchement");
        }

        [Test, Description("Vérifie qu'« un ennemi est vaincu » se déclenche une fois pour chaque ennemi vaincu par le même coup, sur le porteur par défaut — parce qu'une réaction se déclenche au plus une fois par événement, et chaque défaite est un événement")]
        public void EnemyDefeated_Triggers_ForEachDefeat()
        {
            var r = React(ReactionTrigger.EnemyDefeated, Fx(EffectType.Heal, 10));
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_aoe", TargetRule.AllEnemies, Fx(EffectType.Damage, 100)) },
            }, Three);
            Cast(sim, 0);
            sim.Unit(2).Hp = 1;
            sim.Unit(3).Hp = 1;
            sim.Unit(0).Hp = sim.Unit(0).MaxHp / 2;
            var events = Cast(sim, 1);
            var t = Triggered(events);
            Assert.That(t.Select(x => x.TriggerUnit), Is.EqualTo(new[] { 2, 3 }), "une fois par défaite, dans l'ordre des événements");
            Assert.That(t.All(x => x.Carrier == 0 && x.Targets.SequenceEqual(new[] { 0 })), Is.True, "sans cible nommée : le porteur");
            Assert.That(events.OfType<HealEvent>().Count(h => h.Target == 0), Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'« un allié est vaincu » se déclenche quand un allié du porteur tombe, et que l'effet vise la cible nommée (ZE) — parce que le déclencheur est relatif au camp du porteur")]
        public void AllyDefeated_Triggers()
        {
            var r = React(ReactionTrigger.AllyDefeated, Fx(EffectType.Damage, 30), target: TargetRule.AllEnemies);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_aoe", TargetRule.AllEnemies, Fx(EffectType.Damage, 100)) },
                new[] { Card("card_hit_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 100)) },
            }, Two);
            Cast(sim, 0);
            sim.Unit(3).Hp = 1;
            Assert.That(Triggered(Cast(sim, 1)), Is.Empty, "un ennemi vaincu n'est pas un allié vaincu");
            sim.Unit(1).Hp = 1;
            var events = Cast(sim, 2, target: 1);
            var t = Triggered(events).Single();
            Assert.That((t.Carrier, t.Trigger, t.TriggerUnit), Is.EqualTo((0, ReactionTrigger.AllyDefeated, 1)));
            Assert.That(t.Targets, Is.EqualTo(new[] { 4 }), "ZE : les ennemis vivants");
            Assert.That(events.Skip(events.IndexOf(t)).OfType<DamageEvent>().Any(d => d.Target == 4), Is.True);
        }

        [Test, Description("Vérifie que « le porteur est touché » se déclenche quand le porteur perd des PV, et pas quand son bouclier absorbe tout — parce qu'être touché veut dire perdre des PV")]
        public void CarrierHit_Triggers_WhenTheCarrierLosesHp()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Damage, 30), target: TargetRule.SingleEnemy);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_hit_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 50)) },
            }, One);
            Cast(sim, 0);
            sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Shield, 3, 100000));
            Assert.That(Triggered(Cast(sim, 1, target: 0)), Is.Empty, "tout absorbé : aucun PV perdu");
            sim.Unit(0).Statuses.Clear();
            sim.Unit(0).Reactions[0].TurnsLeft = 3;
            var events = Cast(sim, 1, target: 0);
            Assert.That(events.OfType<DamageEvent>().Any(d => d.Target == 0), Is.True, "le porteur perd des PV");
            var t = Triggered(events).Single();
            Assert.That((t.Carrier, t.Trigger, t.TriggerUnit), Is.EqualTo((0, ReactionTrigger.CarrierHit, 1)), "unité concernée : l'attaquant");
            Assert.That(t.Targets, Is.EqualTo(new[] { 2 }), "cible nommée 1E : le premier ennemi vivant");
            Assert.That(Triggered(Cast(sim, 1, target: 1)), Is.Empty, "un autre allié touché ne compte pas");
        }

        [Test, Description("Vérifie que « le porteur inflige un critique » ne se déclenche que sur un critique du porteur (pas sur celui d'un allié), une fois par coup critique — parce que le déclencheur suit l'auteur du coup")]
        public void CarrierCrit_Triggers_OnTheCarriersCritical()
        {
            var r = React(ReactionTrigger.CarrierCrit, Fx(EffectType.Heal, 10));
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)), Card("card_hit", TargetRule.SingleEnemy, Fx(EffectType.Damage, 30)) },
                new[] { Card("card_hit2", TargetRule.SingleEnemy, Fx(EffectType.Damage, 30)) },
            }, Two);
            Cast(sim, 0);
            sim.Unit(2).Statuses.Add(new StatusInstance(StatusType.Mark, 1, 0)); // la Marque rend le coup critique (sans tirage)
            var ally = Cast(sim, 1, target: 2);
            Assert.That(ally.OfType<DamageEvent>().Single(d => d.Target == 2).Crit, Is.True, "critique de l'allié");
            Assert.That(Triggered(ally), Is.Empty, "le critique d'un allié ne compte pas");
            sim.Unit(2).Statuses.Add(new StatusInstance(StatusType.Mark, 1, 0));
            sim.Unit(0).Hp = sim.Unit(0).MaxHp / 2;
            var own = Cast(sim, 0, slot: 3, target: 2);
            Assert.That(own.OfType<DamageEvent>().Single(d => d.Target == 2).Crit, Is.True, "critique du porteur");
            var t = Triggered(own).Single();
            Assert.That((t.Carrier, t.Trigger, t.TriggerUnit), Is.EqualTo((0, ReactionTrigger.CarrierCrit, 2)), "unité concernée : celle qui a subi le critique");
            var plain = Cast(sim, 0, slot: 3, target: 3); // sans Marque : critique ou non selon le tirage
            bool crit = plain.OfType<DamageEvent>().Where(d => d.Target == 3).Any(d => d.Crit);
            Assert.That(Triggered(plain).Count, Is.EqualTo(crit ? 1 : 0), "déclenché seulement si le coup est critique");
        }

        // ------------------------------------------------------------------ Valeur et cibles

        [Test, Description("Vérifie que l'effet d'une réaction garde sa valeur fixe à tous les niveaux et bonus de carte (ScalePercent, CardBonuses.Apply, With), alors que les dégâts de la carte augmentent — parce que les effets de réaction ne sont jamais remis à l'échelle")]
        public void ReactionEffect_UsesFixedValue_NotScaledByLevel()
        {
            var r = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 60), StatusType.Burn, onTrigger: true);
            var card = Card("card_r0", TargetRule.SingleEnemy, Arm(r), Fx(EffectType.Damage, 100), Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2));
            foreach (var skill in new[] { CardLevels.SkillAt(card, 10), CardBonuses.Apply(card.Skill, 20), CardLevels.ScalePercent(card.Skill, 300) })
            {
                var arm = skill.Effects.Single(e => e.Type == EffectType.ArmReaction);
                Assert.That(arm.Reaction, Is.SameAs(r), "la réaction est conservée par la reconstruction");
                Assert.That(arm.Reaction!.Effect.Value, Is.EqualTo(60), "valeur fixe");
                Assert.That(skill.Effects.Single(e => e.Type == EffectType.Damage).Value, Is.GreaterThan(100), "la carte, elle, augmente");
            }
            var copy = Arm(r).With(value: 5, duration: 2, chance: 50);
            Assert.That(copy.Reaction, Is.SameAs(r), "With copie la réaction");

            int ReactionDamage(int level, out int cardDamage)
            {
                var sim = Fight(new[] { new[] { card } }, One, seed: 5, cardLevel: level);
                var events = Cast(sim, 0, target: 1);
                var t = Triggered(events).Single();
                var hits = events.OfType<DamageEvent>().Where(d => d.Target == 1).ToList();
                cardDamage = hits.First().Amount;
                return events.Skip(events.IndexOf(t)).OfType<DamageEvent>().Single(d => d.Target == 1).Amount;
            }
            int low = ReactionDamage(1, out int lowCard);
            int high = ReactionDamage(10, out int highCard);
            Assert.That(highCard, Is.GreaterThan(lowCard), "les dégâts de la carte suivent le niveau");
            Assert.That(high, Is.EqualTo(low), "les dégâts de la réaction ne changent pas avec le niveau");
        }

        static uint ProbeAfter(CombatSimulation sim, int extraDraws)
        {
            for (int i = 0; i < extraDraws; i++) sim.RngProbeForTests();
            return sim.RngProbeForTests();
        }

        [Test, Description("Vérifie les cibles d'une réaction : l'unité déclenchante, 1E:aléatoire (exactement un tirage de plus que 1E par défaut), ZA et 1A:prochain (sans le porteur) — parce qu'une cible nommée suit la règle d'une compétence lancée par le porteur")]
        public void ReactionTarget_TriggerUnit_AndNamedTargets()
        {
            var vuln = Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2); // effet sans tirage
            CombatSimulation Run(ReactionDef r, out ReactionTriggeredEvent t, Action<CombatSimulation>? before = null)
            {
                var sim = Fight(new[]
                {
                    new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                    new[] { Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
                    new[] { Card("card_noop", TargetRule.Self, Fx(EffectType.Heal, 0)) },
                }, Three, seed: 5);
                Cast(sim, 0);
                before?.Invoke(sim);
                t = Triggered(Cast(sim, 1, target: 4)).Single();
                return sim;
            }

            var trig = Run(React(ReactionTrigger.EnemyReceivesStatus, vuln, StatusType.Burn, onTrigger: true), out var tu);
            Assert.That(tu.Targets, Is.EqualTo(new[] { 4 }), "déclencheur");
            Assert.That(trig.Unit(4).Has(StatusType.Vulnerable), Is.True);

            var reference = Run(React(ReactionTrigger.EnemyReceivesStatus, vuln, StatusType.Burn, target: TargetRule.SingleEnemy), out var td);
            Assert.That(td.Targets, Is.EqualTo(new[] { 3 }), "1E par défaut : le premier ennemi vivant, sans tirage");
            var random = Run(React(ReactionTrigger.EnemyReceivesStatus, vuln, StatusType.Burn, target: TargetRule.SingleEnemy, pick: TargetPick.Random), out var tr);
            Assert.That(tr.Targets.Length, Is.EqualTo(1));
            Assert.That(tr.Targets[0], Is.InRange(3, 5));
            Assert.That(random.RngProbeForTests(), Is.EqualTo(ProbeAfter(reference, 1)), "1E:aléatoire : exactement un tirage de plus");

            var za = Run(React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Shield, 10, d: 2), StatusType.Burn, target: TargetRule.AllAllies), out var tza,
                         s => s.Unit(2).Hp = 0);
            Assert.That(tza.Targets, Is.EqualTo(new[] { 0, 1 }), "ZA : les alliés vivants, porteur compris");
            Assert.That(za.Unit(0).Has(StatusType.Shield) && za.Unit(1).Has(StatusType.Shield), Is.True);

            Run(React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Advance, 10), StatusType.Burn, target: TargetRule.SingleAlly, pick: TargetPick.NextToAct), out var tnext,
                s => { s.Unit(0).Point = s.Unit(0).ActionPoint - 1; s.Unit(2).Point = s.Unit(2).ActionPoint - 2; }); // givre repart au point de reprise après sa carte
            Assert.That(tnext.Targets, Is.EqualTo(new[] { 2 }), "1A:prochain : l'allié le plus avancé hors porteur (le porteur, plus avancé encore, est écarté)");
        }

        // ------------------------------------------------------------------ Cible « déclencheur » selon l'événement

        [Test, Description("Vérifie que pour « le porteur est touché », la cible déclencheur est l'attaquant (l'auteur du coup) — parce que la riposte vise celui qui a frappé")]
        public void TriggerUnit_IsTheAttacker_ForCarrierHit()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2), onTrigger: true);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_hit_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 30)) },
            }, One);
            Cast(sim, 0);
            var t = Triggered(Cast(sim, 1, target: 0)).Single();
            Assert.That(t.TriggerUnit, Is.EqualTo(1), "l'attaquant");
            Assert.That(t.Targets, Is.EqualTo(new[] { 1 }));
            Assert.That(sim.Unit(1).Has(StatusType.Vulnerable), Is.True, "l'effet frappe l'attaquant");
            Assert.That(sim.Unit(0).Has(StatusType.Vulnerable), Is.False, "pas le porteur");
        }

        [Test, Description("Vérifie que pour « le porteur inflige un critique », la cible déclencheur est l'ennemi touché par ce critique — parce que la réaction prolonge le coup sur sa victime")]
        public void TriggerUnit_IsTheHitEnemy_ForCarrierCrit()
        {
            var r = React(ReactionTrigger.CarrierCrit, Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2), onTrigger: true);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)), Card("card_hit", TargetRule.SingleEnemy, Fx(EffectType.Damage, 30)) } }, Two);
            Cast(sim, 0);
            sim.Unit(2).Statuses.Add(new StatusInstance(StatusType.Mark, 1, 0));
            var t = Triggered(Cast(sim, 0, slot: 3, target: 2)).Single();
            Assert.That(t.TriggerUnit, Is.EqualTo(2));
            Assert.That(t.Targets, Is.EqualTo(new[] { 2 }));
            Assert.That(sim.Unit(2).Has(StatusType.Vulnerable), Is.True);
            Assert.That(sim.Unit(1).Has(StatusType.Vulnerable), Is.False, "l'autre ennemi n'est pas visé");
        }

        [Test, Description("Vérifie que pour « un allié reçoit <Statut> », la cible déclencheur est l'allié qui reçoit le statut (et non le porteur) — parce que l'unité concernée est celle qui reçoit")]
        public void TriggerUnit_IsTheStatusReceiver()
        {
            var r = React(ReactionTrigger.AllyReceivesStatus, Fx(EffectType.Shield, 10, d: 2), StatusType.Burn, onTrigger: true);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                Array.Empty<SkillCardDef>(),
                new[] { Card("card_burn_ally", TargetRule.SingleAlly, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, One);
            Cast(sim, 0);
            var t = Triggered(Cast(sim, 2, target: 1)).Single();
            Assert.That((t.Trigger, t.TriggerUnit), Is.EqualTo((ReactionTrigger.AllyReceivesStatus, 1)));
            Assert.That(t.Targets, Is.EqualTo(new[] { 1 }));
            Assert.That(sim.Unit(1).Has(StatusType.Shield), Is.True, "le receveur est protégé");
            Assert.That(sim.Unit(0).Has(StatusType.Shield), Is.False, "pas le porteur");
        }

        [Test, Description("Vérifie qu'une réaction « un ennemi est vaincu » visant le déclencheur (l'unité morte) ignore son effet de dégâts sans aucun tirage — parce qu'on ne frappe pas une unité vaincue et qu'un effet ignoré ne doit pas déplacer le générateur")]
        public void TriggerUnit_DeadDefeatedUnit_IgnoresADamageEffect()
        {
            CombatSimulation Run(ReactionTrigger trigger, out List<CombatEvent> events)
            {
                var r = React(trigger, Fx(EffectType.Damage, 50), onTrigger: true);
                var sim = Fight(new[]
                {
                    new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                    new[] { Card("card_hit", TargetRule.SingleEnemy, Fx(EffectType.Damage, 100)) },
                }, Two, seed: 5);
                Cast(sim, 0);
                sim.Unit(2).Hp = 1;
                events = Cast(sim, 1, target: 2);
                return sim;
            }
            var reference = Run(ReactionTrigger.AllyDefeated, out _); // ne se déclenche pas : référence de tirages
            var dead = Run(ReactionTrigger.EnemyDefeated, out var events);
            Assert.That(events.OfType<UnitDefeatedEvent>().Any(u => u.Unit == 2), Is.True, "l'ennemi est vaincu");
            Assert.That(events.OfType<DamageEvent>().Count(), Is.EqualTo(1), "aucun dégât de réaction");
            Assert.That(Triggered(events), Is.Empty, "effet ignoré faute de cible vivante");
            Assert.That(dead.RngProbeForTests(), Is.EqualTo(reference.RngProbeForTests()), "aucun tirage");
        }

        // ------------------------------------------------------------------ Durée, file, ordre

        [Test, Description("Vérifie qu'une réaction de 2 tours perd un tour au début de chaque tour de son porteur (pas à celui d'un autre) et disparaît au second — parce que sa durée se compte en tours du porteur, comme un statut")]
        public void Reaction_ExpiresAfterItsTurns_AtTheCarriersTurnStart()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10), turns: 2);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)) }, Array.Empty<SkillCardDef>() }, One);
            Cast(sim, 0);
            Assert.That(sim.Unit(0).Reactions.Single().TurnsLeft, Is.EqualTo(2));
            sim.Unit(1).Point = sim.Unit(1).ActionPoint - 1; // givre prend son tour avant verre
            var seen = new List<(int Unit, int Turns)>();
            for (int i = 0; i < 3000 && !sim.IsOver && seen.Count(s => s.Unit == 0) < 2; i++)
            {
                foreach (var g in sim.Step(1).OfType<GaugeFullEvent>())
                    if (g.Unit <= 1) seen.Add((g.Unit, sim.Unit(0).Reactions.Count == 0 ? 0 : sim.Unit(0).Reactions[0].TurnsLeft));
                foreach (var u in sim.Units) if (u.Side == Side.Ally && u.IsReady) sim.Submit(CombatCommand.Pass(u.Id));
            }
            Assert.That(seen.First().Unit, Is.EqualTo(1), "givre agit d'abord");
            Assert.That(seen.First().Turns, Is.EqualTo(2), "le tour d'un autre allié ne décompte rien");
            Assert.That(seen.Where(s => s.Unit == 0).Select(s => s.Turns), Is.EqualTo(new[] { 1, 0 }), "1 tour au premier début de tour du porteur, retirée au second");
            Assert.That(sim.Unit(0).Reactions, Is.Empty);
        }

        [Test, Description("Vérifie que les statuts posés par des réactions ne déclenchent aucune réaction : chaque réaction part une seule fois sur l'événement d'origine — parce que les réactions se résolvent en file, jamais en récursion")]
        public void ReactionsDoNotTriggerReactions_NoLoop()
        {
            var spread = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2), StatusType.Burn, target: TargetRule.AllEnemies);
            var echo = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2), StatusType.Burn, onTrigger: true);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(spread)) },
                new[] { Card("card_r1", TargetRule.Self, Arm(echo)) },
                new[] { Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, Two);
            Cast(sim, 0);
            Cast(sim, 1);
            var events = Cast(sim, 2, target: 3);
            Assert.That(Triggered(events).Select(t => t.Carrier), Is.EqualTo(new[] { 0, 1 }), "une fois chacune");
            Assert.That(events.OfType<StatusAppliedEvent>().Count(s => s.Status == StatusType.Burn), Is.EqualTo(4), "origine + ZE (2) + déclencheur");
            Assert.That(Triggered(sim.ApplyCommands()), Is.Empty, "rien n'est relu au pas suivant");
        }

        [Test, Description("Vérifie que plusieurs réactions sur le même événement partent par numéro de porteur croissant, puis par ordre d'armement — parce que l'ordre de résolution doit être déterministe")]
        public void SeveralReactions_OrderedByCarrierIdThenArming()
        {
            var first = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Heal, 10), StatusType.Burn);
            var second = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Shield, 10, d: 2), StatusType.Burn);
            var zero = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.ApplyStatus, 0, StatusType.Haste, 2), StatusType.Burn);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(zero)) },
                new[] { Card("card_r1", TargetRule.Self, Arm(second)), Card("card_r2", TargetRule.Self, Arm(first)) },
                new[] { Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, One);
            Cast(sim, 1, slot: 3); // givre arme « first » en premier
            Cast(sim, 1, slot: 2); // puis « second »
            Cast(sim, 0);          // verre (unité 0) arme en dernier
            Assert.That(sim.Unit(1).Reactions.Select(x => x.Def), Is.EqualTo(new[] { first, second }));
            var events = Cast(sim, 2, target: 3);
            var t = Triggered(events);
            Assert.That(t.Select(x => x.Carrier), Is.EqualTo(new[] { 0, 1, 1 }), "porteur 0 d'abord malgré un armement plus tardif");
            var after = t.Select(x => events[events.IndexOf(x) + 1]).ToList();
            Assert.That(after[0], Is.TypeOf<StatusAppliedEvent>(), "réaction de verre : Hâte");
            Assert.That(after[1], Is.TypeOf<HealEvent>(), "givre : « first », armée la première");
            Assert.That(after[2], Is.TypeOf<StatusAppliedEvent>().And.Property("Status").EqualTo(StatusType.Shield), "givre : « second »");
        }

        // ------------------------------------------------------------------ Sans réaction / porteur vaincu

        static string Trace(IEnumerable<CombatEvent> log)
            => string.Join(";", log.Where(e => !(e is ReactionArmedEvent)).Select(e => e.GetType().Name + "@" + e.Tick + (e is DamageEvent d ? ":" + d.Target + "/" + d.Amount : "")));

        [Test, Description("Vérifie qu'un combat sans réaction armée n'en journalise aucune, et qu'une réaction armée qui ne se déclenche jamais ne change ni les tirages ni les événements (hors son armement) — parce que le moteur de réactions ne doit rien coûter aux combats qui n'en déclenchent pas")]
        public void NoReaction_DrawsNothing_AndLogsNothing()
        {
            CombatSimulation Auto(params EffectDef[] fx)
            {
                var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, fx) }, Array.Empty<SkillCardDef>() }, Two, seed: 21, auto: true);
                for (int i = 0; i < 400 && !sim.IsOver; i++) sim.Step(1);
                return sim;
            }
            var plain = Auto(Fx(EffectType.Damage, 100));
            Assert.That(plain.Log.OfType<SkillUsedEvent>().Any(e => e.Skill.Value == "card_r0"), Is.True, "la carte est jouée");
            Assert.That(plain.Log.Any(e => e is ReactionArmedEvent || e is ReactionTriggeredEvent), Is.False, "aucune réaction journalisée");
            Assert.That(plain.Units.All(u => u.Reactions.Count == 0), Is.True);

            var never = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 50), StatusType.Anchor, onTrigger: true);
            var armed = Auto(Fx(EffectType.Damage, 100), Arm(never));
            Assert.That(armed.Log.OfType<ReactionArmedEvent>().Any(), Is.True, "la réaction est armée");
            Assert.That(armed.Log.OfType<ReactionTriggeredEvent>().Any(), Is.False, "et ne se déclenche jamais");
            Assert.That(Trace(armed.Log), Is.EqualTo(Trace(plain.Log)), "mêmes événements");
            Assert.That(armed.RngProbeForTests(), Is.EqualTo(plain.RngProbeForTests()), "même empreinte du générateur");
        }

        [Test, Description("Vérifie qu'une réaction portée par une unité vaincue ne se déclenche pas — parce qu'un porteur hors de combat ne réagit plus")]
        public void DeadCarrier_DoesNotTrigger()
        {
            var r = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 40), StatusType.Burn, onTrigger: true);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, One);
            Cast(sim, 0);
            Assert.That(Triggered(Cast(sim, 1, target: 2)).Count, Is.EqualTo(1), "vivant : la réaction part");
            sim.Unit(0).Hp = 0;
            Assert.That(Triggered(Cast(sim, 1, target: 2)), Is.Empty, "vaincu : plus rien");
        }
    }
}
