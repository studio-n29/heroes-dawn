using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Repos ; renvoie true pour se reposer.</summary>
    public interface IRestPopupView : IPopupView
    {
        UniTask<bool> ChooseAsync(RestPopupData data, CancellationToken ct);
    }
}
