#nullable enable
using System;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Calcul de la confirmation d'achat (M31) : solde avant, coût, solde après, manque éventuel.</summary>
    public readonly struct RecruitBalance
    {
        public int Before { get; }
        public int Cost { get; }
        public int After { get; }
        public int Missing { get; }
        public bool CanAfford => Missing == 0;

        RecruitBalance(int before, int cost)
        {
            Before = before;
            Cost = cost;
            After = Math.Max(0, before - cost);
            Missing = Math.Max(0, cost - before);
        }

        public static RecruitBalance Of(int pr, int cost) => new RecruitBalance(pr, cost);
    }
}
