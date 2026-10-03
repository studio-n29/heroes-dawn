#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    /// <summary>Ouvre la boutique : nouveau tirage si le jour de boutique a changé (9h00 locale), sinon rien.</summary>
    public interface IOpenShopUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
