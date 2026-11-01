using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Phases des boss : seuils de PV dans les données, bonus permanents, événement de changement de phase (lot L8 de la 0.15.0).</summary>
    public sealed class BossPhaseTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        static CombatSimulation Sim(string boss = "brick", ulong seed = 11)
            => new CombatSimulation(C, new CombatSetup(seed, CombatRank.Boss,
                new[] { new AllyEntry(new MemberId("ancre"), 5, 0) },
                new[] { new EnemyEntry(new EnemyId(boss), 5) }, false));

        /// <summary>L'allié frappe le boss jusqu'à lui infliger des dégâts (un coup peut rater) ; renvoie tous les événements produits.</summary>
        static List<CombatEvent> HitBoss(CombatSimulation s, CombatUnit boss)
        {
            var ally = s.Units.First(u => u.Side == Side.Ally);
            var all = new List<CombatEvent>();
            for (int i = 0; i < 30 && !all.OfType<DamageEvent>().Any(d => d.Target == boss.Id); i++)
            {
                foreach (var u in s.Units.Where(x => x.Side == Side.Enemy)) u.Point = 0;
                ally.Point = ally.ActionPoint;
                s.Submit(CombatCommand.UseSkill(ally.Id, 0, boss.Id));
                all.AddRange(s.Step(1));
            }
            Assert.That(all.OfType<DamageEvent>().Any(d => d.Target == boss.Id), Is.True, "le boss a reçu un coup");
            return all;
        }

        [Test, Description("Vérifie que les trois boss de la campagne ont deux phases aux seuils 66 % puis 33 % — parce que chaque combat de boss doit changer de rythme deux fois")]
        public void CampaignBosses_HaveTwoPhases_AtDecreasingThresholds()
        {
            foreach (var id in new[] { "brick", "bridge_titan", "crocodile" })
            {
                var def = C.Enemy(new EnemyId(id));
                Assert.That(def.IsBoss, Is.True, id);
                Assert.That(def.Phases.Select(p => p.ThresholdPercent), Is.EqualTo(new[] { 66, 33 }), id);
            }
        }

        [Test, Description("Vérifie qu'une définition refuse des phases sur un ennemi ordinaire, un seuil hors de 1 à 99 % et des seuils qui ne décroissent pas — parce que des données incohérentes doivent échouer au chargement")]
        public void EnemyDef_RejectsInconsistentPhases()
        {
            var skills = new List<SkillDef>();
            var stats = new StatBlock(100, 10, 10, 10);
            BossPhaseDef P(int pct) => new BossPhaseDef(pct, "k", 0, 0, 0);
            EnemyDef Make(bool boss, params BossPhaseDef[] phases) => new EnemyDef(new EnemyId("x"), "k", Role.Tank, Family.Matter, stats, skills, boss, phases);
            Assert.DoesNotThrow(() => Make(true, P(66), P(33)));
            Assert.Throws<ArgumentException>(() => Make(false, P(50)), "pas de phase sans être boss");
            Assert.Throws<ArgumentException>(() => Make(true, P(0)));
            Assert.Throws<ArgumentException>(() => Make(true, P(100)));
            Assert.Throws<ArgumentException>(() => Make(true, P(33), P(66)), "seuils croissants");
            Assert.Throws<ArgumentException>(() => Make(true, P(50), P(50)), "seuils égaux");
        }

        [Test, Description("Vérifie que tant que les PV restent au-dessus du premier seuil, aucune phase ne s'ouvre — parce que le combat commence en phase 1")]
        public void NoPhaseChange_AboveTheFirstThreshold()
        {
            var s = Sim();
            var boss = s.Units.First(u => u.Side == Side.Enemy);
            Assert.That(boss.PhaseReached, Is.EqualTo(0));
            var events = HitBoss(s, boss);
            Assert.That(events.OfType<BossPhaseChangedEvent>(), Is.Empty);
            Assert.That(boss.PhaseReached, Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un coup qui fait passer le boss sous 66 % ouvre la phase 2 avec un seul événement et que le boss gagne ses bonus permanents — parce que le joueur doit voir le combat changer de rythme")]
        public void CrossingTheFirstThreshold_OpensPhaseTwo_AndBuffsTheBoss()
        {
            var s = Sim();
            var boss = s.Units.First(u => u.Side == Side.Enemy);
            boss.Hp = boss.MaxHp * 66 / 100 + 1;
            int atk = boss.Atk, spd = boss.Spd;
            var events = HitBoss(s, boss);
            var changes = events.OfType<BossPhaseChangedEvent>().ToList();
            Assert.That(changes.Count, Is.EqualTo(1));
            Assert.That(changes[0].Unit, Is.EqualTo(boss.Id));
            Assert.That(changes[0].Phase, Is.EqualTo(2), "numéro de la phase ouverte");
            Assert.That(changes[0].PhaseCount, Is.EqualTo(3), "trois phases en tout : deux seuils");
            Assert.That(changes[0].NameKey, Is.EqualTo("enemies.brick.phase2"));
            Assert.That(boss.PhaseReached, Is.EqualTo(1));
            Assert.That(boss.Atk, Is.EqualTo(atk + atk * 10 / 100), "+10 % de force");
            Assert.That(boss.Spd, Is.EqualTo(spd + spd * 5 / 100), "+5 % de vitesse");
        }

        [Test, Description("Vérifie qu'un boss déjà sous les deux seuils ouvre les deux phases d'un coup, dans l'ordre, puis qu'un coup suivant n'en rouvre aucune — parce qu'une phase ne se déclenche qu'une fois")]
        public void CrossingBothThresholds_OpensBothInOrder_ThenNeverAgain()
        {
            var s = Sim();
            var boss = s.Units.First(u => u.Side == Side.Enemy);
            boss.Hp = boss.MaxHp * 33 / 100;
            int def = boss.Def;
            var events = HitBoss(s, boss);
            Assert.That(events.OfType<BossPhaseChangedEvent>().Select(e => e.Phase), Is.EqualTo(new[] { 2, 3 }));
            Assert.That(boss.PhaseReached, Is.EqualTo(2));
            Assert.That(boss.Def, Is.LessThan(def), "la phase 3 sacrifie de la défense pour la force");
            if (boss.IsAlive) Assert.That(HitBoss(s, boss).OfType<BossPhaseChangedEvent>(), Is.Empty);
        }

        [Test, Description("Vérifie qu'un ennemi ordinaire n'a aucune phase et ne produit jamais d'événement de phase — parce que seuls les boss changent de phase")]
        public void OrdinaryEnemies_HaveNoPhases()
        {
            var s = Sim("brute");
            var enemy = s.Units.First(u => u.Side == Side.Enemy);
            Assert.That(enemy.Phases, Is.Empty);
            enemy.Hp = Math.Max(2, enemy.MaxHp / 5);
            Assert.That(HitBoss(s, enemy).OfType<BossPhaseChangedEvent>(), Is.Empty);
        }

        [Test, Description("Vérifie que l'empreinte de l'état change quand une phase s'ouvre — parce que le rejeu de vérification doit détecter un combat de boss différent")]
        public void StateHash_IncludesThePhaseReached()
        {
            var a = Sim();
            var b = Sim();
            Assert.That(a.StateHash(), Is.EqualTo(b.StateHash()));
            b.Units.First(u => u.Side == Side.Enemy).PhaseReached = 1;
            Assert.That(b.StateHash(), Is.Not.EqualTo(a.StateHash()));
        }
    }
}
