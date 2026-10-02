#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>
    /// Sauvegarde du profil. Les écritures sont sérialisées : si une sauvegarde est en cours,
    /// la suivante est marquée « en attente » et relancée une seule fois à la fin.
    /// </summary>
    public sealed class SaveScheduler
    {
        readonly GameSession _session;
        readonly IProfileGateway _profiles;
        readonly IClock _clock;
        readonly ILogger _log;
        readonly IPublisher<SaveStateChanged> _saving;
        bool _busy;
        bool _pending;

        public SaveScheduler(GameSession session, IProfileGateway profiles, IClock clock, ILogger log, IPublisher<SaveStateChanged> saving)
        {
            _session = session; _profiles = profiles; _clock = clock; _log = log; _saving = saving;
        }

        /// <summary>Demande une sauvegarde (sans attendre).</summary>
        public void RequestSave() => SaveAsync(CancellationToken.None).Forget();

        public async UniTask SaveAsync(CancellationToken ct)
        {
            if (_session.Account == null) return;
            if (_busy) { _pending = true; return; }
            _busy = true;
            _saving.Publish(new SaveStateChanged(true));
            try
            {
                do
                {
                    _pending = false;
                    var data = _session.ToProfile(_clock.UtcNow);
                    await _profiles.SaveAsync(_session.Account.PlayerId, data, ct).AsUniTask(useCurrentSynchronizationContext: false);
                } while (_pending && !ct.IsCancellationRequested);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _log.Error("Sauvegarde impossible : " + e.Message);
            }
            finally
            {
                _busy = false;
                _saving.Publish(new SaveStateChanged(false));
            }
        }
    }
}
