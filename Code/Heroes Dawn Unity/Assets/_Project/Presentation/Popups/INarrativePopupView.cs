using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Événement ; renvoie le choix du joueur.</summary>
    public interface INarrativePopupView : IPopupView
    {
        UniTask<NarrativeChoice> ChooseAsync(NarrativePopupData data, CancellationToken ct);
    }
}
