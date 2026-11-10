#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Trois cartes équipables : jusqu'à cinq compétences en combat (bloc A de la 1.0.0).</summary>
    public sealed class CardSlotsCombatTests
    {
        static SkillCardDef Card(string id, TargetRule target, params EffectDef[] fx)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", Family.Matter, 1, new SkillDef(new SkillId(id), SkillSlot.A3, 0, target, fx, 0));

        static ContentCatalog Catalog(ContentCatalog baseCatalog, params SkillCardDef[] extra)
        {
            var col = baseCatalog.Collection;
            return baseCatalog.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(extra).ToList(), col.Boosters));
        }

        static readonly SkillCardDef A = Card("t_a", TargetRule.SingleEnemy, new EffectDef(EffectType.Damage, 100));
        static readonly SkillCardDef B = Card("t_b", TargetRule.SingleEnemy, new EffectDef(EffectType.Damage, 110));
        static readonly SkillCardDef C = Card("t_c", TargetRule.SingleEnemy, new EffectDef(EffectType.Damage, 120));
        static readonly SkillCardDef Heal = Card("t_heal", TargetRule.SingleAlly, new EffectDef(EffectType.Heal, 50));

        static CombatSimulation Sim(ContentCatalog catalog, string member, params string?[] cards)
        {
            var ally = new AllyEntry(new MemberId(member), 5, 0, 0, null, cards.Select(c => c == null ? (CardId?)null : new CardId(c)).ToArray());
            return new CombatSimulation(catalog, new CombatSetup(11, CombatRank.Normal, new[] { ally },
                                                                new[] { new EnemyEntry(new EnemyId("thug"), 5) }, false));
        }

        static ContentCatalog Proto() => Catalog(PrototypeContent.Build(new BalanceConfig()), A, B, C, Heal);

        [Test, Description("Vérifie qu'un agent à deux compétences de base et trois cartes a cinq compétences en combat — parce que l'éventail passe à 5 places")]
        public void Un_agent_a_cinq_competences_avec_trois_cartes()
        {
            var u = Sim(Proto(), "darius_brooks", "t_a", "t_b", "t_c").Unit(0);
            Assert.That(u.Skills.Count, Is.EqualTo(5));
            Assert.That(CombatSimulation.ActiveSkillCount(u), Is.EqualTo(5));
        }

        [Test, Description("Vérifie que les cartes sont ajoutées après les compétences de base dans l'ordre des emplacements, en sautant un emplacement vide — parce que l'ordre des emplacements fixe l'ordre de l'éventail")]
        public void Les_cartes_sont_ajoutees_dans_l_ordre_des_emplacements()
        {
            var u = Sim(Proto(), "darius_brooks", "t_c", null, "t_a").Unit(0);
            Assert.That(u.Skills.Skip(2).Select(s => s.Id.Value), Is.EqualTo(new[] { "t_c", "t_a" }));
        }

        [Test, Description("Vérifie qu'une carte portée deux fois n'ajoute qu'une compétence — parce qu'un agent ne porte jamais deux fois la même carte")]
        public void Une_carte_en_double_n_est_ajoutee_qu_une_fois()
        {
            var u = Sim(Proto(), "darius_brooks", "t_a", "t_a").Unit(0);
            Assert.That(u.Skills.Count, Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'un agent à l'ancien kit de trois compétences voit sa 3e remplacée par la première carte, la 2e carte s'ajoutant — parce que le comportement des agents de test est conservé")]
        public void Un_agent_a_l_ancien_kit_de_trois_competences_remplace_la_3e()
        {
            var catalog = Catalog(TestContent.Build(), A, B);
            Assert.That(catalog.Member(new MemberId("verre")).Skills.Count, Is.EqualTo(3));
            var one = Sim(catalog, "verre", "t_a").Unit(0);
            Assert.That(one.Skills.Count, Is.EqualTo(3));
            Assert.That(one.Skills[2].Id.Value, Is.EqualTo("t_a"));
            var two = Sim(catalog, "verre", "t_a", "t_b").Unit(0);
            Assert.That(two.Skills.Select(s => s.Id.Value).Skip(2), Is.EqualTo(new[] { "t_a", "t_b" }));
        }

        [Test, Description("Vérifie que la 5e compétence (la 3e carte) se lance par une commande du joueur — parce que les cinq places doivent être jouables")]
        public void La_cinquieme_competence_est_jouable_par_le_joueur()
        {
            var sim = Sim(Proto(), "darius_brooks", "t_a", "t_b", "t_c");
            foreach (var u in sim.Units) u.Point = 0;
            var hero = sim.Unit(0);
            hero.Point = hero.ActionPoint;
            hero.Energy = hero.MaxEnergy;
            Assert.That(sim.CanUse(hero.Id, 4), Is.True);
            sim.Submit(CombatCommand.UseSkill(hero.Id, 4, 1));
            var events = sim.Step(1).ToList();
            Assert.That(events.OfType<SkillUsedEvent>().Single().Skill.Value, Is.EqualTo("t_c"));
        }

        [Test, Description("Vérifie qu'en mode auto l'IA joue la 5e compétence quand c'est le seul soin et qu'un allié est sous 40 % de PV — parce que l'IA ne doit plus être plafonnée à 4 compétences")]
        public void L_IA_joue_la_cinquieme_competence()
        {
            var sim = Sim(Proto(), "darius_brooks", "t_a", "t_b", "t_heal");
            sim.Submit(CombatCommand.SetAuto(true));
            foreach (var u in sim.Units) u.Point = 0;
            var hero = sim.Unit(0);
            hero.Hp = hero.MaxHp / 5;
            hero.Point = hero.ActionPoint;
            hero.Energy = hero.MaxEnergy;
            var events = sim.Step(1).ToList();
            Assert.That(events.OfType<SkillUsedEvent>().First(e => e.Actor == 0).Skill.Value, Is.EqualTo("t_heal"));
        }
    }
}
