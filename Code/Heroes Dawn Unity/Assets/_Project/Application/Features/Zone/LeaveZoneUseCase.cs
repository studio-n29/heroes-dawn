#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public sealed class LeaveZoneUseCase : ILeaveZoneUseCase
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly ILogger _log;

        public LeaveZoneUseCase(GameSession session, SaveScheduler save, ILogger log) { _s = session; _save = save; _log = log; }

        public UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (_s.Run == null) return UniTask.FromResult(Result.Ok());
            _log.Info("Zone quittée");
            _s.Run = null;
            _s.RunZone = null;
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
