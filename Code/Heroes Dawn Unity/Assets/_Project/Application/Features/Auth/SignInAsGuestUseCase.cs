#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Auth
{
    public sealed class SignInAsGuestUseCase : ISignInAsGuestUseCase
    {
        readonly GameSession _session;
        readonly IAuthGateway _auth;
        readonly IProfileGateway _profiles;
        readonly IClock _clock;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;
        readonly ILogger _log;

        public SignInAsGuestUseCase(GameSession session, IAuthGateway auth, IProfileGateway profiles, IClock clock,
                                    SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster, ILogger log)
        {
            _session = session; _auth = auth; _profiles = profiles; _clock = clock; _save = save; _wallet = wallet; _roster = roster; _log = log;
        }

        public async UniTask<Result<SignInReport>> ExecuteAsync(CancellationToken ct = default)
        {
            Result<PlayerAccount> auth;
            try
            {
                auth = await _auth.SignInAsGuestAsync(ct);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e)
            {
                _log.Error("Connexion : " + e.Message);
                return Result<SignInReport>.Fail(new DomainError("auth.network", "errors.network"));
            }
            if (auth.IsFailure) return Result<SignInReport>.Fail(auth.Error!);

            var account = auth.Value;
            ProfileData? data;
            try
            {
                data = await _profiles.LoadAsync(account.PlayerId, ct);
            }
            catch (OperationCanceledException) { throw; }
            catch (Exception e)
            {
                _log.Error("Chargement du profil : " + e.Message);
                return Result<SignInReport>.Fail(new DomainError("profile.load", "errors.profile_load"));
            }

            _session.Account = account;
            int recovered = 0;
            bool isNew = data == null;
            if (data == null) _session.ResetToNewProfile();
            else
            {
                _session.ApplyProfile(data);
                recovered = FatigueRecovery.Apply(_session.Roster, _clock.UtcNow, data.lastSeenUtcTicks, Meta.LabBonuses.RecoveryPerMinute(_session.Meta));
            }
            _log.Info($"Connecté : {account.DisplayName} ({account.PlayerId}) — profil {(isNew ? "nouveau" : "chargé")}");
            _wallet.PublishWallet(_session);
            _roster.PublishRoster();
            await _save.SaveAsync(ct);
            return Result<SignInReport>.Ok(new SignInReport(isNew, recovered));
        }
    }
}
