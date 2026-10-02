#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Déplace le pion vers une destination proposée par le domaine.</summary>
    public interface IMoveToUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(CaseId target, CancellationToken ct = default);
    }
}
