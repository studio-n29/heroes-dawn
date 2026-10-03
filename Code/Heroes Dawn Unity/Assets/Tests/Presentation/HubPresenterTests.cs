using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Shop;
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
        sealed class PendingRecruit : IRecruitFromShopUseCase
        {
            public int Calls;
            public readonly UniTaskCompletionSource<Result> Done = new UniTaskCompletionSource<Result>();
            public UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default) { Calls++; return Done.Task; }
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

        HubPresenter Make(IRecruitFromShopUseCase recruit = null, IResetProgressUseCase reset = null)
        {
            _p = new HubPresenter(_view, _app.Session, PrototypeContent.Zone11,
                                  _app.Hub.ToggleSquad, _app.Hub.Train, _app.Hub.Heal,
                                  _app.Shop.Open, recruit ?? _app.Shop.Recruit, _app.Shop.Reroll, _app.Time,
                                  _app.Zone.Start, _app.Account.SignOut, reset ?? _app.Account.Reset,
                                  _app.Subscriber<RosterChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<SaveStateChanged>(),
                                  _app.Subscriber<ShopChanged>(), _app.Publisher<NavigateRequested>(), new ModalService(_host), _toasts, new LocLocalization());
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

        void OnShopTab(DateTime local)
        {
            _app.Time.LocalNow = local;
            Make();
            _view.ClickTab(HubTab.Recruit);
        }

        static readonly DateTime Noon = new DateTime(2026, 10, 3, 12, 0, 0);

        [Test, Description("Vérifie que l'onglet Boutique montre les 3 héros du jour, le compte à rebours jusqu'à 9h00 et remonte la liste en haut — parce que la boutique se consulte depuis le QG (spec §5)")]
        public void ShopTab_ShowsOffers_WithCountdown()
        {
            OnShopTab(Noon);
            Assert.That(_view.Last.Tab, Is.EqualTo(HubTab.Recruit));
            Assert.That(_view.Last.Recruits.Length, Is.EqualTo(3));
            Assert.That(_view.Last.ShopCountdown, Is.EqualTo("Nouveaux héros dans 21 h 00"));
            Assert.That(_view.Scrolls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que sous une heure le compte à rebours s'affiche en minutes — parce que « dans 0 h 12 » se lit mal")]
        public void Countdown_UnderOneHour_ShowsMinutes()
        {
            OnShopTab(new DateTime(2026, 10, 3, 8, 48, 0));
            Assert.That(_view.Last.ShopCountdown, Is.EqualTo("Nouveaux héros dans 12 min"));
        }

        [Test, Description("Vérifie qu'un recrutement payable ajoute la recrue, l'annonce, et que sa carte passe à Recruté sans bouton actif — parce que l'emplacement reste visible jusqu'au prochain tirage (spec §3)")]
        public void RecruitedOffer_ShownAsRecruited()
        {
            _app.Session.Wallet.Set(_app.Session.Wallet.Credits, 5000);
            OnShopTab(Noon);
            var def = _app.Session.Content.Members.First(m => m.Id == _app.Session.Shop.Offers[0].Id);
            int roster = _app.Session.Roster.Count;
            _view.ClickRecruit(0);
            Assert.That(_app.Session.Roster.Count, Is.EqualTo(roster + 1));
            Assert.That(_toasts.Shown.Select(t => t.Text), Does.Contain(Loc.F("hub.recruited", Loc.MemberName(def))));
            var card = _view.Last.Recruits[0];
            Assert.That(card.Recruited, Is.True);
            Assert.That(card.RecruitEnabled, Is.False);
            Assert.That(card.StampText, Is.EqualTo(Loc.T("shop.recruited")));
            Assert.That(_view.Last.Recruits.Skip(1).All(c => !c.Recruited), Is.True);
        }

        [Test, Description("Vérifie qu'une boutique sans héros éligible affiche « collection complète » et désactive Renouveler — parce qu'il n'y a plus rien à tirer")]
        public void NoEligible_ShowsEmptyText()
        {
            foreach (var id in new[] { "arc", "rempart", "etincelle" })
                _app.Session.Roster.Add(new TeamMember(_app.Session.Content.Members.First(m => m.Id.Value == id), MetaRules.RecruitLevel));
            OnShopTab(Noon);
            Assert.That(_view.Last.Recruits, Is.Empty);
            Assert.That(_view.Last.EmptyRecruitText, Is.EqualTo(Loc.T("shop.empty")));
            Assert.That(_view.Last.RerollEnabled, Is.False);
        }

        [Test, Description("Vérifie que le renouvellement gratuit se fait sans confirmation — parce que seule une dépense demande confirmation (spec §5)")]
        public void Reroll_Free_NoConfirm()
        {
            OnShopTab(Noon);
            Assert.That(_view.Last.RerollLabel, Is.EqualTo(Loc.T("shop.reroll.free")));
            _view.ClickReroll();
            Assert.That(_host.Spawned, Is.Empty);
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'un renouvellement payant demande confirmation et ne débite qu'après acceptation — parce que toute dépense passe par la confirmation commune (spec §5)")]
        public void Reroll_Paid_AsksConfirmThenCharges()
        {
            _app.Session.Wallet.Set(1000, _app.Session.Wallet.Pr);
            OnShopTab(Noon);
            _view.ClickReroll();
            Assert.That(_view.Last.RerollLabel, Is.EqualTo(Loc.F("shop.reroll.cost", 100)));
            _view.ClickReroll();
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(1000));
            ((FakeConfirmPopup)_host.Spawned.Single()).Answer(true);
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(900));
            Assert.That(_app.Session.Shop.Draw, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que Renouveler est désactivé quand les Crédits ne suffisent pas — parce que le joueur ne doit pas lancer une action qu'il ne peut pas payer")]
        public void Reroll_NotEnoughCredits_ButtonDisabled()
        {
            OnShopTab(Noon);
            _view.ClickReroll();
            _app.Session.Wallet.Set(50, _app.Session.Wallet.Pr);
            _app.Publisher<WalletChanged>().Publish(new WalletChanged(50, _app.Session.Wallet.Pr));
            Assert.That(_view.Last.RerollEnabled, Is.False);
        }

        [Test, Description("Vérifie que deux clics rapides sur Recruter ne lancent qu'un achat — parce qu'un double clic ne doit pas acheter deux fois (spec §6)")]
        public void RecruitClick_Twice_RecruitsOnce()
        {
            var recruit = new PendingRecruit();
            _app.Time.LocalNow = Noon;
            Make(recruit);
            _view.ClickTab(HubTab.Recruit);
            _view.ClickRecruit(0);
            _view.ClickRecruit(0);
            Assert.That(recruit.Calls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie que la boutique se renouvelle d'elle-même à 9h00 si l'onglet est affiché — parce que le joueur peut rester sur l'écran au moment du renouvellement (spec §5)")]
        public void Tick_DayChangesWhileShown_RefreshesOffers()
        {
            OnShopTab(new DateTime(2026, 10, 3, 8, 59, 0));
            Assert.That(_app.Session.Shop.Day, Is.EqualTo("2026-10-02"));
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 9, 0, 0);
            _p.Tick();
            Assert.That(_app.Session.Shop.Day, Is.EqualTo("2026-10-03"));
            Assert.That(_view.Last.ShopCountdown, Is.EqualTo("Nouveaux héros dans 24 h 00"));
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
            _view.ClickTab(HubTab.Recruit);
            _view.ClickRecruit(0);
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
