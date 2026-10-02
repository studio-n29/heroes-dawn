using System.Linq;
using N29.HeroesDawn.Application.Messages;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class SaveSchedulerTests
    {
        [Test]
        public void RequestSave_PublishesSavingTrueThenFalse_AndWritesProfile()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Save.RequestSave();
            Assert.That(app.Profiles.Saves, Is.EqualTo(1));
            Assert.That(app.Received<SaveStateChanged>().Select(m => m.Saving), Is.EqualTo(new[] { true, false }));
        }

        [Test]
        public void RequestSave_WhileSaving_RunsExactlyOneMoreWrite()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Profiles.BlockNextSave();
            app.Save.RequestSave();
            app.Save.RequestSave();
            app.Save.RequestSave();
            app.Profiles.ReleaseBlockedSave();
            System.Threading.SpinWait.SpinUntil(() => app.Profiles.Saves >= 2, 1000); // port en Task : relance sur le pool de threads (synchrone après la tâche 6)
            System.Threading.Thread.Sleep(50);
            Assert.That(app.Profiles.Saves, Is.EqualTo(2));
        }

        [Test]
        public void RequestSave_WithoutAccount_DoesNothing()
        {
            var app = new TestApp();
            app.Save.RequestSave();
            Assert.That(app.Profiles.Saves, Is.EqualTo(0));
        }
    }
}
