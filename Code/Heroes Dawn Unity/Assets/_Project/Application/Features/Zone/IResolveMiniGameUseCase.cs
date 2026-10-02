#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Mini-jeu terminé : récompense en cas de réussite ; la case reste jouable en cas d'échec.</summary>
    public interface IResolveMiniGameUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(bool success, CancellationToken ct = default);
    }
}
