#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Effets « Consomme » (retire un statut puis frappe ou soigne selon ses tours restants) et « Propage » (copie un statut sur les autres ennemis).</summary>
    public sealed class ConsumeSpreadTests
    {
        static readonly string[] One = { "brute" };
        static readonly string[] Three = { "brute", "brute", "brute" };

        static SkillCardDef Card(string id, TargetRule target, params EffectDef[] effects)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", Family.Elemental, 1, new SkillDef(new SkillId(id), SkillSlot.A3, 0, target, effects, 0), CardNature.Offensive); // ni famille ni position de « verre » : aucun bonus de carte sur les dégâts

        static EffectDef Fx(EffectType t, int v, StatusType s = StatusType.None, int d = 0, int chance = 100) => new EffectDef(t, v, s, d, chance);

        /// <summary>Un allié (unité 0) portant la carte, puis les ennemis ; toutes les unités partent au point 0.</summary>
        static CombatSimulation Fight(SkillCardDef card, string[] enemies, ulong seed = 11)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var allies = new[] { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { card.Id }, new[] { 1 }) };
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, allies,
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));
            foreach (var u in sim.Units) u.Point = 0;
            return sim;
        }

        static List<CombatEvent> Cast(CombatSimulation sim, int target)
        {
            var u = sim.Unit(0);
            u.Point = u.ActionPoint;
            u.Energy = u.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(0, 2, target));
            var events = sim.Step(1).ToList();
            Assert.That(events.OfType<SkillUsedEvent>().Any(e => e.Actor == 0), Is.True, "la carte est lancée");
            return events;
        }

        static void Give(CombatSimulation sim, int unit, StatusType s, int turns, int value = 0) => sim.Unit(unit).Statuses.Add(new StatusInstance(s, turns, value));

        static DamageEvent TheHit(IEnumerable<CombatEvent> events) => events.OfType<DamageEvent>().Single();

        /// <summary>Référence : le même combat, même graine, avec un simple coup de dégâts de la puissance donnée sur l'ennemi 1 (sans statut).</summary>
        static DamageEvent PlainHit(int power)
        {
            var sim = Fight(Card("card_ref", TargetRule.SingleEnemy, Fx(EffectType.Damage, power)), One);
            return TheHit(Cast(sim, 1));
        }

        static void AssertSameHit(DamageEvent actual, DamageEvent expected)
        {
            Assert.That((actual.Amount, actual.Crit), Is.EqualTo((expected.Amount, expected.Crit)));
        }

        // ------------------------------------------------------------------ Consomme

        [Test, Description("Vérifie que ConsumeDamage retire le statut puis frappe une seule fois à N × tours restants — parce que la brûlure consommée se convertit en un coup unique")]
        public void ConsumeDamage_RemovesTheStatus_AndHitsForNTimesRemainingTurns()
        {
            var sim = Fight(Card("card_c0", TargetRule.SingleEnemy, Fx(EffectType.ConsumeDamage, 30, StatusType.Burn, 4)), One);
            Give(sim, 1, StatusType.Burn, 3);
            var events = Cast(sim, 1);
            Assert.That(sim.Unit(1).Has(StatusType.Burn), Is.False, "statut retiré");
            Assert.That(events.OfType<StatusExpiredEvent>().Count(e => e.Target == 1 && e.Status == StatusType.Burn), Is.EqualTo(1));
            AssertSameHit(TheHit(events), PlainHit(90));
        }

        [Test, Description("Vérifie que ConsumeDamage compte au plus M tours — parce que le maximum de la fiche borne la puissance")]
        public void ConsumeDamage_IsCappedByMaxTurns()
        {
            var sim = Fight(Card("card_c1", TargetRule.SingleEnemy, Fx(EffectType.ConsumeDamage, 30, StatusType.Burn, 4)), One);
            Give(sim, 1, StatusType.Burn, 7);
            AssertSameHit(TheHit(Cast(sim, 1)), PlainHit(120));
        }

        [Test, Description("Vérifie que consommer une Marque compte un seul tour, quel que soit son compteur — parce que la Marque n'a pas de compte à rebours")]
        public void ConsumeMark_CountsOneTurn()
        {
            var sim = Fight(Card("card_c2", TargetRule.SingleEnemy, Fx(EffectType.ConsumeDamage, 70, StatusType.Mark, 1)), One);
            Give(sim, 1, StatusType.Mark, 5);
            var events = Cast(sim, 1);
            Assert.That(sim.Unit(1).Has(StatusType.Mark), Is.False);
            AssertSameHit(TheHit(events), PlainHit(70));
        }

        [Test, Description("Vérifie que ConsumeHeal retire le statut et soigne la cible de N × tours (plafonné par M) % de ses PV max — parce que la régénération restante est encaissée d'un coup")]
        public void ConsumeHeal_HealsTheTarget()
        {
            var sim = Fight(Card("card_c3", TargetRule.Self, Fx(EffectType.ConsumeHeal, 18, StatusType.Regen, 3)), One);
            var me = sim.Unit(0);
            me.Hp = me.MaxHp / 4;
            int before = me.Hp;
            Give(sim, 0, StatusType.Regen, 5);
            var events = Cast(sim, -1);
            int expected = me.MaxHp * 54 / 100;
            Assert.That(me.Has(StatusType.Regen), Is.False);
            Assert.That(me.Hp, Is.EqualTo(before + expected));
            Assert.That(events.OfType<HealEvent>().Single(e => e.Target == 0).Amount, Is.EqualTo(expected));
            Assert.That(events.OfType<StatusExpiredEvent>().Any(e => e.Target == 0 && e.Status == StatusType.Regen), Is.True);
        }

        [Test, Description("Vérifie que Consomme ne fait rien, sans événement ni tirage, quand la cible n'a pas le statut — parce que l'effet n'a rien à consommer et ne doit pas décaler le hasard du combat")]
        public void Consume_DoesNothing_WhenTheStatusIsAbsent()
        {
            var sim = Fight(Card("card_c4", TargetRule.SingleEnemy, Fx(EffectType.ConsumeDamage, 30, StatusType.Burn, 4), Fx(EffectType.ConsumeHeal, 10, StatusType.Regen, 2)), One);
            var events = Cast(sim, 1);
            Assert.That(events.OfType<DamageEvent>(), Is.Empty);
            Assert.That(events.OfType<HealEvent>(), Is.Empty);
            Assert.That(events.OfType<StatusExpiredEvent>(), Is.Empty);
            var reference = Fight(Card("card_c4", TargetRule.SingleEnemy), One);
            Cast(reference, 1);
            Assert.That(sim.RngProbeForTests(), Is.EqualTo(reference.RngProbeForTests()), "aucun tirage consommé");
        }

        [Test, Description("Vérifie que le coup de ConsumeDamage suit la formule normale (critique, défense, variance : un seul coup de puissance N × tours) sur plusieurs graines — parce qu'il passe par HitWithDamage")]
        public void ConsumeDamage_UsesTheNormalDamageFormula()
        {
            for (ulong seed = 1; seed <= 12; seed++)
            {
                var sim = Fight(Card("card_c5", TargetRule.SingleEnemy, Fx(EffectType.ConsumeDamage, 25, StatusType.Burn, 4)), One, seed);
                Give(sim, 1, StatusType.Burn, 2);
                var events = Cast(sim, 1);
                var reference = Fight(Card("card_ref", TargetRule.SingleEnemy, Fx(EffectType.Damage, 50)), One, seed);
                AssertSameHit(TheHit(events), TheHit(Cast(reference, 1)));
                Assert.That(sim.RngProbeForTests(), Is.EqualTo(reference.RngProbeForTests()), "mêmes tirages qu'un coup normal, graine " + seed);
            }
        }

        // ------------------------------------------------------------------ Propage

        [Test, Description("Vérifie que Propage copie le statut (mêmes tours restants) sur tous les autres ennemis vivants, en gardant le maximum de tours chez ceux qui l'ont déjà — parce que la propagation étend le mal sans l'affaiblir")]
        public void Spread_CopiesTheStatusWithRemainingTurnsToAllOtherEnemies()
        {
            var sim = Fight(Card("card_s0", TargetRule.SingleEnemy, Fx(EffectType.Spread, 0, StatusType.Burn)), Three);
            Give(sim, 1, StatusType.Burn, 3);
            Give(sim, 3, StatusType.Burn, 5);
            var events = Cast(sim, 1);
            Assert.That(sim.Unit(2).Find(StatusType.Burn)!.TurnsLeft, Is.EqualTo(3));
            Assert.That(sim.Unit(3).Find(StatusType.Burn)!.TurnsLeft, Is.EqualTo(5), "le maximum de tours est conservé");
            Assert.That(sim.Unit(1).Find(StatusType.Burn)!.TurnsLeft, Is.EqualTo(3), "la source ne change pas");
            Assert.That(events.OfType<StatusAppliedEvent>().Any(e => e.Target == 2 && e.Status == StatusType.Burn), Is.True, "passe par l'application normale");
            Assert.That(sim.Unit(0).Statuses, Is.Empty, "aucune copie chez les alliés");
        }

        [Test, Description("Vérifie que Propage saute les ennemis vaincus et la cible source — parce qu'on ne ressuscite pas un statut sur un mort et que la source le porte déjà")]
        public void Spread_SkipsDeadAndSourceTargets()
        {
            var sim = Fight(Card("card_s1", TargetRule.SingleEnemy, Fx(EffectType.Spread, 0, StatusType.Shock)), Three);
            Give(sim, 1, StatusType.Shock, 2);
            sim.Unit(2).Hp = 0;
            var events = Cast(sim, 1);
            Assert.That(sim.Unit(2).Statuses, Is.Empty, "mort ignoré");
            Assert.That(sim.Unit(3).Find(StatusType.Shock)!.TurnsLeft, Is.EqualTo(2));
            Assert.That(events.OfType<StatusAppliedEvent>().Any(e => e.Target == 1), Is.False, "la source ne reçoit rien");
            Assert.That(events.OfType<StatusAppliedEvent>().Any(e => e.Target == 2), Is.False);
        }

        [Test, Description("Vérifie que Propage ne fait rien quand la cible n'a pas le statut — parce qu'il n'y a rien à copier")]
        public void Spread_DoesNothing_WhenAbsent()
        {
            var sim = Fight(Card("card_s2", TargetRule.SingleEnemy, Fx(EffectType.Spread, 0, StatusType.Bind)), Three);
            Give(sim, 2, StatusType.Bind, 4); // un autre ennemi le porte : seule la cible compte
            var events = Cast(sim, 1);
            Assert.That(events.OfType<StatusAppliedEvent>(), Is.Empty);
            Assert.That(sim.Unit(1).Statuses, Is.Empty);
            Assert.That(sim.Unit(3).Statuses, Is.Empty);
        }
    }
}
