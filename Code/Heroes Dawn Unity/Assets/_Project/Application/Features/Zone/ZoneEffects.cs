#nullable enable
using MessagePipe;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Ports;
using N29.HeroesDawn.Application.Session;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>Dépendances communes aux use cases du plateau (session, bus, sauvegarde, journal).</summary>
    public sealed class ZoneEffects
    {
        public GameSession Session { get; }
        public IPublisher<WalletChanged> Wallet { get; }
        public IPublisher<ZoneValidated> Validated { get; }
        public SaveScheduler Save { get; }
        public ILogger Log { get; }
        /// <summary>Horloge du telephone (jours d evenement) ; nulle : l heure du systeme.</summary>
        public IClock? Clock { get; }

        public ZoneEffects(GameSession session, IPublisher<WalletChanged> wallet, IPublisher<ZoneValidated> validated,
                           SaveScheduler save, ILogger log, IClock? clock = null)
        {
            Session = session; Wallet = wallet; Validated = validated; Save = save; Log = log; Clock = clock;
        }
    }
}
