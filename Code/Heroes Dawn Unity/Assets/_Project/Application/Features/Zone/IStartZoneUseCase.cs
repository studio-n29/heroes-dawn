#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Ouvre une partie sur une zone (nouvelle graine).</summary>
    public interface IStartZoneUseCase
    {
        UniTask<Result> ExecuteAsync(ZoneId zoneId, CancellationToken ct = default);
    }
}
