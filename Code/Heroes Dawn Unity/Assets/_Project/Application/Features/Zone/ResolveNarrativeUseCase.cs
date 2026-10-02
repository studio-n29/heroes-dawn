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
    public sealed class ResolveNarrativeUseCase : IResolveNarrativeUseCase
    {
        public const int Credits = 250;
        public const int Pr = 10;
        public const int Morale = 15;

        readonly ZoneEffects _fx;
        readonly IPublisher<RosterChanged> _roster;

        public ResolveNarrativeUseCase(ZoneEffects fx, IPublisher<RosterChanged> roster) { _fx = fx; _roster = roster; }

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(NarrativeChoice choice, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var res = ZoneActions.Apply(_fx, r => r.ResolveCase(true));
            if (res.IsFailure) return UniTask.FromResult(res);
            var s = _fx.Session;
            switch (choice)
            {
                case NarrativeChoice.Credits: s.Wallet.Add(Credits, 0); break;
                case NarrativeChoice.Research: s.Wallet.Add(0, Pr); break;
                case NarrativeChoice.Morale: foreach (var m in s.Squad) m.AddFatigue(-Morale); break;
            }
            _fx.Wallet.PublishWallet(s);
            _roster.PublishRoster();
            _fx.Save.RequestSave();
            return UniTask.FromResult(res);
        }
    }
}
