using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Duel entre deux équipes de joueurs (camp adverse piloté par l'IA ou de l'extérieur, empreinte d'état) — bloc D de la 1.0.0.</summary>
    public sealed class DuelSimulationTests
    {
        static readonly ContentCatalog C = TestContent.Build();

        static AllyEntry Agent(string id, int level = 5, params string[] items)
            => new AllyEntry(new MemberId(id), level, 0, 0, items.Select(i => new ItemId(i)).ToArray());

        static CombatSetup Duel(ulong seed, SideControl control, bool auto, AllyEntry[]? mine = null, AllyEntry[]? theirs = null)
            => new CombatSetup(seed, CombatRank.Elite, mine ?? new[] { Agent("ancre"), Agent("givre") }, new EnemyEntry[0], auto)
                .WithOpponentTeam(theirs ?? new[] { Agent("ancre"), Agent("givre") }, control);

        static (List<string> Events, ulong Hash) RunAuto(ulong seed)
        {
            var s = new CombatSimulation(C, Duel(seed, SideControl.Ai, true));
            var names = new List<string>();
            for (int i = 0; i < 20000 && !s.IsOver; i++) foreach (var e in s.Step(1)) names.Add(e.Tick + e.GetType().Name);
            return (names, s.StateHash());
        }

        [Test, Description("Vérifie que deux duels hors ligne de même graine et mêmes équipes donnent exactement les mêmes événements, le camp adverse étant mené par l'IA — parce que le serveur fournit la graine et que le résultat doit être reproductible")]
        public void Un_duel_hors_ligne_est_deterministe()
        {
            var a = RunAuto(123);
            var b = RunAuto(123);
            Assert.That(a.Events.Count, Is.GreaterThan(10));
            Assert.That(b.Events, Is.EqualTo(a.Events));
            Assert.That(b.Hash, Is.EqualTo(a.Hash));
            var s = new CombatSimulation(C, Duel(123, SideControl.Ai, true));
            Assert.That(s.Units.Count(u => u.Side == Side.Enemy), Is.EqualTo(2));
            Assert.That(s.AwaitingSide, Is.Null, "avec l'auto du joueur et l'IA adverse, rien n'attend de commande");
        }

        [Test, Description("Vérifie que l'équipe adverse a les statistiques d'un agent (PV, ATQ, DEF, VIT) et non celles d'un ennemi — parce que c'est l'équipe d'un joueur, calculée comme la sienne")]
        public void L_equipe_adverse_a_les_stats_d_un_agent_et_non_d_un_ennemi()
        {
            var entry = Agent("ancre", 7);
            var s = new CombatSimulation(C, Duel(5, SideControl.Ai, false, new[] { entry }, new[] { entry }));
            var mine = s.Unit(0);
            var theirs = s.Unit(1);
            Assert.That(mine.Side, Is.EqualTo(Side.Ally));
            Assert.That(theirs.Side, Is.EqualTo(Side.Enemy));
            Assert.That(theirs.MaxHp, Is.EqualTo(mine.MaxHp));
            Assert.That(theirs.Atk, Is.EqualTo(mine.Atk));
            Assert.That(theirs.Def, Is.EqualTo(mine.Def));
            Assert.That(theirs.Spd, Is.EqualTo(mine.Spd));
            Assert.That(theirs.DefId, Is.EqualTo("ancre"));
            Assert.That(theirs.Style, Is.EqualTo(AiStyles.Default(theirs.Position)));
        }

        [Test, Description("Vérifie que les objets de l'adversaire comptent dans ses statistiques — parce que sa capture contient son équipement")]
        public void Les_objets_adverses_comptent()
        {
            var plain = new CombatSimulation(C, Duel(5, SideControl.Ai, false, null, new[] { Agent("ancre") })).Unit(2);
            var geared = new CombatSimulation(C, Duel(5, SideControl.Ai, false, null, new[] { Agent("ancre", 5, "kevlar_vest") })).Unit(2);
            Assert.That(geared.MaxHp, Is.GreaterThan(plain.MaxHp));
        }

        [Test, Description("Vérifie que les cartes de l'adversaire ajoutent des compétences à son agent — parce que la capture contient ses cartes équipées")]
        public void Les_cartes_adverses_comptent()
        {
            var card = C.Collection.Cards.First();
            var with = new AllyEntry(new MemberId("ancre"), 5, 0, 0, null, new CardId?[] { card.Id });
            var plain = new CombatSimulation(C, Duel(5, SideControl.Ai, false, null, new[] { Agent("ancre") })).Unit(2);
            var s = new CombatSimulation(C, Duel(5, SideControl.Ai, false, null, new[] { with })).Unit(2);
            Assert.That(CombatSimulation.ActiveSkillCount(s), Is.GreaterThanOrEqualTo(CombatSimulation.ActiveSkillCount(plain)));
            Assert.That(s.Skills.Any(k => k.Id == card.Skill.Id), Is.True);
        }

        [Test, Description("Vérifie qu'en mode External la simulation attend la commande du camp Enemy : un ordre venu de Ally est refusé, un ordre de Enemy fait avancer — parce que chaque camp ne joue que ses unités")]
        public void En_mode_External_la_simulation_attend_la_commande_du_camp_Enemy()
        {
            var s = new CombatSimulation(C, Duel(9, SideControl.External, false));
            foreach (var u in s.Units) u.Point = 0;
            var foe = s.Units.First(u => u.Side == Side.Enemy);
            foe.Point = foe.ActionPoint;
            s.Step(1);
            Assert.That(s.AwaitingSide, Is.EqualTo(Side.Enemy));
            Assert.That(s.Submit(CombatCommand.Pass(foe.Id), Side.Ally), Is.Empty, "le camp Ally n'a pas la main");
            Assert.That(s.AwaitingSide, Is.EqualTo(Side.Enemy));
            Assert.That(s.Submit(CombatCommand.UseSkill(0, 0, foe.Id), Side.Enemy), Is.Empty, "l'unité 0 n'appartient pas au camp Enemy");
            var events = s.Submit(CombatCommand.UseSkill(foe.Id, 0, 0), Side.Enemy);
            Assert.That(events.OfType<SkillUsedEvent>().Count(), Is.EqualTo(1));
            Assert.That(s.AwaitingSide, Is.Null);
        }

        [Test, Description("Vérifie qu'en mode External l'allié prêt a la main avant l'adversaire et que l'IA n'agit pas à la place du camp adverse — parce que l'ordre doit être identique sur les deux appareils")]
        public void En_mode_External_l_IA_n_agit_pas_pour_le_camp_adverse()
        {
            var s = new CombatSimulation(C, Duel(9, SideControl.External, false));
            foreach (var u in s.Units) u.Point = 0;
            var foe = s.Units.First(u => u.Side == Side.Enemy);
            foe.Point = foe.ActionPoint;
            var events = s.Step(5);
            Assert.That(events.OfType<SkillUsedEvent>(), Is.Empty);
            Assert.That(foe.IsReady, Is.True);
        }

        /// <summary>Joue un duel External : chaque camp joue la compétence 0 sur l'adversaire le plus faible ; renvoie l'empreinte à chaque tick.</summary>
        static List<ulong> RunExternal(ulong seed, int passAtCommand = -1)
        {
            var s = new CombatSimulation(C, Duel(seed, SideControl.External, false));
            var hashes = new List<ulong>();
            int commands = 0;
            for (int guard = 0; guard < 30000 && !s.IsOver; guard++)
            {
                var side = s.AwaitingSide;
                if (side.HasValue)
                {
                    var actor = s.Units.First(u => u.Side == side.Value && u.IsReady);
                    var foe = s.Units.Where(u => u.Side != side.Value && u.IsAlive).OrderBy(u => u.Hp).First();
                    var cmd = commands++ == passAtCommand ? CombatCommand.Pass(actor.Id) : CombatCommand.UseSkill(actor.Id, 0, foe.Id);
                    s.Submit(cmd, side.Value);
                }
                else s.Step(1);
                hashes.Add(s.StateHash());
            }
            Assert.That(s.IsOver, Is.True);
            return hashes;
        }

        [Test, Description("Vérifie que deux simulations identiques recevant les mêmes commandes ont la même empreinte à chaque étape — parce que les deux appareils comparent l'empreinte pour détecter une désynchronisation")]
        public void Deux_simulations_identiques_ont_la_meme_empreinte_a_chaque_tick_de_controle()
        {
            var a = RunExternal(77);
            var b = RunExternal(77);
            Assert.That(a.Count, Is.GreaterThan(40));
            Assert.That(b, Is.EqualTo(a));
        }

        [Test, Description("Vérifie qu'une commande différente change l'empreinte — parce que sinon une désynchronisation passerait inaperçue")]
        public void Une_commande_differente_change_l_empreinte()
        {
            var a = RunExternal(77);
            var b = RunExternal(77, passAtCommand: 2);
            Assert.That(b, Is.Not.EqualTo(a));
            Assert.That(b.Take(5), Is.EqualTo(a.Take(5)), "avant la commande différente les deux se ressemblent");
        }

        [Test, Description("Vérifie qu'un duel entre équipes de niveau égal, IA contre IA, se termine avec un vainqueur en moins de 400 actions — parce qu'un duel ne doit jamais rester bloqué")]
        public void Le_combat_se_termine_avec_un_vainqueur()
        {
            foreach (ulong seed in new ulong[] { 1, 2, 3, 4, 5 })
            {
                var s = new CombatSimulation(C, Duel(seed, SideControl.Ai, true));
                int actions = 0;
                for (int i = 0; i < 20000 && !s.IsOver; i++) actions += s.Step(1).OfType<SkillUsedEvent>().Count();
                Assert.That(s.IsOver, Is.True, "graine " + seed);
                Assert.That(s.Victory.HasValue, Is.True);
                Assert.That(actions, Is.LessThan(400), "graine " + seed);
            }
        }

        [Test, Description("Vérifie qu'un combat classique n'a ni équipe adverse ni camp externe et que l'ennemi y reste mené par l'IA — parce que le duel ne doit rien changer aux combats existants")]
        public void Les_combats_classiques_ne_changent_pas()
        {
            var setup = new CombatSetup(3, CombatRank.Normal, new[] { Agent("ancre") }, new[] { new EnemyEntry(new EnemyId("thug"), 5) }, false);
            Assert.That(setup.OpponentTeam, Is.Empty);
            Assert.That(setup.EnemyControl, Is.EqualTo(SideControl.Ai));
            var s = new CombatSimulation(C, setup);
            Assert.That(s.Units.Count, Is.EqualTo(2));
            foreach (var u in s.Units) u.Point = 0;
            s.Unit(1).Point = s.Unit(1).ActionPoint;
            Assert.That(s.AwaitingSide, Is.Null);
            Assert.That(s.Step(1).OfType<SkillUsedEvent>().Count(), Is.EqualTo(1), "l'ennemi joue seul");
            s.Unit(0).Point = s.Unit(0).ActionPoint;
            s.Step(1);
            Assert.That(s.AwaitingSide, Is.EqualTo(Side.Ally));
        }
    }
}
