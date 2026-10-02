#nullable enable
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Application.Session
{
    /// <summary>Membre de l'effectif en lecture seule.</summary>
    public interface ITeamMemberReadModel
    {
        MemberId Id { get; }
        MemberDef Def { get; }
        int Level { get; }
        int Fatigue { get; }
        bool InSquad { get; }
        bool Exhausted { get; }
    }
}
