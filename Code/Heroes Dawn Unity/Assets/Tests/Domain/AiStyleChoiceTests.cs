using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Choix de compétence et de cible par style d'IA, et finesse selon le rang du combat (bloc C de la 1.0.0).</summary>
    public sealed class AiStyleChoiceTests
    {
        static EffectDef Dmg(int pct) => new EffectDef(EffectType.Damage, pct);
        static EffectDef St(StatusType s, int turns) => new EffectDef(EffectType.ApplyStatus, 0, s, turns, 100);
        static SkillDef S(string id, SkillSlot slot, TargetRule t, int cost, params EffectDef[] fx) => new SkillDef(new SkillId(id), slot, 0, t, fx, cost);
        static EnemyDef E(string id, Position p, AiStyle style, params SkillDef[] skills)
            => new EnemyDef(new EnemyId(id), "enemies." + id + ".name", p, Family.Matter, new StatBlock(3000, 100, 50, 100), skills, false, null, style);

        static readonly ContentCatalog Base = TestContent.Build();

        static readonly ContentCatalog C = new ContentCatalog(Base.Balance, Base.Members,
            Base.Enemies.Concat(new[]
            {
                // Agressif : A2 est la plus lourde (180), A3 (150) est la suivante.
                E("t_aggr", Position.Attacker, AiStyle.Aggressive,
                    S("t_aggr_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(100)),
                    S("t_aggr_a2", SkillSlot.A2, TargetRule.SingleEnemy, 20, Dmg(180)),
                    S("t_aggr_a3", SkillSlot.A3, TargetRule.SingleEnemy, 20, Dmg(150))),
                E("t_def", Position.FrontLine, AiStyle.Defensive,
                    S("t_def_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(100)),
                    S("t_def_a2", SkillSlot.A2, TargetRule.Self, 20, new EffectDef(EffectType.Shield, 20, StatusType.Shield, 2), St(StatusType.Taunt, 2))),
                E("t_ctrl", Position.Controller, AiStyle.Control,
                    S("t_ctrl_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(100)),
                    S("t_ctrl_a2", SkillSlot.A2, TargetRule.SingleEnemy, 20, Dmg(60), St(StatusType.Shock, 1))),
                E("t_ctrl_plain", Position.Controller, AiStyle.Control,
                    S("t_ctrl_plain_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(100)),
                    S("t_ctrl_plain_a2", SkillSlot.A2, TargetRule.SingleEnemy, 20, Dmg(140))),
                E("t_sup", Position.Support, AiStyle.Support,
                    S("t_sup_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(100)),
                    S("t_sup_a2", SkillSlot.A2, TargetRule.SingleAlly, 20, new EffectDef(EffectType.Heal, 30))),
                E("t_sup_plain", Position.Support, AiStyle.Support,
                    S("t_sup_plain_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(100)),
                    S("t_sup_plain_a2", SkillSlot.A2, TargetRule.SingleEnemy, 20, Dmg(130))),
                E("t_dummy", Position.FrontLine, AiStyle.Balanced,
                    S("t_dummy_a1", SkillSlot.A1, TargetRule.SingleEnemy, 0, Dmg(50))),
            }).ToList(), Base.Synergies, Base.Zones, Base.Activities, Base.Collection, Base.StarterTeam);

        /// <summary>Alliés « ancre » (id 0) et « givre » (id 1), puis les ennemis donnés (ids 2, 3…). Aucun allié n'agit : seul l'ennemi prêt joue.</summary>
        static CombatSimulation Sim(CombatRank rank, ulong seed, params string[] enemies) => Sim(rank, seed, false, enemies);

        static CombatSimulation Sim(CombatRank rank, ulong seed, bool auto, params string[] enemies)
            => new CombatSimulation(C, new CombatSetup(seed, rank,
                new[] { new AllyEntry(new MemberId("ancre"), 5, 0), new AllyEntry(new MemberId("givre"), 5, 0) },
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), auto));

        /// <summary>Remet tout le monde au calme, rend l'ennemi prêt avec toute son énergie, fait un pas et renvoie ce qu'il a joué.</summary>
        static (string Skill, int[] Targets) Act(CombatSimulation s, CombatUnit enemy)
        {
            foreach (var u in s.Units) u.Point = 0;
            enemy.Point = enemy.ActionPoint;
            enemy.Energy = enemy.MaxEnergy;
            enemy.PlannedSlot = -1;
            var used = s.Step(1).OfType<SkillUsedEvent>().Single(e => e.Actor == enemy.Id);
            return (used.Skill.Value, used.Targets);
        }

        [Test, Description("Vérifie qu'un ennemi Agressif joue sa compétence de dégâts la plus lourde sur l'allié qui a le moins de PV — parce que ce style cherche à achever le plus faible")]
        public void Aggressive_joue_la_competence_de_degats_la_plus_lourde_sur_l_allie_le_plus_faible()
        {
            var s = Sim(CombatRank.Elite, 5, "t_aggr");
            s.Unit(1).Hp = 100;
            var (skill, targets) = Act(s, s.Unit(2));
            Assert.That(skill, Is.EqualTo("t_aggr_a2"));
            Assert.That(targets, Is.EqualTo(new[] { 1 }));
        }

        [Test, Description("Vérifie qu'un ennemi Défensif se protège sous 60 % de PV (lui ou le groupe) et sinon attaque la menace la plus forte — parce que ce style protège d'abord, puis neutralise le plus gros ATQ")]
        public void Defensive_se_protege_sous_60_pourcent_sinon_attaque_la_plus_forte_ATQ()
        {
            var s = Sim(CombatRank.Elite, 5, "t_def");
            var enemy = s.Unit(2);
            s.Unit(0).Atk = 500;
            var (sain, cibleSaine) = Act(s, enemy);
            Assert.That(sain, Is.EqualTo("t_def_a1"), "en bonne santé : attaque");
            Assert.That(cibleSaine, Is.EqualTo(new[] { 0 }), "la menace la plus forte (ATQ la plus haute)");

            enemy.Hp = enemy.MaxHp * 59 / 100;
            Assert.That(Act(s, enemy).Skill, Is.EqualTo("t_def_a2"), "sous 60 % : bouclier et provocation");
        }

        [Test, Description("Vérifie qu'un ennemi de Contrôle pose son statut sur la cible la plus rapide qui n'est pas déjà contrôlée — parce que ce style cherche à bloquer le plus vif sans gaspiller le contrôle")]
        public void Control_pose_un_statut_sur_la_cible_la_plus_rapide_sans_statut_de_controle()
        {
            var s = Sim(CombatRank.Elite, 5, "t_ctrl");
            var enemy = s.Unit(2);
            s.Unit(0).Spd = 300;
            s.Unit(1).Spd = 150;
            var (skill, targets) = Act(s, enemy);
            Assert.That(skill, Is.EqualTo("t_ctrl_a2"));
            Assert.That(targets, Is.EqualTo(new[] { 0 }));

            s.Unit(0).Statuses.Add(new StatusInstance(StatusType.Freeze, 3));
            var (_, suivante) = Act(s, enemy);
            Assert.That(suivante, Is.EqualTo(new[] { 1 }), "la plus rapide déjà contrôlée est laissée de côté");
        }

        [Test, Description("Vérifie qu'un ennemi de Contrôle attaque (compétence la plus forte) quand toutes les cibles sont déjà contrôlées — parce qu'un contrôle de plus serait perdu")]
        public void Control_attaque_quand_toutes_les_cibles_sont_deja_controlees()
        {
            var s = Sim(CombatRank.Elite, 5, "t_ctrl");
            foreach (var ally in s.Units.Where(u => u.Side == Side.Ally)) ally.Statuses.Add(new StatusInstance(StatusType.Mark, 3));
            Assert.That(Act(s, s.Unit(2)).Skill, Is.EqualTo("t_ctrl_a1"), "100 % de dégâts contre 60 % pour la compétence de contrôle");
        }

        [Test, Description("Vérifie qu'un ennemi de Soutien soigne un allié ennemi sous 50 % de PV et sinon attaque — parce que ce style garde le groupe en vie avant de frapper")]
        public void Support_soigne_un_allie_ennemi_sous_50_pourcent_sinon_attaque()
        {
            var s = Sim(CombatRank.Elite, 5, "t_sup", "t_dummy");
            var healer = s.Unit(2);
            var wounded = s.Unit(3);
            Assert.That(Act(s, healer).Skill, Is.EqualTo("t_sup_a1"), "tout le monde en forme : attaque");
            wounded.Hp = wounded.MaxHp * 49 / 100;
            var (skill, targets) = Act(s, healer);
            Assert.That(skill, Is.EqualTo("t_sup_a2"));
            Assert.That(targets, Is.EqualTo(new[] { 3 }));
        }

        [Test, Description("Vérifie qu'un style sans compétence adaptée (Soutien sans soin, Contrôle sans statut) joue sa compétence la plus forte sans exception — parce qu'un tour ne doit jamais être perdu")]
        public void Un_style_sans_competence_adaptee_attaque_avec_la_plus_forte()
        {
            var s = Sim(CombatRank.Elite, 5, "t_sup_plain", "t_dummy");
            s.Unit(3).Hp = 10;
            Assert.That(Act(s, s.Unit(2)).Skill, Is.EqualTo("t_sup_plain_a2"));

            var c = Sim(CombatRank.Elite, 5, "t_ctrl_plain");
            Assert.That(Act(c, c.Unit(2)).Skill, Is.EqualTo("t_ctrl_plain_a2"));
        }

        [Test, Description("Vérifie qu'un ennemi lié (Entrave) joue sa première compétence malgré son style — parce que l'Entrave interdit toute autre compétence")]
        public void Un_ennemi_lie_joue_A1()
        {
            var s = Sim(CombatRank.Boss, 5, "t_aggr");
            s.Unit(2).Statuses.Add(new StatusInstance(StatusType.Bind, 3));
            Assert.That(Act(s, s.Unit(2)).Skill, Is.EqualTo("t_aggr_a1"));
        }

        [Test, Description("Vérifie qu'un ennemi sans énergie suffisante joue sa première compétence — parce que les autres coûtent de l'énergie")]
        public void Un_ennemi_sans_energie_joue_A1()
        {
            var s = Sim(CombatRank.Boss, 5, "t_aggr");
            var enemy = s.Unit(2);
            foreach (var u in s.Units) u.Point = 0;
            enemy.Point = enemy.ActionPoint;
            enemy.Energy = 0;
            var used = s.Step(1).OfType<SkillUsedEvent>().Single(e => e.Actor == enemy.Id);
            Assert.That(used.Skill.Value, Is.EqualTo("t_aggr_a1"));
        }

        [Test, Description("Vérifie qu'en Facile l'ennemi ignore son style (au moins trois choix différents sur 200 tours) — parce que le rang Facile garde le hasard")]
        public void Facile_ignore_le_style()
        {
            var s = Sim(CombatRank.Easy, 9, "t_aggr");
            var seen = new HashSet<string>();
            for (int i = 0; i < 200; i++)
            {
                foreach (var a in s.Units.Where(u => u.Side == Side.Ally)) a.Hp = a.MaxHp;
                var (skill, targets) = Act(s, s.Unit(2));
                seen.Add(skill + ">" + string.Join(",", targets));
            }
            Assert.That(seen.Count, Is.GreaterThanOrEqualTo(3), string.Join(" ; ", seen));
        }

        [Test, Description("Vérifie qu'en Normal l'ennemi applique son style environ une fois sur deux (50 ± 8 % sur 1 000 choix) — parce que le rang Normal mélange style et logique simple")]
        public void Normal_applique_le_style_environ_une_fois_sur_deux()
        {
            var s = Sim(CombatRank.Normal, 21, "t_aggr");
            int styled = 0;
            for (int i = 0; i < 1000; i++)
            {
                foreach (var a in s.Units.Where(u => u.Side == Side.Ally)) a.Hp = a.MaxHp;
                s.Unit(1).Hp = 100;
                // La logique Normale joue A3 : seul le style Agressif joue A2.
                if (Act(s, s.Unit(2)).Skill == "t_aggr_a2") styled++;
            }
            Assert.That(styled, Is.InRange(420, 580));
        }

        [Test, Description("Vérifie qu'en Élite et en Boss l'ennemi applique toujours son style (100 choix sur 100) — parce que ces rangs jouent avec toutes leurs priorités")]
        public void Elite_et_Boss_appliquent_toujours_le_style()
        {
            foreach (var rank in new[] { CombatRank.Elite, CombatRank.Boss })
            {
                var s = Sim(rank, 33, "t_aggr");
                for (int i = 0; i < 100; i++)
                {
                    foreach (var a in s.Units.Where(u => u.Side == Side.Ally)) a.Hp = a.MaxHp;
                    s.Unit(1).Hp = 100;
                    var (skill, targets) = Act(s, s.Unit(2));
                    Assert.That(skill, Is.EqualTo("t_aggr_a2"), rank.ToString());
                    Assert.That(targets, Is.EqualTo(new[] { 1 }), rank.ToString());
                }
            }
        }

        [Test, Description("Vérifie que deux combats à la même graine produisent exactement les mêmes événements et la même empreinte — parce que le style d'IA ne doit introduire aucun hasard hors du générateur du combat")]
        public void Le_combat_reste_deterministe()
        {
            (int Count, ulong Hash) Run()
            {
                var s = Sim(CombatRank.Boss, 77, true, "t_aggr", "t_sup", "t_ctrl");
                int count = 0;
                for (int i = 0; i < 400 && !s.IsOver; i++) count += s.Step(1).Count;
                return (count, s.StateHash());
            }
            var a = Run();
            var b = Run();
            Assert.That(a.Count, Is.GreaterThan(0));
            Assert.That(b, Is.EqualTo(a));
        }
    }
}
