using N29.HeroesDawn.Presentation.Common;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Mvp
{
    /// <summary>Traduction des tons sémantiques en couleurs du thème.</summary>
    public static class ToneColors
    {
        public static Color Of(Tone t)
        {
            switch (t)
            {
                case Tone.Muted: return Theme.TextMuted;
                case Tone.Dark: return Theme.TextDark;
                case Tone.Primary: return Theme.Primary;
                case Tone.Success: return Theme.Success;
                case Tone.Warning: return Theme.Fatigue;
                case Tone.Danger: return Theme.Danger;
                case Tone.Info: return Theme.Info;
                case Tone.Perception: return Theme.Perception;
                case Tone.Credits: return Theme.Credits;
                case Tone.Pr: return Theme.Pr;
                case Tone.Shield: return Theme.Shield;
                default: return Theme.Text;
            }
        }
    }
}
