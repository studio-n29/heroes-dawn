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
    /// <summary>Écran Profil de la planche 12.2 : bandeau, statistiques en six tuiles, amis, liens, liste des succès.</summary>
    public sealed class ProfilePagePresenterTests
    {
        TestApp _app = null!;
        FakeProfilePageView _view = null!;
        FakeToasts _toasts = null!;
        HubNavigator _nav = null!;
        readonly LocLocalization _loc = new LocLocalization();

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
            _app.Session.Meta.onboarding.pseudo = "Dorian";
            _view = new FakeProfilePageView();
            _toasts = new FakeToasts();
            _nav = new HubNavigator();
        }

        ProfilePagePresenter Profile(bool connected = false)
        {
            var p = new ProfilePagePresenter(_view, _app.Profile, _app.Friends, _nav, _app.Session, _loc, _toasts, _app.Subscriber<MetaChanged>(),
                                             _app.Subscriber<WalletChanged>(), _app.Subscriber<RosterChanged>(), _app.Time,
                                             new ModalService(new FakeModalHost()), connected ? _app.ConnectedAccountLink() : null, null, _app.Lairs);
            _view.Show();
            ((VContainer.Unity.IStartable)p).Start();
            return p;
        }

        [Test, Description("Vérifie que l'écran montre les six tuiles de la planche dans l'ordre (niveau d'agent, agents, opérations, missions, succès, repaire) — parce que la grille de statistiques est la pièce centrale du Profil")]
        public void Show_HasTheSixTilesInThePlateOrder()
        {
            Stats.Add(_app.Session.Meta, Stats.CombatsWon, 5);
            var p = Profile();
            Assert.That(_view.Last.Tiles.Select(t => t.Id), Is.EqualTo(ProfileTiles.All));
            Assert.That(_view.Last.Tiles.Single(t => t.Id == ProfileTiles.Operations).Value, Is.EqualTo("5"));
            Assert.That(_view.Last.Tiles.Single(t => t.Id == ProfileTiles.Agents).Value, Is.EqualTo(_app.Profile.Stats().Agents.ToString()));
            Assert.That(_view.Last.Tiles.Single(t => t.Id == ProfileTiles.Level).Label, Is.EqualTo(_loc.T("profile.tile.level")));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bandeau montre le pseudo et, hors ligne, l'invitation à créer un compte au lieu du tag (copie impossible) — parce que le tag n'existe qu'avec un compte")]
        public void Header_Guest_ShowsThePseudoAndTheGuestHint()
        {
            var p = Profile();
            Assert.That(_view.Last.Pseudo, Is.EqualTo("Dorian"));
            Assert.That(_view.Last.TagLine, Is.EqualTo(_loc.T("profile.tag_guest")));
            Assert.That(_view.Last.TagCopyable, Is.False);
            _view.ClickTag();
            Assert.That(_toasts.Shown, Is.Empty, "pas de tag à copier hors ligne");
            p.Dispose();
        }

        [Test, Description("Vérifie que, connecté, le bandeau montre le tag et qu'un appui dessus le copie avec un message — parce que la copie dans le presse-papiers ne se voit pas")]
        public void Header_Connected_ShowsTheTag_AndCopyToasts()
        {
            var p = Profile(connected: true);
            Assert.That(_view.Last.TagLine, Is.EqualTo(_app.Profile.Tag));
            Assert.That(_view.Last.TagCopyable, Is.True);
            _view.ClickTag();
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Text == _loc.T("profile.copied")));
            p.Dispose();
        }

        [Test, Description("Vérifie qu'un appui sur l'avatar passe à l'avatar suivant de la galerie — parce que la carte de profil se personnalise depuis le bandeau")]
        public void Avatar_Click_CyclesTheAvatar()
        {
            var p = Profile();
            string before = _app.Profile.Avatar;
            _view.ClickAvatar();
            Assert.That(_app.Profile.Avatar, Is.Not.EqualTo(before));
            p.Dispose();
        }

        [Test, Description("Vérifie que sans compte fourni le bloc Compte est masqué — parce que les configurations sans compte ne doivent pas montrer un bouton inerte")]
        public void AccountBlock_IsHidden_WithoutAnAccountFlow()
        {
            var p = Profile();
            Assert.That(_view.Last.AccountVisible, Is.False);
            p.Dispose();
        }

        [Test, Description("Vérifie que les tuiles Agents et Repaire ouvrent le registre des agents et la page du repaire — parce que chaque tuile cliquable est un raccourci")]
        public void Tiles_AgentsAndLair_OpenTheirPages()
        {
            _nav.SelectTab(HubPage.Profile);
            var p = Profile();
            Assert.That(_view.Last.Tiles.Single(t => t.Id == ProfileTiles.Agents).Tappable, Is.True);
            _view.ClickTile(ProfileTiles.Agents);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Agents));
            _nav.Back();
            _view.ClickTile(ProfileTiles.Lair);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Lair));
            p.Dispose();
        }

        [Test, Description("Vérifie que les trois liens du bas ouvrent le registre des agents, les archives et le tournoi en s'empilant sur l'onglet — parce que le Profil reste le point d'accès à ces pages")]
        public void Links_OpenRegistryArchivesAndTournament()
        {
            _nav.SelectTab(HubPage.Profile);
            var p = Profile();
            Assert.That(_view.Last.LinkLabels, Is.EqualTo(new[] { _loc.T("profile.registry"), _loc.T("profile.archives"), _loc.T("profile.tournament") }));
            _view.ClickLink(0);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Agents));
            _nav.Back();
            _view.ClickLink(1);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Archives));
            Assert.That(_nav.Stack.Count, Is.EqualTo(2));
            _nav.Back();
            _view.ClickLink(2);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Tournament));
            p.Dispose();
        }

        [Test, Description("Vérifie que la tuile Succès ouvre la liste complète des succès et que Retour revient aux statistiques — parce que le détail des succès remplace l'ancienne page liste")]
        public void AchievementsTile_OpensTheFullList_AndBackReturns()
        {
            var p = Profile();
            Assert.That(_view.Last.Achievements, Is.Null);
            _view.ClickTile(ProfileTiles.Achievements);
            Assert.That(_view.Last.Achievements, Is.Not.Null);
            Assert.That(_view.Last.Achievements!.Length, Is.EqualTo(_app.Profile.Achievements().List().Count));
            _view.ClickBack();
            Assert.That(_view.Last.Achievements, Is.Null);
            p.Dispose();
        }

        [Test, Description("Vérifie que chaque ami est listé avec son tag et son état, et que le titre porte le compte (n / 20) — parce que la planche affiche « Amis (3 / 20) »")]
        public void Friends_AreListed_WithTheirCount()
        {
            _app.Friends.Add("Mara#0007"); _app.Friends.Add("Iris#0002");
            var p = Profile();
            Assert.That(_view.Last.Friends.Select(f => f.Tag), Is.EqualTo(new[] { "Mara#0007", "Iris#0002" }));
            Assert.That(_view.Last.Friends[0].Name, Is.EqualTo("Mara"));
            Assert.That(_view.Last.FriendsTitle, Is.EqualTo(_loc.F("profile.friends_title", 2, ProfilePagePresenter.MaxFriends)));
            p.Dispose();
        }

        [Test, Description("Vérifie que retirer un ami le retire et redessine la liste, et que copier son tag confirme sans le retirer — parce que ce sont les deux actions d'une rangée d'ami")]
        public void Friends_RemoveAndCopy()
        {
            _app.Friends.Add("Mara#0007"); _app.Friends.Add("Iris#0002");
            var p = Profile();
            _view.ClickFriendCopy(0);
            Assert.That(_toasts.Shown, Has.Some.Matches<(string Text, ToastKind Kind)>(t => t.Text == _loc.T("profile.copied")));
            Assert.That(_app.Friends.Friends().Count, Is.EqualTo(2));
            _view.ClickFriendRemove(0);
            Assert.That(_app.Friends.Friends().Select(f => f.Tag), Is.EqualTo(new[] { "Iris#0002" }));
            Assert.That(_view.Last.Friends.Length, Is.EqualTo(1));
            p.Dispose();
        }

        [Test, Description("Vérifie que sans ami l'écran affiche un texte d'invitation — parce qu'une liste vide doit dire quoi faire")]
        public void EmptyFriends_ShowsAHint()
        {
            var p = Profile();
            Assert.That(_view.Last.Friends, Is.Empty);
            Assert.That(_view.Last.FriendsEmpty, Is.EqualTo(_loc.T("profile.friends_empty")));
            p.Dispose();
        }

        [Test, Description("Vérifie que le bouton Ajouter est désactivé hors ligne avec sa raison, et actif une fois connecté — parce que l'ajout par tag demande un compte")]
        public void AddButton_NeedsAnAccount()
        {
            var p = Profile();
            Assert.That(_view.Last.CanAddFriend, Is.False);
            Assert.That(_view.Last.AddReason, Is.EqualTo(_loc.T("profile.tag_guest_reason")));
            p.Dispose();
            _view = new FakeProfilePageView();
            var q = Profile(connected: true);
            Assert.That(_view.Last.CanAddFriend, Is.True);
            q.Dispose();
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
