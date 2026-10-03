#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    /// <summary>Ajoute ou retire un membre de l'escouade (1 à 4 membres, hors mission).</summary>
    public interface IToggleSquadUseCase
    {
        UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default);
    }
}
