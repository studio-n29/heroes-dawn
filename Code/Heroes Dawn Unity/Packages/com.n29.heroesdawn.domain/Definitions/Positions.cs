using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Types de cartes boostés par chaque position (bonus de position des cartes).</summary>
    public static class Positions
    {
        static readonly CardNature[] Defensive = { CardNature.Defensive };
        static readonly CardNature[] Offensive = { CardNature.Offensive };
        static readonly CardNature[] Control = { CardNature.Control };
        static readonly CardNature[] Support = { CardNature.Support };
        static readonly CardNature[] OffensiveAndControl = { CardNature.Offensive, CardNature.Control };

        /// <summary>Première ligne : Défensif · Attaquant : Offensif · Contrôle : Contrôle · Soutien : Soutien · Précision à distance : Offensif et Contrôle.</summary>
        public static IReadOnlyList<CardNature> BoostedTypes(Position p)
        {
            switch (p)
            {
                case Position.FrontLine: return Defensive;
                case Position.Attacker: return Offensive;
                case Position.Controller: return Control;
                case Position.Support: return Support;
                default: return OffensiveAndControl;
            }
        }
    }
}
