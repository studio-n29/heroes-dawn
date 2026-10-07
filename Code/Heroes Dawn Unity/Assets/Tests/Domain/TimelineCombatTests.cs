using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Frise chronologique, zone de préparation, contrôles, Time Bank (spec 2026-10-05).</summary>
    public sealed class TimelineCombatTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static CombatSimulation Sim(string[] allies, string[] enemies, CombatRank rank = CombatRank.Normal, ulong seed = 11)
            => new CombatSimulation(C, new CombatSetup(seed, rank,
                allies.Select(a => new AllyEntry(new MemberId(a), 5, 0)).ToArray(),
                enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));

        static int Pct(CombatUnit u) => (int)(u.Gauge * 100 / u.GaugeMax);

        /// <summary>Met l'unité à une position de la frise (sans tour déclenché).</summary>
        static void At(CombatUnit u, int percent) => u.Gauge = u.GaugeMax * percent / 100;

        /// <summary>Tous les ennemis loin de leur zone d'action : rien ne vient perturber le test.</summary>
        static void Calm(CombatSimulation s) { foreach (var u in s.Units.Where(x => x.Side == Side.Enemy)) At(u, 0); }

        static void Act(CombatSimulation s, CombatUnit ally, int slot, int target)
        {
            At(ally, 100);
            s.Submit(CombatCommand.UseSkill(ally.Id, slot, target));
            s.Step(1);
        }

        [Test, Description("Vérifie que la compétence A1 replace l'unité à 30 % de la frise, A2 à 15 % et A3 à 0 % — parce que le poids d'une action se paie en temps (spec §A)")]
        public void SkillWeight_SetsPositionAfterAction()
        {
            foreach (var (slot, expected) in new[] { (0, 30), (1, 15), (2, 0) })
            {
                var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
                Calm(s);
                var givre = s.Unit(0);
                Act(s, givre, slot, 2);
                Assert.That(Pct(givre), Is.EqualTo(expected), "slot " + slot);
            }
        }

        [Test, Description("Vérifie qu'un combo impose une canalisation : les deux partenaires repartent en dessous de 0 % et doivent la rattraper — parce que les combos sont l'action la plus lourde (spec §A)")]
        public void Combo_ImposesChannelling_OnBothPartners()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            Calm(s);
            var givre = s.Unit(0); var ancre = s.Unit(1);
            At(givre, 100); At(ancre, 100);
            s.Submit(CombatCommand.UseCombo(0, 1, 2));
            var ev = s.Step(1);
            Assert.That(ev.OfType<ComboUsedEvent>().Any(), Is.True);
            Assert.That(Pct(givre), Is.EqualTo(-15));
            Assert.That(Pct(ancre), Is.EqualTo(-15));
            Assert.That(givre.IsReady, Is.False);
        }

        [Test, Description("Vérifie qu'un ennemi qui entre en zone de préparation (80 %) annonce son action sur la frise, puis l'exécute — parce que le joueur doit voir venir la grosse attaque (spec §A, §C)")]
        public void Enemy_AnnouncesAction_InPrepZone_AndExecutesIt()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" }, CombatRank.Elite);
            var brute = s.Unit(1);
            At(s.Unit(0), 0);
            At(brute, 79);
            Assert.That(brute.PlannedSlot, Is.EqualTo(-1));
            var ev = s.Step(40).ToList();
            var planned = ev.OfType<ActionPlannedEvent>().FirstOrDefault(e => e.Unit == brute.Id);
            Assert.That(planned, Is.Not.Null);
            var used = ev.OfType<SkillUsedEvent>().FirstOrDefault(e => e.Actor == brute.Id);
            Assert.That(used, Is.Not.Null);
            Assert.That(used.Skill, Is.EqualTo(brute.Skills[planned.Slot].Id), "l'action annoncée est celle qui part");
        }

        [Test, Description("Vérifie qu'une unité Perception vivante dans l'équipe révèle l'action ennemie dès 50 % de la frise, contre 80 % sans elle — parce que la Perception donne la précognition (spec §C)")]
        public void Perception_RevealsEnemyActionsEarlier()
        {
            var with = Sim(new[] { "echo", "ancre" }, new[] { "brute" });
            var without = Sim(new[] { "ancre", "givre" }, new[] { "brute" });
            foreach (var s in new[] { with, without }) { Calm(s); At(s.Unit(2), 55); }
            Assert.That(with.Step(1).OfType<ActionPlannedEvent>().Any(), Is.True);
            Assert.That(without.Step(1).OfType<ActionPlannedEvent>().Any(), Is.False);
            Assert.That(with.PlanStartPercent(with.Unit(2)), Is.EqualTo(50));
            Assert.That(without.PlanStartPercent(without.Unit(2)), Is.EqualTo(80));
        }

        [Test, Description("Vérifie qu'un contrôle (Entrave) repousse l'ennemi sur la frise et annule son action annoncée — parce que le contrôle est la réponse à la grosse attaque (spec §A)")]
        public void Bind_PushesBack_AndCancelsAnnouncedAction()
        {
            var s = Sim(new[] { "verre", "ancre" }, new[] { "brute" }, CombatRank.Elite);
            var brute = s.Unit(2);
            At(s.Unit(1), 0);
            At(brute, 85);
            s.Step(1);
            Assert.That(brute.PlannedSlot, Is.GreaterThanOrEqualTo(0), "action annoncée à 85 %");
            At(s.Unit(0), 100);
            s.Submit(CombatCommand.UseSkill(0, 1, 2));
            var ev = s.Step(1);
            Assert.That(ev.OfType<PushedBackEvent>().Any(e => e.Unit == brute.Id && e.Percent == 25), Is.True);
            Assert.That(ev.OfType<ActionInterruptedEvent>().Any(e => e.Unit == brute.Id && e.Reason == InterruptReason.PushBack), Is.True);
            Assert.That(brute.PlannedSlot, Is.EqualTo(-1));
            Assert.That(Pct(brute), Is.LessThan(60));
        }

        [Test, Description("Vérifie qu'un coup critique en zone de préparation annule l'action et renvoie l'unité à 40 %, mais pas en dehors de la zone — parce que la zone 80-100 % est la zone de risque (spec §C)")]
        public void Crit_InPrepZone_Interrupts_OutsideDoesNot()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" }, CombatRank.Elite);
            var brute = s.Unit(1);
            At(s.Unit(0), 0);
            At(brute, 90);
            brute.PlannedSlot = 1;
            var ev = s.ApplyDamageForTests(brute, 10, crit: true);
            Assert.That(ev.OfType<ActionInterruptedEvent>().Any(e => e.Reason == InterruptReason.Critical), Is.True);
            Assert.That(Pct(brute), Is.EqualTo(40));
            Assert.That(brute.PlannedSlot, Is.EqualTo(-1));

            At(brute, 60);
            ev = s.ApplyDamageForTests(brute, 10, crit: true);
            Assert.That(ev.OfType<ActionInterruptedEvent>().Any(), Is.False);
            Assert.That(Pct(brute), Is.EqualTo(60));

            At(brute, 90);
            ev = s.ApplyDamageForTests(brute, 10, crit: false);
            Assert.That(ev.OfType<ActionInterruptedEvent>().Any(), Is.False, "un coup non critique n'interrompt pas");
        }

        [Test, Description("Vérifie que l'Ancrage protège du recul et de l'interruption — parce que la Matière crée des ancres que la frise ne peut pas déplacer (spec §C)")]
        public void Anchor_BlocksPushBackAndInterrupt()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" }, CombatRank.Elite);
            var brute = s.Unit(1);
            brute.Statuses.Add(new StatusInstance(StatusType.Anchor, 2));
            At(brute, 90);
            brute.PlannedSlot = 1;
            var ev = s.ApplyDamageForTests(brute, 10, crit: true);
            Assert.That(ev.OfType<ActionInterruptedEvent>().Any(), Is.False);
            Assert.That(Pct(brute), Is.EqualTo(90));
        }

        [Test, Description("Vérifie que la compétence ultime de l'Ancre pose l'Ancrage sur toute l'équipe — parce que c'est l'exemple de la spec : l'Ancre empêche l'équipe d'être repoussée")]
        public void AncreUltimate_AnchorsTheWholeTeam()
        {
            var s = Sim(new[] { "ancre", "givre" }, new[] { "brute" });
            Calm(s);
            Act(s, s.Unit(0), 2, 2);
            Assert.That(s.Unit(0).Has(StatusType.Anchor) && s.Unit(1).Has(StatusType.Anchor), Is.True);
        }

        [Test, Description("Vérifie que la brûlure inflige des dégâts à chaque pas sur la frise (proportionnels à la distance parcourue) et plus rien à l'arrêt — parce que l'Élémentaire brûle en avançant (spec §C)")]
        public void Burn_DamagesAsTheUnitAdvances()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            var u = s.Unit(0);
            At(u, 0);
            At(s.Unit(1), 0);
            u.Statuses.Add(new StatusInstance(StatusType.Burn, 3));
            int hp0 = u.Hp;
            long g0 = u.Gauge;
            s.Step(60);
            long moved = u.Gauge - g0;
            int expected = (int)((long)u.MaxHp * 8 * moved / (u.GaugeMax * 100));
            Assert.That(hp0 - u.Hp, Is.InRange(expected - 2, expected + 2));
            Assert.That(hp0 - u.Hp, Is.GreaterThan(0));
        }

        [Test, Description("Vérifie que la brûlure ne coûte rien à une unité qui n'avance pas — parce que les dégâts suivent le mouvement sur la frise")]
        public void Burn_NoDamageWhileReadyAndWaiting()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            var u = s.Unit(0);
            At(s.Unit(1), 0);
            At(u, 100);
            u.Statuses.Add(new StatusInstance(StatusType.Burn, 3));
            int hp = u.Hp;
            s.Step(40);
            Assert.That(u.Hp, Is.EqualTo(hp));
        }

        [Test, Description("Vérifie que le Temps d'équipe démarre à 40, se régénère d'un point toutes les 10 ticks et ne dépasse jamais 100 — parce que c'est la ressource partagée de la Time Bank (spec §B)")]
        public void TimeBank_StartsAt40_Regenerates_IsCapped()
        {
            var s = Sim(new[] { "ancre" }, new[] { "brute" });
            Assert.That(s.TimeBank, Is.EqualTo(40));
            Calm(s);
            s.Unit(0).Spd = 0; // l allié n arrive jamais à 0 : la pause d action ne fige pas la réserve
            s.Step(100);
            Assert.That(s.TimeBank, Is.EqualTo(50));
            s.Step(2000);
            Assert.That(s.TimeBank, Is.LessThanOrEqualTo(s.TimeBankMax));
        }

        [Test, Description("Vérifie que le dépassement coûte 40 de Temps, met l'allié à 100 %, repousse l'ennemi de 40 % et coupe son action annoncée — parce que c'est l'usage « saut de tour » de la Time Bank (spec §B)")]
        public void TimeSkip_AdvancesAlly_CutsEnemyAction()
        {
            var s = Sim(new[] { "givre" }, new[] { "brute" }, CombatRank.Elite);
            var givre = s.Unit(0); var brute = s.Unit(1);
            At(givre, 50);
            At(brute, 90);
            s.Step(1);
            Assert.That(brute.PlannedSlot, Is.GreaterThanOrEqualTo(0));
            int bank = s.TimeBank;
            s.Submit(CombatCommand.TimeSkip(0, 1));
            var ev = s.Step(1);
            Assert.That(ev.OfType<TimeSkipEvent>().Any(), Is.True);
            Assert.That(givre.IsReady, Is.True);
            Assert.That(Pct(brute), Is.LessThanOrEqualTo(55));
            Assert.That(brute.PlannedSlot, Is.EqualTo(-1));
            Assert.That(s.TimeBank, Is.EqualTo(bank - 40 + (s.Tick % 10 == 0 ? 1 : 0)));
        }

        [Test, Description("Vérifie que le dépassement est refusé (et non journalisé) sans assez de Temps ou si l'allié est déjà prêt — parce qu'une commande invalide ne doit rien coûter ni se rejouer")]
        public void TimeSkip_RefusedWhenTooPoorOrAlreadyReady()
        {
            var s = Sim(new[] { "givre" }, new[] { "brute" });
            Calm(s);
            At(s.Unit(0), 30);
            s.Submit(CombatCommand.TimeSkip(0, 1));
            s.Step(1);
            Assert.That(s.Unit(0).IsReady, Is.True, "le premier dépassement passe (40 de Temps au départ)");
            At(s.Unit(0), 30);
            s.Submit(CombatCommand.TimeSkip(0, 1));
            s.Step(1);
            Assert.That(s.InputLog.Count(i => i.Command.Type == CommandType.TimeSkip), Is.EqualTo(1), "le second est refusé : plus assez de Temps");
            Assert.That(s.CanTimeSkip(0, 1), Is.False);
            var ready = Sim(new[] { "givre" }, new[] { "brute" });
            At(ready.Unit(0), 100);
            Assert.That(ready.CanTimeSkip(0, 1), Is.False, "un allié déjà prêt n a rien à sauter");
        }

        [Test, Description("Vérifie qu'un combo synchronisé rattrape un partenaire à 60 % ou plus pour 25 de Temps et déclenche le combo — parce que le joueur n'a plus à viser le même instant exact (spec §B)")]
        public void SyncCombo_PullsPartner_SpendsTime()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            Calm(s);
            At(s.Unit(0), 100);
            At(s.Unit(1), 70);
            Assert.That(s.AvailableCombos().Count, Is.Zero, "partenaire pas prêt : combo normal impossible");
            Assert.That(s.AvailableSyncCombos().Count, Is.EqualTo(1));
            int bank = s.TimeBank;
            s.Submit(CombatCommand.SyncCombo(0, 1, 2));
            var ev = s.Step(1);
            Assert.That(ev.OfType<ComboUsedEvent>().Any(), Is.True);
            Assert.That(s.TimeBank, Is.EqualTo(bank - 25 + (s.Tick % 10 == 0 ? 1 : 0)));
            Assert.That(Pct(s.Unit(1)), Is.EqualTo(-15));
        }

        [Test, Description("Vérifie que la synchronisation est refusée si le partenaire est trop loin sur la frise (< 60 %) — parce que le Temps ne rattrape qu'un petit écart")]
        public void SyncCombo_RefusedWhenPartnerTooFar()
        {
            var s = Sim(new[] { "givre", "ancre" }, new[] { "brute" });
            Calm(s);
            At(s.Unit(0), 100);
            At(s.Unit(1), 40);
            Assert.That(s.AvailableSyncCombos().Count, Is.Zero);
            s.Submit(CombatCommand.SyncCombo(0, 1, 2));
            Assert.That(s.Step(1).OfType<ComboUsedEvent>().Any(), Is.False);
            Assert.That(s.InputLog.Any(), Is.False);
        }

        [Test, Description("Vérifie qu'un combat manuel utilisant dépassement et combo synchronisé se rejoue à l'identique depuis son journal — parce que la vérification serveur repose sur le rejeu des commandes (doc 05)")]
        public void NewCommands_ReplayToSameHash()
        {
            var setup = new CombatSetup(21UL, CombatRank.Normal,
                new[] { new AllyEntry(new MemberId("givre"), 5, 0), new AllyEntry(new MemberId("ancre"), 5, 0), new AllyEntry(new MemberId("echo"), 5, 0) },
                new[] { new EnemyEntry(new EnemyId("brute"), 5), new EnemyEntry(new EnemyId("thug"), 5) }, false);
            var sim = new CombatSimulation(C, setup);
            int guard = 0;
            while (!sim.IsOver && guard++ < 30000)
            {
                foreach (var u in sim.Units.Where(x => x.Side == Side.Ally && x.IsReady))
                {
                    var sync = sim.AvailableSyncCombos();
                    if (sync.Count > 0 && sim.CanUse(u.Id, 0)) { sim.Submit(CombatCommand.SyncCombo(sync[0].A, sync[0].B, -1)); continue; }
                    int slot = sim.CanUse(u.Id, 2) ? 2 : sim.CanUse(u.Id, 1) ? 1 : 0;
                    sim.Submit(CombatCommand.UseSkill(u.Id, slot, -1));
                }
                var foe = sim.Units.FirstOrDefault(x => x.Side == Side.Enemy && x.IsAlive && x.PlannedSlot >= 0);
                var slow = sim.Units.FirstOrDefault(x => x.Side == Side.Ally && x.IsAlive && !x.IsReady);
                if (foe != null && slow != null && sim.CanTimeSkip(slow.Id, foe.Id)) sim.Submit(CombatCommand.TimeSkip(slow.Id, foe.Id));
                sim.Step(1);
            }
            Assert.That(sim.IsOver);
            Assert.That(sim.InputLog.Any(i => i.Command.Type == CommandType.TimeSkip), Is.True, "le test exerce bien le dépassement");
            var replay = CombatReplay.Run(C, setup, sim.InputLog);
            Assert.That(replay.StateHash, Is.EqualTo(sim.StateHash()));
        }
    }
}
