using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Onglets Tournois et Repaires de l'écran QG : un appui ouvre la page, Retour revient au QG.</summary>
    public sealed class QgSubTabsTests
    {
        TestApp _app = null!;
        FakeQgPageView _view = null!;
        HubNavigator _nav = null!;
        QgPagePresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeQgPageView();
            _nav = new HubNavigator();
            var loc = new LocLocalization();
            var mission = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), new FakeToasts(), loc);
            _p = new QgPagePresenter(_view, _app.Session, _nav, mission, _app.Time, loc, _app.Overview, _app.Subscriber<RosterChanged>(), _app.Subscriber<MetaChanged>());
            _view.Show();
            _p.Start();
        }

        [TearDown] public void TearDown() => _p.Dispose();

        [Test, Description("Vérifie que l'onglet Tournois du QG ouvre la page Tournoi par-dessus le QG — parce que le tournoi est désormais accessible directement depuis le QG")]
        public void TournamentsTab_OpensTheTournamentPage()
        {
            _nav.SelectTab(HubPage.Qg);
            _view.ClickSubTab(HubPage.Tournament);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Tournament));
            Assert.That(_nav.Stack.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que l'onglet Repaires du QG ouvre la page Repaire et que Retour ramène au QG — parce que les repaires ne passent plus par la page Chapitres")]
        public void LairsTab_OpensTheLairPage_AndBackReturnsToTheQg()
        {
            _nav.SelectTab(HubPage.Qg);
            _view.ClickSubTab(HubPage.Lair);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Lair));
            _nav.Back();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
        }
    }
}
