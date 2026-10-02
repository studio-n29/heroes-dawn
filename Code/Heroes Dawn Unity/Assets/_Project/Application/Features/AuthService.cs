#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features
{
    public sealed class SignInReport
    {
        public bool NewProfile { get; }
        public int FatigueRecovered { get; }
        public SignInReport(bool newProfile, int fatigueRecovered) { NewProfile = newProfile; FatigueRecovered = fatigueRecovered; }
    }

    /// <summary>Connexion invité → chargement du profil → récupération passive de fatigue → sauvegarde.</summary>
    public sealed class AuthService
    {
        readonly GameSession _session;
        readonly IAuthGateway _auth;
        readonly IProfileGateway _profiles;
        readonly IClock _clock;
        readonly SaveScheduler _save;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;
        readonly ILogger _log;

        public AuthService(GameSession session, IAuthGateway auth, IProfileGateway profiles, IClock clock,
                           SaveScheduler save, IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster, ILogger log)
        {
            _session = session; _auth = auth; _profiles = profiles; _clock = clock; _save = save; _wallet = wallet; _roster = roster; _log = log;
        }

        public bool IsSignedIn => _session.Account != null;

        public async Task<Result<SignInReport>> SignInAsGuestAsync(CancellationToken ct)
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
                recovered = RecoverFatigue(data.lastSeenUtcTicks);
            }
            _log.Info($"Connecté : {account.DisplayName} ({account.PlayerId}) — profil {(isNew ? "nouveau" : "chargé")}");
            _wallet.PublishWallet(_session);
            _roster.PublishRoster();
            await _save.SaveAsync(ct);
            return Result<SignInReport>.Ok(new SignInReport(isNew, recovered));
        }

        int RecoverFatigue(long lastSeenTicks)
        {
            if (lastSeenTicks <= 0) return 0;
            var elapsed = _clock.UtcNow - new DateTime(lastSeenTicks, DateTimeKind.Utc);
            if (elapsed.TotalMinutes < 1) return 0;
            int points = (int)Math.Min(100, elapsed.TotalMinutes * MetaRules.FatigueRecoveryPerMinute);
            int total = 0;
            foreach (var m in _session.Roster)
            {
                int before = m.Fatigue;
                m.AddFatigue(-points);
                total += before - m.Fatigue;
            }
            return total;
        }

        public void SignOut()
        {
            _save.RequestSave();
            _auth.SignOut();
            _session.Account = null;
            _session.Run = null;
            _session.RunZone = null;
        }

        /// <summary>Efface la sauvegarde locale du joueur courant et repart d'un profil neuf.</summary>
        public void ResetProgress()
        {
            if (_session.Account != null) _profiles.Delete(_session.Account.PlayerId);
            _session.ResetToNewProfile();
            _wallet.PublishWallet(_session);
            _roster.PublishRoster();
            _save.RequestSave();
        }
    }
}
