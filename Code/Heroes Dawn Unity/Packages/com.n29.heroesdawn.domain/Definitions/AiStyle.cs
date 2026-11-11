namespace N29.HeroesDawn.Definitions
{
    /// <summary>Style d'IA d'un ennemi : décide de la compétence et de la cible choisies (spec du bloc C, §1).</summary>
    public enum AiStyle { Balanced, Aggressive, Defensive, Control, Support }

    public static class AiStyles
    {
        /// <summary>Style d'un ennemi sans style saisi, selon sa position : Attaquant et Précision à distance → Aggressive, Première ligne → Defensive, Contrôle → Control, Soutien → Support.</summary>
        public static AiStyle Default(Position position)
        {
            switch (position)
            {
                case Position.Attacker:
                case Position.Ranged: return AiStyle.Aggressive;
                case Position.FrontLine: return AiStyle.Defensive;
                case Position.Controller: return AiStyle.Control;
                case Position.Support: return AiStyle.Support;
                default: return AiStyle.Balanced;
            }
        }
    }
}
