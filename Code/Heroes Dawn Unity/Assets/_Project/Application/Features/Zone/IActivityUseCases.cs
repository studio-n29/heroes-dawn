#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Prépare l'activité de la case Mini-jeu courante (niveau généré, environnement du plateau, patient éventuel).</summary>
    public interface IPlanActivityUseCase
    {
        Result<ActivityPlan> Execute();
    }

    /// <summary>
    /// Activité jouée : applique ses gains (pièces au prorata du score, même en cas d'échec), le malus d'agent éventuel et la suite
    /// (combat ou autre activité) ; la case reste jouable en cas d'échec.
    /// </summary>
    public interface IResolveActivityUseCase
    {
        UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(ActivityResult result, CancellationToken ct = default);
    }
}
