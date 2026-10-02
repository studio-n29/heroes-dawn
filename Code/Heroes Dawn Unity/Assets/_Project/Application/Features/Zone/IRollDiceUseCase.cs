#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Lance le dé de déplacement.</summary>
    public interface IRollDiceUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(CancellationToken ct = default);
    }
}
