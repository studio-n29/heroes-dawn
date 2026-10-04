#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Hub
{
    public enum MemberSort { Level, Rarity, Fatigue }

    /// <summary>Filtre par famille et tri décroissant (stable) de l'effectif, pour l'écran Équipe.</summary>
    public static class MemberQuery
    {
        public static IReadOnlyList<ITeamMemberReadModel> Apply(IReadOnlyList<ITeamMemberReadModel> all, Family? family, MemberSort sort)
        {
            var q = all.Where(m => family == null || m.Def.Family == family.Value);
            q = sort switch
            {
                MemberSort.Rarity => q.OrderByDescending(m => m.Def.Rarity).ThenByDescending(m => m.Level),
                MemberSort.Fatigue => q.OrderByDescending(m => m.Fatigue),
                _ => q.OrderByDescending(m => m.Level),
            };
            return q.ToList();
        }
    }
}
