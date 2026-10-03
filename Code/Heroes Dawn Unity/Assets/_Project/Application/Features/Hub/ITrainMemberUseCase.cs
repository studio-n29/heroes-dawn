#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    /// <summary>Entraîne un membre : +1 niveau contre des Crédits.</summary>
    public interface ITrainMemberUseCase
    {
        UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default);
    }
}
