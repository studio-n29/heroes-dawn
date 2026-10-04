#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Zone
{
    public static class ZoneErrors
    {
        public static readonly DomainError NoRun = new DomainError("zone.no_run", "errors.no_run");
        public static readonly DomainError NotAllowed = new DomainError("zone.not_allowed", "errors.board.not_allowed");
        public static readonly DomainError SquadExhausted = new DomainError("zone.exhausted", "errors.squad_exhausted");
        public static readonly DomainError UnknownZone = new DomainError("zone.unknown", "errors.unknown_zone");
        public static readonly DomainError Locked = new DomainError("zone.locked", "errors.zone_locked");
    }

    public enum NarrativeChoice { Credits, Research, Morale }

    /// <summary>Récompenses de validation de zone (GDD §5 : 1 500 Crédits + 40 PR la première fois).</summary>
    public sealed class ZoneRewards
    {
        public int Credits { get; }
        public int Pr { get; }
        public bool FirstClear { get; }
        public ZoneRewards(int credits, int pr, bool firstClear) { Credits = credits; Pr = pr; FirstClear = firstClear; }
    }
}
