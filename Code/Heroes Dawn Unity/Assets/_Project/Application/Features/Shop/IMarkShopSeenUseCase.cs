#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    /// <summary>Retient que la boutique du jour a été vue (efface le badge « N » de l'onglet Recruter).</summary>
    public interface IMarkShopSeenUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
