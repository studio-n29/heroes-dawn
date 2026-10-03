#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    /// <summary>Renouvelle la boutique : gratuit une fois par jour, puis payant en Crédits.</summary>
    public interface IRerollShopUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
