using System.Threading;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class PresenterBaseTests
    {
        sealed class Probe : PresenterBase<FakeView>, IBackHandler
        {
            public int ShownCalls;
            public CancellationToken Token => Lifetime;
            public Probe(FakeView v, BackButtonRouter r) : base(v, r) { }
            protected override IBackHandler BackHandler => this;
            protected override void OnShown() => ShownCalls++;
            public bool HandleBack() => true;
        }

        FakeView _view = null!;
        BackButtonRouter _router = null!;
        Probe _p = null!;

        [SetUp]
        public void SetUp()
        {
            _view = new FakeView();
            _router = new BackButtonRouter(new FakeBackInput(), new ModalService(new FakeModalHost()));
            _p = new Probe(_view, _router);
        }

        [Test, Description("Vérifie qu'un présentateur démarré sur une vue déjà visible se rafraîchit aussitôt — parce que la vue d'une scène est instanciée active avant le démarrage du présentateur")]
        public void Start_WhenViewVisible_CallsOnShown()
        {
            _view.Show();
            _p.Start();
            Assert.That(_p.ShownCalls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que masquer la vue retire son gestionnaire Retour — parce que le plateau masqué pendant le combat ne doit plus répondre à Échap (Review Focus 2)")]
        public void Hidden_RemovesBackHandler()
        {
            _p.Start();
            _view.Show();
            Assert.That(_router.Dispatch(), Is.True);
            _view.Hide();
            Assert.That(_router.Dispatch(), Is.False);
        }

        [Test, Description("Vérifie que Dispose annule le jeton de vie du présentateur — parce qu'un appel asynchrone en cours doit s'arrêter quand la scène est déchargée (Review Focus 4)")]
        public void Dispose_CancelsLifetimeToken()
        {
            _p.Start();
            var token = _p.Token;
            _p.Dispose();
            Assert.That(token.IsCancellationRequested, Is.True);
        }

        [Test, Description("Vérifie qu'après Dispose, un affichage de la vue n'appelle plus le présentateur — parce qu'un présentateur détruit ne doit plus toucher sa vue (Review Focus 4)")]
        public void Dispose_ThenShown_DoesNotCallOnShown()
        {
            _p.Start();
            _p.Dispose();
            _view.Show();
            Assert.That(_p.ShownCalls, Is.Zero);
        }
    }
}
