#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public sealed class PlanActivityUseCase : IPlanActivityUseCase
    {
        readonly ZoneEffects _fx;
        public PlanActivityUseCase(ZoneEffects fx) { _fx = fx; }

        public Result<ActivityPlan> Execute()
        {
            var run = _fx.Session.Run;
            if (run == null) return Result<ActivityPlan>.Fail(ZoneErrors.NoRun);
            try
            {
                var squad = _fx.Session.Squad.Select(m => m.Id).ToList();
                return Result<ActivityPlan>.Ok(run.PlanActivity(squad));
            }
            catch (InvalidOperationException e)
            {
                _fx.Log.Warn("Activité refusée : " + e.Message);
                return Result<ActivityPlan>.Fail(ZoneErrors.NotAllowed);
            }
        }
    }

    public sealed class ResolveActivityUseCase : IResolveActivityUseCase
    {
        readonly ZoneEffects _fx;
        public ResolveActivityUseCase(ZoneEffects fx) { _fx = fx; }

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(ActivityResult result, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var res = ZoneActions.Apply(_fx, r => r.ResolveActivity(result));
            if (res.IsSuccess)
            {
                foreach (var e in res.Value)
                {
                    if (!(e is ActivityResolvedEvent ar) || (ar.Outcome.Credits == 0 && ar.Outcome.Pr == 0)) continue;
                    _fx.Session.Wallet.Add(ar.Outcome.Credits, ar.Outcome.Pr);
                    _fx.Wallet.PublishWallet(_fx.Session);
                }
                _fx.Save.RequestSave();
            }
            return UniTask.FromResult(res);
        }
    }

    /// <summary>Ancien mini-jeu (réussite / échec) : même chemin que les autres activités, avec l'activité par défaut de la case.</summary>
    public sealed class ResolveMiniGameUseCase : IResolveMiniGameUseCase
    {
        readonly IResolveActivityUseCase _activity;
        public ResolveMiniGameUseCase(IResolveActivityUseCase activity) { _activity = activity; }

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(bool success, CancellationToken ct = default)
            => _activity.ExecuteAsync(new ActivityResult(success), ct);
    }
}
