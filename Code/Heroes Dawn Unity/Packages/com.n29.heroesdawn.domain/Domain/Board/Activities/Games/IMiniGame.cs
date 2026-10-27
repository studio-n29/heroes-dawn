#nullable enable

namespace N29.HeroesDawn.Domain.Activities.Games
{
    /// <summary>
    /// Forme commune des mini-jeux 2D : l'état, le score, le minuteur et les vies lus par la fenêtre, et ce qu'il faut pour avancer, abandonner et conclure.
    /// Logique pure, déterministe (graine) et sans moteur : le même niveau et les mêmes entrées donnent la même partie.
    /// </summary>
    public interface IMiniGame
    {
        GameStatus Status { get; }
        int Score { get; }
        /// <summary>Score à atteindre pour la victoire (affiché « score / cible »).</summary>
        int Target { get; }
        /// <summary>Secondes restantes ; -1 quand le jeu n'a pas de minuteur.</summary>
        float TimeLeft { get; }
        /// <summary>Vies (ou essais) restants ; -1 quand le jeu n'en a pas.</summary>
        int Lives { get; }
        void Step(float dt);
        /// <summary>Abandon : une partie en cours est perdue.</summary>
        void Abandon();
        ActivityResult Result();
    }
}
