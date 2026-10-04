#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    public sealed class TrainMemberUseCase : ITrainMemberUseCase
    {
        readonly GameSession _s;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;

        public TrainMemberUseCase(GameSession session, SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster)
        {
            _s = session; _save = save; _wallet = wallet; _roster = roster;
        }

        public UniTask<Result> ExecuteAsync(MemberId id, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            var m = _s.Find(id);
            if (m == null) return UniTask.FromResult(Result.Fail(HubErrors.UnknownMember));
            if (m.Level >= MetaRules.MaxLevel) return UniTask.FromResult(Result.Fail(HubErrors.MaxLevel));
            if (!_s.Wallet.TrySpend(_s.TrainCostFor(m.Level), 0)) return UniTask.FromResult(Result.Fail(HubErrors.NotEnoughCredits));
            m.Level++;
            _wallet.PublishWallet(_s);
            _roster.PublishRoster();
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
