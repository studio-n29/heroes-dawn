using System.Linq;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Page Tournoi : classement, manches du jour, bouton du bandeau (L13 de la 0.15.0).</summary>
    public sealed class TournamentPageTests
    {
        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        readonly LocLocalization _loc = new LocLocalization();

        TournamentPagePresenter Page()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _toasts = new FakeToasts();
            var p = new TournamentPagePresenter(_view, _app.Tournament, _loc, _toasts, _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(),
                                                _app.Subscriber<RosterChanged>(), _app.Time);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que la page montre le classement de dix joueurs avec le joueur en évidence, les trois manches du jour et le bouton de la première manche — parce que c'est l'écran de départ du tournoi")]
        public void Start_ShowsBoardRoundsAndTheFirstMatchButton()
        {
            var p = Page();
            Assert.That(_view.Rows.Length, Is.EqualTo(Tournament.BoardSize));
            Assert.That(_view.Rows.Count(r => r.Highlight), Is.EqualTo(1));
            Assert.That(_view.Rows.First(r => r.Highlight).Title, Is.EqualTo(_loc.T("tournament.you")));
            Assert.That(_view.Side.Rows.Length, Is.EqualTo(Tournament.Rounds + 1));
            Assert.That(_view.Header.A.Label, Is.EqualTo(_loc.F("tournament.play", _loc.T("tournament.round.quarter"))));
            Assert.That(_view.Header.A.Enabled, Is.True);
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un match joué annonce son issue, fait avancer la page et que le tournoi fini propose la récompense — parce que le joueur doit voir où il en est")]
        public void Playing_ToTheEnd_ThenClaiming()
        {
            var p = Page();
            int guard = 0;
            while (!_app.Tournament.Finished && guard++ < 5)
            {
                int toasts = _toasts.Shown.Count;
                _view.ClickHeader(0);
                Assert.That(_toasts.Shown.Count, Is.EqualTo(toasts + 1));
            }
            Assert.That(_app.Tournament.Finished, Is.True);
            Assert.That(_view.Header.A.Label, Is.EqualTo(_loc.T("tournament.claim")));
            int credits = _app.Session.Wallet.Credits;
            _view.ClickHeader(0);
            Assert.That(_app.Session.Wallet.Credits, Is.GreaterThan(credits));
            Assert.That(_view.Header.A.Label, Is.EqualTo(_loc.T("tournament.tomorrow")));
            Assert.That(_view.Header.A.Enabled, Is.False);
            p.Dispose();
        }
    }
}
