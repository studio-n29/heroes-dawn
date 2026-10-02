#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Applique l'issue du combat terminé : Crédits, fatigue, plateau, retour au QG si l'escouade est épuisée.</summary>
    public interface IFinishCombatUseCase
    {
        UniTask<Result<CombatReport>> ExecuteAsync(CancellationToken ct = default);
    }
}
