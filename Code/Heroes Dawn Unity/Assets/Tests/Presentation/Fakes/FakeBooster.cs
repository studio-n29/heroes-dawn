#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Ouverture de booster de test : garde les données montrées, se termine sur <see cref="Continue"/> (ou Retour).</summary>
    public sealed class FakeBoosterOpeningPopup : FakePopup, IBoosterOpeningPopupView
    {
        UniTaskCompletionSource? _done;
        public BoosterOpeningData? Shown { get; private set; }
        public FakeBoosterOpeningPopup() { Cancelable = true; }

        public UniTask ShowAsync(BoosterOpeningData data, CancellationToken ct)
        {
            Shown = data;
            _done = new UniTaskCompletionSource();
            return _done.Task;
        }

        public void Continue() => _done?.TrySetResult();
        public override void RequestCancel() { base.RequestCancel(); Continue(); }
    }
}
