#nullable enable
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Login
{
    /// <summary>Motif de l'illustration provisoire d'un écran de la cinématique (remplacé par une illustration dessinée quand elle existe).</summary>
    public enum IntroMotif { Battle, Wormhole, Arrival, Phoenix, Farewell, Portal, Wave, Dna, Guild }

    /// <summary>Un écran de la cinématique d'introduction : texte, motif et couleurs du fond, illustration éventuelle.</summary>
    public struct IntroSlideData
    {
        public string Text;
        public IntroMotif Motif;
        public Color Top;
        public Color Bottom;
        /// <summary>Illustration dessinée (Resources/Intro/intro_NN) ; null : le motif provisoire est dessiné par code.</summary>
        public Sprite? Art;
    }

    public struct IntroCinematicTexts
    {
        public string Skip;
        public string TapHint;
    }

    /// <summary>Choix du pseudo, sous forme de dialogue avec l'accueil de la Guilde.</summary>
    public struct PseudoPromptData
    {
        public string Speaker;
        public string Line;
        public string Placeholder;
        public string Confirm;
        public string Suggestion;
    }

    public struct CampaignCardData
    {
        public string Id;
        public string Name;
        public string Description;
        public bool Available;
        public string Badge;
    }

    public struct ChooseCampaignData
    {
        public string Title;
        public string Confirm;
        public System.Collections.Generic.IReadOnlyList<CampaignCardData> Cards;
    }
}
