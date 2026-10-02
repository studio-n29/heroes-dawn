#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public sealed class StartZoneUseCase : IStartZoneUseCase
    {
        readonly GameSession _s;
        readonly ISeedProvider _seeds;
        readonly ILogger _log;

        public StartZoneUseCase(GameSession session, ISeedProvider seeds, ILogger log) { _s = session; _seeds = seeds; _log = log; }

        public UniTask<Result> ExecuteAsync(ZoneId zoneId, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            ZoneDef? zone = null;
            foreach (var z in _s.Content.Zones) if (z.Id == zoneId) zone = z;
            if (zone == null) return UniTask.FromResult(Result.Fail(ZoneErrors.UnknownZone));
            if (_s.SquadExhausted) return UniTask.FromResult(Result.Fail(ZoneErrors.SquadExhausted));
            ulong seed = _seeds.NextSeed("zone:" + zoneId.Value);
            _s.Run = new ZoneRun(_s.Content, zone, seed);
            _s.RunZone = zone;
            _log.Info($"Zone {zoneId} démarrée (graine {seed})");
            return UniTask.FromResult(Result.Ok());
        }
    }
}
