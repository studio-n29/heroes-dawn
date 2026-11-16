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
        static CombatSimulation Fight(SkillCardDef[][] cardsPerAlly, string[] enemies, ulong seed = 11, int cardLevel = 1, bool auto = false, string[]? items = null)
        {
            var (catalog, setup) = Setup(cardsPerAlly, enemies, seed, cardLevel, auto, items);
            var sim = new CombatSimulation(catalog, setup);
            foreach (var u in sim.Units) u.Point = 0;
            return sim;
        }

        /// <summary>Catalogue (cartes de test ajoutées) et mise en place du combat de <see cref="Fight"/>, sans remise des points à 0 (pour le rejeu).</summary>
        static (ContentCatalog Catalog, CombatSetup Setup) Setup(SkillCardDef[][] cardsPerAlly, string[] enemies, ulong seed = 11, int cardLevel = 1, bool auto = false, string[]? items = null)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var all = cardsPerAlly.SelectMany(x => x).ToList();
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(all).ToList(), col.Boosters));
            var allies = cardsPerAlly.Select((cards, i) => new AllyEntry(new MemberId(AllyIds[i]), 5, 0, 0, i == 0 && items != null ? items.Select(x => new ItemId(x)).ToList() : null,
                cards.Select(k => (CardId?)k.Id).ToArray(), cards.Select(_ => cardLevel).ToArray())).ToArray();
            return (catalog, new CombatSetup(seed, CombatRank.Normal, allies, enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), auto));
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

        static ReactionDef Listen() => React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Heal, 10), StatusType.Burn);

        [Test, Description("Vérifie qu'une carte qui achève sa cible arme quand même sa réaction — parce que la réaction est armée sur le porteur quelle que soit la cible")]
        public void Arm_HappensEvenWhenTheCardKillsItsTarget()
        {
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 100), Arm(Listen())) } }, Two);
            sim.Unit(1).Hp = 1;
            var events = Cast(sim, 0, target: 1);
            Assert.That(events.OfType<UnitDefeatedEvent>().Any(u => u.Unit == 1), Is.True, "la cible est vaincue");
            Assert.That(sim.Unit(0).Reactions.Count, Is.EqualTo(1), "la réaction est armée");
        }

        [Test, Description("Vérifie qu'une carte dont le coup est raté (Esquive) arme quand même sa réaction — parce qu'un raté n'arrête que les effets sur la cible")]
        public void Arm_HappensEvenWhenTheCardMisses()
        {
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 100), Arm(Listen())) } }, One);
            sim.Unit(1).Statuses.Add(new StatusInstance(StatusType.Evade, 2, 0));
            var events = Cast(sim, 0, target: 1);
            Assert.That(events.OfType<MissEvent>().Any(m => m.Target == 1), Is.True, "le coup est raté");
            Assert.That(sim.Unit(0).Reactions.Count, Is.EqualTo(1), "la réaction est armée");
        }

        [Test, Description("Vérifie qu'une carte de zone sur trois ennemis n'arme sa réaction qu'une fois (un seul événement, une seule durée) — parce que l'armement se fait une fois par lancer, pas par cible")]
        public void Arm_HappensOnceForAnAreaCard()
        {
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.AllEnemies, Fx(EffectType.Damage, 10), Arm(Listen())) } }, Three);
            var events = Cast(sim, 0);
            Assert.That(events.OfType<DamageEvent>().Count(), Is.EqualTo(3), "trois cibles touchées");
            Assert.That(events.OfType<ReactionArmedEvent>().Count(), Is.EqualTo(1), "un seul armement");
            Assert.That(sim.Unit(0).Reactions.Single().TurnsLeft, Is.EqualTo(3), "une seule durée");
        }

        [Test, Description("Vérifie qu'une réaction entend les effets de la carte qui l'arme (une fois), que l'armement soit placé avant ou après la Brûlure, puis le lancer suivant — parce qu'une réaction armée par une carte écoute à partir du début du lancer de cette carte")]
        public void Arm_ListensToItsOwnCardsEffects()
        {
            var burn = Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2);
            foreach (var fx in new[] { new[] { Arm(Listen()), burn }, new[] { burn, Arm(Listen()) } })
            {
                var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, fx) } }, One);
                Assert.That(Triggered(Cast(sim, 0, target: 1)).Count, Is.EqualTo(1), "sa propre carte la déclenche une fois");
                Assert.That(Triggered(Cast(sim, 0, target: 1)).Count, Is.EqualTo(1), "le lancer suivant aussi");
            }
        }

        [Test, Description("Vérifie qu'une réaction n'entend pas les événements du pas antérieurs au lancer de la carte qui l'arme (une Brûlure posée juste avant, dans le même pas) — parce que l'écoute commence au début du lancer, pas au début du pas")]
        public void Arm_DoesNotListenToEventsBeforeItsCard()
        {
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(Listen())) },
                new[] { Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, One);
            foreach (var id in new[] { 1, 0 }) { var u = sim.Unit(id); u.Point = u.ActionPoint; u.Energy = u.MaxEnergy; }
            sim.Submit(CombatCommand.UseSkill(1, 2, 2)); // givre brûle d'abord, puis verre arme, dans le même pas
            sim.Submit(CombatCommand.UseSkill(0, 2, -1));
            var events = sim.Step(1).ToList();
            Assert.That(events.OfType<ReactionArmedEvent>().Count(), Is.EqualTo(1), "armée dans ce pas");
            Assert.That(events.OfType<StatusAppliedEvent>().Any(s => s.Status == StatusType.Burn), Is.True, "Brûlure posée avant l'armement");
            Assert.That(Triggered(events), Is.Empty, "la Brûlure antérieure à la carte n'est pas entendue");
        }

        [Test, Description("Vérifie que l'Entrave posée par la carte de Fil piège elle-même coûte de la jauge à l'ennemi lié — parce que la réaction entend la carte qui l'arme")]
        public void Tripwire_OwnBindCostsGauge()
        {
            var react = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.GaugeDelta, -300), StatusType.Bind, turns: 2, onTrigger: true);
            int PointAfter(bool withReaction)
            {
                var fx = new List<EffectDef> { Fx(EffectType.Damage, 105), Fx(EffectType.ApplyStatus, 0, StatusType.Bind, 1) };
                if (withReaction) fx.Add(Arm(react));
                var sim = Fight(new[] { new[] { Card("card_tripwire_test", TargetRule.SingleEnemy, fx.ToArray()) } }, One, seed: 5);
                sim.Unit(1).Point = sim.Unit(1).ActionPoint - 2;
                var events = Cast(sim, 0, target: 1);
                if (withReaction)
                {
                    var t = Triggered(events).Single();
                    Assert.That((t.Trigger, t.TriggerUnit), Is.EqualTo((ReactionTrigger.EnemyReceivesStatus, 1)), "sa propre Entrave la déclenche");
                }
                return sim.Unit(1).Point;
            }
            Assert.That(PointAfter(true), Is.LessThan(PointAfter(false)), "l'ennemi lié perd de la jauge");
        }

        [Test, Description("Vérifie que la victoire obtenue par le coup de Chute en cascade déclenche sa propre réaction « un ennemi est vaincu » — parce que la réaction entend la carte qui l'arme")]
        public void Cascade_OwnKillTriggersEnemyDefeatedReaction()
        {
            var react = React(ReactionTrigger.EnemyDefeated, Fx(EffectType.Damage, 70), turns: 2, target: TargetRule.SingleEnemy, pick: TargetPick.Random);
            var sim = Fight(new[] { new[] { Card("card_cascade_test", TargetRule.SingleEnemy, Fx(EffectType.Damage, 105), Arm(react)) } }, Three);
            sim.Unit(1).Hp = 1;
            var events = Cast(sim, 0, target: 1);
            Assert.That(events.OfType<UnitDefeatedEvent>().Any(u => u.Unit == 1), Is.True, "la carte achève sa cible");
            var t = Triggered(events).Single();
            Assert.That((t.Trigger, t.TriggerUnit), Is.EqualTo((ReactionTrigger.EnemyDefeated, 1)));
            Assert.That(t.Targets.Single(), Is.InRange(2, 3), "second coup sur un ennemi vivant au hasard");
        }

        [Test, Description("Vérifie qu'un réarmement entend sa carte exactement comme un premier armement (une fois, ni zéro ni deux) — parce qu'une réaction armée par une carte entend toujours cette carte")]
        public void Rearming_BehavesLikeFirstArming()
        {
            var burn = Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, burn, Arm(Listen())) } }, One);
            int first = Triggered(Cast(sim, 0, target: 1)).Count;
            Assert.That(sim.Unit(0).Reactions.Count, Is.EqualTo(1));
            int again = Triggered(Cast(sim, 0, target: 1)).Count;
            Assert.That(sim.Unit(0).Reactions.Count, Is.EqualTo(1), "réarmée, sans doublon");
            Assert.That((first, again), Is.EqualTo((1, 1)), "premier armement et réarmement entendent la carte une fois");
        }

        [Test, Description("Vérifie qu'une réaction dont l'effet est un ArmReaction ne part pas et ne boucle pas, même quand sa propre carte produit l'événement — parce qu'une réaction n'en arme jamais une autre")]
        public void ReactionEffect_ArmReaction_DoesNotLoop()
        {
            var inner = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 10), StatusType.Burn);
            var outer = React(ReactionTrigger.EnemyReceivesStatus, Arm(inner), StatusType.Burn, onTrigger: true);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2), Arm(outer)) } }, One);
            var events = Cast(sim, 0, target: 1);
            Assert.That(Triggered(events), Is.Empty, "aucun déclenchement");
            Assert.That(sim.Unit(0).Reactions.Select(r => r.Def), Is.EqualTo(new[] { outer }), "seule la réaction de la carte est armée");
            Assert.That(Triggered(Cast(sim, 0, target: 1)), Is.Empty);
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

        [Test, Description("Vérifie que « le porteur est touché » se déclenche quand un coup porte sur le porteur et lui retire des PV, avec l'attaquant pour unité concernée, et pas pour un autre allié touché — parce que la riposte suit le coup reçu par le porteur")]
        public void CarrierHit_Triggers_WhenTheCarrierLosesHp()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Damage, 30), target: TargetRule.SingleEnemy);
            var sim = Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_hit_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 50)) },
            }, One);
            Cast(sim, 0);
            var events = Cast(sim, 1, target: 0);
            Assert.That(events.OfType<DamageEvent>().Any(d => d.Target == 0), Is.True, "le porteur perd des PV");
            var t = Triggered(events).Single();
            Assert.That((t.Carrier, t.Trigger, t.TriggerUnit), Is.EqualTo((0, ReactionTrigger.CarrierHit, 1)), "unité concernée : l'attaquant");
            Assert.That(t.Targets, Is.EqualTo(new[] { 2 }), "cible nommée 1E : le premier ennemi vivant");
            Assert.That(Triggered(Cast(sim, 1, target: 1)), Is.Empty, "un autre allié touché ne compte pas");
        }

        static CombatSimulation HitFight(ReactionDef r)
            => Fight(new[]
            {
                new[] { Card("card_r0", TargetRule.Self, Arm(r)) },
                new[] { Card("card_hit_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 50)) },
            }, One);

        [Test, Description("Vérifie que « le porteur est touché » se déclenche quand le bouclier du porteur absorbe tout le coup (aucun PV perdu) — parce qu'un coup qui porte compte même s'il est absorbé, sinon les cartes qui posent leur propre bouclier ne riposteraient jamais")]
        public void CarrierHit_FiresWhenTheShieldAbsorbsEverything()
        {
            var sim = HitFight(React(ReactionTrigger.CarrierHit, Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2), onTrigger: true));
            Cast(sim, 0);
            sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Shield, 3, 100000));
            var events = Cast(sim, 1, target: 0);
            Assert.That(events.OfType<ShieldAbsorbedEvent>().Any(s => s.Target == 0), Is.True, "le bouclier absorbe");
            Assert.That(events.OfType<DamageEvent>().Any(d => d.Target == 0), Is.False, "aucun PV perdu");
            var t = Triggered(events).Single();
            Assert.That((t.Carrier, t.Trigger, t.TriggerUnit), Is.EqualTo((0, ReactionTrigger.CarrierHit, 1)), "unité concernée : l'attaquant");
            Assert.That(sim.Unit(1).Has(StatusType.Vulnerable), Is.True, "la riposte frappe l'attaquant");
        }

        [Test, Description("Vérifie qu'un coup dont le bouclier absorbe une partie et les PV le reste ne déclenche « le porteur est touché » qu'une fois — parce que la réaction compte les coups, pas les événements de dégâts")]
        public void CarrierHit_FiresOncePerBlow_WhenShieldAndHpBothLose()
        {
            var sim = HitFight(React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10)));
            Cast(sim, 0);
            sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Shield, 3, 1));
            var events = Cast(sim, 1, target: 0);
            Assert.That(events.OfType<ShieldAbsorbedEvent>().Any(s => s.Target == 0), Is.True, "le bouclier absorbe une part");
            Assert.That(events.OfType<DamageEvent>().Any(d => d.Target == 0), Is.True, "les PV perdent le reste");
            Assert.That(Triggered(events).Count, Is.EqualTo(1), "une seule riposte pour un seul coup");
        }

        [Test, Description("Vérifie que « le porteur inflige un critique » se déclenche quand le bouclier de l'ennemi absorbe tout le coup critique — parce que le critique est un fait du coup, pas des PV perdus")]
        public void CarrierCrit_FiresWhenTheShieldAbsorbsTheCriticalBlow()
        {
            var r = React(ReactionTrigger.CarrierCrit, Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2), onTrigger: true);
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)), Card("card_hit", TargetRule.SingleEnemy, Fx(EffectType.Damage, 30)) } }, Two);
            Cast(sim, 0);
            sim.Unit(1).Statuses.Add(new StatusInstance(StatusType.Shield, 3, 100000));
            sim.Unit(1).Statuses.Add(new StatusInstance(StatusType.Mark, 1, 0)); // la Marque rend le coup critique (sans tirage)
            var events = Cast(sim, 0, slot: 3, target: 1);
            Assert.That(events.OfType<ShieldAbsorbedEvent>().Any(s => s.Target == 1), Is.True, "tout absorbé");
            Assert.That(events.OfType<DamageEvent>().Any(d => d.Target == 1), Is.False);
            var t = Triggered(events).Single();
            Assert.That((t.Trigger, t.TriggerUnit), Is.EqualTo((ReactionTrigger.CarrierCrit, 1)), "unité concernée : l'ennemi touché");
            Assert.That(sim.Unit(1).Has(StatusType.Vulnerable), Is.True);
        }

        [Test, Description("Vérifie que la brûlure du porteur, même absorbée par son bouclier, ne déclenche pas « le porteur est touché » — parce que la brûlure n'est pas un coup")]
        public void CarrierHit_StillIgnoresBurn()
        {
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10)))) } }, One);
            Cast(sim, 0);
            sim.Unit(0).Point = 0; // comme CarrierHit_IgnoresBurnDamage : personne prêt avant 8 pas
            sim.Unit(1).Point = 1;
            sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Shield, 5, 100000));
            sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Burn, 5, 0));
            var events = new List<CombatEvent>();
            for (int i = 0; i < 5; i++) events.AddRange(sim.Step(1));
            Assert.That(events.OfType<SkillUsedEvent>(), Is.Empty, "personne n'agit");
            Assert.That(events.OfType<ShieldAbsorbedEvent>().Count(s => s.Target == 0), Is.GreaterThan(1), "le bouclier absorbe la brûlure");
            Assert.That(Triggered(events), Is.Empty, "aucun déclenchement");
        }

        [Test, Description("Vérifie qu'un coup esquivé par le porteur (Esquive) ne déclenche pas « le porteur est touché », alors que le même coup sans Esquive le déclenche — parce qu'un coup esquivé ne porte pas")]
        public void CarrierHit_IgnoresADodgedBlow()
        {
            int TriggersWith(bool evade)
            {
                var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10));
                var mine = new[] { Card("card_r0", TargetRule.Self, Arm(r)) };
                var theirs = new[] { Card("card_hit_foe", TargetRule.SingleEnemy, Fx(EffectType.Damage, 30)) };
                var c = TestContent.Build();
                var col = c.Collection;
                var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(mine).Concat(theirs).ToList(), col.Boosters));
                var sim = new CombatSimulation(catalog, new CombatSetup(3, CombatRank.Elite,
                    new[] { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, mine.Select(k => (CardId?)k.Id).ToArray()) }, new EnemyEntry[0], false)
                    .WithOpponentTeam(new[] { new AllyEntry(new MemberId("ancre"), 5, 0, 0, null, theirs.Select(k => (CardId?)k.Id).ToArray()) }, SideControl.External));
                foreach (var u in sim.Units) u.Point = 0;
                sim.Unit(0).Point = sim.Unit(0).ActionPoint;
                Assert.That(sim.Submit(CombatCommand.UseSkill(0, 2, -1), Side.Ally).OfType<ReactionArmedEvent>().Count(), Is.EqualTo(1), "armée");
                if (evade) sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Evade, 2, 0));
                var foe = sim.Unit(1);
                foe.Point = foe.ActionPoint;
                foe.Energy = foe.MaxEnergy;
                var events = sim.Submit(CombatCommand.UseSkill(1, 2, 0), Side.Enemy);
                Assert.That(events.OfType<SkillUsedEvent>().Any(e => e.Actor == 1), Is.True, "l'adversaire frappe");
                if (evade) Assert.That(events.OfType<MissEvent>().Any(m => m.Target == 0), Is.True, "coup esquivé");
                return Triggered(events).Count;
            }
            Assert.That(TriggersWith(false), Is.EqualTo(1), "le coup qui porte déclenche");
            Assert.That(TriggersWith(true), Is.EqualTo(0), "le coup esquivé ne déclenche rien");
        }

        [Test, Description("Vérifie que la brûlure du porteur ne déclenche pas « le porteur est touché », alors qu'elle lui retire des PV à chaque point de frise — parce que seul un coup d'une unité compte, sinon la réaction partirait une dizaine de fois par tour")]
        public void CarrierHit_IgnoresBurnDamage()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10));
            var sim = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)) } }, One);
            Cast(sim, 0);
            sim.Unit(0).Point = 0; // frise de 10 points ; l'ennemi un point devant : pas de rencontre, personne prêt avant 8 pas
            sim.Unit(1).Point = 1;
            sim.Unit(0).Statuses.Add(new StatusInstance(StatusType.Burn, 5, 0));
            var events = new List<CombatEvent>();
            for (int i = 0; i < 5; i++) events.AddRange(sim.Step(1));
            Assert.That(events.OfType<SkillUsedEvent>(), Is.Empty, "personne n'agit");
            Assert.That(events.OfType<DamageEvent>().Count(d => d.Target == 0), Is.GreaterThan(1), "la brûlure mord plusieurs fois");
            Assert.That(Triggered(events), Is.Empty, "aucun déclenchement");
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
            sim.Unit(0).CritBonusPercent = -1000; // chance de critique nulle : aucun critique, sans tirage
            var plain = Cast(sim, 0, slot: 3, target: 3);
            Assert.That(plain.OfType<DamageEvent>().Single(d => d.Target == 3).Crit, Is.False, "coup du porteur non critique");
            Assert.That(Triggered(plain), Is.Empty, "un coup non critique du porteur ne déclenche rien");
        }

        // ------------------------------------------------------------------ Valeur et cibles

        static EffectDef ArmOf(SkillDef skill) => skill.Effects.Single(e => e.Type == EffectType.ArmReaction);

        /// <summary>Compare une réaction reconstruite à l'originale : tout est copié sauf l'effet.</summary>
        static void AssertSameReactionButEffect(ReactionDef actual, ReactionDef expected)
            => Assert.That((actual.Trigger, actual.Status, actual.Turns, actual.Target, actual.Pick, actual.PickStatus, actual.OnTriggerUnit),
                           Is.EqualTo((expected.Trigger, expected.Status, expected.Turns, expected.Target, expected.Pick, expected.PickStatus, expected.OnTriggerUnit)), "le reste de la réaction est copié");

        [Test, Description("Vérifie que l'effet d'une réaction suit le niveau de la carte (SkillAt, ScalePercent) et les bonus de carte (CardBonuses.Apply) avec les mêmes formules qu'un effet de base, le reste de la réaction étant copié, et qu'en combat les dégâts de la réaction montent avec le niveau — parce que la spec met la réaction à l'échelle comme le reste de la carte")]
        public void ReactionEffect_ScalesWithLevelAndBonuses()
        {
            var r = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 60), StatusType.Burn, onTrigger: true);
            var card = Card("card_r0", TargetRule.SingleEnemy, Arm(r), Fx(EffectType.Damage, 100), Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2));
            foreach (var (skill, expected) in new[] { (CardLevels.SkillAt(card, 10), 60 * 190 / 100), (CardBonuses.Apply(card.Skill, 20), 60 * 120 / 100), (CardLevels.ScalePercent(card.Skill, 300), 180) })
            {
                var arm = ArmOf(skill);
                Assert.That(arm.Reaction!.Effect.Value, Is.EqualTo(expected), "effet de la réaction mis à l'échelle");
                Assert.That(arm.Reaction.Effect.Type, Is.EqualTo(EffectType.Damage));
                AssertSameReactionButEffect(arm.Reaction, r);
                Assert.That(skill.Effects.Single(e => e.Type == EffectType.Damage).Value, Is.GreaterThan(100), "la carte augmente aussi");
            }
            Assert.That(ArmOf(CardLevels.SkillAt(card, 1)).Reaction, Is.SameAs(r), "niveau 1 : réaction inchangée");
            var copy = Arm(r).With(value: 5, duration: 2, chance: 50);
            Assert.That(copy.Reaction, Is.SameAs(r), "With copie la réaction");

            int ReactionDamage(int level, out int cardDamage)
            {
                var sim = Fight(new[] { new[] { card } }, One, seed: 5, cardLevel: level);
                var events = Cast(sim, 0, target: 1); // la Brûlure de la carte déclenche sa propre réaction
                var t = Triggered(events).Single();
                cardDamage = events.OfType<DamageEvent>().First(d => d.Target == 1).Amount;
                return events.Skip(events.IndexOf(t)).OfType<DamageEvent>().Single(d => d.Target == 1).Amount;
            }
            int low = ReactionDamage(1, out int lowCard);
            int high = ReactionDamage(10, out int highCard);
            Assert.That(highCard, Is.GreaterThan(lowCard), "les dégâts de la carte suivent le niveau");
            Assert.That(high, Is.GreaterThan(low), "les dégâts de la réaction aussi");
        }

        [Test, Description("Vérifie que soin, bouclier, recul, élan et jauge d'une réaction suivent CardBonuses.Apply, et que ScalePercent ne touche que dégâts, soins et boucliers — parce qu'un effet imbriqué suit les formules de son type")]
        public void ReactionEffect_ScalesLikeATopLevelEffectOfItsType()
        {
            foreach (var (fx, bonus, scaled) in new[]
            {
                (Fx(EffectType.Heal, 40), 40 * 120 / 100, 40 * 150 / 100),
                (Fx(EffectType.Shield, 20, d: 2), 20 * 120 / 100, 20 * 150 / 100),
                (Fx(EffectType.PushBack, 25), 25 * 120 / 100, 25),
                (Fx(EffectType.Advance, 30), 30 * 120 / 100, 30),
                (Fx(EffectType.GaugeDelta, -100), -100 * 120 / 100, -100),
                (Fx(EffectType.Purge, 1), 1, 1),
            })
            {
                var r = React(ReactionTrigger.CarrierHit, fx);
                var skill = Card("card_r0", TargetRule.Self, Arm(r)).Skill;
                Assert.That(ArmOf(CardBonuses.Apply(skill, 20)).Reaction!.Effect.Value, Is.EqualTo(bonus), "bonus " + fx.Type);
                Assert.That(ArmOf(CardLevels.ScalePercent(skill, 150)).Reaction!.Effect.Value, Is.EqualTo(scaled), "niveau/objets " + fx.Type);
                Assert.That(ArmOf(CardBonuses.Apply(skill, 20)).Reaction!.Effect.Duration, Is.EqualTo(fx.Duration), "durée inchangée hors statut " + fx.Type);
            }
        }

        [Test, Description("Vérifie que l'effet de statut d'une réaction suit CardBonuses.Apply comme un statut de base (durée arrondie, chance plafonnée à 100 %) et que ScalePercent ne le change pas — parce qu'un effet imbriqué suit les formules de son type")]
        public void ReactionStatusEffect_ScalesDurationAndChance_WithCardBonuses()
        {
            var r = React(ReactionTrigger.EnemyDefeated, new EffectDef(EffectType.ApplyStatus, 0, StatusType.Burn, 3, 50), target: TargetRule.AllEnemies);
            var skill = Card("card_r0", TargetRule.Self, Arm(r)).Skill;
            var fx = ArmOf(CardBonuses.Apply(skill, 20)).Reaction!.Effect;
            Assert.That((fx.Type, fx.Status, fx.Duration, fx.ChancePercent), Is.EqualTo((EffectType.ApplyStatus, StatusType.Burn, (3 * 120 + 50) / 100, 60)));
            var capped = ArmOf(CardBonuses.Apply(skill, 150)).Reaction!.Effect;
            Assert.That(capped.ChancePercent, Is.EqualTo(100), "chance plafonnée");
            Assert.That(ArmOf(CardLevels.ScalePercent(skill, 190)).Reaction, Is.SameAs(r), "le niveau ne change pas un statut : réaction inchangée");
        }

        [Test, Description("Vérifie qu'en combat l'effet de la réaction armée suit le bonus de famille (+5 %) et la puissance de compétence d'un objet (+15 %), comme les dégâts de la carte — parce que la réaction est mise à l'échelle comme le reste de la carte")]
        public void ReactionEffect_InCombat_ScalesWithFamilyBonusAndItemSkillPower()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Damage, 60));
            var card = Card("card_r0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 100), Arm(r));
            var plain = Fight(new[] { new[] { card } }, One).Unit(0).Skills[2];
            Assert.That(ArmOf(plain).Reaction!.Effect.Value, Is.EqualTo(60 * 105 / 100), "bonus de famille de verre (Matière)");
            Assert.That(plain.Effects[0].Value, Is.EqualTo(100 * 105 / 100), "comme les dégâts de la carte");
            var withItem = Fight(new[] { new[] { card } }, One, items: new[] { "power_gauntlet" }).Unit(0).Skills[2];
            Assert.That(ArmOf(withItem).Reaction!.Effect.Value, Is.EqualTo(60 * 105 / 100 * 115 / 100), "puis puissance de compétence de l'objet");
            Assert.That(withItem.Effects[0].Value, Is.EqualTo(100 * 105 / 100 * 115 / 100));
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

        [Test, Description("Vérifie qu'une réaction se déclenche aussi sur une commande appliquée par Submit(cmd, camp) en duel mené de l'extérieur — parce que ces commandes ne passent pas par Step")]
        public void Reaction_Triggers_ThroughSubmitFromSide()
        {
            var r = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2), StatusType.Burn, onTrigger: true);
            var cards = new[] { Card("card_r0", TargetRule.Self, Arm(r)), Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) };
            var c = TestContent.Build();
            var col = c.Collection;
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(cards).ToList(), col.Boosters));
            var mine = new[] { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, cards.Select(k => (CardId?)k.Id).ToArray()) };
            var theirs = new[] { new AllyEntry(new MemberId("ancre"), 5, 0) };
            var sim = new CombatSimulation(catalog, new CombatSetup(3, CombatRank.Elite, mine, new EnemyEntry[0], false).WithOpponentTeam(theirs, SideControl.External));
            foreach (var u in sim.Units) u.Point = 0;
            var verre = sim.Unit(0);
            verre.Point = verre.ActionPoint;
            Assert.That(sim.Submit(CombatCommand.UseSkill(0, 2, -1), Side.Ally).OfType<ReactionArmedEvent>().Count(), Is.EqualTo(1), "armée par Submit");
            verre.Point = verre.ActionPoint;
            var events = sim.Submit(CombatCommand.UseSkill(0, 3, 1), Side.Ally);
            var t = Triggered(events).Single();
            Assert.That((t.Carrier, t.TriggerUnit), Is.EqualTo((0, 1)));
            Assert.That(sim.Unit(1).Has(StatusType.Vulnerable), Is.True);
            Assert.That(sim.Log.OfType<ReactionTriggeredEvent>().Count(), Is.EqualTo(1), "journalisé une fois");
        }

        [Test, Description("Vérifie que l'empreinte d'état tient compte des réactions armées (durée restante) — parce que le rejeu compare cette empreinte, et qu'elle ne change pas pour un combat sans réaction")]
        public void StateHash_IncludesArmedReactions()
        {
            var r = React(ReactionTrigger.CarrierHit, Fx(EffectType.Heal, 10));
            var armed = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)) } }, One);
            var plain = Fight(new[] { new[] { Card("card_r0", TargetRule.Self, Arm(r)) } }, One);
            Assert.That(armed.StateHash(), Is.EqualTo(plain.StateHash()), "avant armement : même empreinte");
            Cast(armed, 0);
            ulong h3 = armed.StateHash();
            armed.Unit(0).Reactions[0].TurnsLeft = 1;
            Assert.That(armed.StateHash(), Is.Not.EqualTo(h3), "la durée restante compte");
            armed.Unit(0).Reactions.Clear();
            Assert.That(armed.StateHash(), Is.Not.EqualTo(h3), "la présence de la réaction compte");
        }

        // ------------------------------------------------------------------ Rejeu

        [Test, Description("Vérifie qu'un combat à réactions mené par Submit puis ApplyCommands (cartes lancées pendant la pause d'action, puis SetAuto journalisé au pas où un allié devient prêt) se rejoue à la même empreinte d'état — parce que la vérification serveur repose sur le rejeu, réactions comprises")]
        public void Replay_WithReactions_DrivenByApplyCommands_MatchesTheDirectHash()
        {
            var echo = React(ReactionTrigger.EnemyReceivesStatus, Fx(EffectType.Damage, 40), StatusType.Burn, turns: 5, onTrigger: true);
            var guard = React(ReactionTrigger.CarrierHit, Fx(EffectType.Shield, 10, d: 2), turns: 5);
            var (catalog, setup) = Setup(new[]
            {
                new[] { Card("card_r0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 60), Arm(echo)), Card("card_r1", TargetRule.Self, Arm(guard)) },
                new[] { Card("card_burn", TargetRule.SingleEnemy, Fx(EffectType.Damage, 40), Fx(EffectType.ApplyStatus, 0, StatusType.Burn, 2)) },
            }, Two, seed: 17);
            var sim = new CombatSimulation(catalog, setup);
            int casts = 0, guardSteps = 0;
            bool autoSet = false;
            while (!sim.IsOver && guardSteps++ < 20000)
            {
                sim.Step(1);
                var ready = sim.Units.Where(u => u.Side == Side.Ally && u.IsReady).ToList();
                if (ready.Count == 0 || autoSet) continue;
                if (casts >= 6)
                {
                    sim.Submit(CombatCommand.SetAuto(true)); // journalisé au pas où l'allié devient prêt
                    sim.ApplyCommands();
                    autoSet = true;
                    continue;
                }
                foreach (var u in ready)
                {
                    int slot = sim.CanUse(u.Id, 3) && casts % 2 == 1 ? 3 : sim.CanUse(u.Id, 2) ? 2 : 0;
                    sim.Submit(CombatCommand.UseSkill(u.Id, slot, -1));
                    sim.ApplyCommands();
                    casts++;
                }
            }
            Assert.That(sim.IsOver, Is.True, "le combat se termine");
            Assert.That(autoSet, Is.True, "SetAuto a été journalisé");
            Assert.That(sim.Log.OfType<ReactionTriggeredEvent>().Any(), Is.True, "des réactions se sont déclenchées");
            Assert.That(sim.InputLog.Any(i => i.Command.Type == CommandType.SetAuto), Is.True);
            Assert.That(CombatReplay.Run(catalog, setup, sim.InputLog).StateHash, Is.EqualTo(sim.StateHash()), "même empreinte au rejeu");
        }

        // ------------------------------------------------------------------ Toutes les cartes à réaction du catalogue

        /// <summary>Cartes de test qui provoquent l'événement voulu : un statut sur un ennemi ou un allié, un coup sur un allié (porteur ou victime).</summary>
        static SkillCardDef StatusOn(StatusType s, bool ally) => Card("card_t_" + (ally ? "a_" : "e_") + s, ally ? TargetRule.SingleAlly : TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, s, 2));

        [Test, Description("Vérifie que chaque carte du catalogue qui arme une réaction (au niveau 10, paliers compris) la voit se déclencher au moins une fois quand l'événement voulu se produit (statut reçu, ennemi ou autre allié vaincu, porteur frappé, critique du porteur) — parce qu'une réaction qui ne part jamais serait une carte morte")]
        public void EveryReactionCard_TriggersAtLeastOnce()
        {
            var catalog = TestContent.Build();
            var reactionCards = catalog.Collection.Cards
                .Where(c => CardLevels.SkillAt(c, CardLevels.Max).Effects.Any(e => e.Type == EffectType.ArmReaction && e.Reaction != null)).ToList();
            Assert.That(reactionCards.Count, Is.GreaterThanOrEqualTo(30), "le catalogue contient les cartes à réaction");
            var failures = new List<string>();
            foreach (var card in reactionCards)
            {
                foreach (var r in CardLevels.SkillAt(card, CardLevels.Max).Effects.Where(e => e.Type == EffectType.ArmReaction && e.Reaction != null).Select(e => e.Reaction!))
                {
                    string? why = ProvokeOnce(card, r.Trigger, r.Status);
                    if (why != null) failures.Add(card.Id.Value + " (" + r.Trigger + ") : " + why);
                }
            }
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        /// <summary>Arme la carte sur verre (unité 0, niveau 10), puis provoque l'événement par givre (unité 1) ; ancre (unité 2) sert de victime alliée. Rend null si la réaction part, sinon la raison.</summary>
        static string? ProvokeOnce(SkillCardDef card, ReactionTrigger trigger, StatusType status)
        {
            var hit = Card("card_t_hit", TargetRule.SingleEnemy, Fx(EffectType.Damage, 30));
            var hitAlly = Card("card_t_hit_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 30));
            var killAlly = Card("card_t_kill_ally", TargetRule.SingleAlly, Fx(EffectType.Damage, 500));
            var kill = Card("card_t_kill", TargetRule.SingleEnemy, Fx(EffectType.Damage, 500));
            var provoke = trigger switch
            {
                ReactionTrigger.EnemyReceivesStatus => StatusOn(status, false),
                ReactionTrigger.AllyReceivesStatus => StatusOn(status, true),
                ReactionTrigger.EnemyDefeated => kill,
                ReactionTrigger.AllyDefeated => killAlly,
                ReactionTrigger.CarrierHit => hitAlly,
                _ => hit,
            };
            var c = TestContent.Build();
            var col = c.Collection;
            var extra = new[] { hit, provoke }.Distinct().Where(k => !col.HasCard(k.Id)).ToList();
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(extra).ToList(), col.Boosters));
            var allies = new[]
            {
                new AllyEntry(new MemberId("verre"), 20, 0, 0, null, new CardId?[] { card.Id, hit.Id }, new[] { CardLevels.Max, 1 }),
                new AllyEntry(new MemberId("givre"), 20, 0, 0, null, new CardId?[] { provoke.Id }, new[] { 1 }),
                new AllyEntry(new MemberId("ancre"), 20, 0),
            };
            var sim = new CombatSimulation(catalog, new CombatSetup(7, CombatRank.Normal, allies, Three.Select(e => new EnemyEntry(new EnemyId(e), 30)).ToArray(), false));
            foreach (var u in sim.Units) u.Point = 0;
            List<CombatEvent> Act(int actor, int slot, int target)
            {
                var u = sim.Unit(actor);
                u.Point = u.ActionPoint;
                u.Energy = u.MaxEnergy;
                sim.Submit(CombatCommand.UseSkill(actor, slot, target));
                return sim.Step(1).ToList();
            }
            Act(0, 2, -1);
            if (sim.IsOver) return "le combat se termine à l'armement";
            if (!sim.Unit(0).Reactions.Any()) return "la réaction n'est pas armée";
            foreach (var u in sim.Units) if (u.IsAlive) u.Hp = u.MaxHp; // l'armement a pu blesser : on repart PV pleins
            foreach (var id in new[] { 2, 3 }) sim.Unit(id).Statuses.RemoveAll(s => s.Type == StatusType.Shield || s.Type == StatusType.Evade); // victimes sans protection
            List<CombatEvent> events;
            switch (trigger)
            {
                case ReactionTrigger.EnemyReceivesStatus: events = Act(1, 2, 4); break;
                case ReactionTrigger.AllyReceivesStatus: events = Act(1, 2, 1); break;
                case ReactionTrigger.EnemyDefeated: sim.Unit(3).Hp = 1; events = Act(1, 2, 3); break;
                case ReactionTrigger.AllyDefeated: sim.Unit(2).Hp = 1; events = Act(1, 2, 2); break;
                case ReactionTrigger.CarrierHit: events = Act(1, 2, 0); break;
                default:
                    sim.Unit(3).Statuses.Add(new StatusInstance(StatusType.Mark, 1, 0)); // critique assuré, sans tirage
                    events = Act(0, 3, 3); break;
            }
            return events.OfType<ReactionTriggeredEvent>().Any(t => t.Carrier == 0 && t.Trigger == trigger) ? null : "aucun déclenchement";
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
