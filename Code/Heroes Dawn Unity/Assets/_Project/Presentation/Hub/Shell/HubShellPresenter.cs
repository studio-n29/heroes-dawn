#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
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

        IHubPageHandle? _page;
        WalletBinder? _wallet;
        IDisposable? _shopSub;
        bool _askingQuit;
        int _navVersion;
        int _lastMinute = -1;

        public HubShellPresenter(IHubShellView view, HubNavigator nav, IHubPageFactory pages, ISessionReadModel session,
                                 ISubscriber<WalletChanged> wallet, ISubscriber<SaveStateChanged> saving, ISubscriber<ShopChanged> shopChanged,
                                 IOpenShopUseCase openShop, IClock clock,
                                 IModalService modals, IToastService toasts, ILocalization loc, IAppQuit quit, BackButtonRouter router)
            : base(view, router)
        {
            _nav = nav; _pages = pages; _session = session; _walletChanged = wallet; _saving = saving; _shopChanged = shopChanged;
            _openShop = openShop; _clock = clock;
            _modals = modals; _toasts = toasts; _loc = loc; _quit = quit;
        }

        protected override IBackHandler BackHandler => this;

        protected override void OnStart()
        {
            _wallet = new WalletBinder(View.Wallet, _session, _walletChanged, _saving);
            _shopSub = _shopChanged.Subscribe(_ => RefreshChrome());
            _nav.Changed += OnNavigated;
            View.TabClicked += _nav.SelectTab;
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
            View.TabClicked -= _nav.SelectTab;
            View.BackClicked -= OnBackClicked;
            View.SettingsClicked -= OpenSettings;
            View.Relay.WipTapped -= OnWipTapped;
            View.Relay.ReasonTapped -= OnReasonTapped;
            _shopSub?.Dispose();
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
            RefreshChrome();
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
                    Active = page == root,
                    Badge = page == HubPage.Recruit && shop.Day.Length > 0 && shop.SeenDay != shop.Day ? "N" : "",
                };
            }
            View.ShowChrome(new HubShellData { Title = _page?.Title ?? "", ShowBack = !_nav.IsOnTab, Tabs = tabs });
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
