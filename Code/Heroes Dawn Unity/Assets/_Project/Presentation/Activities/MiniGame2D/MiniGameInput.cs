#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>
    /// Entrées d'une image pour un mini-jeu 2D. <see cref="Pos"/> est normalisée dans la zone de jeu (0..1, y vers le bas) ;
    /// <see cref="Down"/> et <see cref="Up"/> ne sont vrais que sur l'image où le doigt se pose ou se relève ; <see cref="Stick"/> est le joystick flottant (nul au repos).
    /// </summary>
    public struct MiniGameInput
    {
        public bool Down;
        public bool Up;
        public bool Held;
        public Vector2 Pos;
        public Vector2 Stick;
    }

    /// <summary>Ce que le bandeau de la fenêtre affiche : score déjà mis en forme, temps restant (&lt; 0 : masqué), vies (&lt; 0 : masquées).</summary>
    public struct MiniGameHud
    {
        public string Score;
        public float TimeLeft;
        public int Lives;
        public MiniGameHud(string score, float timeLeft, int lives) { Score = score; TimeLeft = timeLeft; Lives = lives; }
    }
}
