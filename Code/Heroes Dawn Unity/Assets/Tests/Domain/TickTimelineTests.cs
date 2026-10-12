using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Frise par points : avance, tour d'agent, reprise, point vide, interruptions, rencontres (cahier des charges 2026-10-11).</summary>
    public sealed class TickTimelineTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static CombatSimulation Sim(string[] allies, string[] enemies, bool auto = false, ulong seed = 11)
            => new CombatSimulation(C, new CombatSetup(seed, CombatRank.Normal,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), auto));

        static void AllAt(CombatSimulation s, int point) { foreach (var u in s.Units) u.Point = point; }

        // ------------------------------------------------------------------ Constantes et départ

        [Test, Description("Vérifie que la frise a 10 points, des reprises à 3, 2, 0 et -2 et une pause de 1,2 s, un pas de 0,35 s et un délai IA de 0,3 à 0,7 s — parce que ce sont les valeurs du cahier des charges 0.8.0")]
        public void Defaults_MatchTheValidatedTimeline()
        {
            var b = new BalanceConfig();
            Assert.That(b.TimelinePoints, Is.EqualTo(10));
            Assert.That(b.TimelineResetLightPoint, Is.EqualTo(3));
            Assert.That(b.TimelineResetMediumPoint, Is.EqualTo(2));
            Assert.That(b.TimelineResetHeavyPoint, Is.EqualTo(0));
            Assert.That(b.TimelineResetComboPoint, Is.EqualTo(-2));
            Assert.That(b.TimelinePausePerEmptyPointMs, Is.EqualTo(1200));
            Assert.That(b.TimelineStepMs, Is.EqualTo(350));
            Assert.That(b.TimelineTurnTimeoutMs, Is.EqualTo(30000));
            Assert.That(b.TimelineAiDelayMinMs, Is.EqualTo(300));
            Assert.That(b.TimelineAiDelayMaxMs, Is.EqualTo(700));
        }

        [Test, Description("Vérifie que chaque unité vivante démarre entre le point 0 et le point 2 de façon déterministe pour une graine — parce que les premiers tours ne doivent pas tomber tous en même temps")]
        public void StartPoints_AreWithinRangeAndDeterministic()
        {
            var a = Sim(new[] { "ancre", "givre" }, new[] { "brute", "thug" }, seed: 11);
            var b = Sim(new[] { "ancre", "givre" }, new[] { "brute", "thug" }, seed: 11);
            foreach (var u in a.Units) Assert.That(u.Point, Is.InRange(0, 2));
            Assert.That(a.Units.Select(u => u.Point), Is.EqualTo(b.Units.Select(u => u.Point)));
            Assert.That(Enumerable.Range(1, 30).Select(seed => Sim(new[] { "ancre" }, new[] { "brute" }, seed: (ulong)seed).Unit(0).Point).Distinct().Count(), Is.GreaterThan(1), "la graine fait varier le départ");
        }

        [Test, Description("Vérifie qu'une unité au point d'action est reconnue prête et qu'une unité en canalisation (point négatif) est reconnue comme telle — parce que la canalisation retarde le tour")]
        public void ActionPoint_AndChannelling_AreDetected()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            var u = s.Unit(0);
            u.Point = u.ActionPoint;
            Assert.That(u.IsReady, Is.True);
            u.Point = -2;
            Assert.That(u.IsChannelling, Is.True);
            Assert.That(u.IsReady, Is.False);
            Assert.That(u.ActionPoint, Is.EqualTo(10));
        }

        // ------------------------------------------------------------------ Avance et tour

        [Test, Description("Vérifie que chaque pas de frise fait avancer toutes les unités vivantes d'exactement un point — parce que tout le monde avance au même rythme")]
        public void EachStep_AdvancesEveryLivingUnitByOnePoint()
        {
            var s = Sim(new[] { "ancre", "givre" }, new[] { "brute" });
            var before = s.Units.ToDictionary(u => u.Id, u => u.Point);
            var events = s.Step(1);
            foreach (var u in s.Units) Assert.That(u.Point, Is.EqualTo(before[u.Id] + 1), "unité " + u.Id);
            Assert.That(events.OfType<TimelineAdvancedEvent>().Count(), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'une unité qui atteint le point 10 prend son tour et que la frise s'arrête tant qu'un allié attend — parce que le tour du joueur bloque la frise")]
        public void ReachingTheActionPoint_StartsTheTurn_AndFreezesTheTimeline()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            s.Unit(1).Point = 2;
            s.Unit(0).Point = 9;
            var events = s.Step(1);
            Assert.That(s.Unit(0).IsReady, Is.True);
            Assert.That(events.OfType<GaugeFullEvent>().Any(e => e.Unit == 0), Is.True);
            Assert.That(s.IsAwaitingAction, Is.True);
            int enemy = s.Unit(1).Point;
            s.Step(20);
            Assert.That(s.Unit(1).Point, Is.EqualTo(enemy), "la frise n'avance pas pendant le tour");
            Assert.That(s.Unit(0).Point, Is.EqualTo(10));
        }

        [Test, Description("Vérifie que la reprise après action place l'unité sur le point du poids de sa compétence (léger 3, intermédiaire 2, lourd 0) — parce que la reprise dépend du poids de l'action")]
        public void Reset_FollowsTheSkillWeight()
        {
            foreach (var (slot, expected) in new[] { (0, 3), (1, 2), (2, 0) })
            {
                var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
                foreach (var u in s.Units) u.Point = 0;
                var givre = s.Unit(0);
                givre.Point = 10;
                s.Submit(CombatCommand.UseSkill(givre.Id, slot, 2));
                s.Step(1);
                Assert.That(givre.Point, Is.EqualTo(expected), "emplacement " + slot);
            }
        }

        [Test, Description("Vérifie qu'un joueur qui passe son tour voit son unité repartir au point 3 sans lancer de compétence, et qu'on ne peut pas passer le tour d'une unité qui n'est pas au point d'action — parce que le délai de 30 s ne doit jamais figer la frise")]
        public void PassTurn_ResetsToPointThree_OnlyWhenReady()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            foreach (var u in s.Units) u.Point = 0;
            s.Submit(CombatCommand.Pass(0));
            s.Step(1);
            Assert.That(s.InputLog.Any(), Is.False, "pas prête : refusé");
            s.Unit(0).Point = 10;
            s.Submit(CombatCommand.Pass(0));
            var events = s.Step(1);
            Assert.That(events.OfType<TurnSkippedEvent>().Any(e => e.Unit == 0), Is.True);
            Assert.That(events.OfType<SkillUsedEvent>(), Is.Empty);
            Assert.That(s.Unit(0).Point, Is.EqualTo(3));
            Assert.That(s.IsAwaitingAction, Is.False);
        }

        [Test, Description("Vérifie que Hâte fait franchir un point de plus un pas sur trois et que Gel saute un pas sur deux — parce que les statuts changent encore le rythme sans toucher à la règle générale")]
        public void HasteAndFreeze_ChangeTheRhythm()
        {
            var s = Sim(new[] { "ancre", "givre" }, new[] { "brute" });
            foreach (var u in s.Units) u.Point = 0;
            s.Unit(0).Statuses.Add(new StatusInstance(StatusType.Haste, 9));
            s.Unit(1).Statuses.Add(new StatusInstance(StatusType.Freeze, 9));
            s.Step(3);
            Assert.That(s.Unit(0).Point, Is.EqualTo(4), "hâte : 1 + 1 + 2");
            Assert.That(s.Unit(1).Point, Is.EqualTo(1), "gel : un seul pas sur trois (pas 2)");
            Assert.That(s.Unit(2).Point, Is.EqualTo(3), "ennemi normal : un point par pas");
        }

        // ------------------------------------------------------------------ Point vide et interruptions

        [Test, Description("Vérifie que la fenêtre d'interruption de 1,2 s s'ouvre au pas qui précède l'arrivée d'un ennemi au point d'action quand un avatar est prêt — parce que c'est la seule menace sur laquelle le joueur peut réagir")]
        public void ImminentEnemy_OpensTheInterruptWindow_For1200ms()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            AllAt(s, 0);
            s.Unit(1).Point = s.Unit(1).ActionPoint - 2;
            var pauses = s.Step(1).OfType<EmptyPointPauseEvent>().ToList();
            Assert.That(pauses.Count, Is.EqualTo(1));
            Assert.That(pauses[0].PauseMs, Is.EqualTo(1200));
            Assert.That(s.InterruptWindowOpen, Is.True);
        }

        [Test, Description("Vérifie qu'aucune pause n'est émise tant qu'aucun ennemi n'est sur le point d'agir — parce que les points vides sans menace ralentissaient inutilement le combat")]
        public void NoThreat_NoPause()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            AllAt(s, 0);
            int pauses = 0;
            for (int i = 0; i < 6; i++) pauses += s.Step(1).OfType<EmptyPointPauseEvent>().Count();
            Assert.That(pauses, Is.EqualTo(0));
            Assert.That(s.InterruptWindowOpen, Is.False);
        }

        [Test, Description("Vérifie que la pause n'arrive qu'une fois, au pas où l'ennemi atteint l'avant-dernier point — parce que le joueur ne doit pas être interrompu à chaque point")]
        public void Window_OpensOnlyOnce_BeforeTheEnemyActs()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            AllAt(s, 0);
            s.Unit(1).Point = s.Unit(1).ActionPoint - 3;
            Assert.That(s.Step(1).OfType<EmptyPointPauseEvent>().Count(), Is.EqualTo(0), "encore deux pas");
            Assert.That(s.Step(1).OfType<EmptyPointPauseEvent>().Count(), Is.EqualTo(1), "un pas avant le tour");
        }

        [Test, Description("Vérifie qu'aucune pause n'est émise quand une unité est au point d'action ou en mode Auto — parce qu'un tour de jeu ou l'IA ne laissent pas de fenêtre d'interruption")]
        public void NoWindow_WhenSomeoneActs_OrInAuto()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            AllAt(s, 0);
            s.Unit(0).Point = 9;
            Assert.That(s.Step(1).OfType<EmptyPointPauseEvent>(), Is.Empty, "un allié arrive au point d'action");
            var auto = Sim(new[] { "ancre" }, new[] { "brute" }, auto: true);
            AllAt(auto, 0);
            Assert.That(auto.Step(1).OfType<EmptyPointPauseEvent>(), Is.Empty, "Auto : pas de pause");
        }

        [Test, Description("Vérifie qu'une compétence d'avatar n'est lançable que pendant la fenêtre d'interruption ou au tour d'un agent, et pas autrement — parce que les interruptions ne sont pas des actions libres")]
        public void AvatarSkills_OnlyInsideTheWindowOrOnATurn()
        {
            var s = Sim(new[] { "ancre", "givre" }, new[] { "brute" });
            Assert.That(s.CanUseAvatar(0), Is.False, "avant le premier pas");
            AllAt(s, 0);
            s.Unit(2).Point = s.Unit(2).ActionPoint - 2;
            s.Unit(0).Point = 2;
            s.Step(1);
            Assert.That(s.InterruptWindowOpen, Is.True);
            Assert.That(s.CanUseAvatar(0), Is.True, "fenêtre ouverte");
            s.Submit(CombatCommand.UseAvatar(0, 2));
            var events = s.ApplyCommands();
            Assert.That(events.OfType<AvatarSkillUsedEvent>().Count(), Is.EqualTo(1), "traité sans avancer la frise");

            var turn = Sim(new[] { "ancre" }, new[] { "brute" });
            AllAt(turn, 0);
            turn.Unit(0).Point = 10;
            turn.Step(1);
            Assert.That(turn.IsAwaitingAction, Is.True);
            Assert.That(turn.CanUseAvatar(0), Is.True, "au tour de l'agent");
        }

        // ------------------------------------------------------------------ Rencontres

        static List<CombatUnit> Units(params (Side Side, int Point)[] spec)
        {
            var list = new List<CombatUnit>();
            for (int i = 0; i < spec.Length; i++)
                list.Add(new CombatUnit(i, spec[i].Side, "x", Family.Matter, Role.Tank, false, 100, 10, 10, 10, 10, System.Array.Empty<SkillDef>()) { Point = spec[i].Point });
            return list;
        }

        [Test, Description("Vérifie que deux alliés sur le même point forment une rencontre de combo, un allié et un ennemi une parade, deux ennemis rien — parce que ce sont les règles de rencontre validées")]
        public void Detect_FollowsTheEncounterRules()
        {
            var b = new BalanceConfig();
            Assert.That(TickEncounters.Detect(Units((Side.Ally, 5), (Side.Ally, 5)), b).Single().Kind, Is.EqualTo(TickEncounterKind.Combo));
            Assert.That(TickEncounters.Detect(Units((Side.Ally, 5), (Side.Enemy, 5)), b).Single().Kind, Is.EqualTo(TickEncounterKind.Parade));
            Assert.That(TickEncounters.Detect(Units((Side.Enemy, 5), (Side.Enemy, 5)), b), Is.Empty);
            Assert.That(TickEncounters.Detect(Units((Side.Ally, 5), (Side.Ally, 6)), b), Is.Empty, "points différents");
        }

        [Test, Description("Vérifie qu'une unité en canalisation ou déjà au point d'action ne rencontre personne — parce que la canalisation doit être rattrapée et qu'une unité prête agit")]
        public void Detect_IgnoresChannellingAndReadyUnits()
        {
            var b = new BalanceConfig();
            Assert.That(TickEncounters.Detect(Units((Side.Ally, -1), (Side.Ally, -1)), b), Is.Empty);
            Assert.That(TickEncounters.Detect(Units((Side.Ally, 10), (Side.Enemy, 10)), b), Is.Empty);
        }

        [Test, Description("Vérifie que trois unités sur un même point donnent une rencontre par couple, une seule fois chacune — parce qu'un départ commun ne doit pas doubler un combo")]
        public void Detect_ReportsEachPairOnce()
        {
            var r = TickEncounters.Detect(Units((Side.Ally, 0), (Side.Ally, 0), (Side.Enemy, 0)), new BalanceConfig());
            Assert.That(r.Count, Is.EqualTo(3));
            Assert.That(r.Select(e => (e.A, e.B)).Distinct().Count(), Is.EqualTo(3));
        }

        [Test, Description("Vérifie qu'un allié qui rejoint un ennemi sur un point lui annule son action annoncée une seule fois et le renvoie au point 3 — parce que la parade annule le coup annoncé")]
        public void Parade_CancelsTheAnnouncedAction_Once()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            var ally = s.Unit(0); var enemy = s.Unit(1);
            ally.Point = 5; enemy.Point = 5; enemy.PlannedSlot = 0;
            var events = s.Step(1);
            Assert.That(events.OfType<EncounterEvent>().Single().Kind, Is.EqualTo(TickEncounterKind.Parade));
            Assert.That(events.OfType<ActionInterruptedEvent>().Count(e => e.Unit == enemy.Id && e.Reason == InterruptReason.Parade), Is.EqualTo(1));
            Assert.That(enemy.PlannedSlot, Is.EqualTo(-1));
            Assert.That(enemy.Point, Is.EqualTo(3));
            var next = s.Step(1);
            Assert.That(next.OfType<ActionInterruptedEvent>(), Is.Empty, "pas de seconde annulation au pas suivant");
        }

        [Test, Description("Vérifie que deux alliés d'une paire valide sur un même point proposent le combo et que le lancer rend le combo effectif avec canalisation — parce que la rencontre de deux alliés est la règle du combo")]
        public void TwoAllies_OnOnePoint_OfferAndPerformTheCombo()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            AllAt(s, 0);
            s.Unit(0).Point = 4; s.Unit(1).Point = 4;
            var events = s.Step(1);
            Assert.That(events.OfType<EncounterEvent>().Any(e => e.Kind == TickEncounterKind.Combo), Is.True);
            Assert.That(s.AvailableSyncCombos().Count, Is.EqualTo(1));
            s.Submit(CombatCommand.SyncCombo(0, 1, 2));
            Assert.That(s.Step(1).OfType<ComboUsedEvent>().Any(), Is.True);
            Assert.That(s.Unit(0).Point, Is.EqualTo(-2));
            Assert.That(s.Unit(1).Point, Is.EqualTo(-2));
        }

        [Test, Description("Vérifie que deux alliés en canalisation sur le même point ne proposent pas de combo tant qu'ils n'ont pas rattrapé le point 0 — parce que la canalisation doit être rattrapée d'abord")]
        public void Channelling_BlocksTheEncounterCombo()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            AllAt(s, 0);
            s.Unit(0).Point = -3; s.Unit(1).Point = -3;
            s.Step(1);
            Assert.That(s.AvailableSyncCombos(), Is.Empty);
        }

        [Test, Description("Vérifie qu'un combat complet se rejoue à l'identique depuis son journal de commandes avec la frise par points — parce que la vérification serveur repose sur le rejeu")]
        public void FullCombat_ReplaysToTheSameHash()
        {
            var setup = new CombatSetup(21UL, CombatRank.Normal,
                new[] { new AllyEntry(new MemberId("givre"), 5, 0), new AllyEntry(new MemberId("ancre"), 5, 0) },
                new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("thug"), 5) }, false);
            var sim = new CombatSimulation(C, setup);
            int guard = 0;
            while (!sim.IsOver && guard++ < 20000)
            {
                foreach (var u in sim.Units.Where(x => x.Side == Side.Ally && x.IsReady))
                    sim.Submit(CombatCommand.UseSkill(u.Id, sim.CanUse(u.Id, 2) ? 2 : sim.CanUse(u.Id, 1) ? 1 : 0, -1));
                sim.Step(1);
            }
            Assert.That(sim.IsOver, Is.True);
            Assert.That(CombatReplay.Run(C, setup, sim.InputLog).StateHash, Is.EqualTo(sim.StateHash()));
        }
    }
}
