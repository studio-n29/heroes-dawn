using System.Linq;
using N29.HeroesDawn.Application.Messages;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    public sealed class SaveSchedulerTests
    {
        [Test, Description("Vérifie qu'une sauvegarde écrit le profil et signale son début puis sa fin — parce que l'indicateur « sauvegarde… » de l'interface en dépend")]
        public void RequestSave_PublishesSavingTrueThenFalse_AndWritesProfile()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Save.RequestSave();
            Assert.That(app.Profiles.Saves, Is.EqualTo(1));
            Assert.That(app.Received<SaveStateChanged>().Select(m => m.Saving), Is.EqualTo(new[] { true, false }));
        }

        [Test, Description("Vérifie que plusieurs demandes pendant une écriture n'en relancent qu'une seule — parce que les écritures sont sérialisées sans perdre la dernière modification")]
        public void RequestSave_WhileSaving_RunsExactlyOneMoreWrite()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Profiles.BlockNextSave();
            app.Save.RequestSave();
            app.Save.RequestSave();
            app.Save.RequestSave();
            app.Profiles.ReleaseBlockedSave();
            Assert.That(app.Profiles.Saves, Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'aucune sauvegarde n'est écrite sans joueur connecté — parce qu'on ne doit jamais créer de profil anonyme")]
        public void RequestSave_WithoutAccount_DoesNothing()
        {
            var app = new TestApp();
            app.Save.RequestSave();
            Assert.That(app.Profiles.Saves, Is.EqualTo(0));
        }
    }
}
