using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Résultats du combat. Annulable : Retour vaut « Continuer ».</summary>
    public interface ICombatResultsPopupView : IPopupView
    {
        /// <summary>Affiche les résultats ; se termine sur « Continuer » (ou Retour).</summary>
        UniTask ShowAsync(CombatResultsData data, CancellationToken ct);
        void SetVerification(string text, Tone tone);
    }
}
