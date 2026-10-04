#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Flow;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>
    /// Bouton MISSION (QG, Équipe) : démarre la zone puis demande le plateau. Un seul départ à la fois ;
    /// après un succès la garde reste levée (la scène du hub va être déchargée) ; un échec affiche l'erreur et permet de réessayer.
    /// </summary>
    public sealed class MissionLauncher
    {
        readonly IStartZoneUseCase _start;
        readonly IPublisher<NavigateRequested> _navigate;
        readonly IToastService _toasts;
        readonly ILocalization _loc;
        readonly BusyGate _gate = new BusyGate();

        public ZoneId Zone { get; }

        public MissionLauncher(IStartZoneUseCase start, ZoneId zone, IPublisher<NavigateRequested> navigate, IToastService toasts, ILocalization loc)
        {
            _start = start; Zone = zone; _navigate = navigate; _toasts = toasts; _loc = loc;
        }

        public void Launch(CancellationToken ct = default)
        {
            if (!_gate.TryEnter()) return;
            LaunchAsync(ct).Forget();
        }

        async UniTaskVoid LaunchAsync(CancellationToken ct)
        {
            Result r;
            try { r = await _start.ExecuteAsync(Zone, ct); }
            catch (OperationCanceledException) { _gate.Exit(); return; }
            catch { _gate.Exit(); throw; }
            if (ct.IsCancellationRequested) { _gate.Exit(); return; }
            if (r.IsFailure)
            {
                _gate.Exit();
                _toasts.Show(_loc.Error(r.Error!), ToastKind.Error);
                return;
            }
            _navigate.Publish(new NavigateRequested(GameState.Board));
        }
    }
}
