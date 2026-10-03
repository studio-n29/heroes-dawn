#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    public sealed class ToggleSquadUseCase : IToggleSquadUseCase
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<RosterChanged> _roster;

        public ToggleSquadUseCase(GameSession session, SaveScheduler save, IPublisher<RosterChanged> roster)
        {
            _s = session; _save = save; _roster = roster;
        }

        public UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (_s.Run != null) return UniTask.FromResult(Result.Fail(HubErrors.InMission));
            var m = _s.Find(id);
            if (m == null) return UniTask.FromResult(Result.Fail(HubErrors.UnknownMember));
            int count = _s.Squad.Count;
            if (m.InSquad)
            {
                if (count <= 1) return UniTask.FromResult(Result.Fail(HubErrors.SquadEmpty));
                m.InSquad = false;
            }
            else
            {
                if (count >= MetaRules.SquadSize) return UniTask.FromResult(Result.Fail(HubErrors.SquadFull));
                m.InSquad = true;
            }
            _roster.PublishRoster();
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
