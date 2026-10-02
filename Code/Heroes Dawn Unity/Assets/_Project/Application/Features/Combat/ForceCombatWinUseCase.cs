#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Combat
{
    public sealed class ForceCombatWinUseCase : IForceCombatWinUseCase
    {
        readonly ActiveCombat _active;
        public ForceCombatWinUseCase(ActiveCombat active) { _active = active; }

        public UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var sim = _active.Simulation;
            if (sim == null || sim.IsOver) return UniTask.FromResult(Result.Ok());
            _active.MarkDebugUsed();
            sim.ForceEnemiesDefeatedForDebug();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
