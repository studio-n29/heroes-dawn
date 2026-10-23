#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Features.Tutorial;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Coque du hub : crée la page courante (une seule à la fois), tient titre, Retour, onglets et badge « N »,
    /// relaie Retour (pile, onglet → QG, QG → « Quitter le jeu ? ») et les touchers BIENTÔT / boutons grisés en toasts.
    /// </summary>
    public sealed class HubShellPresenter : PresenterBase<IHubShellView>, IBackHandler, VContainer.Unity.ITickable
    {
        readonly HubNavigator _nav;
        readonly IHubPageFactory _pages;
        readonly ISessionReadModel _session;
        readonly ISubscriber<WalletChanged> _walletChanged;
        readonly ISubscriber<SaveStateChanged> _saving;
        readonly ISubscriber<ShopChanged> _shopChanged;
        readonly IOpenShopUseCase _openShop;
        readonly IClock _clock;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly IAppQuit _quit;
        readonly MetaOverview? _meta;
        readonly ISubscriber<MetaChanged>? _metaChanged;
        readonly TutorialGuide? _tutorial;
        IDisposable? _metaSub;

        IHubPageHandle? _page;
        WalletBinder? _wallet;
        IDisposable? _shopSub;
        bool _askingQuit;
        int _navVersion;
        int _lastMinute = -1;

        public HubShellPresenter(IHubShellView view, HubNavigator nav, IHubPageFactory pages, ISessionReadModel session,
                                 ISubscriber<WalletChanged> wallet, ISubscriber<SaveStateChanged> saving, ISubscriber<ShopChanged> shopChanged,
                                 IOpenShopUseCase openShop, IClock clock,
                                 IModalService modals, IToastService toasts, ILocalization loc, IAppQuit quit, BackButtonRouter router,
                                 MetaOverview? meta = null, ISubscriber<MetaChanged>? metaChanged = null, TutorialGuide? tutorial = null)
            : base(view, router)
        {
            _nav = nav; _pages = pages; _session = session; _walletChanged = wallet; _saving = saving; _shopChanged = shopChanged;
            _openShop = openShop; _clock = clock;
            _modals = modals; _toasts = toasts; _loc = loc; _quit = quit;
            _meta = meta; _metaChanged = metaChanged; _tutorial = tutorial;
        }

        protected override IBackHandler BackHandler => this;

        protected override void OnStart()
        {
            _wallet = new WalletBinder(View.Wallet, _session, _walletChanged, _saving);
            _shopSub = _shopChanged.Subscribe(_ => RefreshChrome());
            _metaSub = _metaChanged?.Subscribe(_ => RefreshChrome());
            _nav.Changed += OnNavigated;
            View.TabClicked += OnTabClicked;
            View.BackClicked += OnBackClicked;
            View.SettingsClicked += OpenSettings;
            View.Relay.WipTapped += OnWipTapped;
            View.Relay.ReasonTapped += OnReasonTapped;
            OnNavigated(_nav.Current);
            OpenShop();
        }

        protected override void OnDispose()
        {
            _nav.Changed -= OnNavigated;
            View.TabClicked -= OnTabClicked;
            View.BackClicked -= OnBackClicked;
            View.SettingsClicked -= OpenSettings;
            View.Relay.WipTapped -= OnWipTapped;
            View.Relay.ReasonTapped -= OnReasonTapped;
            _shopSub?.Dispose();
            _metaSub?.Dispose();
            _wallet?.Dispose();
            _page?.Dispose();
            _page = null;
        }

        /// <summary>Les pages vivent dans un scope enfant, non tické : la coque leur transmet le tick.</summary>
        public void Tick()
        {
            if (Disposed) return;
            var now = _clock.LocalNow;
            if (now.Hour * 60 + now.Minute != _lastMinute) OpenShop();
            _page?.Tick();
        }

        /// <summary>Tire la boutique du jour si besoin (à l'ouverture, puis une fois par minute : nouveau tirage à 9h00) ; le badge « N » suit.</summary>
        void OpenShop()
        {
            var now = _clock.LocalNow;
            _lastMinute = now.Hour * 60 + now.Minute;
            _openShop.ExecuteAsync(Lifetime).Forget();
        }

        /// <summary>
        /// Remplace la page courante. Une page peut naviguer pendant sa propre création (fiche de boutique périmée → Recruter) :
        /// la navigation imbriquée gagne, la page dont la création se termine après elle est détruite aussitôt.
        /// </summary>
        void OnNavigated(HubLocation location)
        {
            if (Disposed) return;
            _page?.Dispose();
            _page = null;
            int version = ++_navVersion;
            var page = _pages.Create(location);
            if (version != _navVersion || Disposed)
            {
                page.Dispose();
                return;
            }
            _page = page;
            if (location.Page == HubPage.Team) _tutorial?.Signal(TutorialSignal.TeamOpened);
            if (location.Page == HubPage.CityMap) _tutorial?.Signal(TutorialSignal.MapOpened);
            if (location.Page == HubPage.Recruit) _tutorial?.Signal(TutorialSignal.RecruitOpened);
            OfferHints(location);
            RefreshChrome();
        }

        /// <summary>Astuces à la première rencontre : boutique, infirmerie avec un membre fatigué, fatigue élevée au QG.</summary>
        void OfferHints(HubLocation location)
        {
            if (_tutorial == null) return;
            if (location.Page == HubPage.Recruit) _tutorial.Hint("recruit");
            bool tired = false, exhausting = false;
            foreach (var m in _session.Roster) { if (m.Fatigue > 0) tired = true; if (m.Fatigue >= 60) exhausting = true; }
            if (location.Page == HubPage.Infirmary && tired) _tutorial.Hint("infirmary");
            if (location.Page == HubPage.Qg && exhausting) _tutorial.Hint("fatigue");
        }

        void RefreshChrome()
        {
            if (Disposed) return;
            var tabs = new HubTabData[HubNavigator.Tabs.Length];
            var root = _nav.Stack[0].Page;
            var shop = _session.Shop;
            for (int i = 0; i < tabs.Length; i++)
            {
                var page = HubNavigator.Tabs[i];
                tabs[i] = new HubTabData
                {
                    Page = page,
                    Label = _loc.T(NavKey(page)),
                    Active = page == HubNavigator.NavTabOf(root),
                    Hidden = page == HubPage.Recruit,
                    Badge = BadgeOf(page, shop),
                };
            }
            var route = _session.Route;
            MemberDef? hero = null;
            foreach (var m in _session.Content.Members) if (m.Id.Value == route.HeroId) { hero = m; break; }
            View.ShowChrome(new HubShellData
            {
                Title = _page?.Title ?? "", ShowBack = !_nav.IsOnTab, Tabs = tabs,
                Avatar = new PortraitData(route.HeroName, hero?.Family ?? Family.Matter, art: AgentArt.For(route.HeroId)),
                AvatarName = route.HeroName.ToUpperInvariant(), AvatarRole = _loc.T("hub.avatar_role"),
            });
        }

        string BadgeOf(HubPage page, IShopReadModel shop)
        {
            if (page == HubPage.Recruit) return "";
            if (page == HubPage.Shop && shop.Day.Length > 0 && shop.SeenDay != shop.Day) return "N";
            if (_meta == null) return "";
            if (page == HubPage.Pass) { int n = _meta.PassBadge; return n > 0 ? n.ToString() : ""; }
            if (page == HubPage.Shop) return _meta.ShopBadge > 0 ? "!" : "";
            return "";
        }

        static string NavKey(HubPage tab) => tab switch
        {
            HubPage.Qg => "nav.qg",
            HubPage.Team => "nav.team",
            HubPage.Recruit => "nav.recruit",
            HubPage.Pass => "nav.pass",
            _ => "nav.shop",
        };

        public bool HandleBack()
        {
            if (Disposed || !View.IsVisible) return false;
            if (_nav.Back() == BackOutcome.AskQuit) AskQuitAsync().Forget();
            return true;
        }

        void OnBackClicked() => HandleBack();

        /// <summary>Un onglet de la barre : la Boutique rouvre la section mémorisée (les héros ont leur page Recruter).</summary>
        void OnTabClicked(HubPage page)
        {
            if (page == HubPage.Shop) _nav.SelectShopTab(_nav.ShopTab);
            else _nav.SelectTab(page);
        }

        void OpenSettings() => _nav.Open(HubPage.Settings);

        async UniTaskVoid AskQuitAsync()
        {
            if (_askingQuit) return;
            _askingQuit = true;
            try
            {
                bool ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("hub.quit.title"), _loc.T("hub.quit.body"),
                                                                     _loc.T("hub.quit.ok"), _loc.T("ui.cancel"), Tone.Danger), Lifetime);
                if (ok && !Disposed) _quit.Quit();
            }
            catch (OperationCanceledException) { }
            finally { _askingQuit = false; }
        }

        void OnWipTapped(string featureId) => _toasts.Show(WipNotice.Message(_loc, featureId), ToastKind.Info);

        void OnReasonTapped(string text) => _toasts.Show(text, ToastKind.Warning);
    }
}
