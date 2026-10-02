#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Bascule le mode auto (mémorisé dans le profil, appliqué au combat en cours).</summary>
    public interface ISetAutoCombatUseCase
    {
        UniTask<Result> ExecuteAsync(bool on, CancellationToken ct = default);
    }
}
