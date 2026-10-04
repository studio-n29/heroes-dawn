#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Wip;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeTapRelay : IUiTapRelay
    {
        public event Action<string>? WipTapped;
        public event Action<string>? ReasonTapped;
        public void TapWip(string id) => WipTapped?.Invoke(id);
        public void TapReason(string text) => ReasonTapped?.Invoke(text);
    }

    public sealed class FakeHubShellView : FakeView, IHubShellView
    {
        public event Action<HubPage>? TabClicked;
        public event Action? BackClicked;
        public event Action? SettingsClicked;

        public readonly FakeWalletView WalletView = new FakeWalletView();
        public readonly FakeTapRelay TapRelay = new FakeTapRelay();
        public readonly List<HubShellData> Shown = new List<HubShellData>();

        public IWalletView Wallet => WalletView;
        public IPageHost Pages => throw new NotSupportedException("Les tests de la coque passent par une fabrique de pages factice.");
        public IUiTapRelay Relay => TapRelay;
        public HubShellData Last => Shown[Shown.Count - 1];

        public void ShowChrome(HubShellData d) => Shown.Add(d);

        public void ClickTab(HubPage p) => TabClicked?.Invoke(p);
        public void ClickBack() => BackClicked?.Invoke();
        public void ClickSettings() => SettingsClicked?.Invoke();
    }

    public sealed class FakePageHandle : IHubPageHandle
    {
        public HubLocation Location { get; }
        public int Disposals { get; private set; }
        public string Title { get; }
        public FakePageHandle(HubLocation l) { Location = l; Title = "Titre " + l; }
        public int Ticks { get; private set; }
        public void Tick() => Ticks++;
        public void Dispose() => Disposals++;
    }

    public sealed class FakePageFactory : IHubPageFactory
    {
        public readonly List<FakePageHandle> Created = new List<FakePageHandle>();
        public FakePageHandle Last => Created[Created.Count - 1];

        /// <summary>Appelé pendant la création (comme le démarrage d'un présentateur qui naviguerait aussitôt).</summary>
        public Action<HubLocation>? OnCreate;

        public IHubPageHandle Create(HubLocation location)
        {
            var h = new FakePageHandle(location);
            Created.Add(h);
            OnCreate?.Invoke(location);
            return h;
        }
    }

    public sealed class FakeAppQuit : IAppQuit
    {
        public int Calls;
        public void Quit() => Calls++;
    }
}
