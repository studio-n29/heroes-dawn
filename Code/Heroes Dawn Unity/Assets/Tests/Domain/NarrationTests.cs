using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Dialogues de narration avant et après une case (plan 2026-10-10, sous-projet I).</summary>
    public sealed class NarrationTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static CaseId Id(string v) => new CaseId(v);

        static NarrationLineDef Line(string speaker, string key) => new NarrationLineDef(speaker, key);

        /// <summary>Entrée n00 puis n01 (case donnée, avec ou sans narration) puis n02.</summary>
        static ZoneRun Run(CaseType type, NarrationDef? narration, int pm = 3)
        {
            var nodes = new List<NodeDef>
            {
                new NodeDef(Id("n00"), CaseType.Entry, true, CombatRank.Normal, 0, 0),
                new NodeDef(Id("n01"), type, false, CombatRank.Normal, 1, 0, null, null, narration),
                new NodeDef(Id("n02"), CaseType.Empty, false, CombatRank.Normal, 2, 0),
            };
            var links = new List<LinkDef> { new LinkDef(Id("n00"), Id("n01")), new LinkDef(Id("n01"), Id("n02")) };
            var encounters = new List<EncounterDef> { new EncounterDef(CombatRank.Normal, new[] { new EnemyId("thug") }) };
            var zone = new ZoneDef(new ZoneId("zone_test"), "zone.test.name", Id("n00"), nodes, links, new List<ObjectiveDef>(), encounters);
            return ZoneRun.ForTests(C, zone, 1UL, pm, true, "n00");
        }

        static NarrationDef Both() => new NarrationDef(new[] { Line("anchor", "k.before") }, new[] { Line("anchor", "k.after.0"), Line("brick", "k.after.1") });

        [Test, Description("Vérifie qu'arriver sur une case sans action (carrefour) joue d'abord le dialogue d'avant puis celui d'après — parce qu'une case sans action n'a pas de moment intermédiaire")]
        public void PlainCase_PlaysBeforeThenAfter()
        {
            var events = Run(CaseType.Empty, Both()).MoveTo(Id("n01"));
            var narration = events.OfType<NarrationEvent>().ToList();
            Assert.That(narration.Select(n => n.Phase), Is.EqualTo(new[] { NarrationPhase.Before, NarrationPhase.After }));
            Assert.That(narration[0].Lines.Select(l => l.TextKey), Is.EqualTo(new[] { "k.before" }));
            Assert.That(narration[1].Lines.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que le dialogue d'avant précède le déclenchement d'un combat et que celui d'après attend la victoire — parce que la narration introduit puis conclut l'action")]
        public void CombatCase_BeforeThenCombat_AfterOnlyOnVictory()
        {
            var run = Run(CaseType.Combat, Both());
            var arrival = run.MoveTo(Id("n01"));
            var list = arrival.ToList();
            int before = list.FindIndex(e => e is NarrationEvent n && n.Phase == NarrationPhase.Before);
            int trigger = list.FindIndex(e => e is CaseTriggeredEvent);
            Assert.That(before, Is.GreaterThanOrEqualTo(0));
            Assert.That(before, Is.LessThan(trigger), "le dialogue d'avant joue avant le combat");
            Assert.That(list.OfType<NarrationEvent>().Any(n => n.Phase == NarrationPhase.After), Is.False, "pas de conclusion avant la victoire");

            var victory = run.ResolveCombat(true);
            Assert.That(victory.OfType<NarrationEvent>().Single().Phase, Is.EqualTo(NarrationPhase.After));
        }

        [Test, Description("Vérifie qu'une défaite ne joue pas le dialogue d'après et qu'un retour sur la case ne rejoue pas celui d'avant — parce que chaque dialogue ne se joue qu'une fois par plateau")]
        public void Defeat_SkipsAfter_AndNothingRepeats()
        {
            var run = Run(CaseType.Combat, Both());
            run.MoveTo(Id("n01"));
            var defeat = run.ResolveCombat(false);
            Assert.That(defeat.OfType<NarrationEvent>(), Is.Empty);
            Assert.That(run.Position, Is.EqualTo(Id("n00")), "le pion recule");
            run.SetPmForTests(3);
            var again = run.MoveTo(Id("n01"));
            Assert.That(again.OfType<NarrationEvent>(), Is.Empty, "l'introduction a déjà été jouée");
            var victory = run.ResolveCombat(true);
            Assert.That(victory.OfType<NarrationEvent>().Single().Phase, Is.EqualTo(NarrationPhase.After));
            Assert.That(Run(CaseType.Combat, Both()).MoveTo(Id("n01")).OfType<NarrationEvent>().Count(), Is.EqualTo(1), "un autre plateau rejoue l'introduction");
        }

        [Test, Description("Vérifie qu'une case sans narration ne produit aucun événement de narration — parce que la narration est optionnelle")]
        public void CaseWithoutNarration_EmitsNothing()
        {
            Assert.That(Run(CaseType.Empty, null).MoveTo(Id("n01")).OfType<NarrationEvent>(), Is.Empty);
            Assert.That(Run(CaseType.Empty, new NarrationDef(null, null)).MoveTo(Id("n01")).OfType<NarrationEvent>(), Is.Empty);
        }

        [Test, Description("Vérifie qu'un dialogue seulement « après » n'émet que sa conclusion — parce qu'on peut placer un dialogue avant et/ou après")]
        public void OnlyAfter_EmitsOnlyTheConclusion()
        {
            var after = new NarrationDef(null, new[] { Line("anchor", "k.after") });
            var events = Run(CaseType.Empty, after).MoveTo(Id("n01"));
            Assert.That(events.OfType<NarrationEvent>().Select(n => n.Phase), Is.EqualTo(new[] { NarrationPhase.After }));
        }

        [Test, Description("Vérifie que le prototype place bien un dialogue avant et après le combat d'élite de Brick — parce que c'est l'exemple jouable de la narration")]
        public void Prototype_BrickFightHasNarration()
        {
            var node = C.Zone(PrototypeContent.Zone11).Nodes.First(n => n.Id.Value == "n23");
            Assert.That(node.Narration, Is.Not.Null);
            Assert.That(node.Narration!.Before.Count, Is.GreaterThan(0));
            Assert.That(node.Narration.After.Count, Is.GreaterThan(0));
        }
    }
}
