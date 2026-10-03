#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Auth
{
    public sealed class ResetProgressUseCase : IResetProgressUseCase
    {
        readonly GameSession _session;
        readonly IProfileGateway _profiles;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;

        public ResetProgressUseCase(GameSession session, IProfileGateway profiles, SaveScheduler save,
                                    IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster)
        {
            _session = session; _profiles = profiles; _save = save; _wallet = wallet; _roster = roster;
        }

        public UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            if (_session.Account != null) _profiles.Delete(_session.Account.PlayerId);
            _session.ResetToNewProfile();
            _wallet.PublishWallet(_session);
            _roster.PublishRoster();
            _save.RequestSave();
            return UniTask.FromResult(Result.Ok());
        }
    }
}
