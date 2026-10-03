#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace N29.HeroesDawn.Application.Flow
{
    /// <summary>Navigation du jeu : seule autorité qui charge, décharge, affiche ou masque les scènes.</summary>
    public interface IGameFlow
    {
        GameState Current { get; }
        bool IsTransitioning { get; }
        /// <summary>Demande une transition ; false si elle est refusée, ignorée (déjà en cours, état courant) ou en échec.</summary>
        UniTask<bool> RequestAsync(GameState target, CancellationToken ct = default);
    }
}
