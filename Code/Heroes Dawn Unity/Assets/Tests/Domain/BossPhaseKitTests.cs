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
    /// <summary>Phases de boss qui changent de compétences et de style (bloc C de la 1.0.0).</summary>
    public sealed class BossPhaseKitTests
    {
        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static SkillDef S(string id, SkillSlot slot, int cost = 0) => new SkillDef(new SkillId(id), slot, 0, TargetRule.SingleEnemy, new[] { Dmg(100) }, cost);
        static SkillDef[] Kit(string prefix, int n) => Enumerable.Range(0, n).Select(i => S(prefix + (i + 1), (SkillSlot)i, i * 10)).ToArray();

        static BossPhaseDef P(int pct, SkillDef[]? skills = null, AiStyle? style = null) => new BossPhaseDef(pct, "enemies.t_boss.phase", 0, 0, 0, skills, style);

        static EnemyDef Boss(string id, params BossPhaseDef[] phases)
            => new EnemyDef(new EnemyId(id), "enemies." + id + ".name", Position.FrontLine, Family.Matter, new StatBlock(10000, 100, 50, 100),
                            Kit("a", 3), true, phases, AiStyle.Defensive);

        static readonly ContentCatalog Base = TestContent.Build();
        static readonly ContentCatalog C = new ContentCatalog(Base.Balance, Base.Members,
            Base.Enemies.Concat(new[]
            {
                Boss("t_boss", P(66, Kit("b", 2), AiStyle.Control), P(33, Kit("c", 3), AiStyle.Aggressive)),
                Boss("t_boss_nokit", P(66, null, AiStyle.Aggressive), P(33)),
            }).ToList(), Base.Synergies, Base.Zones, Base.Activities, Base.Collection, Base.StarterTeam);

        static CombatSimulation Sim(string boss, ulong seed = 11, bool auto = false)
            => new CombatSimulation(C, new CombatSetup(seed, CombatRank.Boss,
                new[] { new AllyEntry(new MemberId("ancre"), 5, 0), new AllyEntry(new MemberId("givre"), 5, 0) },
                new[] { new EnemyEntry(new EnemyId(boss), 5) }, auto));

        /// <summary>Ramène le boss à <paramref name="percent"/> % de ses PV en un seul coup et renvoie les événements.</summary>
        static IReadOnlyList<CombatEvent> DropTo(CombatSimulation s, CombatUnit boss, int percent)
        {
            int target = (int)(boss.MaxHp * (long)percent / 100);
            return s.ApplyDamageForTests(boss, boss.Hp - target);
        }

        static string Ids(CombatUnit u) => string.Join(",", u.Skills.Select(k => k.Id.Value));

        [Test, Description("Vérifie qu'une phase remplace le jeu de compétences et le style du boss, phase après phase — parce qu'un boss doit changer de façon de combattre en cours de combat")]
        public void Une_phase_remplace_le_jeu_de_competences_et_le_style()
        {
            var s = Sim("t_boss");
            var boss = s.Units.Single(u => u.Side == Side.Enemy);
            Assert.That(Ids(boss), Is.EqualTo("a1,a2,a3"));
            Assert.That(boss.Style, Is.EqualTo(AiStyle.Defensive));

            DropTo(s, boss, 60);
            Assert.That(Ids(boss), Is.EqualTo("b1,b2"));
            Assert.That(boss.Style, Is.EqualTo(AiStyle.Control));

            DropTo(s, boss, 30);
            Assert.That(Ids(boss), Is.EqualTo("c1,c2,c3"));
            Assert.That(boss.Style, Is.EqualTo(AiStyle.Aggressive));
        }

        [Test, Description("Vérifie que l'action annoncée sur la frise est annulée à l'ouverture d'une phase — parce que l'ancien emplacement n'existe peut-être plus dans le nouveau jeu (la recharge des compétences ne dépend que de l'énergie, conservée)")]
        public void L_action_annoncee_est_annulee_a_l_ouverture_d_une_phase()
        {
            var s = Sim("t_boss");
            var boss = s.Units.Single(u => u.Side == Side.Enemy);
            boss.PlannedSlot = 2;
            boss.PlannedTarget = 0;
            int energy = boss.Energy;
            DropTo(s, boss, 60);
            Assert.That(boss.PlannedSlot, Is.EqualTo(-1));
            Assert.That(boss.PlannedTarget, Is.EqualTo(-1));
            Assert.That(boss.Energy, Is.EqualTo(energy));
        }

        [Test, Description("Vérifie qu'une phase sans jeu de compétences garde le précédent (et son style s'il n'en a pas) — parce qu'une phase peut ne changer que les statistiques")]
        public void Une_phase_sans_competences_garde_le_kit_precedent()
        {
            var s = Sim("t_boss_nokit");
            var boss = s.Units.Single(u => u.Side == Side.Enemy);
            DropTo(s, boss, 60);
            Assert.That(Ids(boss), Is.EqualTo("a1,a2,a3"));
            Assert.That(boss.Style, Is.EqualTo(AiStyle.Aggressive), "la phase 2 change le style seulement");
            DropTo(s, boss, 30);
            Assert.That(Ids(boss), Is.EqualTo("a1,a2,a3"));
            Assert.That(boss.Style, Is.EqualTo(AiStyle.Aggressive), "la phase 3 ne change rien : le style de la phase 2 reste");
        }

        [Test, Description("Vérifie qu'un coup qui franchit deux seuils ouvre les deux phases dans l'ordre, que le dernier jeu est celui de la phase 3 et que KitChanged n'est vrai que si le jeu change — parce que l'interface doit annoncer chaque phase et savoir quand rafraîchir les compétences")]
        public void Un_coup_qui_franchit_deux_seuils_ouvre_les_deux_phases_dans_l_ordre()
        {
            var s = Sim("t_boss");
            var boss = s.Units.Single(u => u.Side == Side.Enemy);
            DropTo(s, boss, 70);
            var events = DropTo(s, boss, 30).OfType<BossPhaseChangedEvent>().ToList();
            Assert.That(events.Select(e => e.Phase), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(events.All(e => e.KitChanged), Is.True);
            Assert.That(Ids(boss), Is.EqualTo("c1,c2,c3"));

            var sansKit = Sim("t_boss_nokit");
            var other = sansKit.Units.Single(u => u.Side == Side.Enemy);
            var changes = DropTo(sansKit, other, 30).OfType<BossPhaseChangedEvent>().ToList();
            Assert.That(changes.Count, Is.EqualTo(2));
            Assert.That(changes.Any(e => e.KitChanged), Is.False);
        }

        [Test, Description("Vérifie que deux combats de boss à la même graine, avec changements de phase, produisent la même empreinte — parce que le rejeu de vérification doit rester exact")]
        public void Le_combat_reste_deterministe()
        {
            ulong Run()
            {
                var s = Sim("t_boss", 123, true);
                var boss = s.Units.Single(u => u.Side == Side.Enemy);
                for (int i = 0; i < 400 && !s.IsOver; i++)
                {
                    s.Step(1);
                    if (i == 20) DropTo(s, boss, 50);
                    if (i == 60) DropTo(s, boss, 20);
                }
                return s.StateHash();
            }
            Assert.That(Run(), Is.EqualTo(Run()));
        }

        [Test, Description("Vérifie qu'un boss accepte de 1 à 3 seuils de phase et refuse 4 — parce qu'un boss a de 2 à 4 phases, phase initiale comprise")]
        public void Un_boss_a_au_plus_3_seuils_de_phase()
        {
            Assert.DoesNotThrow(() => Boss("x1", P(50)));
            Assert.DoesNotThrow(() => Boss("x3", P(75), P(50), P(25)));
            Assert.Throws<ArgumentException>(() => Boss("x4", P(80), P(60), P(40), P(20)));
        }
    }
}
