using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Authoring;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>Les 9 mini-jeux du PDF sont placés dans les 4 zones de Brooklyn et chaque case se planifie (lot H).</summary>
    public sealed class MiniGamePlacementTests
    {
        /// <summary>Case → activité attendue (la table du plan : les plus simples en début de zone, les plus durs en fin).</summary>
        static readonly (string Zone, string Node, string Activity)[] Expected =
        {
            ("zone_ny_1_1", "n08", "dna_lab"),
            ("zone_ny_1_1", "n15", "rhythm_rooftop"),
            ("zone_ny_1_1", "n21", "dialogue_dockers"),
            ("zone_ny_1_2", "n06", "crate_pick_depot"),
            ("zone_ny_1_2", "n13", "gears_workshop"),
            ("zone_ny_1_3", "n06", "free_throw_court"),
            ("zone_ny_1_3", "n11", "infiltration_camp"),
            ("zone_ny_1_4", "n06", "rescue_tool"),
            ("zone_ny_1_4", "n11", "survival_snow"),
        };

        static ContentCatalog Content() => PrototypeContent.Build().WithZones(ZoneLibrary.Load().Zones);

        static IEnumerable<(ZoneDef Zone, NodeDef Node)> MiniGameCases(ContentCatalog c)
            => c.Zones.Where(z => z.Borough == "brooklyn").SelectMany(z => z.Nodes.Where(n => n.Type == CaseType.MiniGame).Select(n => (z, n)));

        [Test, Description("Vérifie que les cases Mini-jeu de Brooklyn lancent exactement les 9 activités de la table (une par case) — parce que les 9 jeux du PDF doivent être jouables au test public")]
        public void AllNineGames_ArePlacedInBrooklyn()
        {
            var placed = MiniGameCases(Content()).Select(x => (x.Zone.Id.Value, x.Node.Id.Value, x.Node.Activity?.Value ?? "")).OrderBy(x => x).ToList();
            Assert.That(placed, Is.EqualTo(Expected.OrderBy(x => x).ToList()));
        }

        [Test, Description("Vérifie que chaque zone de Brooklyn a au moins deux mini-jeux et que les neuf activités placées sont toutes différentes — parce que le test public doit varier les jeux")]
        public void EveryBrooklynZone_HasAtLeastTwoMiniGames_AndAllDistinct()
        {
            var cases = MiniGameCases(Content()).ToList();
            foreach (var z in cases.GroupBy(x => x.Zone.Id.Value)) Assert.That(z.Count(), Is.GreaterThanOrEqualTo(2), z.Key);
            Assert.That(cases.Select(x => x.Node.Activity!.Value.Value).Distinct().Count(), Is.EqualTo(9));
        }

        [Test, Description("Vérifie que chaque activité placée existe au catalogue, que son type est enregistré, et que chaque type sauf le dialogue a un pilote 2D — parce qu'une case sans jeu bloquerait le plateau (bug du playtest)")]
        public void EveryPlacedActivity_IsKnown_AndHasADriver()
        {
            var content = Content();
            var loc = new LocLocalization();
            foreach (var (zone, node) in MiniGameCases(content))
            {
                Assert.That(content.HasActivity(node.Activity!.Value), Is.True, node.Activity.Value.Value);
                var def = content.Activity(node.Activity.Value);
                Assert.That(ActivityRegistry.Default.TryGet(def.Kind, out _), Is.True, def.Kind);
                var plan = ActivityPlanner.Plan(ActivityRegistry.Default, def, zone.Environment, 11UL, node.Id, 1,
                                                new[] { new MemberId("ancre") });
                var driver = MiniGameDrivers.Create(plan, loc);
                if (def.Kind == ActivityKinds.Dialogue) Assert.That(driver, Is.Null, "le dialogue garde sa propre fenêtre");
                else Assert.That(driver, Is.Not.Null, def.Kind);
                driver?.Dispose();
            }
        }

        [Test, Description("Vérifie que les quatre zones de Brooklyn restent valides après le placement (aucune erreur du validateur) — parce que la conversion d'une case en mini-jeu ne doit pas casser la zone")]
        public void BrooklynZones_StayValid()
        {
            var content = Content();
            foreach (var z in content.Zones.Where(z => z.Borough == "brooklyn"))
            {
                var errors = ZoneValidator.Validate(z, content).Where(m => m.Severity == IssueSeverity.Error).Select(m => z.Id.Value + " : " + m.Message).ToList();
                Assert.That(errors, Is.Empty);
            }
        }

        [Test, Description("Vérifie que les objectifs « terminer un mini-jeu » de Brooklyn visent bien une case Mini-jeu — parce qu'un objectif sur une autre case ne pourrait jamais être rempli")]
        public void MiniGameObjectives_TargetMiniGameCases()
        {
            foreach (var z in Content().Zones.Where(z => z.Borough == "brooklyn"))
                foreach (var o in z.Objectives.Where(o => o.Type == ObjectiveType.CompleteMiniGame))
                    Assert.That(z.Nodes.Single(n => n.Id == o.Target).Type, Is.EqualTo(CaseType.MiniGame), z.Id.Value + "/" + o.Id);
        }
    }
}
