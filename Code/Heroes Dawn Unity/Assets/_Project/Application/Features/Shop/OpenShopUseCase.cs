#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    public sealed class OpenShopUseCase : IOpenShopUseCase
    {
        readonly GameSession _s;
        readonly IClock _clock;
        readonly SaveScheduler _save;
        readonly IPublisher<ShopChanged> _changed;

        public OpenShopUseCase(GameSession session, IClock clock, SaveScheduler save, IPublisher<ShopChanged> changed)
        {
            _s = session; _clock = clock; _save = save; _changed = changed;
        }

        public UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (ShopDrawing.IsCurrent(_s, _clock)) return UniTask.FromResult(Result.Ok());
            ShopDrawing.Redraw(_s, ShopDrawing.Today(_s, _clock), 0);
            _changed.Publish(default);
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
