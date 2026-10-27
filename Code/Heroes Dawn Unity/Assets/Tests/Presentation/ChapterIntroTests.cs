using System;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Fenêtre d'ouverture d'un chapitre au premier déploiement (lot G de la 0.14.0).</summary>
    public sealed class ChapterIntroTests
    {
        sealed class FakeMapView : FakeView, ICityMapPageView
        {
            public event Action<string>? ZoneSelected;
            public event Action? DeployClicked;
            public event Action<string>? LockedTapped;
            public event Action? ChaptersClicked;
            public CityMapPageData Last;
            public void Show(CityMapPageData d) => Last = d;
            public void Select(string z) => ZoneSelected?.Invoke(z);
            public void Deploy() => DeployClicked?.Invoke();
        }

        sealed class FakeChapterPopup : IChapterPopupView
        {
            public readonly UniTaskCompletionSource Done = new UniTaskCompletionSource();
            public ChapterPopupData? Data;
            public int Shown;
            public bool Cancelable => true;
            public void RequestCancel() => Done.TrySetResult();
            public UniTask ShowAsync(ChapterPopupData data, CancellationToken ct) { Data = data; Shown++; return Done.Task; }
        }

        TestApp _app = null!;
        FakeMapView _view = null!;
        FakeChapterPopup _popup = null!;
        readonly LocLocalization _loc = new LocLocalization();

        CityMapPagePresenter Make()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _view = new FakeMapView();
            _popup = new FakeChapterPopup();
            var host = new FakeModalHost();
            host.Register<IChapterPopupView>(() => _popup);
            var toasts = new FakeToasts();
            var mission = new MissionLauncher(_app.Zone.Start, PrototypeContent.Zone11, _app.Publisher<NavigateRequested>(), toasts, _loc);
            var p = new CityMapPagePresenter(_view, _app.Session, mission, toasts, _loc, _app.Subscriber<RosterChanged>(), null,
                                             _app.Chapters, new ChapterIntroFlow(new ModalService(host), _loc));
            _view.Show();
            p.Start();
            return p;
        }

        [Test, Description("Vérifie qu'au premier déploiement dans une zone de Brooklyn la fenêtre « CHAPITRE 1 · BROOKLYN » s'affiche avec le texte d'ouverture, que le déploiement attend sa fermeture, puis que le chapitre est marqué vu — parce que chaque chapitre s'ouvre par son texte")]
        public void Deploy_FirstTime_ShowsTheChapterIntroThenLaunches()
        {
            var p = Make();
            _view.Deploy();
            Assert.That(_popup.Data!.Value.Heading, Is.EqualTo("CHAPITRE 1 · BROOKLYN"));
            Assert.That(_popup.Data!.Value.Text, Is.EqualTo(_loc.T("chapter.ny_brooklyn.intro")));
            Assert.That(_app.Session.Run, Is.Null, "le plateau n'est pas encore ouvert");
            Assert.That(_app.Chapters.Seen("ny_brooklyn"), Is.False);
            _popup.Done.TrySetResult();
            Assert.That(_app.Chapters.Seen("ny_brooklyn"), Is.True);
            Assert.That(_app.Session.RunZone!.Id.Value, Is.EqualTo("zone_ny_1_1"));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un chapitre déjà vu ne rouvre pas la fenêtre et que le déploiement a lieu tout de suite — parce que l'ouverture ne s'affiche qu'une fois")]
        public void Deploy_SecondTime_LaunchesDirectly()
        {
            var p = Make();
            _app.Chapters.MarkSeen("ny_brooklyn");
            _view.Deploy();
            Assert.That(_popup.Shown, Is.EqualTo(0));
            Assert.That(_app.Session.RunZone!.Id.Value, Is.EqualTo("zone_ny_1_1"));
            p.Dispose();
        }

        [Test, Description("Vérifie que fermer la fenêtre par Retour lance quand même le déploiement et marque le chapitre vu — parce qu'on ne doit pas rester bloqué sur la fenêtre")]
        public void Deploy_IntroClosedByBack_StillLaunches()
        {
            var p = Make();
            _view.Deploy();
            _popup.RequestCancel();
            Assert.That(_app.Chapters.Seen("ny_brooklyn"), Is.True);
            Assert.That(_app.Session.RunZone, Is.Not.Null);
            p.Dispose();
        }

        [Test, Description("Vérifie qu'une page détruite pendant la fenêtre ne lance pas le déploiement — parce que le joueur a quitté la carte")]
        public void Deploy_DisposedDuringIntro_DoesNotLaunch()
        {
            var p = Make();
            _view.Deploy();
            p.Dispose();
            _popup.Done.TrySetResult();
            Assert.That(_app.Session.Run, Is.Null);
        }

        [Test, Description("Vérifie que les 8 chapitres ont un titre et un texte d'ouverture traduits de 15 à 60 mots et que l'intitulé d'un chapitre de Moscou est « CHAPITRE 4 · LA PLACE ROUGE » — parce que chaque chapitre doit se lire à l'écran")]
        public void EveryChapter_HasATitleAndAnIntro()
        {
            foreach (var c in Chapters.All)
            {
                Assert.That(_loc.T(c.TitleKey), Is.Not.EqualTo(c.TitleKey), c.Id);
                string intro = _loc.T(c.IntroKey);
                Assert.That(intro, Is.Not.EqualTo(c.IntroKey), c.Id);
                int words = intro.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.That(words, Is.InRange(15, 60), c.Id);
            }
            var d = new ChapterIntroFlow(new ModalService(new FakeModalHost()), _loc).DataOf(Chapters.Find("mos_place_rouge")!);
            Assert.That(d.Heading, Is.EqualTo("CHAPITRE 4 · LA PLACE ROUGE"));
        }
    }
}
