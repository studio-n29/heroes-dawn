#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Account
{
    /// <summary>
    /// Envoi du profil vers le cloud pour un joueur connecté : au plus un envoi toutes les 20 s (les sauvegardes rapprochées sont regroupées),
    /// un seul envoi à la fois, vidage immédiat à la fermeture, réessais à 5, 20 puis 60 s après un échec. L'écriture est conditionnelle sur la date lue :
    /// si le compte a changé, l'envoi s'arrête (<see cref="ConflictDetected"/>) et rien n'est écrasé. Un échec réseau ne gêne jamais la sauvegarde locale.
    /// </summary>
    public sealed class CloudSync : ILocalSaveObserver
    {
        public static readonly TimeSpan Interval = TimeSpan.FromSeconds(20);
        static readonly TimeSpan[] RetryDelays = { TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(60) };

        readonly ICloudProfileGateway _cloud;
        readonly ICloudSyncStateStore _state;
        readonly GameSession _session;
        readonly IClock _clock;
        readonly ILogger _log;
        readonly Func<TimeSpan, CancellationToken, UniTask> _delay;

        string? _expected;
        bool _active;
        bool _dirty;
        bool _running;
        int _generation;
        UniTaskCompletionSource<bool>? _inflight;
        DateTime? _lastUpload;

        public CloudSync(ICloudProfileGateway cloud, ICloudSyncStateStore state, GameSession session, IClock clock, ILogger log,
                                Func<TimeSpan, CancellationToken, UniTask>? delay = null, SaveScheduler? save = null)
        {
            _cloud = cloud; _state = state; _session = session; _clock = clock; _log = log;
            _delay = delay ?? ((t, ct) => UniTask.Delay(t, DelayType.Realtime, PlayerLoopTiming.Update, ct));
            if (save != null) save.Observer = this;
        }

        /// <summary>Vrai quand un conflit a arrêté les envois (le joueur doit choisir).</summary>
        public bool IsPaused { get; private set; }
        public bool IsActive => _active;
        public event Action? ConflictDetected;
        /// <summary>Levé quand le compte refuse la session (jeton expiré, non renouvelable) : les envois sont arrêtés et le joueur redevient invité.</summary>
        public event Action? SessionLost;

        static DomainError SessionError => new DomainError("account.session", "errors.account_session");

        /// <summary>Active l'envoi : <paramref name="expectedUpdatedAt"/> est la dernière date d'écriture connue du compte.</summary>
        public void Start(string expectedUpdatedAt)
        {
            _generation++;
            _expected = expectedUpdatedAt;
            _active = true;
            IsPaused = false;
        }

        public void Stop()
        {
            _generation++;   // un envoi encore en vol est périmé : son résultat sera ignoré
            _active = false;
            IsPaused = false;
            _dirty = false;
            _expected = null;
            _lastUpload = null;
        }

        public void NotifyLocalSaved()
        {
            if (!_active || IsPaused) return;
            _dirty = true;
            if (_running || _inflight != null) return;   // l'envoi en cours relance la boucle s'il reste du travail
            StartLoop();
        }

        void StartLoop()
        {
            _running = true;
            RunAsync().Forget();
        }

        async UniTaskVoid RunAsync()
        {
            try
            {
                int retry = 0;
                while (_dirty && _active && !IsPaused)
                {
                    if (_lastUpload.HasValue && retry == 0)
                    {
                        var wait = Interval - (_clock.UtcNow - _lastUpload.Value);
                        if (wait > TimeSpan.Zero) await _delay(wait, CancellationToken.None);
                        if (!_dirty || !_active || IsPaused) break;   // un vidage a pu tout envoyer pendant l'attente
                    }
                    var r = await UploadOnceAsync(CancellationToken.None);
                    if (r.IsSuccess) { retry = 0; continue; }
                    if (!_active || retry >= RetryDelays.Length) break;   // session perdue, ou on attend la prochaine sauvegarde locale
                    await _delay(RetryDelays[retry], CancellationToken.None);
                    retry++;
                }
            }
            catch (Exception e) { _log.Error("Synchronisation : " + e.Message); }
            finally { _running = false; }
        }

        /// <summary>Envoie tout de suite (fermeture, arrière-plan, « Synchroniser »), sans attendre l'intervalle. Échoue sans synchronisation active : on ne dit jamais « synchronisé » quand rien n'est parti.</summary>
        public async UniTask<Result> FlushAsync(CancellationToken ct)
        {
            if (!_active) return Result.Fail(SessionError);
            if (IsPaused) return Result.Ok();
            if (_inflight != null) await _inflight.Task;   // l'envoi en cours finit d'abord
            if (!_active) return Result.Fail(SessionError);
            if (IsPaused) return Result.Ok();
            if (!_dirty && _lastUpload.HasValue) return Result.Ok();
            return await UploadOnceAsync(ct);
        }

        async UniTask<Result> UploadOnceAsync(CancellationToken ct)
        {
            if (_inflight != null) { await _inflight.Task; return Result.Ok(); }
            var done = new UniTaskCompletionSource<bool>();
            _inflight = done;
            int generation = _generation;
            try
            {
                _dirty = false;   // les modifications arrivées pendant l'envoi le remettront à vrai
                var profile = _session.ToProfile(_clock.UtcNow);
                var result = await _cloud.UpsertAsync(profile, _expected, ct);
                if (generation != _generation) return Result.Ok();   // déconnexion ou nouveau compte pendant l'envoi : résultat périmé
                if (result.IsFailure)
                {
                    _dirty = true;
                    _log.Warn("Synchronisation : envoi impossible (" + result.Error!.Code + ")");
                    if (result.Error!.LocKey == SessionError.LocKey)
                    {
                        Stop();
                        SessionLost?.Invoke();
                    }
                    return Result.Fail(result.Error!);
                }
                if (result.Value.Status == CloudWriteStatus.Conflict)
                {
                    IsPaused = true;
                    _dirty = true;
                    _log.Info("Synchronisation : le compte a changé, choix de progression requis");
                    ConflictDetected?.Invoke();
                    return Result.Ok();
                }
                _expected = result.Value.UpdatedAt;
                _lastUpload = _clock.UtcNow;
                var saved = _state.Load() ?? new CloudSyncState();
                saved.LastUpdatedAt = result.Value.UpdatedAt;
                saved.LastSyncUtcTicks = _clock.UtcNow.Ticks;
                _state.Save(saved);
                return Result.Ok();
            }
            finally
            {
                _inflight = null;
                done.TrySetResult(true);
                if (_dirty && _active && !IsPaused && !_running && generation == _generation) StartLoop();   // une sauvegarde est arrivée pendant l'envoi
            }
        }
    }
}
