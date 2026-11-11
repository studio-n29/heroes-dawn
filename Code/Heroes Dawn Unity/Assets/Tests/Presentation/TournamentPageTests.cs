using N29.HeroesDawn.Application.Features.Meta;
using System.Linq;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Page Tournoi (version minimale du bloc D) : tournoi, palier, bouton de réclamation.</summary>
    public sealed class TournamentPageTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        readonly LocLocalization _loc = new LocLocalization();

        TournamentPagePresenter Page(bool signed = true)
        {
            _app = new TestApp();
            if (signed) _app.SignInDirect("me");
            _app.Time.LocalNow = new System.DateTime(2026, 12, 10, 12, 0, 0);
            _app.Time.UtcNow = new System.DateTime(2026, 12, 10, 12, 0, 0, System.DateTimeKind.Utc);
            _view = new FakeListView();
            _toasts = new FakeToasts();
            var p = new TournamentPagePresenter(_view, _app.Tournament, _loc, _toasts, _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(),
                                                _app.Subscriber<RosterChanged>(), _app.Time);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que la page montre le nom du tournoi en cours et un bouton de réclamation grisé tant qu'aucun palier n'est atteint — parce que c'est l'écran de départ du tournoi")]
        public void Start_ShowsTheTournamentAndADisabledClaimButton()
        {
            var p = Page();
            Assert.That(_view.Header.Title, Is.EqualTo(_loc.T("tournament.christmas")));
            Assert.That(_view.Header.A.Enabled, Is.False);
            p.Dispose();
        }

        [Test, Description("Vérifie que la page invite à se connecter quand il n'y a pas de compte — parce que le tournoi demande un compte")]
        public void WithoutAccount_AsksToSignIn()
        {
            var p = Page(signed: false);
            Assert.That(_view.Header.Line, Is.EqualTo(_loc.T("tournament.sign_in")));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton du bandeau réclame le premier palier atteint et verse des Crédits — parce que c'est le seul geste du joueur sur cette page pour l'instant")]
        public void ClaimButton_ClaimsTheFirstReachedTier()
        {
            var p = Page();
            _app.Tournament.Current();
            _app.Session.Meta.arena.points = 60;
            _app.Arena.PlayerId = "me";
            _app.Arena.SetPoints("christmas:2026", "me", 60);
            _app.MetaCtx.Touch(N29.HeroesDawn.Application.Features.Meta.MetaArea.Profile);
            Assert.That(_view.Header.A.Enabled, Is.True);
            int credits = _app.Session.Wallet.Credits;
            _view.ClickHeader(0);
            Assert.That(_app.Session.Wallet.Credits, Is.GreaterThan(credits));
            p.Dispose();
        }
    }
}
