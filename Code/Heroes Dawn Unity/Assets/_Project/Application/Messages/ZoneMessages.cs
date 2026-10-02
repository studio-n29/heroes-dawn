#nullable enable
using N29.HeroesDawn.Application.Features.Zone;

namespace N29.HeroesDawn.Application.Messages
{
    /// <summary>La zone en cours vient d'être validée ; les récompenses sont déjà créditées.</summary>
    public readonly struct ZoneValidated
    {
        public readonly ZoneRewards Rewards;
        public ZoneValidated(ZoneRewards rewards) { Rewards = rewards; }
    }
}
