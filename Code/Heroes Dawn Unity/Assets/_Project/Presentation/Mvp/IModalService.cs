using System.Threading;
using Cysharp.Threading.Tasks;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Modales : une seule affichée à la fois, les suivantes attendent leur tour (file).</summary>
    public interface IModalService
    {
        bool AnyOpen { get; }

        /// <summary>Ouvre une modale dès que c'est son tour ; annulée en attente, elle n'est jamais créée.</summary>
        UniTask<TView> OpenAsync<TView>(CancellationToken ct) where TView : class, IPopupView;

        /// <summary>Ferme la modale affichée, puis ouvre la suivante de la file.</summary>
        void Close(IPopupView view);

        /// <summary>Retour : annule la modale affichée si elle est annulable.</summary>
        bool TryCancelTop();

        UniTask<bool> ConfirmAsync(ConfirmData data, CancellationToken ct);
    }
}
