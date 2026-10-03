#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Domain.Shop;

namespace N29.HeroesDawn.Application.Features.Shop
{
    public sealed class RerollShopUseCase : IRerollShopUseCase
    {
        readonly GameSession _s;
        readonly IClock _clock;
        readonly SaveScheduler _save;
        readonly IOpenShopUseCase _open;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<ShopChanged> _changed;

        public RerollShopUseCase(GameSession session, IClock clock, SaveScheduler save, IOpenShopUseCase open,
                                 IPublisher<WalletChanged> wallet, IPublisher<ShopChanged> changed)
        {
            _s = session; _clock = clock; _save = save; _open = open; _wallet = wallet; _changed = changed;
        }

        public async UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (!ShopDrawing.IsCurrent(_s, _clock))
            {
                // Le renouvellement gratuit du nouveau jour n'est pas consommé.
                await _open.ExecuteAsync(ct);
                return Result.Fail(ShopErrors.Expired);
            }
            int cost = ShopRules.RerollCost(_s.Shop.Draw, _s.Content.Balance);
            if (cost > 0 && !_s.Wallet.TrySpend(cost, 0)) return Result.Fail(HubErrors.NotEnoughCredits);

            ShopDrawing.Redraw(_s, _s.Shop.Day, _s.Shop.Draw + 1);
            if (cost > 0) _wallet.PublishWallet(_s);
            _changed.Publish(default);
            _save.RequestSave();
            return Result.Ok();
        }
    }
}
