using System;
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
    /// <summary>Écran Tournoi (planches 13.1 et 13.2) : classement, cinq paliers à la place des manches, état, réclamation.</summary>
    public sealed class TournamentPageTests
    {
        TestApp _app = null!;
        FakeTournamentPageView _view = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        readonly LocLocalization _loc = new LocLocalization();

        TournamentPagePresenter Page(bool signed = true, DateTime? today = null)
        {
            _app = new TestApp();
            if (signed) _app.SignInDirect("me");
            var now = today ?? new DateTime(2026, 12, 10, 12, 0, 0);
            _app.Time.LocalNow = now;
            _app.Time.UtcNow = DateTime.SpecifyKind(now, DateTimeKind.Utc);
            _view = new FakeTournamentPageView();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
            var p = new TournamentPagePresenter(_view, _app.Tournament, _loc, _toasts, _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(),
                                                _app.Subscriber<RosterChanged>(), _app.Time, _nav);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        void Reach(int points)
        {
            _app.Tournament.Current();
            _app.Session.Meta.arena.points = points;
            _app.Arena.PlayerId = "me";
            _app.Arena.SetPoints("christmas:2026", "me", points);
            _app.MetaCtx.Touch(MetaArea.Profile);
        }

        [Test, Description("Vérifie que l'écran montre le nom du tournoi en cours avec l'état « En cours » — parce que c'est l'en-tête de la planche 13.1")]
        public void Header_ShowsTheActiveTournamentAndItsState()
        {
            var p = Page();
            Assert.That(_view.Last.Title, Is.EqualTo(_loc.T("tournament.christmas")));
            Assert.That(_view.Last.StateLabel, Is.EqualTo(_loc.T("tournament.state.active")));
            Assert.That(_view.Last.Finished, Is.False);
            p.Dispose();
        }

        [Test, Description("Vérifie que les cinq pastilles de palier remplacent les manches : aucune validée au départ, la première en cours — parce que la progression du tournoi se lit en paliers de points")]
        public void Tiers_AtStart_FirstIsCurrentOthersLocked()
        {
            var p = Page();
            Assert.That(_view.Last.Tiers.Length, Is.EqualTo(5));
            Assert.That(_view.Last.Tiers.Select(t => t.State), Is.EqualTo(new[] { TournamentTierState.Current, TournamentTierState.Locked, TournamentTierState.Locked, TournamentTierState.Locked, TournamentTierState.Locked }));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'à 150 points les deux premiers paliers sont validés et le troisième est en cours — parce que les pastilles suivent le palier atteint")]
        public void Tiers_At150Points_TwoDoneThirdCurrent()
        {
            var p = Page();
            Reach(150);
            Assert.That(_view.Last.Tiers.Select(t => t.State), Is.EqualTo(new[] { TournamentTierState.Done, TournamentTierState.Done, TournamentTierState.Current, TournamentTierState.Locked, TournamentTierState.Locked }));
            Assert.That(_view.Last.StatusMain, Is.EqualTo(_loc.F("tournament.tier_status", 2, 5)));
            p.Dispose();
        }

        [Test, Description("Vérifie que l'état indique les points, le prochain palier et les duels restants aujourd'hui — parce que le joueur doit savoir ce qu'il lui reste à jouer")]
        public void Status_ShowsPointsNextTierAndDuelsLeft()
        {
            var p = Page();
            Reach(60);
            Assert.That(_view.Last.StatusSub, Does.Contain("60"));
            Assert.That(_view.Last.StatusSub, Does.Contain("150"));
            Assert.That(_view.Last.StatusSub, Does.Contain(_loc.F("tournament.duels_left", 10)));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton Réclamer est grisé avec sa raison tant qu'aucun palier n'est atteint, puis actif et qu'il verse des Crédits — parce que réclamer est le geste du joueur sur cet écran")]
        public void Claim_IsDisabledUntilATierIsReached_ThenPaysCredits()
        {
            var p = Page();
            Assert.That(_view.Last.CanClaim, Is.False);
            Assert.That(_view.Last.ClaimReason, Is.EqualTo(_loc.T("tournament.nothing_to_claim")));
            Reach(60);
            Assert.That(_view.Last.CanClaim, Is.True);
            int credits = _app.Session.Wallet.Credits;
            _view.ClickClaim();
            Assert.That(_app.Session.Wallet.Credits, Is.GreaterThan(credits));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton de duel est grisé avec la mention « Bientôt » — parce que le lancement d'un duel n'est pas encore branché dans l'interface")]
        public void DuelButton_IsDisabled_WithASoonReason()
        {
            var p = Page();
            Assert.That(_view.Last.CanDuel, Is.False);
            Assert.That(_view.Last.DuelReason, Is.EqualTo(_loc.T("tournament.duel_soon")));
            p.Dispose();
        }

        [Test, Description("Vérifie que sans compte l'écran invite à se connecter et ne propose ni classement ni réclamation — parce que le tournoi demande un compte")]
        public void WithoutAccount_AsksToSignIn()
        {
            var p = Page(signed: false);
            Assert.That(_view.Last.StatusSub, Is.EqualTo(_loc.T("tournament.sign_in")));
            Assert.That(_view.Last.Ranking, Is.Empty);
            Assert.That(_view.Last.CanClaim, Is.False);
            p.Dispose();
        }

        [Test, Description("Vérifie qu'entre deux tournois l'écran passe à l'état « Terminé » avec le prochain tournoi annoncé et un bouton Retour au QG qui ouvre le QG — parce que c'est la planche 13.2")]
        public void BetweenTournaments_ShowsFinishedStateAndBackToQg()
        {
            var p = Page(today: new DateTime(2026, 5, 10, 12, 0, 0));
            Assert.That(_view.Last.Finished, Is.True);
            Assert.That(_view.Last.StateLabel, Is.EqualTo(_loc.T("tournament.state.upcoming")));
            Assert.That(_view.Last.StatusSub, Does.Contain(_loc.T("tournament.summer")));
            _nav.Open(HubPage.Profile);
            _view.ClickBackToQg();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
            p.Dispose();
        }

        [Test, Description("Vérifie que le classement affiché reprend les lignes de la passerelle (rang, nom, points) et marque celle du joueur — parce que c'est le panneau de gauche des planches")]
        public void Ranking_ShowsGatewayRows_AndMarksThePlayer()
        {
            var p = Page();
            _app.Arena.PlayerId = "me";
            _app.Arena.SetPoints("christmas:2026", "me", 60);
            _app.Arena.SetPoints("christmas:2026", "other", 90);
            _view.Hide();
            _view.Show();   // la page rechargé le classement à chaque affichage
            var rows = _view.Last.Ranking;
            Assert.That(rows.Length, Is.GreaterThanOrEqualTo(1), "le classement n'est pas vide");
            Assert.That(rows.Select(r => r.Rank), Is.Ordered.Ascending);
            Assert.That(rows.Count(r => r.IsPlayer), Is.EqualTo(1));
            p.Dispose();
        }
    }
}
