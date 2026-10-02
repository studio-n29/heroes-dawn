#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public sealed class EndMoveUseCase : IEndMoveUseCase
    {
        readonly ZoneEffects _fx;
        public EndMoveUseCase(ZoneEffects fx) { _fx = fx; }

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            return UniTask.FromResult(ZoneActions.Apply(_fx, r => r.EndMove()));
        }
    }
}
