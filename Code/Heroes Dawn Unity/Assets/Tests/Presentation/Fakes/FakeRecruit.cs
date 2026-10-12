#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Shop;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Popups;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    public sealed class FakeRecruitPageView : FakeView, IRecruitPageView
    {
        public event Action<int>? CardTapped;
        public event Action<int>? RecruitClicked;
        public event Action? RerollClicked;
        public event Action? RegistryClicked;
        public event Action<int>? ShopTabClicked;
        public int ShopTabShown = -1;
        public void ShowShopTab(int active) => ShopTabShown = active;
        public void ClickShopTab(int tab) => ShopTabClicked?.Invoke(tab);
        public readonly List<RecruitPageData> Shown = new List<RecruitPageData>();
        public RecruitPageData Last => Shown[Shown.Count - 1];
        public void Show(RecruitPageData d) => Shown.Add(d);
        public void TapCard(int slot) => CardTapped?.Invoke(slot);
        public void ClickRecruit(int slot) => RecruitClicked?.Invoke(slot);
        public void ClickReroll() => RerollClicked?.Invoke();
        public void ClickRegistry() => RegistryClicked?.Invoke();
    }

    /// <summary>Confirmation d'achat de test : garde les données demandées, répond par <see cref="Answer"/> ; Retour = non.</summary>
    public sealed class FakeRecruitConfirmPopup : FakePopup, IRecruitConfirmPopupView
    {
        UniTaskCompletionSource<bool>? _tcs;
        public RecruitConfirmData? Asked { get; private set; }
        public FakeRecruitConfirmPopup() { Cancelable = true; }

        public UniTask<bool> AskAsync(RecruitConfirmData d, CancellationToken ct)
        {
            Asked = d;
            _tcs = new UniTaskCompletionSource<bool>();
            ct.Register(() => _tcs.TrySetCanceled());
            return _tcs.Task;
        }

        public void Answer(bool ok) => _tcs?.TrySetResult(ok);
        public override void RequestCancel() { base.RequestCancel(); Answer(false); }
    }

    public sealed class PendingRecruit : IRecruitFromShopUseCase
    {
        public int Calls;
        public readonly UniTaskCompletionSource<Result> Done = new UniTaskCompletionSource<Result>();
        public UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default) { Calls++; return Done.Task; }
    }
}
