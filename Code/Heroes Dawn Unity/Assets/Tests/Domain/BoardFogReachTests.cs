using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Plateau : cases cachées à portée surlignées, avec leur arrêt forcé (spec 2026-10-07-plateau-deplacement).</summary>
    public sealed class BoardFogReachTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static ZoneRun NewRun(int pm)
        {
            var run = new ZoneRun(C, C.Zone(PrototypeContent.Zone11), 5UL);
            run.SetPmForTests(pm);
            return run;
        }

        [Test, Description("Vérifie que les cases encore sous le brouillard mais à portée des PM sont listées, avec leur premier arrêt forcé comme vraie destination — parce que le joueur doit voir où mène son chemin même au-delà du brouillard")]
        public void FogReach_ListsHiddenCasesInRange_WithTheirForcedStop()
        {
            var run = NewRun(6);
            Assert.That(run.FogReach, Is.Not.Empty, "au départ, des cases cachées sont à moins de 6 pas");
            foreach (var kv in run.FogReach)
            {
                var plan = kv.Value;
                Assert.That(run.Fog.IsRevealed(kv.Key), Is.False, "la case est bien cachée");
                Assert.That(run.Destinations.ContainsKey(kv.Key), Is.False, "une case cachée n'est jamais une destination directe");
                Assert.That(run.Destinations.ContainsKey(plan.StopAt), Is.True, "l'arrêt forcé est une vraie destination");
                Assert.That(plan.Cost, Is.LessThanOrEqualTo(6));
                Assert.That(plan.StopPath.Count, Is.LessThan(plan.FullPath.Count), "on s'arrête avant d'atteindre la case cachée");
                Assert.That(plan.FullPath.Last(), Is.EqualTo(kv.Key));
            }
        }

        [Test, Description("Vérifie que la portée cachée se limite aux PM en main — parce qu'avec 1 PM aucune case cachée n'est à portée")]
        public void FogReach_RespectsTheMovementPoints()
        {
            var run = NewRun(1);
            Assert.That(run.FogReach, Is.Empty);
            var far = NewRun(6);
            Assert.That(far.FogReach.Values.All(p => p.Cost <= 6));
        }

        [Test, Description("Vérifie que sans brouillard (tout révélé) il n'y a aucune case cachée à surligner — parce que la portée cachée n'existe que sous le brouillard")]
        public void FogReach_IsEmpty_WhenEverythingIsRevealed()
        {
            var run = ZoneRun.ForTests(C, C.Zone(PrototypeContent.Zone11), 5UL, 6, true);
            Assert.That(run.FogReach, Is.Empty);
        }

        [Test, Description("Vérifie que la portée cachée est vidée dès que les PM sont versés dans la réserve (fin du déplacement) — parce que plus aucune case n'est disponible")]
        public void FogReach_IsCleared_WhenTheMoveEnds()
        {
            var run = NewRun(6);
            run.EndMove();
            Assert.That(run.FogReach, Is.Empty);
            Assert.That(run.Destinations, Is.Empty);
        }
    }
}
