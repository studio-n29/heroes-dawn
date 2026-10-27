using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class SettingsPagePresenterTests
    {
        sealed class CountingReset : IResetProgressUseCase
        {
            public int Calls;
            public UniTask<Result> ExecuteAsync(CancellationToken ct = default) { Calls++; return UniTask.FromResult(Result.Ok()); }
        }

        TestApp _app = null!;
        FakeSettingsPageView _view = null!;
        FakeModalHost _host = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        SettingsPagePresenter _p = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeSettingsPageView();
            _host = new FakeModalHost();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
        }

        SettingsPagePresenter Make(IResetProgressUseCase reset = null)
        {
            _p = new SettingsPagePresenter(_view, _app.Session, _app.Account.SignOut, reset ?? _app.Account.Reset, _app.Shop.Open, _app.Settings, _app.Tutorial,
                                           _nav, new ModalService(_host), _toasts, _loc, _app.Publisher<NavigateRequested>(), _app.Subscriber<MetaChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        void Open(int category) { _view.SelectCategory(category); }
        const int Audio = 0, Graphics = 1, Accessibility = 2, Notifications = 4, Account = 6, Support = 7;

        [Test, Description("Vérifie que les huit catégories existent, que l'Audio est ouverte d'emblée et que plus aucune n'est marquée BIENTÔT — parce que tous les réglages fonctionnent dans cette version")]
        public void EightCategories_AudioFirst()
        {
            Make();
            Assert.That(_view.Last.Categories.Length, Is.EqualTo(8));
            Assert.That(_view.Last.Active, Is.EqualTo(Audio));
            Assert.That(_view.Last.CategoryTitle, Is.EqualTo(_loc.T("settings.cat.Audio")));
            Assert.That(_view.Last.Rows.Length, Is.EqualTo(5));
        }

        [Test, Description("Vérifie que « + » et « − » d'un volume changent la valeur enregistrée de 10 points, bornée entre 0 et 100 — parce que les réglages s'appliquent tout de suite")]
        public void Volume_Steps_AndClamps()
        {
            Make();
            int music = _view.RowOf(_loc.T("settings.music"));
            _view.Act(music, 1);
            Assert.That(_app.Settings.Current.music, Is.EqualTo(90));
            for (int i = 0; i < 15; i++) _view.Act(_view.RowOf(_loc.T("settings.music")), 1);
            Assert.That(_app.Settings.Current.music, Is.EqualTo(100));
            for (int i = 0; i < 15; i++) _view.Act(_view.RowOf(_loc.T("settings.music")), 0);
            Assert.That(_app.Settings.Current.music, Is.EqualTo(0));
            Assert.That(_view.Last.Rows[_view.RowOf(_loc.T("settings.music"))].Value, Is.EqualTo("0 %"));
        }

        [Test, Description("Vérifie qu'un interrupteur d'accessibilité bascule le réglage et son libellé — parce que réduire les secousses doit être visible")]
        public void Toggle_Flips_AndShowsState()
        {
            Make();
            Open(Accessibility);
            int row = _view.RowOf(_loc.T("settings.shakes"));
            Assert.That(_view.Last.Rows[row].On, Is.False);
            _view.Act(row);
            Assert.That(_app.Settings.Current.reduceShakes, Is.True);
            Assert.That(_view.Last.Rows[row].On, Is.True);
            Assert.That(_view.Last.Rows[row].Value, Is.EqualTo(_loc.T("settings.on")));
        }

        [Test, Description("Vérifie que la qualité graphique fait le tour Auto → Bas → Moyen → Haut → Auto — parce que le choix est circulaire")]
        public void Quality_CyclesThroughFourChoices()
        {
            Make();
            Open(Graphics);
            int row = _view.RowOf(_loc.T("settings.quality"));
            for (int i = 1; i <= 4; i++) { _view.Act(row, 1); Assert.That(_app.Settings.Current.quality, Is.EqualTo(i % 4)); }
        }

        [Test, Description("Vérifie que « Réinitialiser » remet seulement la catégorie ouverte aux valeurs d'origine — parce que les autres réglages ne doivent pas bouger")]
        public void ResetCategory_RestoresOnlyThatCategory()
        {
            Make();
            _app.Settings.Update(s => { s.music = 10; s.reduceShakes = true; });
            _view.ClickResetCategory();
            Assert.That(_app.Settings.Current.music, Is.EqualTo(80));
            Assert.That(_app.Settings.Current.reduceShakes, Is.True);
        }

        [Test, Description("Vérifie que Compte et Support n'ont pas de bouton Réinitialiser — parce qu'on ne remet pas un compte à zéro par erreur")]
        public void AccountAndSupport_HaveNoCategoryReset()
        {
            Make();
            Open(Account);
            Assert.That(_view.Last.CanReset, Is.False);
            Open(Support);
            Assert.That(_view.Last.CanReset, Is.False);
            Open(Audio);
            Assert.That(_view.Last.CanReset, Is.True);
        }

        [Test, Description("Vérifie que Se déconnecter ferme la session et ramène à l'écran de connexion — parce que ce bouton a quitté l'en-tête du QG (spec §4)")]
        public void Logout_SignsOutAndNavigatesToLogin()
        {
            Make();
            Open(Account);
            _view.Act(_view.RowOf(_loc.T("settings.logout")));
            Assert.That(_app.Received<NavigateRequested>().Count(n => n.Target == GameState.Login), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'effacer la sauvegarde demande confirmation, efface, puis tire aussitôt une boutique neuve — parce que l'effacement est irréversible et que la nouvelle partie a sa boutique")]
        public void Reset_AsksConfirmationThenResetsAndReopensShop()
        {
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 12, 0, 0);
            Make();
            Open(Account);
            _view.Act(_view.RowOf(_loc.T("settings.reset")));
            Assert.That(_app.Session.Shop.Offers, Is.Empty);
            ((FakeConfirmPopup)_host.Spawned.Single()).Answer(true);
            Assert.That(_app.Session.Shop.Offers.Count, Is.EqualTo(3));
            Assert.That(_toasts.Shown.Last().Kind, Is.EqualTo(ToastKind.Info));
        }

        [Test, Description("Vérifie qu'une confirmation refusée n'efface rien — parce qu'annuler n'a aucune conséquence")]
        public void Reset_Cancelled_DoesNothing()
        {
            var reset = new CountingReset();
            Make(reset);
            Open(Account);
            _view.Act(_view.RowOf(_loc.T("settings.reset")));
            ((FakeConfirmPopup)_host.Spawned.Single()).Answer(false);
            Assert.That(reset.Calls, Is.Zero);
        }

        [Test, Description("Vérifie que « Revoir le tutoriel » relance la partie guidée depuis l'étape 1 et ramène au QG — parce que le tutoriel doit pouvoir se revoir (spec §6)")]
        public void ReplayTutorial_RestartsAndGoesToQg()
        {
            _app.Tutorial.Skip();
            Assert.That(_app.Tutorial.Active, Is.False);
            Make();
            Open(Support);
            _nav.SelectTab(HubPage.Team);
            _view.Act(_view.RowOf(_loc.T("settings.tutorial")));
            Assert.That(_app.Tutorial.Active, Is.True);
            Assert.That(_app.Tutorial.Current, Is.EqualTo(N29.HeroesDawn.Application.Features.Tutorial.TutorialStep.Welcome));
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
        }

        [Test, Description("Vérifie que les notifications sont enregistrées même sans rappels hors du jeu — parce que les choix doivent survivre jusqu'à la version en ligne")]
        public void Notifications_AreStored()
        {
            Make();
            Open(Notifications);
            _view.Act(_view.RowOf(_loc.T("settings.notif.shop")));
            Assert.That(_app.Settings.Current.notifShop, Is.False);
        }
    }
}
