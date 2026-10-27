#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    public sealed class HealMemberUseCase : IHealMemberUseCase
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;

        public HealMemberUseCase(GameSession session, SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster)
        {
            _s = session; _save = save; _wallet = wallet; _roster = roster;
        }

        public UniTask<Result> ExecuteAsync(MemberId member, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var m = _s.Find(member);
            if (m == null) return UniTask.FromResult(Result.Fail(HubErrors.UnknownMember));
            if (m.Fatigue <= 0) return UniTask.FromResult(Result.Fail(HubErrors.NothingToHeal));
            if (!_s.Wallet.TrySpend(MetaRules.HealMemberCost, 0)) return UniTask.FromResult(Result.Fail(HubErrors.NotEnoughCredits));
            m.AddFatigue(-MetaRules.InfirmaryReduction);
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
