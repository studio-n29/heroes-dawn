#nullable enable
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    public sealed class CaseOutcome
    {
        public CaseId Case { get; }
        public CaseType Type { get; }
        public CombatRank Rank { get; }
        public EncounterDef? Encounter { get; }
        public CaseOutcome(CaseId c, CaseType t, CombatRank rank, EncounterDef? e) { Case = c; Type = t; Rank = rank; Encounter = e; }
    }
}
