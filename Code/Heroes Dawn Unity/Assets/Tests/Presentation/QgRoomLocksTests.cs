#nullable enable
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Tests.Domain.Application;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class QgRoomLocksTests
    {
        [Test, Description("Vérifie que le Registre se déverrouille avec le chapitre 1.1, et que le réglage de test « tout débloquer » ouvre toutes les pièces — parce que les pièces s'ouvrent au fil de l'histoire, et le test doit pouvoir tout voir")]
        public void Agents_UnlocksAfterChapter11_AndUnlockAllOpensEverything()
        {
            var app = new TestApp();
            Assert.That(QgRoomLocks.Reason(HubPage.Agents, app.Session, true, false), Is.EqualTo("zone_ny_1_1"));
            app.Session.CompletedZones.Add("zone_ny_1_1");
            Assert.That(QgRoomLocks.Reason(HubPage.Agents, app.Session, true, false), Is.Null);
            Assert.That(QgRoomLocks.Reason(HubPage.Operations, app.Session, true, false), Is.EqualTo("zone_ny_1_3"));
            Assert.That(QgRoomLocks.Reason(HubPage.Operations, app.Session, true, true), Is.Null, "tout débloquer ouvre tout");
        }

        [Test, Description("Vérifie que le Grimoire et l'Inventaire attendent la fin du tutoriel, et que les pièces sans condition restent ouvertes — parce que le tutoriel présente ces pièces au joueur")]
        public void GrimoireAndInventory_WaitForTutorial_OthersAlwaysOpen()
        {
            var app = new TestApp();
            Assert.That(QgRoomLocks.Reason(HubPage.Grimoire, app.Session, false, false), Is.EqualTo("tutorial"));
            Assert.That(QgRoomLocks.Reason(HubPage.Inventory, app.Session, false, false), Is.EqualTo("tutorial"));
            Assert.That(QgRoomLocks.Reason(HubPage.Grimoire, app.Session, true, false), Is.Null);
            Assert.That(QgRoomLocks.Reason(HubPage.Team, app.Session, false, false), Is.Null);
        }
    }
}
