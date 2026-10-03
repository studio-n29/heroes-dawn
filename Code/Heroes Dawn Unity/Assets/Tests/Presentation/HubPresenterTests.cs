using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class HubPresenterTests
    {
        sealed class PendingRecruit : IRecruitMemberUseCase
        {
            public readonly UniTaskCompletionSource<Result> Done = new UniTaskCompletionSource<Result>();
            public UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default) => Done.Task;
        }

        sealed class CountingReset : IResetProgressUseCase
        {
            public int Calls;
            public UniTask<Result> ExecuteAsync(CancellationToken ct = default) { Calls++; return UniTask.FromResult(Result.Ok()); }
        }

        TestApp _app = null!;
        FakeHubView _view = null!;
        FakeToasts _toasts = null!;
        FakeModalHost _host = null!;
        HubPresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeHubView();
            _toasts = new FakeToasts();
            _host = new FakeModalHost();
        }

        HubPresenter Make(IRecruitMemberUseCase recruit = null, IResetProgressUseCase reset = null)
        {
            _p = new HubPresenter(_view, _app.Session, PrototypeContent.Zone11,
                                  _app.Hub.ToggleSquad, _app.Hub.Train, recruit ?? _app.Hub.Recruit, _app.Hub.Heal,
                                  _app.Zone.Start, _app.Account.SignOut, reset ?? _app.Account.Reset,
                                  _app.Subscriber<RosterChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<SaveStateChanged>(),
                                  _app.Publisher<NavigateRequested>(), new ModalService(_host), _toasts, new LocLocalization());
            _view.Show();
            _p.Start();
            return _p;
        }

        [Test, Description("Vérifie qu'à l'affichage le QG montre une carte par membre de l'effectif, onglet Équipe — parce que c'est l'écran d'accueil du QG (UX §Hub)")]
        public void Shown_ShowsRosterOnTeamTab()
        {
            Make();
            Assert.That(_view.Last.Tab, Is.EqualTo(HubTab.Team));
            Assert.That(_view.Last.Team.Length, Is.EqualTo(_app.Session.Roster.Count));
            Assert.That(_view.WalletView.Last.Credits, Is.EqualTo(_app.Session.Wallet.Credits));
        }

        [Test, Description("Vérifie que l'onglet Recrutement liste les recrues disponibles et remonte la liste en haut — parce que la pool du jour se consulte depuis le QG (UX §Recrutement)")]
        public void RecruitTab_ListsAvailableRecruits()
        {
            Make();
            _view.ClickTab(HubTab.Recruit);
            Assert.That(_view.Last.Tab, Is.EqualTo(HubTab.Recruit));
            Assert.That(_view.Last.Recruits.Length, Is.EqualTo(_app.Session.AvailableRecruits().Count));
            Assert.That(_view.Scrolls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un recrutement payable ajoute la recrue, l'annonce et rafraîchit l'écran — parce que le recrutement se paie en PR et la recrue rejoint l'effectif")]
        public void Recruit_Success_ToastsAndRefreshes()
        {
            Make();
            var d = _app.Session.AvailableRecruits().First(m => MetaRules.RecruitCost(m.Rarity) <= _app.Session.Wallet.Pr);
            int roster = _app.Session.Roster.Count, shows = _view.Shown.Count;
            _view.ClickRecruit(d.Id);
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(roster + 1));
            Assert.That(_toasts.Shown.Select(t => t.Text), Does.Contain(Loc.F("hub.recruited", Loc.MemberName(d))));
            Assert.That(_view.Shown.Count, Is.GreaterThan(shows));
        }

        [Test, Description("Vérifie qu'un entraînement trop cher est désactivé sur la carte — parce que le joueur ne doit pas pouvoir lancer une action qu'il ne peut pas payer")]
        public void Train_NotEnoughCredits_CardDisabled()
        {
            _app.Session.Wallet.Set(0, _app.Session.Wallet.Pr);
            Make();
            Assert.That(_view.Last.Team.All(c => !c.TrainEnabled), Is.True);
        }

        [Test, Description("Vérifie que l'infirmerie est désactivée quand personne n'est fatigué — parce que soigner une escouade reposée coûterait des Crédits pour rien")]
        public void Heal_NoFatigue_Disabled()
        {
            Make();
            Assert.That(_app.Session.Roster.All(m => m.Fatigue == 0), Is.True);
            Assert.That(_view.Last.HealEnabled, Is.False);
        }

        [Test, Description("Vérifie que deux clics sur Déployer ne démarrent la zone et ne demandent le plateau qu'une fois — parce qu'un second déploiement relancerait la zone (correctif du sous-projet 2)")]
        public void Deploy_TwiceQuickly_StartsZoneOnceAndNavigatesOnce()
        {
            Make();
            _view.ClickDeploy();
            var run = _app.Session.Run;
            _view.ClickDeploy();
            Assert.That(_app.Session.Run, Is.SameAs(run));
            Assert.That(_app.Received<NavigateRequested>().Count(n => n.Target == GameState.Board), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un déploiement refusé affiche l'erreur et permet de réessayer — parce que la garde ne doit pas bloquer l'écran après un échec")]
        public void Deploy_Failure_ShowsErrorAndAllowsRetry()
        {
            foreach (var m in _app.Session.Roster) ((TeamMember)m).InSquad = false;
            Make();
            _view.ClickDeploy();
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Kind == ToastKind.Error));
            Assert.That(_app.Received<NavigateRequested>(), Is.Empty);

            foreach (var m in _app.Session.Roster) ((TeamMember)m).InSquad = true;
            _view.ClickDeploy();
            Assert.That(_app.Received<NavigateRequested>().Count, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un QG masqué ne se rafraîchit plus sur un changement de portefeuille — parce que ses abonnements suivent l'affichage")]
        public void Hidden_ThenWalletChanged_NoRefresh()
        {
            Make();
            _view.Hide();
            int shows = _view.Shown.Count;
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(1, 2));
            Assert.That(_view.Shown.Count, Is.EqualTo(shows));
        }

        [Test, Description("Vérifie qu'un présentateur détruit pendant un recrutement n'appelle plus la vue à la fin de l'appel — parce que la scène du QG peut être déchargée pendant un appel (Review Focus 4)")]
        public void Dispose_DuringRecruit_NoViewCallAfterwards()
        {
            var recruit = new PendingRecruit();
            Make(recruit);
            _view.ClickRecruit(_app.Session.AvailableRecruits()[0].Id);
            int shows = _view.Shown.Count;
            _p.Dispose();
            recruit.Done.TrySetResult(Result.Ok());
            Assert.That(_view.Shown.Count, Is.EqualTo(shows));
            Assert.That(_toasts.Shown, Is.Empty);
        }

        [Test, Description("Vérifie qu'effacer la sauvegarde demande une confirmation, puis l'efface une fois acceptée — parce que l'effacement est irréversible")]
        public void Reset_AsksConfirmationThenResets()
        {
            var reset = new CountingReset();
            Make(reset: reset);
            _view.ClickReset();
            Assert.That(reset.Calls, Is.Zero);
            ((FakeConfirmPopup)_host.Spawned[0]).Answer(true);
            Assert.That(reset.Calls, Is.EqualTo(1));
        }
    }
}
