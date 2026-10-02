#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Prépare le combat de la case en attente : escouade apte, ennemis au niveau de l'escouade + écart du rang.</summary>
    public interface IBeginCombatUseCase
    {
        UniTask<Result<CombatSimulation>> ExecuteAsync(CancellationToken ct = default);
    }
}
