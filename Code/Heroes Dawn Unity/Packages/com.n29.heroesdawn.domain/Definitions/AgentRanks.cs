namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Rang AMIF d un agent, de 1 (capacités secondaires) à 5 (menace planétaire). Remplace l ancienne rareté d agent
    /// (Agent, Élite, Légende) et la puissance : il fixe le multiplicateur de stats, le coût de recrutement et le poids en boutique.
    /// </summary>
    public static class AgentRanks
    {
        public const int Min = 1, Max = 5;

        static readonly int[] StatPercents = { 100, 115, 130, 145, 165 };
        // ⚑ Valeurs ajoutées par le plan du bloc A (la spec ne les fixe pas) : à régler au playtest.
        static readonly int[] RecruitCosts = { 150, 220, 450, 800, 1200 };
        static readonly int[] ShopWeights = { 45, 25, 20, 8, 2 };

        /// <summary>Ramène un rang dans 1..5.</summary>
        public static int Clamp(int rank) => rank < Min ? Min : rank > Max ? Max : rank;

        /// <summary>Multiplicateur de stats en pourcentage : 100, 115, 130, 145 ou 165.</summary>
        public static int StatPercent(int rank) => StatPercents[Clamp(rank) - 1];

        /// <summary>Coût de recrutement en PR.</summary>
        public static int RecruitCost(int rank) => RecruitCosts[Clamp(rank) - 1];

        /// <summary>Poids relatif d apparition en boutique (seuls les agents de boutique y entrent).</summary>
        public static int ShopWeight(int rank) => ShopWeights[Clamp(rank) - 1];
    }
}
