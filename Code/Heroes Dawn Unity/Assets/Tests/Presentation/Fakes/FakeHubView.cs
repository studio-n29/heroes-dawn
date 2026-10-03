#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeHubView : FakeView, IHubView
    {
        public event Action<HubTab>? TabClicked;
        public event Action? LogoutClicked;
        public event Action? ResetClicked;
        public event Action? HealClicked;
        public event Action? DeployClicked;
        public event Action<MemberId>? SquadToggleClicked;
        public event Action<MemberId>? TrainClicked;
        public event Action<int>? RecruitClicked;
        public event Action? RerollClicked;

        public readonly FakeWalletView WalletView = new FakeWalletView();
        public IWalletView Wallet => WalletView;
        public readonly List<HubData> Shown = new List<HubData>();
        public int Scrolls;
        public HubData Last => Shown[Shown.Count - 1];

        public void Show(HubData data) => Shown.Add(data);
        public void ScrollToTop() => Scrolls++;

        public void ClickTab(HubTab t) => TabClicked?.Invoke(t);
        public void ClickLogout() => LogoutClicked?.Invoke();
        public void ClickReset() => ResetClicked?.Invoke();
        public void ClickHeal() => HealClicked?.Invoke();
        public void ClickDeploy() => DeployClicked?.Invoke();
        public void ClickSquad(MemberId id) => SquadToggleClicked?.Invoke(id);
        public void ClickTrain(MemberId id) => TrainClicked?.Invoke(id);
        public void ClickRecruit(int slot) => RecruitClicked?.Invoke(slot);
        public void ClickReroll() => RerollClicked?.Invoke();
    }
}
