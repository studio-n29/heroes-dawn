#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public sealed class ResolveMiniGameUseCase : IResolveMiniGameUseCase
    {
        readonly ZoneEffects _fx;
        public ResolveMiniGameUseCase(ZoneEffects fx) { _fx = fx; }

        public UniTask<Result<IReadOnlyList<BoardEvent>>> ExecuteAsync(bool success, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var res = ZoneActions.Apply(_fx, r => r.ResolveCase(success));
            if (res.IsSuccess && success)
            {
                var b = _fx.Session.Content.Balance;
                _fx.Session.Wallet.Add(b.MiniGameCredits, b.MiniGamePr);
                _fx.Wallet.PublishWallet(_fx.Session);
                _fx.Save.RequestSave();
            }
            return UniTask.FromResult(res);
        }
    }
}
