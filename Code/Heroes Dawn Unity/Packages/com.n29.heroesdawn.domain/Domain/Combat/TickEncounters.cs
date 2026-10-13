#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public enum TickEncounterKind { Parade }

    /// <summary>Deux unites sur le meme point de la frise : une parade (un allie et un ennemi).</summary>
    public readonly struct TickEncounter
    {
        public int A { get; }
        public int B { get; }
        public TickEncounterKind Kind { get; }
        public TickEncounter(int a, int b, TickEncounterKind kind) { A = a; B = b; Kind = kind; }
    }

    /// <summary>
    /// Rencontres sur la frise : un allie et un ennemi sur un meme point
    /// provoquent une parade, deux ennemis ne font rien. Une unite en canalisation (point negatif) ou deja au point d action
    /// (elle agit) ne rencontre personne. Chaque couple est rapporte une seule fois par appel, dans l ordre des identifiants.
    /// </summary>
    public static class TickEncounters
    {
        public static IReadOnlyList<TickEncounter> Detect(IReadOnlyList<CombatUnit> units, BalanceConfig b)
        {
            var result = new List<TickEncounter>();
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                if (!Eligible(u)) continue;
                for (int j = i + 1; j < units.Count; j++)
                {
                    var v = units[j];
                    if (!Eligible(v) || v.Point != u.Point) continue;
                    if (u.Side != v.Side) result.Add(new TickEncounter(u.Id, v.Id, TickEncounterKind.Parade));
                }
            }
            return result;
        }

        static bool Eligible(CombatUnit u) => u.IsAlive && !u.IsReady && u.Point >= 0;
    }
}
