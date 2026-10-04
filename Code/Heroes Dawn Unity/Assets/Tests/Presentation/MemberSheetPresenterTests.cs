using System.Linq;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class MemberSheetPresenterTests
    {
        TestApp _app = null!;
        FakeMemberSheetView _view = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        MemberSheetPresenter _p = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeMemberSheetView();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
        }

        MemberSheetPresenter Make(string id = "givre", ITrainMemberUseCase train = null)
        {
            _nav.Open(HubPage.MemberSheet, new MemberId(id));
            _p = new MemberSheetPresenter(_view, _nav.Current, _app.Session, train ?? _app.Hub.Train, _nav, _toasts, _loc,
                                          _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>());
            _view.Show();
            _p.Start();
            return _p;
        }

        [Test, Description("Vérifie que la fiche montre stats, compétences décrites et combos de la famille — parce que c'est le détail d'un membre (M21)")]
        public void ShowsStatsSkillsAndCombos()
        {
            Make();
            var d = _view.Last;
            Assert.That(d.Card.Id, Is.EqualTo(new MemberId("givre")));
            Assert.That(d.Card.LevelText, Is.EqualTo("Niv. 5"));
            Assert.That(d.Card.Stats, Does.StartWith("PV "));
            var givre = _app.Session.Content.Member(new MemberId("givre"));
            Assert.That(d.Skills.Select(s => s.Name), Is.EqualTo(givre.Skills.Select(_loc.SkillName)));
            Assert.That(d.Skills[0].Description, Is.EqualTo(_loc.DescribeSkill(givre.Skills[0])));
            Assert.That(d.Combos.Length, Is.EqualTo(4));
            Assert.That(d.Combos.Any(c => c.StartsWith("Point faible")), Is.True);
            Assert.That(d.Owned, Is.True);
            Assert.That(d.ShowArrows && d.ShowRooms, Is.True);
        }

        [Test, Description("Vérifie que les flèches parcourent l'effectif en boucle — parce que la fiche permet de passer d'un membre à l'autre (M21 ①)")]
        public void NextPrevious_CycleThroughRoster()
        {
            Make("givre");
            _view.ClickNext();
            Assert.That(_view.Last.Card.Id.Value, Is.EqualTo("echo"));
            _view.ClickPrevious();
            _view.ClickPrevious();
            Assert.That(_view.Last.Card.Id.Value, Is.EqualTo("ancre"));
            _view.ClickPrevious();
            Assert.That(_view.Last.Card.Id.Value, Is.EqualTo("seve"));
        }

        [Test, Description("Vérifie qu'entraîner monte le niveau, débite et affiche le toast de réussite — parce que l'action principale de la fiche est « Entraîner · N CR » (M21)")]
        public void Train_SuccessToastsAndRefreshes()
        {
            Make();
            Assert.That(_view.Last.PrimaryLabel, Is.EqualTo("ENTRAÎNER · 500 CR"));
            Assert.That(_view.Last.PrimaryEnabled, Is.True);
            _view.ClickPrimary();
            Assert.That(_app.Session.Find(new MemberId("givre"))!.Level, Is.EqualTo(6));
            Assert.That(_app.Session.Wallet.Credits, Is.EqualTo(300));
            Assert.That(_view.Last.Card.LevelText, Is.EqualTo("Niv. 6"));
            Assert.That(_toasts.Shown.Last().Kind, Is.EqualTo(ToastKind.Reward));
        }

        [Test, Description("Vérifie que sans assez de Crédits le bouton est grisé et dit ce qui manque — parce qu'un bouton désactivé doit dire pourquoi (contraintes)")]
        public void Train_NotEnoughCredits_DisabledWithReason()
        {
            _app.Session.Wallet.Set(100, 0);
            Make();
            Assert.That(_view.Last.PrimaryEnabled, Is.False);
            Assert.That(_view.Last.PrimaryReason, Is.EqualTo("Il manque 400 CR"));
        }

        [Test, Description("Vérifie qu'au niveau maximum le bouton est grisé avec « NIVEAU MAX » — parce qu'on ne peut plus entraîner")]
        public void Train_MaxLevel_Disabled()
        {
            _app.Session.Find(new MemberId("givre"))!.Level = 50;
            Make();
            Assert.That(_view.Last.PrimaryEnabled, Is.False);
            Assert.That(_view.Last.PrimaryLabel, Is.EqualTo(_loc.T("hub.train.max")));
        }

        [Test, Description("Vérifie que les raccourcis ouvrent le Salon et l'Infirmerie — parce que la fiche y mène directement (M21)")]
        public void Shortcuts_OpenSalonAndInfirmary()
        {
            Make();
            _view.ClickRoom(HubPage.Salon);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Salon));
            _nav.Back();
            _view.ClickRoom(HubPage.Infirmary);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Infirmary));
        }

        [Test, Description("Vérifie qu'une fiche détruite pendant un entraînement n'appelle plus la vue ni les toasts — parce que la page peut être quittée à tout moment (Review Focus 3)")]
        public void Dispose_DuringTrain_NoViewCallAfterwards()
        {
            var pending = new PendingTrain();
            Make(train: pending);
            _view.ClickPrimary();
            Assert.That(pending.Calls, Is.EqualTo(1));
            int shows = _view.Shown.Count;
            _p.Dispose();
            pending.Done.TrySetResult(Result.Ok());
            Assert.That(_view.Shown.Count, Is.EqualTo(shows));
            Assert.That(_toasts.Shown, Is.Empty);
        }

        [Test, Description("Vérifie qu'un double toucher sur Entraîner ne lance qu'un entraînement — parce qu'un double débit serait une perte pour le joueur")]
        public void Train_DoubleTap_TrainsOnce()
        {
            var pending = new PendingTrain();
            Make(train: pending);
            _view.ClickPrimary();
            _view.ClickPrimary();
            Assert.That(pending.Calls, Is.EqualTo(1));
        }
    }
}
