#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Features.Hub
{
    public sealed class RecruitMemberUseCase : IRecruitMemberUseCase
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;

        public RecruitMemberUseCase(GameSession session, SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster)
        {
            _s = session; _save = save; _wallet = wallet; _roster = roster;
        }

        public UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (_s.Owns(id)) return UniTask.FromResult(Result.Fail(HubErrors.AlreadyOwned));
            MemberDef? def = null;
            foreach (var d in _s.Content.Members) if (d.Id == id) def = d;
            if (def == null) return UniTask.FromResult(Result.Fail(HubErrors.UnknownMember));
            if (!_s.Wallet.TrySpend(0, MetaRules.RecruitCost(def.Rarity))) return UniTask.FromResult(Result.Fail(HubErrors.NotEnoughPr));
            var member = new TeamMember(def, MetaRules.RecruitLevel);
            member.InSquad = _s.Squad.Count < MetaRules.SquadSize && _s.Run == null;
            _s.Roster.Add(member);
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
