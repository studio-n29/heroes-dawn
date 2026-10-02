#nullable enable
using System;
using System.Threading;
using System.Threading.Tasks;
using N29.HeroesDawn.Application.Events;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Features
{
    /// <summary>
    /// Sauvegarde du profil. Les écritures sont sérialisées : si une sauvegarde est en cours,
    /// la suivante est marquée « en attente » et relancée une seule fois à la fin.
    /// </summary>
    public sealed class SaveService
    {
        readonly GameSession _session;
        readonly IProfileGateway _profiles;
        readonly IClock _clock;
        readonly ILogger _log;
        readonly GameEvents _events;
        bool _saving;
        bool _pending;

        public SaveService(GameSession session, IProfileGateway profiles, IClock clock, ILogger log, GameEvents events)
        {
            _session = session; _profiles = profiles; _clock = clock; _log = log; _events = events;
        }

        /// <summary>Demande une sauvegarde (sans attendre).</summary>
        public void RequestSave() => _ = SaveAsync(CancellationToken.None);

        public async Task SaveAsync(CancellationToken ct)
        {
            if (_session.Account == null) return;
            if (_saving) { _pending = true; return; }
            _saving = true;
            _events.RaiseSaving(true);
            try
            {
                do
                {
                    _pending = false;
                    var data = _session.ToProfile(_clock.UtcNow);
                    await _profiles.SaveAsync(_session.Account.PlayerId, data, ct);
                } while (_pending && !ct.IsCancellationRequested);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                _log.Error("Sauvegarde impossible : " + e.Message);
            }
            finally
            {
                _saving = false;
                _events.RaiseSaving(false);
            }
        }
    }
}
