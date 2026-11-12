using System.Linq;
using System.Reflection;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Activities;
using N29.HeroesDawn.Presentation.Board;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class ActivityCoverageTests
    {
        [Test, Description("Vérifie que chaque type d'activité du jeu a un mini-jeu 2D ou une fenêtre dédiée — parce que la fenêtre générique « simuler la réussite » ne doit jamais être atteinte par un joueur (elle offre une victoire gratuite)")]
        public void EveryActivityKind_HasADedicatedGame()
        {
            var kinds = typeof(ActivityKinds).GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(f => f.IsLiteral && f.FieldType == typeof(string)).Select(f => (string)f.GetRawConstantValue()!).ToList();
            Assert.That(kinds.Count, Is.GreaterThanOrEqualTo(14), "les types d'activité sont bien recensés");
            var uncovered = kinds.Where(k => !MiniGameDrivers.Has(k) && !CasePopupPresenter.HasDedicatedWindow(k)).ToList();
            Assert.That(uncovered, Is.Empty, "types sans jeu dédié : " + string.Join(", ", uncovered));
        }
    }
}
