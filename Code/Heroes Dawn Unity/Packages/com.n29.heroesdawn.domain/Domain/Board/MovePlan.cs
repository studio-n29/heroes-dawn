using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Board
{
    public sealed class MovePlan
    {
        public CaseId Target { get; }
        /// <summary>Chemin sans la case de départ ; se termine par Target.</summary>
        public IReadOnlyList<CaseId> Path { get; }
        /// <summary>PM dépensés : somme des coûts des chemins empruntés.</summary>
        public int Cost { get; }
        public MovePlan(CaseId target, IReadOnlyList<CaseId> path, int cost) { Target = target; Path = path; Cost = cost; }
    }
}
