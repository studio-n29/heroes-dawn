using System.Linq;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;
using N29.HeroesDawn.Tests.Domain.Application;
using N29.HeroesDawn.Tests.Presentation.Fakes;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    public sealed class HubShellPresenterTests
    {
        TestApp _app = null!;
        FakeHubShellView _view = null!;
        FakePageFactory _pages = null!;
        FakeModalHost _host = null!;
        ModalService _modals = null!;
        BackButtonRouter _router = null!;
        FakeToasts _toasts = null!;
        FakeAppQuit _quit = null!;
        HubNavigator _nav = null!;
        HubShellPresenter _p = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.Account.SignIn.ExecuteAsync().GetAwaiter().GetResult();
            _view = new FakeHubShellView();
            _pages = new FakePageFactory();
            _host = new FakeModalHost();
            _modals = new ModalService(_host);
            _router = new BackButtonRouter(new FakeBackInput(), _modals);
            _toasts = new FakeToasts();
            _quit = new FakeAppQuit();
            _nav = new HubNavigator();
            _p = new HubShellPresenter(_view, _nav, _pages, _app.Session, _app.Subscriber<WalletChanged>(), _app.Subscriber<SaveStateChanged>(),
                                       _app.Subscriber<ShopChanged>(), _modals, _toasts, new LocLocalization(), _quit, _router);
            _view.Show();
            _p.Start();
        }

        [Test, Description("Vérifie que la coque affiche la page du QG au démarrage et que le titre suit la page courante — parce que la barre haute porte le titre de l'écran (maquettes §1)")]
        public void Title_FollowsCurrentPage()
        {
            Assert.That(_pages.Last.Location.Page, Is.EqualTo(HubPage.Qg));
            Assert.That(_view.Last.Title, Is.EqualTo(_pages.Last.Title));
            _view.ClickTab(HubPage.Team);
            Assert.That(_view.Last.Title, Is.EqualTo(_pages.Last.Title));
        }

        [Test, Description("Vérifie que le bouton Retour n'apparaît que sur une page empilée — parce qu'un onglet n'a pas de page précédente (maquettes §1)")]
        public void StackedPage_ShowsBackButton_TabDoesNot()
        {
            Assert.That(_view.Last.ShowBack, Is.False);
            _nav.Open(HubPage.Salon);
            Assert.That(_view.Last.ShowBack, Is.True);
        }

        [Test, Description("Vérifie que l'onglet actif est celui de la base de la pile — parce qu'une pièce ouverte depuis le QG garde l'onglet QG actif (M11)")]
        public void ActiveTab_IsHighlighted()
        {
            _nav.Open(HubPage.Salon);
            var active = _view.Last.Tabs.Where(t => t.Active).Select(t => t.Page).ToArray();
            Assert.That(active, Is.EqualTo(new[] { HubPage.Qg }));
            Assert.That(_view.Last.Tabs.Select(t => t.Page), Is.EqualTo(HubNavigator.Tabs));
        }

        [Test, Description("Vérifie que l'onglet Recruter porte le badge « N » tant que la boutique du jour n'a pas été vue — parce que le joueur doit savoir que de nouveaux héros sont arrivés (spec §5)")]
        public void RecruitTab_ShowsNBadge_WhenShopNotSeen_AndClearsAfterSeen()
        {
            _app.Shop.Open.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(_view.Last.Tabs.Single(t => t.Page == HubPage.Recruit).Badge, Is.EqualTo("N"));
            _app.Shop.MarkSeen.ExecuteAsync().GetAwaiter().GetResult();
            Assert.That(_view.Last.Tabs.Single(t => t.Page == HubPage.Recruit).Badge, Is.Empty);
        }

        [Test, Description("Vérifie que toucher un onglet crée sa page et détruit l'ancienne une seule fois — parce qu'il ne doit jamais y avoir deux pages vivantes (Review Focus 1)")]
        public void TabClick_SelectsTab_AndReplacesPage()
        {
            var qg = _pages.Last;
            _view.ClickTab(HubPage.Recruit);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Recruit));
            Assert.That(_pages.Last.Location.Page, Is.EqualTo(HubPage.Recruit));
            Assert.That(qg.Disposals, Is.EqualTo(1));
            Assert.That(_pages.Created.Count(h => h.Disposals == 0), Is.EqualTo(1));
        }

        [Test, Description("Vérifie que des touchers d'onglets rapides ne créent qu'une page par navigation réelle — parce qu'un double toucher ne doit pas recréer la page (Review Focus 1)")]
        public void QuickTabSpam_CreatesOnePagePerNavigation()
        {
            _view.ClickTab(HubPage.Team);
            _view.ClickTab(HubPage.Team);
            _view.ClickTab(HubPage.Team);
            Assert.That(_pages.Created.Count, Is.EqualTo(2));
        }

        [Test, Description("Vérifie que Retour sur le QG demande « Quitter le jeu ? » puis quitte si le joueur confirme — parce que QG → quitter, jamais sans confirmation (spec §7)")]
        public void Back_OnQg_AsksQuitConfirmation_ThenQuits()
        {
            Assert.That(_router.Dispatch(), Is.True);
            var confirm = (FakeConfirmPopup)_host.Spawned.Single();
            Assert.That(_quit.Calls, Is.Zero);
            confirm.Answer(true);
            Assert.That(_quit.Calls, Is.EqualTo(1));
        }

        [Test, Description("Vérifie qu'annuler « Quitter le jeu ? » laisse le joueur sur le QG — parce que la confirmation protège d'un départ accidentel")]
        public void Back_OnQg_ConfirmCancelled_DoesNotQuit()
        {
            _router.Dispatch();
            ((FakeConfirmPopup)_host.Spawned.Single()).Answer(false);
            Assert.That(_quit.Calls, Is.Zero);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
        }

        [Test, Description("Vérifie que Retour avec une modale ouverte ferme la modale sans changer de page — parce que Retour annule d'abord la modale (Review Focus 4)")]
        public void Back_WithModalOpen_ClosesModalPageUnchanged()
        {
            _nav.SelectTab(HubPage.Team);
            int created = _pages.Created.Count;
            _modals.ConfirmAsync(new ConfirmData("t", "b", "ok", "non", Tone.Primary), default).Forget();
            Assert.That(_router.Dispatch(), Is.True);
            Assert.That(_modals.AnyOpen, Is.False);
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Team));
            Assert.That(_pages.Created.Count, Is.EqualTo(created));
        }

        [Test, Description("Vérifie que la coque masquée ne répond plus à Retour — parce que pendant une mission seule la scène de mission répond")]
        public void HiddenShell_NotHandlingBack()
        {
            _view.Hide();
            Assert.That(_router.Dispatch(), Is.False);
        }

        [Test, Description("Vérifie que le bouton Retour de la barre haute fait comme Retour — parce que le bouton, Échap et le geste Android font la même chose (maquettes §1)")]
        public void BackButton_PopsStackedPage()
        {
            _nav.Open(HubPage.Infirmary);
            _view.ClickBack();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Qg));
        }

        [Test, Description("Vérifie que toucher un élément BIENTÔT affiche un message sans naviguer — parce qu'aucun élément ne doit être un cul-de-sac muet (spec §3.4)")]
        public void WipTapped_ShowsSoonToast_NoNavigation()
        {
            int created = _pages.Created.Count;
            _view.TapRelay.TapWip("pass");
            Assert.That(_toasts.Shown.Select(t => t.Text), Does.Contain(WipNotice.Message(new LocLocalization(), "pass")));
            Assert.That(_pages.Created.Count, Is.EqualTo(created));
        }

        [Test, Description("Vérifie qu'un bouton grisé touché affiche sa raison — parce que le joueur doit savoir pourquoi il ne peut pas agir (spec §6)")]
        public void ReasonTapped_ShowsReasonToast()
        {
            _view.TapRelay.TapReason("PR insuffisants");
            Assert.That(_toasts.Shown.Select(t => t.Text), Does.Contain("PR insuffisants"));
        }

        [Test, Description("Vérifie que ⚙ ouvre les Paramètres par-dessus la page courante — parce que Retour doit revenir d'où l'on vient (M80)")]
        public void SettingsClick_OpensSettings()
        {
            _view.ClickSettings();
            Assert.That(_nav.Current.Page, Is.EqualTo(HubPage.Settings));
            Assert.That(_nav.IsOnTab, Is.False);
        }

        [Test, Description("Vérifie que la barre de ressources affiche les Crédits et les PR de la session, Gemmes à 0 — parce que la monnaie premium n'existe pas encore (spec §3.4)")]
        public void Wallet_ShowsSessionValues_GemsZero()
        {
            Assert.That(_view.WalletView.Last.Credits, Is.EqualTo(_app.Session.Wallet.Credits));
            Assert.That(_view.WalletView.Last.Pr, Is.EqualTo(_app.Session.Wallet.Pr));
            Assert.That(_view.WalletView.Last.Gems, Is.Zero);
        }

        [Test, Description("Vérifie que détruire la coque détruit la page courante une seule fois — parce que la scène du hub est déchargée en partant en mission")]
        public void Dispose_DisposesCurrentPageOnce()
        {
            var page = _pages.Last;
            _p.Dispose();
            _p.Dispose();
            Assert.That(page.Disposals, Is.EqualTo(1));
        }
    }
}
