#nullable enable
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Hub;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Shop
{
    public sealed class RecruitFromShopUseCase : IRecruitFromShopUseCase
    {
        readonly GameSession _s;
        readonly IClock _clock;
        readonly SaveScheduler _save;
        readonly IOpenShopUseCase _open;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;
        readonly IPublisher<ShopChanged> _changed;

        public RecruitFromShopUseCase(GameSession session, IClock clock, SaveScheduler save, IOpenShopUseCase open,
                                      IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster, IPublisher<ShopChanged> changed)
        {
            _s = session; _clock = clock; _save = save; _open = open; _wallet = wallet; _roster = roster; _changed = changed;
        }

        public async UniTask<Result> ExecuteAsync(int slot, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (!ShopDrawing.IsCurrent(_s, _clock))
            {
                await _open.ExecuteAsync(ct);
                return Result.Fail(ShopErrors.Expired);
            }
            var offers = _s.Shop.Offers;
            if (slot < 0 || slot >= offers.Count) return Result.Fail(ShopErrors.UnknownSlot);
            var offer = offers[slot];
            if (offer.Recruited || _s.Owns(offer.Id)) return Result.Fail(ShopErrors.AlreadyRecruited);
            var def = _s.Content.Members.FirstOrDefault(m => m.Id == offer.Id);
            if (def == null) return Result.Fail(HubErrors.UnknownMember);
            if (!_s.Wallet.TrySpend(0, MetaRules.RecruitCost(def.Rarity))) return Result.Fail(HubErrors.NotEnoughPr);

            // Comme l'ancien recrutement : dans l'escouade s'il y a de la place et pas de partie en cours.
            _s.Roster.Add(new TeamMember(def, MetaRules.RecruitLevel) { InSquad = _s.Squad.Count < MetaRules.SquadSize && _s.Run == null });
            offer.Recruited = true;
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _changed.Publish(default);
            _save.RequestSave();
            return Result.Ok();
        }
    }
}
