#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>État du combat en cours, partagé par les use cases Combat.</summary>
    public sealed class ActiveCombat : IActiveCombatReadModel
    {
        readonly List<TeamMember> _fighters = new List<TeamMember>();

        public CombatSimulation? Simulation { get; private set; }
        public CombatSetup? Setup { get; private set; }
        public CaseOutcome? Case { get; private set; }
        public IReadOnlyList<ITeamMemberReadModel> Fighters => _fighters;
        public bool DebugUsed { get; private set; }

        internal IReadOnlyList<TeamMember> FighterMembers => _fighters;

        internal void Start(CombatSimulation simulation, CombatSetup setup, CaseOutcome pending, IReadOnlyList<TeamMember> fighters)
        {
            Simulation = simulation;
            Setup = setup;
            Case = pending;
            _fighters.Clear();
            _fighters.AddRange(fighters);
            DebugUsed = false;
        }

        internal void MarkDebugUsed() => DebugUsed = true;

        public void Clear()
        {
            Simulation = null;
            Setup = null;
            Case = null;
            _fighters.Clear();
        }
    }
}
