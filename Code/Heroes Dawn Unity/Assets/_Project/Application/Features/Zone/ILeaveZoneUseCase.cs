#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Quitte la partie en cours, terminée ou non : le brouillard se referme, les récompenses déjà gagnées restent.</summary>
    public interface ILeaveZoneUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
