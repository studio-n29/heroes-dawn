#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Wip;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Coque du hub (prefab) : barre de ressources en haut, barre de navigation en bas, zone de page au milieu.</summary>
    public interface IHubShellView : IView
    {
        event Action<HubPage>? TabClicked;
        event Action? BackClicked;
        event Action? SettingsClicked;

        IWalletView Wallet { get; }
        IPageHost Pages { get; }
        IUiTapRelay Relay { get; }

        void ShowChrome(HubShellData data);
    }
}
