#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Confirmation d'achat (annulable : Retour = Annuler) ; vrai = acheter.</summary>
    public interface IRecruitConfirmPopupView : IPopupView
    {
        UniTask<bool> AskAsync(RecruitConfirmData data, CancellationToken ct);
    }
}
