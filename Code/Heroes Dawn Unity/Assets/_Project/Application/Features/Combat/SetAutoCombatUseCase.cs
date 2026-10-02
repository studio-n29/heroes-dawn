#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    public sealed class SetAutoCombatUseCase : ISetAutoCombatUseCase
    {
        readonly GameSession _s;
        readonly ActiveCombat _active;
        readonly SaveScheduler _save;

        public SetAutoCombatUseCase(GameSession session, ActiveCombat active, SaveScheduler save) { _s = session; _active = active; _save = save; }

        public UniTask<Result> ExecuteAsync(bool on, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _s.AutoCombat = on;
            _active.Simulation?.Submit(CombatCommand.SetAuto(on));
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
