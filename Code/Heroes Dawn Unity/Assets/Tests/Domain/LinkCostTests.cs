using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Maps;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Chemins : coût en PM par chemin, trajet le moins cher, courbes de Bézier (plan 2026-10-10, sous-projet F).</summary>
    public sealed class LinkCostTests
    {
        static CaseId Id(string v) => new CaseId(v);

        static NodeDef N(string id, CaseType type, int x, int y) => new NodeDef(Id(id), type, false, CombatRank.Normal, x, y);

        static LinkDef L(string a, string b, int cost = 1, LinkCurve? curve = null) => new LinkDef(Id(a), Id(b), cost, curve);

        /// <summary>Entrée n00, deux routes vers n02 : par n01 (1 + 1) ou en direct (coût donné).</summary>
        static BoardGraph Graph(int directCost, CaseType middle = CaseType.Empty)
        {
            var nodes = new List<NodeDef> { N("n00", CaseType.Entry, 0, 0), N("n01", middle, 1, 0), N("n02", CaseType.Empty, 2, 0), N("n03", CaseType.Empty, 1, 1) };
            var links = new List<LinkDef> { L("n00", "n01"), L("n01", "n02"), L("n00", "n02", directCost), L("n00", "n03", 2), L("n03", "n02", 2) };
            var zone = new ZoneDef(new ZoneId("zone_test"), "zone.test.name", Id("n00"), nodes, links, new List<ObjectiveDef>(),
                                new List<EncounterDef> { new EncounterDef(CombatRank.Normal, new[] { new EnemyId("grunt") }) });
            return new BoardGraph(zone);
        }

        static IReadOnlyDictionary<CaseId, MovePlan> Destinations(BoardGraph g, int pm, System.Func<CaseId, bool>? blocking = null)
        {
            var fog = new FogOfWar(g, 2);
            fog.Restore(g.AllIds);
            return MovementRules.FindDestinations(g, fog, Id("n00"), pm, blocking ?? (_ => false));
        }

        [Test, Description("Vérifie qu'un chemin créé sans coût coûte 1 PM et qu'un coût inférieur à 1 est relevé à 1 — parce que tout trajet doit coûter au moins un PM")]
        public void LinkDef_CostDefaultsToOne_AndNeverBelowOne()
        {
            Assert.That(L("a", "b").Cost, Is.EqualTo(1));
            Assert.That(L("a", "b", 0).Cost, Is.EqualTo(1));
            Assert.That(L("a", "b", -4).Cost, Is.EqualTo(1));
            Assert.That(L("a", "b", 3).Cost, Is.EqualTo(3));
        }

        [Test, Description("Vérifie que le chemin retenu est le moins cher en PM même s'il passe par plus de cases — parce qu'un trajet risqué mais direct doit rester un choix, pas une évidence")]
        public void Destinations_PickTheCheapestRoute_NotTheShortestInCases()
        {
            var plan = Destinations(Graph(directCost: 3), 5)[Id("n02")];
            Assert.That(plan.Path.Select(c => c.Value), Is.EqualTo(new[] { "n01", "n02" }));
            Assert.That(plan.Cost, Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'un raccourci à coût inférieur est préféré au détour — parce qu'un chemin plus rapide doit faire gagner des PM")]
        public void Destinations_PickTheCheaperShortcut()
        {
            var plan = Destinations(Graph(directCost: 1), 5)[Id("n02")];
            Assert.That(plan.Path.Select(c => c.Value), Is.EqualTo(new[] { "n02" }));
            Assert.That(plan.Cost, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'à coût égal le trajet le plus court en cases puis l'ordre des identifiants départagent — parce que le trajet doit rester déterministe")]
        public void Destinations_BreakTiesByHopsThenIds()
        {
            var plan = Destinations(Graph(directCost: 2), 5)[Id("n02")];
            Assert.That(plan.Path.Count, Is.EqualTo(1), "le raccourci à 2 PM bat le détour à 2 PM en cases");
            var again = Destinations(Graph(directCost: 2), 5)[Id("n02")];
            Assert.That(again.Path.Select(c => c.Value), Is.EqualTo(plan.Path.Select(c => c.Value)));
        }

        [Test, Description("Vérifie que les destinations se limitent aux PM en main, mesurés en coût des chemins — parce qu'un chemin à 3 PM ne se prend pas avec 2 PM")]
        public void Destinations_RespectTheBudgetInPm()
        {
            var g = Graph(directCost: 3);
            var two = Destinations(g, 2);
            Assert.That(two.Keys.Select(c => c.Value), Is.EquivalentTo(new[] { "n01", "n02", "n03" }));
            Assert.That(two[Id("n03")].Cost, Is.EqualTo(2));
            var one = Destinations(g, 1);
            Assert.That(one.Keys.Select(c => c.Value), Is.EquivalentTo(new[] { "n01" }));
        }

        [Test, Description("Vérifie qu'un combat non résolu en travers du chemin n'empêche plus de viser plus loin : il reste une destination et le plus court trajet peut le traverser — parce que le joueur choisit sa destination finale, le combat l'arrêtera en route")]
        public void Destinations_GoBeyondAnUnresolvedCombat()
        {
            var g = Graph(directCost: 3, middle: CaseType.Combat);
            var dest = Destinations(g, 6, c => c.Value == "n01");
            Assert.That(dest.ContainsKey(Id("n01")), Is.True, "le combat est une destination");
            var toTarget = dest[Id("n02")];
            Assert.That(toTarget.Path.Select(c => c.Value), Does.Contain("n01"), "la case au-delà du combat est atteignable par le combat");
            Assert.That(toTarget.Cost, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que le coût total d'un trajet est la somme des coûts de ses chemins — parce que l'affichage et la dépense de PM reposent sur la même valeur")]
        public void PathCost_SumsEveryLink()
        {
            var g = Graph(directCost: 3);
            Assert.That(g.PathCost(Id("n00"), new[] { Id("n03"), Id("n02") }), Is.EqualTo(4));
            Assert.That(g.CostBetween(Id("n02"), Id("n00")), Is.EqualTo(3), "le coût ne dépend pas du sens");
        }

        [Test, Description("Vérifie que se déplacer dépense le coût des chemins empruntés, pas le nombre de cases — parce que les PM en main doivent baisser du coût affiché")]
        public void ZoneRun_MoveSpendsThePathCost()
        {
            var g = Graph(directCost: 3);
            var run = ZoneRun.ForTests(new ContentCatalogStub().Catalog, g.Zone, 1UL, 5, true);
            var plan = run.Destinations[Id("n02")];
            var events = run.MoveTo(Id("n02"));
            Assert.That(plan.Cost, Is.EqualTo(2));
            var banked = events.OfType<PmBankedEvent>().Single().Pm;
            Assert.That(banked, Is.EqualTo(5 - plan.Cost), "les PM restants (5 moins le coût du trajet) sont versés à la réserve");
        }

        [Test, Description("Vérifie que le pion traverse sans s'arrêter les cases sans combat et ne s'arrête qu'en entrant sur un combat non résolu — parce que le déplacement doit être continu (plan 2026-10-10, sous-projet G)")]
        public void ZoneRun_TravelsThroughPlainCases_AndStopsOnlyOnACombat()
        {
            var plain = ZoneRun.ForTests(new ContentCatalogStub().Catalog, Graph(directCost: 9).Zone, 1UL, 5, true);
            var events = plain.MoveTo(Id("n02"));
            Assert.That(events.OfType<PawnMovedEvent>().Single().Path.Select(c => c.Value), Is.EqualTo(new[] { "n01", "n02" }), "n01 est traversée sans arrêt");
            Assert.That(plain.Position, Is.EqualTo(Id("n02")));

            var fight = ZoneRun.ForTests(new ContentCatalogStub().Catalog, Graph(directCost: 9, middle: CaseType.Combat).Zone, 1UL, 5, true);
            Assert.That(fight.Destinations[Id("n01")].Path.Count, Is.EqualTo(1));
            Assert.That(fight.Destinations[Id("n02")].Path.Select(c => c.Value), Does.Contain("n01"), "le trajet peut traverser le combat, qui arrêtera le pion");
            fight.MoveTo(Id("n01"));
            Assert.That(fight.Phase, Is.EqualTo(ZoneRunPhase.InCombat), "le pion s'arrête en entrant sur le combat");
        }

        [Test, Description("Vérifie que le pion s'arrête sur un combat en travers de sa route, garde ses PM restants, puis rejoint la destination choisie après la victoire — parce que le joueur ne doit pas redonner l'ordre une fois le combat gagné")]
        public void ZoneRun_StopsOnACombatOnTheRoute_ThenResumesAfterVictory()
        {
            var run = ZoneRun.ForTests(new ContentCatalogStub().Catalog, Graph(directCost: 9, middle: CaseType.Combat).Zone, 1UL, 5, true);
            var events = run.MoveTo(Id("n02"));
            Assert.That(events.OfType<PawnMovedEvent>().Single().Path.Select(c => c.Value), Is.EqualTo(new[] { "n01" }), "le pion s'arrête sur le combat");
            Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.InCombat));
            Assert.That(run.PmInHand, Is.EqualTo(4), "les PM restants sont gardés pendant le combat");

            var after = run.ResolveCombat(true);
            Assert.That(after.OfType<PawnMovedEvent>().Single().Path.Select(c => c.Value), Is.EqualTo(new[] { "n02" }), "le pion repart vers la destination prévue");
            Assert.That(run.Position, Is.EqualTo(Id("n02")));
        }

        [Test, Description("Vérifie qu'une défaite ramène le pion en arrière et abandonne la destination prévue — parce qu'on ne repart pas après un échec")]
        public void ZoneRun_AfterADefeat_DoesNotResume()
        {
            var run = ZoneRun.ForTests(new ContentCatalogStub().Catalog, Graph(directCost: 9, middle: CaseType.Combat).Zone, 1UL, 5, true);
            run.MoveTo(Id("n02"));
            var after = run.ResolveCombat(false);
            Assert.That(after.OfType<PawnMovedEvent>(), Is.Empty);
            Assert.That(run.Position, Is.EqualTo(Id("n00")));
            Assert.That(run.Phase, Is.EqualTo(ZoneRunPhase.AwaitingRoll));
        }

        [Test, Description("Vérifie que la courbe générée d'un chemin est stable, relie bien ses deux cases et n'est jamais une droite — parce que le tracé organique doit être le même en jeu et dans l'éditeur")]
        public void AutoCurve_IsDeterministic_AndNeverStraight()
        {
            var a = N("n00", CaseType.Entry, 0, 0); var b = N("n01", CaseType.Empty, 3, 1);
            var link = L("n00", "n01");
            var c1 = LinkCurves.Resolve(a, b, link);
            var c2 = LinkCurves.Resolve(a, b, link);
            Assert.That(c1.C1.X, Is.EqualTo(c2.C1.X));
            Assert.That(c1.C2.Y, Is.EqualTo(c2.C2.Y));
            Assert.That(c1.P0.X, Is.EqualTo(0f)); Assert.That(c1.P3.X, Is.EqualTo(3f));
            var m = c1.Middle;
            float lineMidX = 1.5f, lineMidY = 0.5f;
            Assert.That(System.Math.Abs(m.X - lineMidX) + System.Math.Abs(m.Y - lineMidY), Is.GreaterThan(0.01f), "le milieu s'écarte de la droite");
        }

        [Test, Description("Vérifie qu'une courbe saisie est reprise telle quelle par le tracé — parce que le level designer doit contrôler la forme du chemin")]
        public void CustomCurve_IsUsedAsGiven()
        {
            var a = N("n00", CaseType.Entry, 0, 0); var b = N("n01", CaseType.Empty, 4, 0);
            var link = L("n00", "n01", 1, new LinkCurve(1f, 2f, 3f, -2f));
            var c = LinkCurves.Resolve(a, b, link);
            Assert.That(c.C1.X, Is.EqualTo(1f)); Assert.That(c.C1.Y, Is.EqualTo(2f));
            Assert.That(c.C2.X, Is.EqualTo(3f)); Assert.That(c.C2.Y, Is.EqualTo(-2f));
        }

        [Test, Description("Vérifie que l'échantillonnage de Bézier part de la première case, finit sur la seconde et que sa longueur dépasse la droite quand la courbe se courbe — parce que le pion et les segments suivent ces points")]
        public void Bezier_SamplesRunFromEndToEnd_AndMeasureLength()
        {
            var curve = new BezierCurve(new PointF(0, 0), new PointF(0, 2), new PointF(4, 2), new PointF(4, 0));
            var pts = curve.Sample(10);
            Assert.That(pts.Length, Is.EqualTo(11));
            Assert.That(pts[0].X, Is.EqualTo(0f)); Assert.That(pts[10].X, Is.EqualTo(4f));
            Assert.That(curve.Length(), Is.GreaterThan(4f));
            var straight = new BezierCurve(new PointF(0, 0), new PointF(1, 0), new PointF(2, 0), new PointF(3, 0));
            Assert.That(straight.Length(), Is.EqualTo(3f).Within(0.001f));
        }

        [Test, Description("Vérifie que le validateur signale un chemin très coûteux ou une courbe très éloignée de ses cases — parce que ces réglages sont presque toujours des erreurs de saisie")]
        public void Validator_WarnsOnExtremeCostsAndFarCurves()
        {
            var nodes = new List<NodeDef> { N("n00", CaseType.Entry, 0, 0), N("n01", CaseType.Empty, 1, 0) };
            var links = new List<LinkDef> { L("n00", "n01", 8, new LinkCurve(10f, 10f, 11f, 11f)) };
            var zone = new ZoneDef(new ZoneId("zone_test"), "zone.test.name", Id("n00"), nodes, links, new List<ObjectiveDef>(), new List<EncounterDef>());
            var issues = ZoneValidator.Validate(zone, N29.HeroesDawn.Content.Prototype.PrototypeContent.Build());
            Assert.That(issues.Any(i => i.Code == "link.cost"), Is.True);
            Assert.That(issues.Any(i => i.Code == "link.curve"), Is.True);
        }

        sealed class ContentCatalogStub
        {
            public ContentCatalog Catalog { get; } = N29.HeroesDawn.Content.Prototype.PrototypeContent.Build();
        }
    }
}
