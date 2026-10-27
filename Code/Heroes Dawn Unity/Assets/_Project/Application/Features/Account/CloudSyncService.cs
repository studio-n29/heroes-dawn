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
    public sealed class CloudSyncService : ILocalSaveObserver
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
        bool _uploading;
        DateTime? _lastUpload;

        public CloudSyncService(ICloudProfileGateway cloud, ICloudSyncStateStore state, GameSession session, IClock clock, ILogger log,
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

        /// <summary>Active l'envoi : <paramref name="expectedUpdatedAt"/> est la dernière date d'écriture connue du compte.</summary>
        public void Start(string expectedUpdatedAt)
        {
            _expected = expectedUpdatedAt;
            _active = true;
            IsPaused = false;
        }

        public void Stop()
        {
            _active = false;
            IsPaused = false;
            _dirty = false;
            _expected = null;
        }

        public void NotifyLocalSaved()
        {
            if (!_active || IsPaused) return;
            _dirty = true;
            if (_running) return;
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
                    if (retry >= RetryDelays.Length) break;   // on attend la prochaine sauvegarde locale
                    await _delay(RetryDelays[retry], CancellationToken.None);
                    retry++;
                }
            }
            catch (Exception e) { _log.Error("Synchronisation : " + e.Message); }
            finally { _running = false; }
        }

        /// <summary>Envoie tout de suite (fermeture, arrière-plan, « Synchroniser »), sans attendre l'intervalle.</summary>
        public async UniTask<Result> FlushAsync(CancellationToken ct)
        {
            if (!_active || IsPaused) return Result.Ok();
            while (_uploading) await UniTask.Yield();   // l'envoi en cours finit d'abord
            if (!_dirty && _lastUpload.HasValue) return Result.Ok();
            return await UploadOnceAsync(ct);
        }

        async UniTask<Result> UploadOnceAsync(CancellationToken ct)
        {
            if (_uploading) return Result.Ok();
            _uploading = true;
            try
            {
                _dirty = false;   // les modifications arrivées pendant l'envoi le remettront à vrai
                var profile = _session.ToProfile(_clock.UtcNow);
                var result = await _cloud.UpsertAsync(profile, _expected, ct);
                if (result.IsFailure)
                {
                    _dirty = true;
                    _log.Warn("Synchronisation : envoi impossible (" + result.Error!.Code + ")");
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
            finally { _uploading = false; }
        }
    }
}
