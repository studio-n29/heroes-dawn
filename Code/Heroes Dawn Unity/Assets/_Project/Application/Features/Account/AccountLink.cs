#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Account
{
    /// <summary>
    /// Compte du joueur : connexion (e-mail, Google, Apple), rattachement de la progression de l'appareil au compte, choix quand les deux ont avancé,
    /// session restaurée au lancement, déconnexion. Règle d'or : rien n'est jamais écrasé sans qu'une sauvegarde de secours de l'autre version existe,
    /// et une version du compte plus récente que celle du jeu n'est jamais appliquée.
    /// </summary>
    public sealed class AccountLink
    {
        readonly IAccountGateway _accounts;
        readonly ICloudProfileGateway _cloud;
        readonly GameSession _session;
        readonly SaveScheduler _save;
        readonly CloudSync _sync;
        readonly ICloudSyncStateStore _state;
        readonly IProgressBackupStore _backups;
        readonly IClock _clock;
        readonly ILogger _log;
        readonly IPublisher<WalletChanged> _wallet;
        readonly IPublisher<RosterChanged> _roster;

        UniTaskCompletionSource<Result<LinkOutcome?>>? _pendingAuth;
        string _pendingAuthKey = "";
        CloudProfile? _pendingCloud;

        public AccountLink(IAccountGateway accounts, ICloudProfileGateway cloud, GameSession session, SaveScheduler save, CloudSync sync,
                              ICloudSyncStateStore state, IProgressBackupStore backups, IClock clock, ILogger log,
                              IPublisher<WalletChanged> wallet, IPublisher<RosterChanged> roster)
        {
            _accounts = accounts; _cloud = cloud; _session = session; _save = save; _sync = sync; _state = state; _backups = backups;
            _clock = clock; _log = log; _wallet = wallet; _roster = roster;
            _sync.ConflictDetected += () => OnSyncConflictAsync().Forget();
            _sync.SessionLost += OnSessionLost;
        }

        /// <summary>Le compte connecté, ou <c>null</c> (joueur invité).</summary>
        public PlayerAccount? Cloud { get; private set; }
        public AccountState State => Cloud != null ? AccountState.Connected : AccountState.Guest;
        /// <summary>Le choix de progression à faire (rien n'est écrit tant qu'il n'est pas fait), ou <c>null</c>.</summary>
        public LinkOutcome? PendingChoice { get; private set; }
        public DateTime? LastSyncUtc => _state.Load() is { LastSyncUtcTicks: > 0 } s && s.UserId == (Cloud?.PlayerId ?? "") ? new DateTime(s.LastSyncUtcTicks, DateTimeKind.Utc) : (DateTime?)null;

        /// <summary>Levé quand un conflit de synchronisation rend un choix de progression nécessaire (le rattachement, lui, rend son résultat à l'appelant).</summary>
        public event Action<LinkOutcome>? ChoiceRequired;

        // ------------------------------------------------------------------ Connexion

        /// <summary>Crée le compte. Succès sans résultat (<c>null</c>) quand l'adresse doit d'abord être confirmée : le joueur reste invité.</summary>
        public UniTask<Result<LinkOutcome?>> SignUpAsync(string email, string password, string displayName, CancellationToken ct)
            => Coalesce("up:" + email, async () =>
            {
                var r = await _accounts.SignUpAsync(email, password, displayName, ct);
                if (r.IsFailure) return Result<LinkOutcome?>.Fail(r.Error!);
                if (r.Value.NeedsConfirmation) return Result<LinkOutcome?>.Ok(null);
                return await LinkAsync(r.Value, ct);
            });

        public UniTask<Result<LinkOutcome?>> SignInAsync(string email, string password, CancellationToken ct)
            => Coalesce("in:" + email, async () =>
            {
                var r = await _accounts.SignInAsync(email, password, ct);
                if (r.IsFailure) return Result<LinkOutcome?>.Fail(r.Error!);
                return await LinkAsync(r.Value, ct);
            });

        public UniTask<Result<LinkOutcome?>> SignInWithProviderAsync(AuthProvider provider, CancellationToken ct)
            => Coalesce("provider:" + provider, async () =>
            {
                var r = await _accounts.SignInWithProviderAsync(provider, ct);
                if (r.IsFailure) return Result<LinkOutcome?>.Fail(r.Error!);
                return await LinkAsync(r.Value, ct);
            });

        /// <summary>Au lancement : rouvre la session mémorisée et synchronise. Succès sans résultat quand il n'y a pas de session.</summary>
        public UniTask<Result<LinkOutcome?>> RestoreAsync(CancellationToken ct)
            => Serialized(async () =>
            {
                var r = await _accounts.RestoreSessionAsync(ct);
                if (r.IsFailure) return Result<LinkOutcome?>.Fail(r.Error!);
                if (r.Value == null) return Result<LinkOutcome?>.Ok(null);
                return await LinkAsync(r.Value, ct);
            });

        public UniTask<Result> ResetPasswordAsync(string email, CancellationToken ct) => _accounts.ResetPasswordAsync(email, ct);

        /// <summary>Déconnexion : le joueur redevient invité avec la progression actuelle de l'appareil, qui n'est plus synchronisée.</summary>
        public async UniTask<Result> SignOutAsync(CancellationToken ct)
        {
            _sync.Stop();
            await _accounts.SignOutAsync(ct);
            _state.Clear();
            Cloud = null;
            PendingChoice = null;
            _pendingCloud = null;
            _log.Info("Compte : déconnecté, le joueur reste invité");
            return Result.Ok();
        }

        /// <summary>
        /// « Synchroniser » : sans compte, échoue (session expirée) ; avec un choix en attente, le repropose ; sans synchronisation active (panne au rattachement), relance le rattachement ;
        /// sinon envoie tout de suite. Ne dit jamais « synchronisé » quand rien n'est parti.
        /// </summary>
        public async UniTask<Result<LinkOutcome?>> SyncNowAsync(CancellationToken ct)
        {
            var account = Cloud;
            if (account == null) return Result<LinkOutcome?>.Fail(new DomainError("account.session", "errors.account_session"));
            if (PendingChoice != null && _pendingCloud != null) return Result<LinkOutcome?>.Ok(PendingChoice);
            if (!_sync.IsActive) return await Serialized(() => LinkAsync(account, ct));
            var flushed = await _sync.FlushAsync(ct);
            if (flushed.IsFailure) return Result<LinkOutcome?>.Fail(flushed.Error!);
            return Result<LinkOutcome?>.Ok(null);
        }

        void OnSessionLost()
        {
            Cloud = null;
            PendingChoice = null;
            _pendingCloud = null;
            _log.Warn("Compte : session perdue, le joueur redevient invité");
        }

        // ------------------------------------------------------------------ Rattachement

        async UniTask<Result<LinkOutcome?>> LinkAsync(PlayerAccount account, CancellationToken ct)
        {
            Cloud = account;
            PendingChoice = null;
            _pendingCloud = null;
            _sync.Stop();
            if (_session.Account == null) return Result<LinkOutcome?>.Ok(new LinkOutcome(LinkStatus.Connected));   // écran titre : aucun profil local chargé, il ne doit ni être envoyé ni remplacer le compte
            var got = await _cloud.GetAsync(ct);
            if (got.IsFailure) return Result<LinkOutcome?>.Fail(got.Error!);   // reste connecté : la synchronisation reprendra à la prochaine connexion
            var cloud = got.Value;
            var local = _session.ToProfile(_clock.UtcNow);
            if (cloud == null) return await UploadAsync(local, null, ct);
            if (TooNew(cloud))
            {
                Cloud = null;   // un jeu plus ancien ne touche pas à une sauvegarde qu'il ne comprend pas
                return Result<LinkOutcome?>.Fail(SchemaError);
            }
            var localSummary = ProfileSummary.Of(local);
            if (localSummary.IsFresh) return await PullAsync(cloud, ct);
            var known = _state.Load();
            if (known != null && known.UserId == account.PlayerId && known.LastUpdatedAt == cloud.UpdatedAt)
                return await UploadAsync(local, cloud.UpdatedAt, ct);   // déjà synchronisés : seul cet appareil a avancé
            return Ask(local, cloud);
        }

        static readonly DomainError SchemaError = new DomainError("account.schema", "errors.account_schema");
        static bool TooNew(CloudProfile cloud) => cloud.Profile.schemaVersion > new ProfileData().schemaVersion;

        Result<LinkOutcome?> Ask(ProfileData local, CloudProfile cloud)
        {
            _pendingCloud = cloud;
            var outcome = new LinkOutcome(LinkStatus.NeedsChoice, ProfileSummary.Of(local), ProfileSummary.Of(cloud.Profile));
            PendingChoice = outcome;
            return Result<LinkOutcome?>.Ok(outcome);
        }

        async UniTask<Result<LinkOutcome?>> UploadAsync(ProfileData local, string? expected, CancellationToken ct)
        {
            var written = await _cloud.UpsertAsync(local, expected, ct);
            if (written.IsFailure) return Result<LinkOutcome?>.Fail(written.Error!);
            if (written.Value.Status == CloudWriteStatus.Conflict)
            {
                var again = await _cloud.GetAsync(ct);   // le compte a changé entre la lecture et l'écriture
                if (again.IsFailure) return Result<LinkOutcome?>.Fail(again.Error!);
                if (again.Value == null) return await UploadAsync(local, null, ct);
                return Ask(local, again.Value);
            }
            RememberSync(written.Value.UpdatedAt);
            _sync.Start(written.Value.UpdatedAt);
            return Result<LinkOutcome?>.Ok(new LinkOutcome(LinkStatus.Uploaded, ProfileSummary.Of(local)));
        }

        async UniTask<Result<LinkOutcome?>> PullAsync(CloudProfile cloud, CancellationToken ct)
        {
            if (TooNew(cloud)) return Result<LinkOutcome?>.Fail(SchemaError);
            var before = _session.ToProfile(_clock.UtcNow);
            _backups.Save("local", before, _clock.UtcNow);   // même un profil « neuf » peut contenir des achats : rien n'est écrasé sans filet
            _session.ApplyProfile(cloud.Profile);
            if (!ReadableBy(_session, cloud.Profile))
            {
                // Aucun agent du compte n'est connu de ce build : ApplyProfile est retombé sur un profil neuf, qui ne doit jamais repartir par-dessus le compte.
                _backups.Save("cloud", cloud.Profile, _clock.UtcNow);
                _session.ApplyProfile(before);   // la partie de l'appareil reste en mémoire telle qu'elle était
                _log.Warn("Compte : profil du compte illisible par ce build, synchronisation non démarrée");
                return Result<LinkOutcome?>.Fail(SchemaError);
            }
            _wallet.PublishWallet(_session);
            _roster.PublishRoster();
            await _save.SaveAsync(ct);   // la synchronisation n'est pas encore active : cette sauvegarde locale ne repart pas en ligne
            RememberSync(cloud.UpdatedAt);
            _sync.Start(cloud.UpdatedAt);
            return Result<LinkOutcome?>.Ok(new LinkOutcome(LinkStatus.Pulled, cloud: ProfileSummary.Of(cloud.Profile)));
        }

        static bool ReadableBy(GameSession session, ProfileData profile)
        {
            if (profile.team == null || profile.team.Count == 0) return true;
            foreach (var s in profile.team)
                foreach (var m in session.Roster)
                    if (m.Id.Value == s.id) return true;
            return false;
        }

        void RememberSync(string updatedAt)
            => _state.Save(new CloudSyncState { UserId = Cloud?.PlayerId ?? "", LastUpdatedAt = updatedAt, LastSyncUtcTicks = _clock.UtcNow.Ticks });

        // ------------------------------------------------------------------ Choix de progression

        /// <summary>Applique le choix du joueur. L'autre version est d'abord gardée dans une sauvegarde de secours.</summary>
        public async UniTask<Result<LinkOutcome?>> ResolveAsync(ChoiceKind choice, CancellationToken ct)
        {
            var cloud = _pendingCloud;
            if (PendingChoice == null || cloud == null || Cloud == null)
                return Result<LinkOutcome?>.Fail(new DomainError("account.no_choice", "errors.account_nochoice"));
            var local = _session.ToProfile(_clock.UtcNow);
            if (choice == ChoiceKind.KeepAccount)
            {
                var fresh = await _cloud.GetAsync(ct);   // le compte a pu changer (ou être mis à jour par un jeu plus récent) depuis que le choix a été proposé
                if (fresh.IsFailure) return Result<LinkOutcome?>.Fail(fresh.Error!);
                if (fresh.Value == null) return await UploadAsync(local, null, ct);
                if (TooNew(fresh.Value)) return Result<LinkOutcome?>.Fail(SchemaError);
                if (fresh.Value.UpdatedAt != cloud.UpdatedAt) return Ask(local, fresh.Value);
                var pulled = await PullAsync(cloud, ct);
                if (pulled.IsSuccess) { PendingChoice = null; _pendingCloud = null; }
                return pulled;
            }
            _backups.Save("cloud", cloud.Profile, _clock.UtcNow);
            var result = await UploadAsync(local, cloud.UpdatedAt, ct);
            if (result.IsSuccess && result.Value!.Status != LinkStatus.NeedsChoice) { PendingChoice = null; _pendingCloud = null; }
            return result;
        }

        async UniTaskVoid OnSyncConflictAsync()
        {
            try
            {
                var got = await _cloud.GetAsync(CancellationToken.None);
                if (got.IsFailure || got.Value == null) return;
                if (TooNew(got.Value)) { _log.Warn("Conflit de synchronisation : le compte vient d'un jeu plus récent, aucun choix proposé"); return; }
                var asked = Ask(_session.ToProfile(_clock.UtcNow), got.Value);
                ChoiceRequired?.Invoke(asked.Value!);   // seul le conflit de synchronisation le lève : le rattachement rend son résultat à l'appelant
            }
            catch (Exception e) { _log.Error("Conflit de synchronisation : " + e.Message); }
        }

        // ------------------------------------------------------------------ Aides

        /// <summary>Un seul rattachement à la fois pour la même opération : un double appui attend le même résultat.</summary>
        async UniTask<Result<LinkOutcome?>> Coalesce(string key, Func<UniTask<Result<LinkOutcome?>>> run)
        {
            if (_pendingAuth != null && _pendingAuthKey == key) return await _pendingAuth.Task;
            var source = new UniTaskCompletionSource<Result<LinkOutcome?>>();
            _pendingAuth = source;
            _pendingAuthKey = key;
            try
            {
                var result = await Serialized(run);
                source.TrySetResult(result);
                return result;
            }
            catch (Exception e) { source.TrySetException(e); throw; }
            finally { if (_pendingAuth == source) { _pendingAuth = null; _pendingAuthKey = ""; } }
        }

        UniTaskCompletionSource? _gate;

        /// <summary>Un seul rattachement à la fois (connexion, restauration, « Synchroniser ») : deux en parallèle mélangeraient les comptes.</summary>
        async UniTask<T> Serialized<T>(Func<UniTask<T>> run)
        {
            while (_gate != null) await _gate.Task;
            var mine = new UniTaskCompletionSource();
            _gate = mine;
            try { return await run(); }
            finally { _gate = null; mine.TrySetResult(); }
        }
    }
}
