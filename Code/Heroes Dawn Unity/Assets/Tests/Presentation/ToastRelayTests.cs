#nullable enable
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class ToastRelayTests
    {
        TestApp _app = null!;
        FakeToasts _toasts = null!;
        ToastRelay _relay = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _toasts = new FakeToasts();
            _relay = new ToastRelay(_app.Subscriber<ToastRequested>(), _toasts, new LocLocalization());
            _relay.Start();
        }

        [TearDown]
        public void TearDown() => _relay.Dispose();

        [Test, Description("Vérifie qu'un message du bus portant une clé de localisation s'affiche traduit, avec son type — parce que la machine d'états publie des clés (ex. errors.navigation), pas des phrases")]
        public void KeyMessage_IsShownTranslated()
        {
            _app.Publisher<ToastRequested>().Publish(new ToastRequested("errors.navigation", ToastKind.Error));
            Assert.That(_toasts.Shown, Is.EqualTo(new[] { (new LocLocalization().T("errors.navigation"), ToastKind.Error) }));
        }

        [Test, Description("Vérifie qu'un message du bus qui n'est pas une clé s'affiche tel quel — parce que certains cas d'usage publient un texte déjà rédigé")]
        public void PlainMessage_IsShownAsIs()
        {
            _app.Publisher<ToastRequested>().Publish(new ToastRequested("Sauvegarde terminée", ToastKind.Info));
            Assert.That(_toasts.Shown, Is.EqualTo(new[] { ("Sauvegarde terminée", ToastKind.Info) }));
        }

        [Test, Description("Vérifie qu'après Dispose plus aucun message n'est affiché — parce que le relais vit avec le scope racine et ne doit pas survivre à sa destruction")]
        public void AfterDispose_NothingShown()
        {
            _relay.Dispose();
            _app.Publisher<ToastRequested>().Publish(new ToastRequested("errors.navigation", ToastKind.Error));
            Assert.That(_toasts.Shown, Is.Empty);
        }
    }
}
