#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Auth;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Catégorie de réglages ; <see cref="WipId"/> non nul = à venir (BIENTÔT).</summary>
    public struct SettingsCategoryData
    {
        public string Title;
        public string? WipId;
        public bool Active;
    }

    public struct SettingsPageData
    {
        public SettingsCategoryData[] Categories;
        public string PlayerLine;
    }

    public interface ISettingsPageView : IView
    {
        event Action LogoutClicked;
        event Action ResetClicked;
        void Show(SettingsPageData data);
    }

    /// <summary>
    /// Paramètres (⚙) : catégories à gauche, seule « Compte » fonctionne (Se déconnecter, Effacer la sauvegarde avec confirmation) ;
    /// Audio, Graphismes, Accessibilité, Notifications et Contrôles sont BIENTÔT.
    /// </summary>
    public sealed class SettingsPagePresenter : PresenterBase<ISettingsPageView>, IPageTitle
    {
        static readonly (string Key, WipFeature? Wip)[] Categories =
        {
            ("settings.account", null),
            ("settings.audio", WipCatalog.SettingsAudio),
            ("settings.graphics", WipCatalog.SettingsGraphics),
            ("settings.accessibility", WipCatalog.SettingsAccessibility),
            ("settings.notifications", WipCatalog.SettingsNotifications),
            ("settings.controls", WipCatalog.SettingsControls),
        };

        readonly ISessionReadModel _session;
        readonly ISignOutUseCase _signOut;
        readonly IResetProgressUseCase _reset;
        readonly IOpenShopUseCase _openShop;
        readonly IModalService _modals;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly PageAction _action;

        public SettingsPagePresenter(ISettingsPageView view, ISessionReadModel session, ISignOutUseCase signOut, IResetProgressUseCase reset,
                                     IOpenShopUseCase openShop, IModalService modals, IToastService toasts, ILocalization loc,
                                     IPublisher<NavigateRequested> navigate)
            : base(view, null)
        {
            _session = session; _signOut = signOut; _reset = reset; _openShop = openShop; _modals = modals; _toasts = toasts; _loc = loc;
            _navigate = navigate;
            _action = new PageAction(toasts, loc);
        }

        public string Title => _loc.T("page.settings");

        protected override void OnStart()
        {
            View.LogoutClicked += Logout;
            View.ResetClicked += Reset;
        }

        protected override void OnShown() => Refresh();

        protected override void OnDispose()
        {
            View.LogoutClicked -= Logout;
            View.ResetClicked -= Reset;
        }

        void Refresh()
        {
            if (Disposed) return;
            var cats = new SettingsCategoryData[Categories.Length];
            for (int i = 0; i < cats.Length; i++)
                cats[i] = new SettingsCategoryData { Title = _loc.T(Categories[i].Key), WipId = Categories[i].Wip?.Id, Active = i == 0 };
            var a = _session.Account;
            View.Show(new SettingsPageData
            {
                Categories = cats,
                PlayerLine = a != null ? _loc.F("settings.player", a.DisplayName, a.PlayerId) : "",
            });
        }

        void Logout() => _action.Run(ct => _signOut.ExecuteAsync(ct), Lifetime, () => _navigate.Publish(new NavigateRequested(GameState.Login)));

        void Reset()
        {
            if (_action.IsBusy) return;
            ResetAsync().Forget();
        }

        async UniTaskVoid ResetAsync()
        {
            bool ok;
            try
            {
                ok = await _modals.ConfirmAsync(new ConfirmData(_loc.T("login.reset.title"), _loc.T("login.reset.body"),
                                                                _loc.T("ui.confirm"), _loc.T("ui.cancel"), Tone.Danger), Lifetime);
            }
            catch (OperationCanceledException) { return; }
            // Nouvelle partie : boutique neuve tout de suite (sinon « collection complète » jusqu'au prochain tirage).
            if (ok && !Disposed)
                _action.Run(ct => _reset.ExecuteAsync(ct), Lifetime, () =>
                {
                    _openShop.ExecuteAsync(Lifetime).Forget();
                    _toasts.Show(_loc.T("settings.reset.done"), ToastKind.Info);
                    Refresh();
                });
        }
    }
}
