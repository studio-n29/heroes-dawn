using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class WalletBinderTests
    {
        [Test, Description("Vérifie qu'un changement de portefeuille ou de sauvegarde est poussé à la vue — parce que la barre de ressources du QG et du plateau suit le portefeuille")]
        public void WalletChanged_PushesNewValues()
        {
            var app = new TestApp();
            var view = new FakeWalletView();
            using var binder = new WalletBinder(view, app.Session, app.Subscriber<WalletChanged>(), app.Subscriber<SaveStateChanged>());
            app.Publisher<WalletChanged>().Publish(new WalletChanged(1234, 56));
            Assert.That(view.Last.Credits, Is.EqualTo(1234));
            Assert.That(view.Last.Pr, Is.EqualTo(56));
            app.Publisher<SaveStateChanged>().Publish(new SaveStateChanged(true));
            Assert.That(view.Saving, Is.True);
        }

        [Test, Description("Vérifie qu'après Dispose la vue ne reçoit plus rien — parce qu'une scène déchargée ne doit garder aucun abonnement")]
        public void Dispose_StopsUpdates()
        {
            var app = new TestApp();
            var view = new FakeWalletView();
            var binder = new WalletBinder(view, app.Session, app.Subscriber<WalletChanged>(), app.Subscriber<SaveStateChanged>());
            int before = view.Shows;
            binder.Dispose();
            app.Publisher<WalletChanged>().Publish(new WalletChanged(1, 1));
            Assert.That(view.Shows, Is.EqualTo(before));
        }
    }
}
