using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Login;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class LoginPresenterTests
    {
        /// <summary>Connexion qui reste en cours tant que le test ne la termine pas, et compte ses appels.</summary>
        sealed class PendingSignIn : ISignInAsGuestUseCase
        {
            public int Calls;
            public readonly UniTaskCompletionSource<Result<SignInReport>> Done = new UniTaskCompletionSource<Result<SignInReport>>();
            public UniTask<Result<SignInReport>> ExecuteAsync(CancellationToken ct = default) { Calls++; return Done.Task; }
        }

        TestApp _app = null!;
        FakeLoginView _view = null!;
        FakeToasts _toasts = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _view = new FakeLoginView();
            _toasts = new FakeToasts();
        }

        LoginPresenter Make(ISignInAsGuestUseCase signIn)
        {
            var p = new LoginPresenter(_view, signIn, _app.Session, _app.Publisher<NavigateRequested>(), _toasts, new LocLocalization(), _app.Logger);
            _view.Show();
            p.Start();
            return p;
        }

        [Test, Description("Vérifie que deux clics rapides sur Jouer ne lancent qu'une connexion — parce qu'un double clic ne doit pas créer deux sessions (spec §6, garde commune)")]
        public void Play_TwiceQuickly_SignsInOnce()
        {
            var signIn = new PendingSignIn();
            Make(signIn);
            _view.ClickPlay();
            _view.ClickPlay();
            Assert.That(signIn.Calls, Is.EqualTo(1));
            Assert.That(_view.Last.Busy, Is.True);
        }

        [Test, Description("Vérifie qu'une connexion réussie souhaite la bienvenue et demande le QG — parce que la connexion mène au QG par la machine d'états (doc 07)")]
        public void Play_Success_NavigatesToHubAndWelcomes()
        {
            Make(_app.Account.SignIn);
            _view.ClickPlay();
            Assert.That(_app.Received<NavigateRequested>(), Has.Exactly(1).Matches<NavigateRequested>(n => n.Target == GameState.Hub));
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Text == Loc.F("login.welcome_new", "Invité-TEST")));
        }

        [Test, Description("Vérifie qu'un échec réseau affiche l'erreur et laisse rejouer — parce que le joueur doit pouvoir réessayer sans relancer le jeu")]
        public void Play_Failure_ShowsErrorAndReleasesGate()
        {
            _app.Auth.ThrowOnSignIn = true;
            Make(_app.Account.SignIn);
            _view.ClickPlay();
            Assert.That(_view.Last.Busy, Is.False);
            Assert.That(_view.Last.Status, Is.EqualTo(Loc.T("errors.network")));
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error));

            _app.Auth.ThrowOnSignIn = false;
            _view.ClickPlay();
            Assert.That(_app.Received<NavigateRequested>().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'à l'affichage l'écran n'est plus occupé et joue son intro — parce qu'un retour à la connexion (déconnexion) doit rendre le bouton Jouer")]
        public void Shown_ResetsBusyAndPlaysIntro()
        {
            var signIn = new PendingSignIn();
            Make(signIn);
            _view.ClickPlay();
            _view.Hide();
            _view.Show();
            Assert.That(_view.Last, Is.EqualTo((false, "")));
            Assert.That(_view.Intros, Is.EqualTo(2));
            _view.ClickPlay();
            Assert.That(signIn.Calls, Is.EqualTo(2));
        }
    }
}
