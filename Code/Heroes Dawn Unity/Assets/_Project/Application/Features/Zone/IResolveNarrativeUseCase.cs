#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Choix d'un événement narratif : Crédits, recherche (PR) ou moral (fatigue de l'escouade).</summary>
    public interface IResolveNarrativeUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(NarrativeChoice choice, CancellationToken ct = default);
    }
}
