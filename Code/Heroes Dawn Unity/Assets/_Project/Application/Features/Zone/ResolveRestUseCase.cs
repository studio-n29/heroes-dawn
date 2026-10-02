#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public sealed class ResolveRestUseCase : IResolveRestUseCase
    {
        readonly ZoneEffects _fx;
        readonly IPublisher<RosterChanged> _roster;

        public ResolveRestUseCase(ZoneEffects fx, IPublisher<RosterChanged> roster) { _fx = fx; _roster = roster; }

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(bool take, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var res = ZoneActions.Apply(_fx, r => r.ResolveCase(take));
            if (res.IsSuccess && take)
            {
                int reduction = _fx.Session.Content.Balance.RestFatigueReduction;
                foreach (var m in _fx.Session.Squad) m.AddFatigue(-reduction);
                _roster.PublishRoster();
                _fx.Save.RequestSave();
            }
            return UniTask.FromResult(res);
        }
    }
}
