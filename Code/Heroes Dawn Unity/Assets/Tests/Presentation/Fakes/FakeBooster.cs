#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Hub;

namespace N29.HeroesDawn.Tests.Presentation.Fakes
{
    /// <summary>Ouverture de booster de test : garde les données montrées, se termine sur <see cref="Continue"/> (ou Retour) ou <see cref="OpenAnother"/>.</summary>
    public sealed class FakeBoosterOpeningPopup : FakePopup, IBoosterOpeningPopupView
    {
        UniTaskCompletionSource<bool>? _done;
        public BoosterOpeningData? Shown { get; private set; }
        /// <summary>Toutes les ouvertures montrées dans cette fenêtre (une par booster si le joueur en enchaîne).</summary>
        public int ShownCount { get; private set; }
        public FakeBoosterOpeningPopup() { Cancelable = true; }

        public UniTask<bool> ShowAsync(BoosterOpeningData data, CancellationToken ct)
        {
            Shown = data; ShownCount++;
            _done = new UniTaskCompletionSource<bool>();
            return _done.Task;
        }

        public void Continue() => _done?.TrySetResult(false);
        /// <summary>Le joueur demande « Ouvrir un autre booster ».</summary>
        public void OpenAnother() => _done?.TrySetResult(true);
        public override void RequestCancel() { base.RequestCancel(); Continue(); }
    }
}
