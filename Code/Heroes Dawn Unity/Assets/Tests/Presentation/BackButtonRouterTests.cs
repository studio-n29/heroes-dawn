using System.Threading;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class BackButtonRouterTests
    {
        sealed class Handler : IBackHandler
        {
            public int Calls;
            public bool HandleBack() { Calls++; return true; }
        }

        FakeModalHost _host = null!;
        ModalService _modals = null!;
        FakeBackInput _input = null!;
        BackButtonRouter _router = null!;

        [SetUp]
        public void SetUp()
        {
            _host = new FakeModalHost();
            _modals = new ModalService(_host);
            _input = new FakeBackInput();
            _router = new BackButtonRouter(_input, _modals);
        }

        [Test, Description("Vérifie que Retour avec une modale ouverte l'annule sans prévenir l'écran — parce que la modale du dessus passe avant l'écran (spec §7)")]
        public void Dispatch_ModalOpen_CancelsModalFirst()
        {
            var screen = new Handler();
            _router.Push(screen);
            var popup = new FakePopup { Cancelable = true };
            _host.Register<FakePopup>(() => popup);
            _modals.OpenAsync<FakePopup>(CancellationToken.None);

            Assert.That(_router.Dispatch(), Is.True);
            Assert.That(popup.CancelRequests, Is.EqualTo(1));
            Assert.That(screen.Calls, Is.Zero);
        }

        [Test, Description("Vérifie que sans modale, Retour va au dernier écran enregistré — parce que seul l'écran visible répond à Retour (spec §7)")]
        public void Dispatch_NoModal_CallsLastPushedHandler()
        {
            var below = new Handler();
            var top = new Handler();
            _router.Push(below);
            _router.Push(top);
            Assert.That(_router.Dispatch(), Is.True);
            Assert.That(top.Calls, Is.EqualTo(1));
            Assert.That(below.Calls, Is.Zero);
        }

        [Test, Description("Vérifie qu'un écran retiré ne reçoit plus Retour — parce qu'Échap en combat ne doit pas atteindre le plateau masqué (Review Focus 2)")]
        public void Dispatch_AfterRemove_SkipsRemovedHandler()
        {
            var board = new Handler();
            _router.Push(board);
            _router.Remove(board);
            Assert.That(_router.Dispatch(), Is.False);
            Assert.That(board.Calls, Is.Zero);
        }

        [Test, Description("Vérifie qu'aucune action n'a lieu sans appui sur Retour, et qu'un appui est relayé — parce que le routeur est appelé à chaque image")]
        public void Tick_WithoutPress_DoesNothing()
        {
            var screen = new Handler();
            _router.Push(screen);
            _router.Tick();
            Assert.That(screen.Calls, Is.Zero);
            _input.Pressed = true;
            _router.Tick();
            Assert.That(screen.Calls, Is.EqualTo(1));
        }
    }
}
