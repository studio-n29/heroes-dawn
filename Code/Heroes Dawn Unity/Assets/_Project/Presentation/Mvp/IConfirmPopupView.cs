using System.Threading;
using Cysharp.Threading.Tasks;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Confirmation oui/non ; Retour = non.</summary>
    public interface IConfirmPopupView : IPopupView
    {
        UniTask<bool> AskAsync(ConfirmData data, CancellationToken ct);
    }
}
