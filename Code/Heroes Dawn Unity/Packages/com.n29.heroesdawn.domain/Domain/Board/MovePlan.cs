using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Domain.Board
{
    public sealed class MovePlan
    {
        public CaseId Target { get; }
        /// <summary>Chemin sans la case de départ ; se termine par Target.</summary>
        public IReadOnlyList<CaseId> Path { get; }
        public int Cost => Path.Count;
        public MovePlan(CaseId target, IReadOnlyList<CaseId> path) { Target = target; Path = path; }
    }
}
