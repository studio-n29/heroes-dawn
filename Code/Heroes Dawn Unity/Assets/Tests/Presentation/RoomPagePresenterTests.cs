using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Flow;
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
    public sealed class SalonPagePresenterTests
    {
        TestApp _app = null!;
        FakeSalonPageView _view = null!;
        FakeToasts _toasts = null!;
        SalonPagePresenter _p = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeSalonPageView();
            _toasts = new FakeToasts();
        }

        SalonPagePresenter Make(ITrainMemberUseCase train = null)
        {
            _p = new SalonPagePresenter(_view, _app.Session, train ?? _app.Hub.Train, _toasts, _loc,
                                        _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        SalonRowData Row(string id) => _view.Last.Rows.Single(r => r.Id == new MemberId(id));

        [Test, Description("Vérifie que le Salon liste les membres du plus bas niveau au plus haut — parce qu'on entraîne d'abord les retardataires")]
        public void MembersSortedByLevel()
        {
            _app.Session.Find(new MemberId("ancre"))!.Level = 9;
            _app.Session.Find(new MemberId("echo"))!.Level = 3;
            Make();
            var ids = _view.Last.Rows.Select(r => r.Id.Value).ToArray();
            Assert.That(ids.First(), Is.EqualTo("echo"));
            Assert.That(ids.Last(), Is.EqualTo("ancre"));
            Assert.That(_p.Title, Is.EqualTo(_loc.T("page.salon")));
        }

        [Test, Description("Vérifie qu'entraîner débite, monte le niveau et rafraîchit la ligne — parce que le Salon est l'entraînement en version simple (spec §4)")]
        public void Train_ChargesAndRefreshes()
        {
            Make();
            Assert.That(Row("givre").Label, Is.EqualTo("ENTRAÎNER · 500 CR"));
            _view.ClickTrain(new MemberId("givre"));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(300));
            Assert.That(Row("givre").Card.LevelText, Is.EqualTo("Niv. 6"));
            Assert.That(_toasts.Shown.Last().Kind, Is.EqualTo(ToastKind.Reward));
        }

        [Test, Description("Vérifie que sans assez de Crédits la ligne est grisée et dit ce qui manque — parce qu'un bouton désactivé doit dire pourquoi (contraintes)")]
        public void Train_NotEnoughCredits_DisabledWithReason()
        {
            _app.Session.Wallet.Set(100, 0);
            Make();
            Assert.That(Row("givre").Enabled, Is.False);
            Assert.That(Row("givre").Reason, Is.EqualTo("Il manque 400 CR"));
        }

        [Test, Description("Vérifie qu'une page détruite pendant un entraînement n'appelle plus la vue ni les toasts (Review Focus 3)")]
        public void Dispose_DuringTrain_NoViewCallAfterwards()
        {
            var pending = new PendingTrain();
            Make(pending);
            _view.ClickTrain(new MemberId("givre"));
            int shows = _view.Shown.Count;
            _p.Dispose();
            pending.Done.TrySetResult(Result.Ok());
            Assert.That(_view.Shown.Count, Is.EqualTo(shows));
            Assert.That(_toasts.Shown, Is.Empty);
        }

        [Test, Description("Vérifie que la page annonce la version simple — parce que le Salon complet (durée, créneaux) viendra plus tard (spec §4)")]
        public void MarkedSimpleVersion()
        {
            Make();
            Assert.That(_view.Last.Banner, Is.EqualTo(_loc.T("salon.simple")));
        }
    }

    public sealed class InfirmaryPagePresenterTests
    {
        TestApp _app = null!;
        FakeInfirmaryPageView _view = null!;
        FakeToasts _toasts = null!;
        InfirmaryPagePresenter _p = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeInfirmaryPageView();
            _toasts = new FakeToasts();
        }

        InfirmaryPagePresenter Make()
        {
            _p = new InfirmaryPagePresenter(_view, _app.Session, _app.Hub.Heal, _toasts, _loc,
                                            _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        void Tire(string id, int fatigue) => _app.Session.Find(new MemberId(id))!.Fatigue = fatigue;

        [Test, Description("Vérifie que l'Infirmerie liste les membres du plus fatigué au moins fatigué — parce qu'on soigne d'abord les plus atteints")]
        public void MembersSortedByFatigueDescending()
        {
            Tire("echo", 30);
            Tire("seve", 80);
            Make();
            Assert.That(_view.Last.Rows.Select(r => r.Id.Value).Take(2), Is.EqualTo(new[] { "seve", "echo" }));
            Assert.That(_p.Title, Is.EqualTo(_loc.T("page.infirmary")));
        }

        [Test, Description("Vérifie les paliers de fatigue (en forme sous 60, fatigué au-dessus, épuisé à 100), toujours écrits — parce que la couleur ne porte jamais seule le sens (contraintes)")]
        public void FatigueBands_LabelledGreenBelow60OrangeAboveRedAt100()
        {
            Tire("ancre", 59);
            Tire("givre", 60);
            Tire("echo", 100);
            Make();
            InfirmaryRowData Row(string id) => _view.Last.Rows.Single(r => r.Id.Value == id);
            Assert.That((Row("ancre").Band, Row("ancre").BandText), Is.EqualTo((FatigueBand.Fit, _loc.T("infirmary.band.fit"))));
            Assert.That((Row("givre").Band, Row("givre").BandText), Is.EqualTo((FatigueBand.Tired, _loc.T("infirmary.band.tired"))));
            Assert.That((Row("echo").Band, Row("echo").BandText), Is.EqualTo((FatigueBand.Exhausted, _loc.T("infirmary.band.exhausted"))));
            Assert.That(Row("seve").FatigueText, Is.EqualTo("Fatigue 0 %"));
        }

        [Test, Description("Vérifie que soigner débite 150 CR et retire 30 points de fatigue à tous — parce que c'est l'infirmerie en version simple (spec §4)")]
        public void Heal_ChargesAndReducesFatigue()
        {
            Tire("echo", 70);
            Make();
            Assert.That(_view.Last.HealLabel, Is.EqualTo(_loc.F("hub.heal", "150 CR")));
            Assert.That(_view.Last.HealEnabled, Is.True);
            _view.ClickHeal();
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(650));
            Assert.That(_app.Session.Find(new MemberId("echo"))!.Fatigue, Is.EqualTo(40));
            Assert.That(_toasts.Shown.Last(), Is.EqualTo((_loc.T("hub.healed"), ToastKind.Reward)));
            Assert.That(_view.Last.Rows.Single(r => r.Id.Value == "echo").Fatigue, Is.EqualTo(40));
        }

        [Test, Description("Vérifie que sans fatigue le soin est grisé avec la raison — parce qu'il n'y a rien à soigner")]
        public void Heal_NoFatigue_Disabled()
        {
            Make();
            Assert.That(_view.Last.HealEnabled, Is.False);
            Assert.That(_view.Last.HealReason, Is.EqualTo(_loc.T("qg.infirmary.none")));
        }

        [Test, Description("Vérifie que sans assez de Crédits le soin est grisé et dit ce qui manque — parce qu'un bouton désactivé doit dire pourquoi")]
        public void Heal_NotEnoughCredits_DisabledWithReason()
        {
            Tire("echo", 70);
            _app.Session.Wallet.Set(100, 0);
            Make();
            Assert.That(_view.Last.HealEnabled, Is.False);
            Assert.That(_view.Last.HealReason, Is.EqualTo("Il manque 50 CR"));
        }
    }

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
        }

        SettingsPagePresenter Make(IResetProgressUseCase reset = null)
        {
            _p = new SettingsPagePresenter(_view, _app.Session, _app.Account.SignOut, reset ?? _app.Account.Reset, _app.Shop.Open,
                                           new ModalService(_host), _toasts, _loc, _app.Publisher<NavigateRequested>());
            _view.Show();
            _p.Start();
            return _p;
        }

        [Test, Description("Vérifie que la catégorie Compte est active et que les cinq autres sont marquées BIENTÔT — parce que seul le compte fonctionne dans ce lot (spec §4)")]
        public void CompteCategory_Functional_OthersWip()
        {
            Make();
            var cats = _view.Last.Categories;
            Assert.That(cats.Length, Is.EqualTo(6));
            Assert.That(cats[0].Title, Is.EqualTo(_loc.T("settings.account")));
            Assert.That(cats[0].WipId, Is.Null);
            Assert.That(cats.Skip(1).Select(c => c.WipId), Is.EqualTo(new[]
            {
                WipCatalog.SettingsAudio.Id, WipCatalog.SettingsGraphics.Id, WipCatalog.SettingsAccessibility.Id,
                WipCatalog.SettingsNotifications.Id, WipCatalog.SettingsControls.Id,
            }));
            Assert.That(_view.Last.PlayerLine, Does.Contain(_app.Session.Account!.DisplayName));
        }

        [Test, Description("Vérifie que Se déconnecter ferme la session et ramène à l'écran de connexion — parce que ce bouton a quitté l'en-tête du QG (spec §4)")]
        public void Logout_SignsOutAndNavigatesToLogin()
        {
            Make();
            _view.ClickLogout();
            Assert.That(_app.Received<NavigateRequested>().Count(n => n.Target == GameState.Login), Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'effacer la sauvegarde demande confirmation, efface, puis tire aussitôt une boutique neuve — parce que l'effacement est irréversible et que la nouvelle partie a sa boutique")]
        public void Reset_AsksConfirmationThenResetsAndReopensShop()
        {
            _app.Time.LocalNow = new DateTime(2026, 10, 3, 12, 0, 0);
            Make();
            _view.ClickReset();
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
            _view.ClickReset();
            ((FakeConfirmPopup)_host.Spawned.Single()).Answer(false);
            Assert.That(reset.Calls, Is.Zero);
        }
    }
}
