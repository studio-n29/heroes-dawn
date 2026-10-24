#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Application.Features.Hub
{
    public static class HubErrors
    {
        public static readonly DomainError NotEnoughCredits = new DomainError("hub.credits", "errors.not_enough_credits");
        public static readonly DomainError NotEnoughPr = new DomainError("hub.pr", "errors.not_enough_pr");
        public static readonly DomainError AlreadyOwned = new DomainError("hub.owned", "errors.already_owned");
        public static readonly DomainError UnknownMember = new DomainError("hub.unknown", "errors.unknown_member");
        public static readonly DomainError MaxLevel = new DomainError("hub.max_level", "errors.max_level");
        public static readonly DomainError SquadFull = new DomainError("hub.squad_full", "errors.squad_full");
        public static readonly DomainError SquadEmpty = new DomainError("hub.squad_empty", "errors.squad_empty");
        public static readonly DomainError PresetEmpty = new DomainError("hub.preset_empty", "errors.preset_empty");
        public static readonly DomainError PresetUnavailable = new DomainError("hub.preset_unavailable", "errors.preset_unavailable");
        public static readonly DomainError NothingToHeal = new DomainError("hub.no_fatigue", "errors.nothing_to_heal");
        public static readonly DomainError InMission = new DomainError("hub.in_mission", "errors.in_mission");
        public static readonly DomainError OnExpedition = new DomainError("hub.expedition", "errors.on_expedition");
    }
}
