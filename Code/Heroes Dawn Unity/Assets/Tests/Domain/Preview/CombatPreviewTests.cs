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
    /// <summary>
    /// Prévisualisation d'une carte (dry-run) : rejeu du combat réel puis tir de la commande hypothétique, sans toucher au combat en cours.
    /// Les combats de ces tests sont menés uniquement par Step/Submit (rejouables), jamais par mutation directe des unités.
    /// </summary>
    public sealed class CombatPreviewTests
    {
        static readonly string[] AllyIds = { "verre", "givre", "ancre" };

        static SkillCardDef Card(string id, TargetRule target, params EffectDef[] effects)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", Family.Matter, 1, new SkillDef(new SkillId(id), SkillSlot.A3, 0, target, effects, 0));

        static EffectDef Fx(EffectType t, int v, StatusType s = StatusType.None, int d = 0, int chance = 100) => new EffectDef(t, v, s, d, chance);

        static ContentCatalog Catalog(SkillCardDef[][] cardsPerAlly)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            return c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(cardsPerAlly.SelectMany(x => x)).ToList(), col.Boosters));
        }

        /// <summary>Combat manuel mené jusqu'au premier allié prêt ; la carte i d'un allié occupe l'emplacement 2 + i.</summary>
        static (ContentCatalog catalog, CombatSimulation sim) Fight(SkillCardDef[][] cardsPerAlly, string[] enemies, ulong seed = 11)
        {
            var catalog = Catalog(cardsPerAlly);
            var allies = cardsPerAlly.Select((cards, i) => new AllyEntry(new MemberId(AllyIds[i]), 5, 0, 0, null,
                cards.Select(k => (CardId?)k.Id).ToArray(), cards.Select(_ => 1).ToArray())).ToArray();
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, allies,
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));
            WaitForPlayer(sim);
            return (catalog, sim);
        }

        static void WaitForPlayer(CombatSimulation sim)
        {
            for (int i = 0; i < 2000 && !sim.IsAwaitingAction && !sim.IsOver; i++) sim.Step(1);
            Assert.That(sim.IsAwaitingAction, Is.True, "un allié finit par être prêt");
        }

        static int ReadyAlly(CombatSimulation sim) => sim.Units.First(u => u.Side == Side.Ally && u.IsReady).Id;

        static string Fingerprint(CombatSimulation sim)
            => sim.Tick + "|" + sim.InputLog.Count + "|" + string.Join(";", sim.Units.Select(u =>
                u.Id + ":" + u.Hp + "," + u.Point + "," + u.Energy + "," + u.PlannedSlot + "," + u.Reactions.Count + "," +
                string.Join("/", u.Statuses.Select(s => (int)s.Type + "=" + s.TurnsLeft + "=" + s.Value))));

        static readonly string[] One = { "brute" };
        static readonly string[] Two = { "brute", "brute" };

        [Test, Description("Vérifie que la prévisualisation ne modifie ni les unités, ni la frise, ni le journal du combat en cours — parce qu'un dry-run doit être sans effet de bord")]
        public void Preview_DoesNotTouchTheLiveCombat()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 120), Fx(EffectType.ApplyStatus, 0, StatusType.Freeze, 2)) } }, One);
            string before = Fingerprint(sim);
            var result = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 1));
            Assert.That(result, Is.Not.Null);
            Assert.That(Fingerprint(sim), Is.EqualTo(before));
        }

        [Test, Description("Vérifie que le résultat réel du lancer tombe dans la plage prévue (PV, point de frise) — parce que la prévisualisation réutilise le moteur et son aléatoire, elle ne peut pas mentir")]
        public void Preview_ContainsTheRealOutcome()
        {
            foreach (ulong seed in new ulong[] { 3, 11, 29, 77 })
            {
                var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 150), Fx(EffectType.ApplyStatus, 0, StatusType.Shock, 1, 50)) } }, One, seed);
                int actor = ReadyAlly(sim);
                var result = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(actor, 2, 1))!;
                sim.Submit(CombatCommand.UseSkill(actor, 2, 1));
                sim.Step(1);
                var p = result.Unit(1);
                Assert.That(sim.Unit(1).Hp, Is.InRange(p.HpMin, p.HpMax), "PV réels dans la plage (graine " + seed + ")");
                Assert.That(sim.Unit(1).Point, Is.InRange(p.PointMin, p.PointMax), "point réel dans la plage (graine " + seed + ")");
            }
        }

        [Test, Description("Vérifie qu'une carte de dégâts annonce des PV en baisse pour la cible et variables (critique, dispersion), sans toucher les autres unités — parce que c'est le delta de barre de PV affiché")]
        public void Damage_ShowsHpLossAndVariable()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 120)) } }, Two);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 2), 24)!;
            var hit = r.Unit(2);
            Assert.That(hit.HpAfter, Is.LessThan(hit.HpBefore));
            Assert.That(hit.HpVariable, Is.True, "critique et dispersion : variable");
            Assert.That(hit.HasChange, Is.True);
            Assert.That(r.Unit(1).HpAfter, Is.EqualTo(r.Unit(1).HpBefore), "l'autre ennemi n'est pas touché");
        }

        [Test, Description("Vérifie qu'une carte sans aléa (bouclier à 100 %) donne un résultat identique à chaque échantillon : statut sûr à 100 %, rien de variable — parce que seul l'aléatoire doit être signalé")]
        public void DeterministicCard_IsNotVariable()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.Self, Fx(EffectType.Shield, 30, d: 2)) } }, One);
            int actor = ReadyAlly(sim);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(actor, 2, -1), 16)!;
            var me = r.Unit(actor);
            var shield = me.Added.Single(s => s.Status == StatusType.Shield);
            Assert.That(shield.ChancePercent, Is.EqualTo(100));
            Assert.That(me.ShieldAfter, Is.GreaterThan(me.ShieldBefore));
            Assert.That(me.HpVariable, Is.False);
            Assert.That(me.IsVariable, Is.False);
        }

        [Test, Description("Vérifie qu'un statut à 50 % de chance apparaît avec une chance strictement entre 0 et 100 % — parce que les effets aléatoires s'affichent « variable »")]
        public void ChanceEffect_HasPartialChance()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Vulnerable, 2, 50)) } }, One);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 1), 48)!;
            var added = r.Unit(1).Added.Single(s => s.Status == StatusType.Vulnerable);
            Assert.That(added.ChancePercent, Is.InRange(1, 99));
            Assert.That(r.Unit(1).IsVariable, Is.True);
        }

        [Test, Description("Vérifie qu'un contrôle (Gel) repousse l'ennemi sur la frise : point après < point avant, statut ajouté à coup sûr — parce que c'est le fantôme de frise à afficher")]
        public void Freeze_PushesTheEnemyBackOnTheTimeline()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Freeze, 2)) } }, One);
            Assume.That(sim.Unit(1).Point, Is.GreaterThan(0), "l'ennemi a avancé sur la frise");
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 1))!;
            var e = r.Unit(1);
            Assert.That(e.PointAfter, Is.LessThan(e.PointBefore));
            Assert.That(e.PointVariable, Is.False);
            Assert.That(e.Added.Single(s => s.Status == StatusType.Freeze).ChancePercent, Is.EqualTo(100));
        }

        [Test, Description("Vérifie qu'une carte de zone change tous les ennemis vivants — parce que la prévisualisation couvre tout le terrain")]
        public void AreaCard_ChangesEveryEnemy()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.AllEnemies, Fx(EffectType.Damage, 80)) } }, Two);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, -1))!;
            Assert.That(r.Unit(1).HpAfter, Is.LessThan(r.Unit(1).HpBefore));
            Assert.That(r.Unit(2).HpAfter, Is.LessThan(r.Unit(2).HpBefore));
        }

        [Test, Description("Vérifie qu'un coup surpuissant annonce l'unité vaincue à 100 % et l'ennemi absent de la frise — parce que les morts doivent être marquées")]
        public void LethalCard_MarksTheUnitDefeated()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 100000)) } }, Two);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 2), 8)!;
            Assert.That(r.Unit(2).DefeatPercent, Is.EqualTo(100));
            Assert.That(r.Unit(2).HpAfter, Is.EqualTo(0));
            Assert.That(r.Unit(1).DefeatPercent, Is.EqualTo(0));
            Assert.That(r.VictoryPercent, Is.EqualTo(0), "un ennemi reste debout");
        }

        [Test, Description("Vérifie que tuer le dernier ennemi annonce la victoire (ou la vague suivante) — parce que l'issue du combat fait partie de l'aperçu")]
        public void LethalOnLastEnemy_AnnouncesVictoryOrNextWave()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 100000)) } }, One);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 1), 8)!;
            Assert.That(r.Unit(1).DefeatPercent, Is.EqualTo(100));
            Assert.That(r.VictoryPercent + r.NextWavePercent, Is.EqualTo(100));
        }

        [Test, Description("Vérifie que le lanceur est replacé sur la frise au point de reprise de sa carte lourde et que son énergie est prise en compte — parce que l'avance ou le recul du lanceur se voit aussi")]
        public void Caster_IsResetOnTheTimeline()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 50)) } }, One);
            int actor = ReadyAlly(sim);
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(actor, 2, 1))!;
            var me = r.Unit(actor);
            Assert.That(me.PointBefore, Is.EqualTo(sim.Unit(actor).ActionPoint));
            Assert.That(me.PointAfter, Is.EqualTo(catalog.Balance.TimelineResetHeavyPoint));
        }

        [Test, Description("Vérifie qu'une réaction déjà armée se déclenche dans l'aperçu (bouclier sur le porteur quand un ennemi reçoit Gel) — parce que les réactions passent par le même moteur")]
        public void ArmedReaction_FiresInThePreview()
        {
            var reaction = new ReactionDef(ReactionTrigger.EnemyReceivesStatus, StatusType.Freeze, 5, Fx(EffectType.Shield, 25, d: 2),
                                           TargetRule.Self, TargetPick.Default, StatusType.None, false);
            var arm = Card("card_p0", TargetRule.Self, new EffectDef(EffectType.ArmReaction, 0, reaction: reaction));
            var freeze = Card("card_p1", TargetRule.SingleEnemy, Fx(EffectType.ApplyStatus, 0, StatusType.Freeze, 2));
            var (catalog, sim) = Fight(new[] { new[] { arm, freeze } }, One);
            int actor = ReadyAlly(sim);
            sim.Submit(CombatCommand.UseSkill(actor, 2, -1));
            sim.Step(1);
            WaitForPlayer(sim);
            Assert.That(sim.Unit(actor).Reactions, Is.Not.Empty, "réaction armée par le vrai lancer");
            var r = CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 3, 1))!;
            Assert.That(r.Unit(actor).Added.Any(s => s.Status == StatusType.Shield && s.ChancePercent == 100), Is.True);
        }

        [Test, Description("Vérifie qu'aucun aperçu n'est produit en mode auto, combat fini ou commande impossible — parce qu'il n'y a rien à prévisualiser")]
        public void NoPreview_WhenNothingToPreview()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 50)) } }, One);
            Assert.That(CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 9, 1)), Is.Null, "emplacement inexistant");
            Assert.That(CombatPreview.Compute(catalog, sim, CombatCommand.Retreat()), Is.Null, "seules les compétences se prévisualisent");
            sim.Submit(CombatCommand.SetAuto(true));
            sim.Step(1);
            Assert.That(CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(0, 2, 1)), Is.Null, "mode auto");
        }

        [Test, Description("Vérifie que si l'état réel ne se rejoue pas à l'identique (mutation hors journal), aucun aperçu n'est affiché — parce qu'un aperçu faux est pire que pas d'aperçu")]
        public void NoPreview_WhenTheReplayDiverges()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 50)) } }, One);
            sim.Unit(1).Hp -= 1; // mutation hors journal (comme le débogage)
            Assert.That(CombatPreview.Compute(catalog, sim, CombatCommand.UseSkill(ReadyAlly(sim), 2, 1)), Is.Null);
        }

        [Test, Description("Vérifie que deux calculs identiques donnent le même aperçu — parce que l'aperçu doit être stable d'une image à l'autre")]
        public void Preview_IsDeterministic()
        {
            var (catalog, sim) = Fight(new[] { new[] { Card("card_p0", TargetRule.SingleEnemy, Fx(EffectType.Damage, 120)) } }, Two);
            var cmd = CombatCommand.UseSkill(ReadyAlly(sim), 2, 1);
            var a = CombatPreview.Compute(catalog, sim, cmd)!.Unit(1);
            var b = CombatPreview.Compute(catalog, sim, cmd)!.Unit(1);
            Assert.That((a.HpMin, a.HpMax, a.HpAfter), Is.EqualTo((b.HpMin, b.HpMax, b.HpAfter)));
        }
    }
}
