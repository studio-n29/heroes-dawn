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
    /// <summary>Page Profil : tag, statistiques, amis, accès au registre et aux archives (lot F de la 0.14.0).</summary>
    public sealed class ProfilePagePresenterTests
    {
        // Deux rangées de navigation, huit statistiques, le titre de la section Amis, puis les amis.
        const int NavRows = 2, StatRows = 8, FirstFriendRow = NavRows + StatRows + 1;
        static readonly string[] StatKeys = { "combatsWon", "combatsLost", "zones", "agents", "topLevel", "bestLink", "crafted", "credits" };

        TestApp _app = null!;
        FakeListView _view = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Meta.onboarding.pseudo = "Dorian";
            _view = new FakeListView();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
        }

        ProfilePagePresenter Profile()
        {
            var p = new ProfilePagePresenter(_view, _app.Profile, _app.Friends, _nav, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                             _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que l'en-tête montre le tag du joueur et que les huit statistiques portent leur valeur — parce que le Profil résume la progression")]
        public void Header_ShowsTheTagAndStats()
        {
            Stats.Add(_app.Session.Meta, Stats.CreditsEarned, 4200);
            Stats.Add(_app.Session.Meta, Stats.CombatsWon, 2);
            var p = Profile();
            Assert.That(_view.Header.Title, Is.EqualTo("Dorian#1274"));
            Assert.That(_view.Header.A.Label, Is.EqualTo(_loc.T("profile.copy")));
            Assert.That(_view.Header.B.Label, Is.EqualTo(_loc.T("profile.add")));
            for (int i = 0; i < StatKeys.Length; i++) Assert.That(_view.Rows[NavRows + i].Title, Is.EqualTo(_loc.T("profile.stat." + StatKeys[i])), StatKeys[i]);
            Assert.That(_view.Rows[NavRows + 0].Tag, Is.EqualTo("2"));
            Assert.That(_view.Rows[NavRows + 7].Tag, Is.EqualTo("4200"));
            p.Dispose();
        }

        [Test, Description("Vérifie que les rangées Registre et Archives ouvrent le registre des agents et les archives et s'empilent sur l'onglet — parce que le Profil donne accès à ces deux pages")]
        public void NavigationRows_OpenRegistryAndArchives()
        {
            _nav.SelectTab(HubPage.Profile);
            var p = Profile();
            _view.ClickRow(0);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Agents));
            _nav.Back();
            _view.ClickRow(1);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Archives));
            Assert.That(_nav.Stack.Count, Is.EqualTo(2));
            p.Dispose();
        }

        [Test, Description("Vérifie que chaque ami a sa rangée avec un bouton Retirer qui le retire et rafraîchit la liste — parce que la liste d'amis se gère depuis le Profil")]
        public void Friends_AreListedWithARemoveButton()
        {
            _app.Friends.Add("Mara#0007"); _app.Friends.Add("Iris#0002");
            var p = Profile();
            Assert.That(_view.Rows.Length, Is.EqualTo(FirstFriendRow + 2));
            Assert.That(_view.Rows[FirstFriendRow].Title, Is.EqualTo("Mara"));
            Assert.That(_view.Rows[FirstFriendRow].Subtitle, Does.Contain("Mara#0007"));
            _view.ClickRow(FirstFriendRow);
            Assert.That(_app.Friends.Friends().Select(f => f.Tag), Is.EqualTo(new[] { "Iris#0002" }));
            Assert.That(_view.Rows.Length, Is.EqualTo(FirstFriendRow + 1));
            p.Dispose();
        }

        [Test, Description("Vérifie que sans ami la page affiche une rangée estompée d'invitation — parce qu'une liste vide doit dire quoi faire")]
        public void EmptyFriends_ShowsAHint()
        {
            var p = Profile();
            Assert.That(_view.Rows.Length, Is.EqualTo(FirstFriendRow + 1));
            Assert.That(_view.Rows[FirstFriendRow].Title, Is.EqualTo(_loc.T("profile.friends_empty")));
            Assert.That(_view.Rows[FirstFriendRow].Dim, Is.True);
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton Copier mon tag confirme par un message — parce que la copie dans le presse-papiers ne se voit pas")]
        public void Header_CopyTag_Toasts()
        {
            var p = Profile();
            _view.ClickHeader(0);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Text == _loc.T("profile.copied")));
            p.Dispose();
        }

        [Test, Description("Vérifie que la barre a six onglets avec le Profil en dernier, que les Archives n'en sont plus un et s'empilent à l'ouverture — parce que le Profil remplace l'ancien onglet Archives")]
        public void NavigatorTabs_AreTheSix_WithProfileLast()
        {
            Assert.That(HubNavigator.Tabs, Is.EqualTo(new[] { HubPage.Qg, HubPage.Team, HubPage.Recruit, HubPage.Pass, HubPage.Shop, HubPage.Profile }));
            Assert.That(HubNavigator.IsTab(HubPage.Archives), Is.False);
            _nav.Open(HubPage.Archives);
            Assert.That(_nav.Stack.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que l'onglet Profil porte le badge des archives non lues — parce que le badge des Archives a suivi leur ancien onglet")]
        public void ShellBadge_OfTheProfileTab_IsTheArchivesBadge()
        {
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            var view = new FakeHubShellView();
            var host = new FakeModalHost();
            var modals = new ModalService(host);
            var shell = new HubShellPresenter(view, _nav, new FakePageFactory(), _app.Session, _app.Subscriber<WalletChanged>(), _app.Subscriber<SaveStateChanged>(),
                                              _app.Subscriber<ShopChanged>(), _app.Shop.Open, _app.Time, modals, _toasts, _loc, new FakeAppQuit(),
                                              new BackButtonRouter(new FakeBackInput(), modals), _app.Overview, _app.Subscriber<MetaChanged>());
            view.Show();
            shell.Start();
            int unread = _app.Overview.ArchivesBadge;
            Assert.That(unread, Is.GreaterThan(0));
            Assert.That(view.Last.Tabs.Single(t => t.Page == HubPage.Profile).Badge, Is.EqualTo(unread.ToString()));
            shell.Dispose();
        }
    }
}
