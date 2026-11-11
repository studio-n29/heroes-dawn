#nullable enable
using System;
using Cysharp.Threading.Tasks;
using MessagePipe;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Infrastructure.Platform;
using VContainer.Unity;

namespace N29.HeroesDawn.Composition
{
    /// <summary>
    /// Au lancement : connecte la passerelle d'achats réels (les transactions en attente remontent alors) et reprend ces transactions dès qu'un profil est chargé
    /// ou qu'une nouvelle transaction arrive. Avec le simulateur, il n'y a rien à reprendre.
    /// </summary>
    public sealed class PurchaseStartup : IStartable, IDisposable
    {
        readonly IPurchaseGateway _gateway;
        readonly PurchaseRecovery _recovery;
        readonly ISubscriber<RosterChanged> _roster;
        IDisposable? _subscription;

        public PurchaseStartup(IPurchaseGateway gateway, PurchaseRecovery recovery, ISubscriber<RosterChanged> roster)
        {
            _gateway = gateway; _recovery = recovery; _roster = roster;
        }

        public void Start()
        {
            if (_gateway is IPendingPurchaseSource source) source.PendingChanged += OnPending;
            _subscription = _roster.Subscribe(_ => _recovery.Resume());
            if (_gateway is UnityIapGateway real) real.StartAsync(default).Forget();
        }

        void OnPending() => _recovery.Resume();

        public void Dispose()
        {
            if (_gateway is IPendingPurchaseSource source) source.PendingChanged -= OnPending;
            _subscription?.Dispose();
            (_gateway as IDisposable)?.Dispose();
        }
    }
}
