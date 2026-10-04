#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Paiement d'un produit en argent réel (M1 : simulé, aucun débit ; M2 : stores / UGS).</summary>
    public interface IPurchaseGateway
    {
        UniTask<Result> PurchaseAsync(string productId, CancellationToken ct);
    }

    public sealed class GemPack
    {
        public string Id { get; }
        public int Gems { get; }
        public string PriceText { get; }
        public GemPack(string id, int gems, string priceText) { Id = id; Gems = gems; PriceText = priceText; }
    }

    public sealed class GemExchange
    {
        public string Id { get; }
        public int GemCost { get; }
        public Reward Reward { get; }
        /// <summary>Repos complet : toute l'équipe retombe à 0 de fatigue.</summary>
        public bool FullRest { get; }
        public GemExchange(string id, int gemCost, Reward reward, bool fullRest = false) { Id = id; GemCost = gemCost; Reward = reward; FullRest = fullRest; }
    }

    /// <summary>Boutique : paquets de Gemmes (achat simulé), échanges Gemmes → ressources, cadeau quotidien gratuit.</summary>
    public sealed class Boutique
    {
        public static readonly Reward DailyGift = new Reward(credits: 100, gems: 1);

        public static readonly IReadOnlyList<GemPack> Packs = new[]
        {
            new GemPack("gems_100", 100, "0,99 €"),
            new GemPack("gems_550", 550, "4,99 €"),
            new GemPack("gems_1200", 1200, "9,99 €"),
        };

        public static readonly IReadOnlyList<GemExchange> Exchanges = new[]
        {
            new GemExchange("ex_credits", 20, new Reward(credits: 1000)),
            new GemExchange("ex_pr", 30, new Reward(pr: 60)),
            new GemExchange("ex_rest", 10, new Reward(), fullRest: true),
        };

        readonly MetaContext _c;
        readonly IPurchaseGateway _payments;
        readonly IPublisher<RosterChanged> _roster;

        public Boutique(MetaContext context, IPurchaseGateway payments, IPublisher<RosterChanged> roster)
        {
            _c = context; _payments = payments; _roster = roster;
        }

        public bool DailyGiftAvailable => _c.Meta.dailyGiftDay != _c.Today;

        public Result<Reward> ClaimDailyGift()
        {
            if (!DailyGiftAvailable) return Result<Reward>.Fail(MetaErrors.AlreadyClaimed);
            _c.Meta.dailyGiftDay = _c.Today;
            _c.Grant(DailyGift);
            _c.Touch(MetaArea.Boutique);
            return Result<Reward>.Ok(DailyGift);
        }

        public async UniTask<Result<int>> BuyPackAsync(string packId, CancellationToken ct)
        {
            GemPack? pack = null;
            foreach (var p in Packs) if (p.Id == packId) pack = p;
            if (pack == null) return Result<int>.Fail(MetaErrors.Unknown);
            var paid = await _payments.PurchaseAsync(pack.Id, ct);
            ct.ThrowIfCancellationRequested();
            if (paid.IsFailure) return Result<int>.Fail(paid.Error!);
            _c.Grant(new Reward(gems: pack.Gems));
            _c.Meta.SetCounter(_c.Meta.purchases, pack.Id, _c.Meta.GetCounter(_c.Meta.purchases, pack.Id) + 1);
            _c.Touch(MetaArea.Boutique);
            return Result<int>.Ok(pack.Gems);
        }

        public Result Exchange(string id)
        {
            GemExchange? ex = null;
            foreach (var e in Exchanges) if (e.Id == id) ex = e;
            if (ex == null) return Result.Fail(MetaErrors.Unknown);
            if (ex.FullRest)
            {
                bool tired = false;
                foreach (var m in _c.Session.Roster) if (m.Fatigue > 0) tired = true;
                if (!tired) return Result.Fail(Hub.HubErrors.NothingToHeal);
            }
            if (!_c.Session.Wallet.TrySpendGems(ex.GemCost)) return Result.Fail(MetaErrors.NotEnoughGems);
            if (ex.FullRest)
            {
                foreach (var m in _c.Session.Roster) m.Fatigue = 0;
                _roster.PublishRoster();
            }
            _c.Grant(ex.Reward);
            _c.Touch(MetaArea.Boutique);
            return Result.Ok();
        }
    }
}
