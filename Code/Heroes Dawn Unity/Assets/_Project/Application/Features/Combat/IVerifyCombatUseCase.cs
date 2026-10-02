#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Vérification du combat par rejeu (null si impossible ou si le debug a été utilisé).</summary>
    public interface IVerifyCombatUseCase
    {
        UniTask<bool?> ExecuteAsync(CancellationToken ct = default);
    }
}
