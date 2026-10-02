#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Utilise un dé bonus de la réserve.</summary>
    public interface IUseBonusDieUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(CancellationToken ct = default);
    }
}
