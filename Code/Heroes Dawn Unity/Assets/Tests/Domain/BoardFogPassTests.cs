using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Plateau : le brouillard ne bloque plus le déplacement (spec 2026-10-12 : seules les cases de combat arrêtent le pion).</summary>
    public sealed class BoardFogPassTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static ZoneRun NewRun(int pm)
        {
            var run = new ZoneRun(C, C.Zone(PrototypeContent.Zone11), 5UL);
            run.SetPmForTests(pm);
            return run;
        }

        [Test, Description("Vérifie que les cases encore sous le brouillard mais à portée des PM sont de vraies destinations — parce que le brouillard ne doit plus arrêter le pion en plein déplacement")]
        public void HiddenCasesInRange_AreDestinations()
        {
            var run = NewRun(6);
            Assert.That(run.FogReach, Is.Empty, "la portée cachée n'existe plus");
            var hidden = run.Destinations.Keys.Where(id => !run.Fog.IsRevealed(id)).ToList();
            Assert.That(hidden, Is.Not.Empty, "au départ, des cases cachées sont à moins de 6 pas");
            foreach (var id in hidden) Assert.That(run.Destinations[id].Cost, Is.LessThanOrEqualTo(6));
        }

        [Test, Description("Vérifie qu'aller sur une case cachée suit tout le trajet sans s'arrêter aux cases qui ouvrent du brouillard, et que le brouillard se lève sur le parcours — parce que le pion ne s'arrête qu'aux cases de combat")]
        public void MovingToAHiddenCase_WalksTheWholePath_AndRevealsAlongTheWay()
        {
            var run = NewRun(6);
            var far = run.Destinations.Values.Where(p => !run.Fog.IsRevealed(p.Target)).OrderByDescending(p => p.Path.Count).First();
            Assert.That(far.Path.Count, Is.GreaterThan(1));
            var events = run.MoveTo(far.Target);
            var moved = events.OfType<PawnMovedEvent>().Single();
            Assert.That(moved.Path.Last(), Is.EqualTo(far.Target), "le pion va jusqu'à la case choisie");
            Assert.That(run.Fog.IsRevealed(far.Target), Is.True, "la case d'arrivée est éclaircie");
            Assert.That(events.OfType<FogRevealedEvent>().Count(), Is.GreaterThanOrEqualTo(1));
        }

        [Test, Description("Vérifie qu'une case de combat non résolue reste un arrêt : on peut l'atteindre mais jamais la traverser — parce que c'est la seule case d'arrêt voulue")]
        public void UnresolvedCombat_StaysTheOnlyStop()
        {
            var run = NewRun(6);
            foreach (var plan in run.Destinations.Values)
                foreach (var step in plan.Path.Take(plan.Path.Count - 1))
                    Assert.That(run.Graph.Node(step).Type == CaseType.Combat && !run.IsSpent(step), Is.False, "pas de combat non résolu au milieu d'un trajet : " + plan.Target);
        }

        [Test, Description("Vérifie que la portée reste limitée aux PM en main — parce qu'avec 1 PM seules les cases voisines sont à portée")]
        public void Destinations_RespectTheMovementPoints()
        {
            var run = NewRun(1);
            Assert.That(run.Destinations.Values.All(p => p.Cost <= 1));
            var far = NewRun(6);
            Assert.That(far.Destinations.Values.All(p => p.Cost <= 6));
        }

        [Test, Description("Vérifie que les destinations sont vidées dès que les PM sont versés dans la réserve (fin du déplacement) — parce que plus aucune case n'est disponible")]
        public void Destinations_AreCleared_WhenTheMoveEnds()
        {
            var run = NewRun(6);
            run.EndMove();
            Assert.That(run.Destinations, Is.Empty);
        }
    }
}
