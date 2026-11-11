using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Kits, styles et phases des trois boss du Repaire de l'AMIF (bloc C de la 1.0.0).</summary>
    public sealed class PrototypeBossKitTests
    {
        static readonly ContentCatalog C = TestContent.Build();
        static readonly string[] Bosses = { "brick", "crocodile", "bridge_titan" };

        static string Kit(System.Collections.Generic.IReadOnlyList<SkillDef> skills) => string.Join(",", skills.Select(s => s.Id.Value));

        [Test, Description("Vérifie que chaque boss de repaire a au moins 2 phases dont une change de jeu de compétences — parce que le boss doit changer de façon de combattre en cours de combat")]
        public void Chaque_boss_de_repaire_a_au_moins_2_phases_dont_une_change_de_kit()
        {
            foreach (var id in Bosses)
            {
                var def = C.Enemy(new EnemyId(id));
                Assert.That(def.Phases.Count, Is.InRange(1, 3), id);
                Assert.That(def.Phases.Any(p => p.Skills != null && Kit(p.Skills) != Kit(def.Skills)), Is.True, id);
            }
        }

        [Test, Description("Vérifie que Brick passe de Défensif (phase 1) à Agressif (phase 3) en changeant de compétences — parce que sa fureur finale doit se sentir dans le jeu")]
        public void Brick_change_de_style_entre_la_phase_1_et_la_phase_3()
        {
            var s = new CombatSimulation(C, new CombatSetup(3, CombatRank.Boss,
                new[] { new AllyEntry(new MemberId("ancre"), 5, 0) }, new[] { new EnemyEntry(new EnemyId("brick"), 5) }, false));
            var brick = s.Units.Single(u => u.Side == Side.Enemy);
            Assert.That(brick.Style, Is.EqualTo(AiStyle.Defensive));
            Assert.That(Kit(brick.Skills), Is.EqualTo("brick_a1,brick_a2,brick_a3"));
            s.ApplyDamageForTests(brick, brick.Hp - brick.MaxHp * 20 / 100);
            Assert.That(brick.PhaseReached, Is.EqualTo(2));
            Assert.That(brick.Style, Is.EqualTo(AiStyle.Aggressive));
            Assert.That(Kit(brick.Skills), Is.EqualTo("brick_p3_a1,brick_p3_a2"));
        }

        [Test, Description("Vérifie que le Crocodile ouvre en Contrôle puis passe Agressif en phase 2, et que le Titan va de Défensif à Équilibré puis Agressif — parce que chaque boss a son propre rythme")]
        public void Crocodile_et_Titan_ont_leurs_styles_par_phase()
        {
            var croc = C.Enemy(new EnemyId("crocodile"));
            Assert.That(croc.Style, Is.EqualTo(AiStyle.Control));
            Assert.That(croc.Phases[0].Style, Is.EqualTo(AiStyle.Aggressive));
            Assert.That(croc.Phases[1].Skills, Is.Null, "la phase 3 ne change que les statistiques");

            var titan = C.Enemy(new EnemyId("bridge_titan"));
            Assert.That(titan.Style, Is.EqualTo(AiStyle.Defensive));
            Assert.That(titan.Phases.Select(p => p.Style), Is.EqualTo(new AiStyle?[] { AiStyle.Balanced, AiStyle.Aggressive }));
        }

        [Test, Description("Vérifie qu'un combat automatique complet contre chaque boss se termine sans exception en moins de 400 actions — parce que les nouveaux kits ne doivent jamais bloquer un combat")]
        public void Un_combat_complet_contre_chaque_boss_se_termine()
        {
            foreach (var id in Bosses)
            {
                var allies = new[] { "ancre", "givre", "echo", "seve" }.Select(m => new AllyEntry(new MemberId(m), 10, 0)).ToArray();
                var s = new CombatSimulation(C, new CombatSetup(7, CombatRank.Boss, allies, new[] { new EnemyEntry(new EnemyId(id), 13) }, true));
                int actions = 0;
                for (int i = 0; i < 20000 && !s.IsOver; i++) actions += s.Step(1).OfType<SkillUsedEvent>().Count();
                Assert.That(s.IsOver, Is.True, id);
                Assert.That(actions, Is.LessThan(400), id);
            }
        }
    }
}
