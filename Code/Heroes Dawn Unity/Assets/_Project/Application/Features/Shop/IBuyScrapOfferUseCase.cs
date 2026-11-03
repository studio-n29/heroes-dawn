#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    /// <summary>Achète un lot de scraps de la boutique du jour (paiement en Crédits).</summary>
    public interface IBuyScrapOfferUseCase
    {
        UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default);
    }
}
