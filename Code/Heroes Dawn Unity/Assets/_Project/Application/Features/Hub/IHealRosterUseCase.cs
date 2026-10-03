#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    /// <summary>Infirmerie : réduit la fatigue de tout l'effectif contre des Crédits.</summary>
    public interface IHealRosterUseCase
    {
        UniTask<Result> ExecuteAsync(CancellationToken ct = default);
    }
}
