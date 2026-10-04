#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Infrastructure.Simulated
{
    /// <summary>Placeholder des achats en magasin (M2) : aucun débit, succès après une courte latence.</summary>
    public sealed class FakePurchaseGateway : IPurchaseGateway
    {
        readonly int _latencyMs;

        /// <summary>Force l'échec du prochain paiement (tests).</summary>
        public bool FailNext { get; set; }

        public FakePurchaseGateway(int latencyMs = 800) { _latencyMs = latencyMs; }

        public async UniTask<Result> PurchaseAsync(string productId, CancellationToken ct)
        {
            await UniTask.Delay(_latencyMs, ignoreTimeScale: true, cancellationToken: ct);
            if (FailNext)
            {
                FailNext = false;
                return Result.Fail(MetaErrors.PaymentFailed);
            }
            return Result.Ok();
        }
    }
}
