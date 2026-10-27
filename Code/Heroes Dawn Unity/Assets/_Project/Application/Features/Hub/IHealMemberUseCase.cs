#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    /// <summary>Soin d'un agent depuis sa fiche : réduit sa fatigue contre des Crédits.</summary>
    public interface IHealMemberUseCase
    {
        UniTask<Result> ExecuteAsync(MemberId member, CancellationToken ct = default);
    }
}
