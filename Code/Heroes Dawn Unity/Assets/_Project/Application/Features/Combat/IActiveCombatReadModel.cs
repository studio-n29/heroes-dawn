#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Domain.Board;
using N29.HeroesDawn.Domain.Combat;

namespace N29.HeroesDawn.Application.Features.Combat
{
    /// <summary>Combat en cours, en lecture pour l'interface (qui fait avancer la simulation au rythme de l'affichage).</summary>
    public interface IActiveCombatReadModel
    {
        CombatSimulation? Simulation { get; }
        CombatSetup? Setup { get; }
        CaseOutcome? Case { get; }
        /// <summary>Membres engagés, dans l'ordre des unités alliées (Id 0..n-1).</summary>
        IReadOnlyList<ITeamMemberReadModel> Fighters { get; }
        bool DebugUsed { get; }
        /// <summary>Oublie le combat terminé (affichage fermé). Ne touche à aucune règle.</summary>
        void Clear();
    }
}
