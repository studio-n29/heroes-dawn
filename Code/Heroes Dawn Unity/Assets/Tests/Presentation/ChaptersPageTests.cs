using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Page Chapitres : les 8 chapitres, leur statut, et « Relire » (lot G de la 0.14.0).</summary>
    public sealed class ChaptersPageTests
    {
        sealed class FakeChapterPopup : IChapterPopupView
        {
            public ChapterPopupData? Data;
            public int Shown;
            public bool Cancelable => true;
            public void RequestCancel() { }
            public UniTask ShowAsync(ChapterPopupData data, CancellationToken ct) { Data = data; Shown++; return UniTask.CompletedTask; }
        }

        TestApp _app = null!;
        FakeListView _view = null!;
        FakeChapterPopup _popup = null!;
        readonly LocLocalization _loc = new LocLocalization();

        ChaptersPagePresenter Page()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeListView();
            _popup = new FakeChapterPopup();
            var host = new FakeModalHost();
            host.Register<IChapterPopupView>(() => _popup);
            var p = new ChaptersPagePresenter(_view, _app.Session, _app.Chapters, new ChapterIntroFlow(new ModalService(host), _loc), _loc, new FakeToasts(),
                                              _app.Subscriber<MetaChanged>(), _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que la page liste les 8 chapitres, New York d'abord, Brooklyn « En cours » mis en avant et les 7 autres estompés avec leur statut, et que l'en-tête compte 0 / 8 — parce que le joueur voit où il en est dans l'histoire")]
        public void Rows_FollowTheDisplayOrder_WithTheirStatus()
        {
            var p = Page();
            Assert.That(_view.Rows.Length, Is.EqualTo(8));
            Assert.That(_view.Rows[0].Title, Is.EqualTo(_loc.T("chapter.ny_brooklyn.title")));
            Assert.That(_view.Rows[0].Tag, Is.EqualTo("1"));
            Assert.That(_view.Rows[0].Highlight, Is.True);
            Assert.That(_view.Rows[0].Subtitle, Does.Contain(_loc.T("chapter.status.Current")));
            for (int i = 1; i < 8; i++)
            {
                Assert.That(_view.Rows[i].Dim, Is.True, "rangée " + i);
                Assert.That(_view.Rows[i].Subtitle, Does.Contain(_loc.T("chapter.status.Soon")), "rangée " + i);
            }
            Assert.That(_view.Rows[4].Title, Is.EqualTo(_loc.T("chapter.mos_sergueiv.title")));
            Assert.That(_view.Header.Line, Is.EqualTo(_loc.F("chapters.line", 0, 8)));
            p.Dispose();
        }

        [Test, Description("Vérifie que Brooklyn terminé passe « Terminé » et que l'en-tête compte 1 / 8 — parce que la progression se lit en tête de page")]
        public void Done_ChapterShowsInTheHeaderCount()
        {
            var p = Page();
            foreach (var z in _app.Session.Content.Zones.Where(z => z.Borough == "brooklyn")) _app.Session.CompletedZones.Add(z.Id.Value);
            _app.MetaCtx.Touch(MetaArea.Profile);
            Assert.That(_view.Header.Line, Is.EqualTo(_loc.F("chapters.line", 1, 8)));
            Assert.That(_view.Rows[0].Subtitle, Does.Contain(_loc.T("chapter.status.Done")));
            p.Dispose();
        }

        [Test, Description("Vérifie que Relire ouvre la fenêtre du chapitre sans le marquer comme vu — parce que relire n'est pas une première ouverture")]
        public void Reread_OpensTheIntro_WithoutMarkingItSeen()
        {
            var p = Page();
            _view.ClickRow(0);
            Assert.That(_popup.Shown, Is.EqualTo(1));
            Assert.That(_popup.Data!.Value.Text, Is.EqualTo(_loc.T("chapter.ny_brooklyn.intro")));
            Assert.That(_app.Session.Meta.chaptersSeen, Is.Empty);
            p.Dispose();
        }

        [Test, Description("Vérifie que Relire est grisé avec une raison pour un chapitre « Bientôt » — parce qu'il n'a pas encore de contenu")]
        public void Reread_IsDisabledForSoonChapters()
        {
            var p = Page();
            Assert.That(_view.Rows[1].A.Enabled, Is.False);
            Assert.That(_view.Rows[1].A.Reason, Is.EqualTo(_loc.T("chapter.soon_reason")));
            Assert.That(_view.Rows[0].A.Enabled, Is.True);
            p.Dispose();
        }

        sealed class FakeMapView : FakeView, ICityMapPageView
        {
            public event Action<string>? ZoneSelected;
            public event Action? DeployClicked;
            public event Action<string>? LockedTapped;
            public event Action? ChaptersClicked;
            public void Show(CityMapPageData d) { }
            public void ClickChapters() => ChaptersClicked?.Invoke();
        }

        [Test, Description("Vérifie que le bouton Chapitres de la carte ouvre la page Chapitres — parce que la carte est l'entrée de la page")]
        public void CityMap_ChaptersButton_OpensTheChaptersPage()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            var view = new FakeMapView();
            var nav = new HubNavigator();
            var toasts = new FakeToasts();
            var mission = new MissionLauncher(app.Zone.Start, N29.HeroesDawn.Content.Prototype.PrototypeContent.Zone11, app.Publisher<NavigateRequested>(), toasts, _loc);
            var p = new CityMapPagePresenter(view, app.Session, mission, toasts, _loc, app.Subscriber<RosterChanged>(), null, null, null, nav);
            view.Show();
            p.Start();
            view.ClickChapters();
            Assert.That(nav.Current.Page, Is.EqualTo(HubPage.Chapters));
            p.Dispose();
        }
    }
}
