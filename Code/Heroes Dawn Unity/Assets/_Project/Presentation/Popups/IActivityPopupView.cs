using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>
    /// Fenêtre d'une activité jouée depuis une case Mini-jeu (hors réflexe) ; renvoie le résultat.
    /// Version de test : décrit l'activité et son niveau généré, le joueur simule le résultat. Les activités complètes (3D, 2D)
    /// la remplaceront type par type.
    /// </summary>
    public interface IActivityPopupView : IPopupView
    {
        UniTask<ActivityResult> PlayAsync(ActivityPopupData data, CancellationToken ct);
    }
}
