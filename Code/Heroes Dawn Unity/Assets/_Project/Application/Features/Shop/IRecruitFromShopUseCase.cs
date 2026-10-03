#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    /// <summary>Recrute le héros d'un emplacement contre des PR ; les autres offres restent.</summary>
    public interface IRecruitFromShopUseCase
    {
        UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default);
    }
}
