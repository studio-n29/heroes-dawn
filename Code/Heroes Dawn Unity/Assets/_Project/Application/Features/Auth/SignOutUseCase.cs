#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Auth
{
    public sealed class SignOutUseCase : ISignOutUseCase
    {
        readonly GameSession _session;
        readonly IAuthGateway _auth;
        readonly SaveScheduler _save;

        public SignOutUseCase(GameSession session, IAuthGateway auth, SaveScheduler save) { _session = session; _auth = auth; _save = save; }

        public UniTask<Result> ExecuteAsync(CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();
            _save.RequestSave();
            _auth.SignOut();
            _session.Account = null;
            _session.Run = null;
            _session.RunZone = null;
            return UniTask.FromResult(Result.Ok());
        }
    }
}
