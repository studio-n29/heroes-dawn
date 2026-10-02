#nullable enable
using MessagePipe;
using N29.HeroesDawn.Application.Features;
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Application.Messages;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using AppLogger = N29.HeroesDawn.Application.Ports.ILogger;

namespace N29.HeroesDawn.Presentation
{
    /// <summary>
    /// Ce que la racine de composition fournit aux écrans : lecture de la session, use cases, abonnements.
    /// Les écrans ne reçoivent jamais <see cref="GameSession"/> ni un service avec état.
    /// </summary>
    public sealed class UiServices
    {
        public ISessionReadModel Session { get; }
        public AuthService Auth { get; }
        public HubService Hub { get; }
        public CombatService Combat { get; }
        public AppLogger Log { get; }
        public ZoneId FirstZone { get; }

        // ---- Zone
        public IStartZoneUseCase StartZone { get; }
        public IRollDiceUseCase RollDice { get; }
        public IUseBonusDieUseCase UseBonusDie { get; }
        public IMoveToUseCase MoveTo { get; }
        public IEndMoveUseCase EndMove { get; }
        public IResolveMiniGameUseCase ResolveMiniGame { get; }
        public IResolveNarrativeUseCase ResolveNarrative { get; }
        public IResolveRestUseCase ResolveRest { get; }
        public ILeaveZoneUseCase LeaveZone { get; }

        // ---- Abonnements
        public ISubscriber<WalletChanged> Wallet { get; }
        public ISubscriber<RosterChanged> Roster { get; }
        public ISubscriber<ToastRequested> Toasts { get; }
        public ISubscriber<SaveStateChanged> Saving { get; }
        public ISubscriber<ZoneValidated> ZoneValidated { get; }

        public UiServices(ISessionReadModel session, AuthService auth, HubService hub, CombatService combat, AppLogger log, ZoneId firstZone,
                          IStartZoneUseCase startZone, IRollDiceUseCase rollDice, IUseBonusDieUseCase useBonusDie, IMoveToUseCase moveTo,
                          IEndMoveUseCase endMove, IResolveMiniGameUseCase resolveMiniGame, IResolveNarrativeUseCase resolveNarrative,
                          IResolveRestUseCase resolveRest, ILeaveZoneUseCase leaveZone,
                          ISubscriber<WalletChanged> wallet, ISubscriber<RosterChanged> roster, ISubscriber<ToastRequested> toasts,
                          ISubscriber<SaveStateChanged> saving, ISubscriber<ZoneValidated> zoneValidated)
        {
            Session = session; Auth = auth; Hub = hub; Combat = combat; Log = log; FirstZone = firstZone;
            StartZone = startZone; RollDice = rollDice; UseBonusDie = useBonusDie; MoveTo = moveTo; EndMove = endMove;
            ResolveMiniGame = resolveMiniGame; ResolveNarrative = resolveNarrative; ResolveRest = resolveRest; LeaveZone = leaveZone;
            Wallet = wallet; Roster = roster; Toasts = toasts; Saving = saving; ZoneValidated = zoneValidated;
        }
    }
}
