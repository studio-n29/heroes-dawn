#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Debug : victoire immédiate (le combat ne sera pas vérifié).</summary>
    public interface IForceCombatWinUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
