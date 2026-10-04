#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    public sealed class MarkShopSeenUseCase : IMarkShopSeenUseCase
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<ShopChanged> _changed;

        public MarkShopSeenUseCase(GameSession session, SaveScheduler save, IPublisher<ShopChanged> changed)
        {
            _s = session; _save = save; _changed = changed;
        }

        public UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (_s.Shop.Day.Length == 0 || _s.Shop.SeenDay == _s.Shop.Day) return UniTask.FromResult(Result.Ok());
            _s.Shop.SetSeenDay(_s.Shop.Day);
            _changed.Publish(default);
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
