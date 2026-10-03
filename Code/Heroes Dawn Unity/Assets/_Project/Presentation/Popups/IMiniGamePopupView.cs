using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Mini-jeu ; renvoie true si le joueur a réussi.</summary>
    public interface IMiniGamePopupView : IPopupView
    {
        UniTask<bool> PlayAsync(MiniGamePopupData data, CancellationToken ct);
    }
}
