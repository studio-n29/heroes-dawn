#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Case Repos : se reposer (fatigue de l'escouade réduite) ou passer (la case reste disponible).</summary>
    public interface IResolveRestUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(bool take, CancellationToken ct = default);
    }
}
