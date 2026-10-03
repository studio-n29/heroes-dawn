using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Zone sécurisée ; se termine quand le joueur choisit de rentrer au QG.</summary>
    public interface IZoneCompletePopupView : IPopupView
    {
        UniTask ShowAsync(ZoneCompleteData data, CancellationToken ct);
    }
}
