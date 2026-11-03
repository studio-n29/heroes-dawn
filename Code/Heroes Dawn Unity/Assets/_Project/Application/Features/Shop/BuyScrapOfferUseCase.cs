#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    public sealed class BuyScrapOfferUseCase : IBuyScrapOfferUseCase
    {
        readonly GameSession _s;
        readonly IClock _clock;
        readonly SaveScheduler _save;
        readonly IOpenShopUseCase _open;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<ShopChanged> _changed;

        public BuyScrapOfferUseCase(GameSession session, IClock clock, SaveScheduler save, IOpenShopUseCase open,
                                    IPublisher<WalletChanged> wallet, IPublisher<ShopChanged> changed)
        {
            _s = session; _clock = clock; _save = save; _open = open; _wallet = wallet; _changed = changed;
        }

        public async UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (!ShopDrawing.IsCurrent(_s, _clock))
            {
                await _open.ExecuteAsync(ct);
                return Result.Fail(ShopErrors.Expired);
            }
            var lots = _s.Shop.Scraps;
            if (slot < 0 || slot >= lots.Count) return Result.Fail(ShopErrors.UnknownSlot);
            var lot = lots[slot];
            if (lot.Bought) return Result.Fail(ShopErrors.AlreadyBought);
            if (!_s.Wallet.TrySpend(lot.Price, 0)) return Result.Fail(HubErrors.NotEnoughCredits);

            CraftingOps.AddScraps(_s, lot.Kind, lot.Count);
            lot.Bought = true;
            _wallet.PublishWallet(_s);
            _changed.Publish(default);
            _save.RequestSave();
            return Result.Ok();
        }
    }
}
