#nullable enable
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Feedback
{
    /// <summary>Onomatopée de bande dessinée jouée avant une action (entrée en combat ou en événement, coup, dégât).</summary>
    public enum ComicCue { Combat, Event, Minigame, Rest, Bang, Ouch }

    /// <summary>Ce que la vue affiche pour une onomatopée : texte, couleur de l'étoile, inclinaison en degrés.</summary>
    public readonly struct ComicCueData
    {
        public string Text { get; }
        public Color Color { get; }
        public float Tilt { get; }
        public ComicCueData(string text, Color color, float tilt) { Text = text; Color = color; Tilt = tilt; }
    }

    /// <summary>Catalogue des onomatopées : une clé de texte, une couleur et une inclinaison par type.</summary>
    public static class ComicCues
    {
        public static string TextKey(ComicCue cue)
        {
            switch (cue)
            {
                case ComicCue.Combat: return "cue.combat";
                case ComicCue.Event: return "cue.event";
                case ComicCue.Minigame: return "cue.minigame";
                case ComicCue.Rest: return "cue.rest";
                case ComicCue.Bang: return "cue.bang";
                default: return "cue.ouch";
            }
        }

        public static Color ColorOf(ComicCue cue)
        {
            switch (cue)
            {
                case ComicCue.Combat: return Theme.Danger;
                case ComicCue.Event: return Theme.Primary;
                case ComicCue.Minigame: return Theme.Info;
                case ComicCue.Rest: return Theme.Success;
                case ComicCue.Bang: return Theme.Primary;
                default: return Theme.Danger;
            }
        }

        /// <summary>Inclinaison stable par type (l'étoile n'est jamais droite).</summary>
        public static float TiltOf(ComicCue cue) => cue == ComicCue.Combat || cue == ComicCue.Ouch ? -7f : 5f;

        /// <summary>Onomatopée jouée en entrant sur une case de ce type ; vide pour les cases sans action (entrée, carrefour, ressource, objectif).</summary>
        public static ComicCue? ForCase(CaseType type)
        {
            switch (type)
            {
                case CaseType.Combat: return ComicCue.Combat;
                case CaseType.Narrative: return ComicCue.Event;
                case CaseType.MiniGame: return ComicCue.Minigame;
                case CaseType.Rest: return ComicCue.Rest;
                default: return null;
            }
        }
    }
}
