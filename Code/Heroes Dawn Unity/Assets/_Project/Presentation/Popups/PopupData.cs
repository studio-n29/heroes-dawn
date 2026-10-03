#nullable enable
using N29.HeroesDawn.Application.Features.Zone;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Popups
{
    /// <summary>Case Repos : se reposer ou passer.</summary>
    public struct RestPopupData
    {
        public string Title;
        public string Body;
        public string Skip;
        public string Take;
    }

    public struct NarrativeChoiceData
    {
        public NarrativeChoice Choice;
        public string Label;
        public Tone Tone;
    }

    /// <summary>Case Événement : texte et choix.</summary>
    public struct NarrativePopupData
    {
        public string Title;
        public string Body;
        public NarrativeChoiceData[] Choices;
    }

    /// <summary>Case Mini-jeu : consignes, bouton Stop, format du score ({0} réussites, {1} essais restants).</summary>
    public struct MiniGamePopupData
    {
        public string Title;
        public string Body;
        public string Rules;
        public string Stop;
        public string ScoreFormat;
    }

    /// <summary>Zone sécurisée : récompenses et retour au QG.</summary>
    public struct ZoneCompleteData
    {
        public string Title;
        public string Rewards;
        public string ToHq;
    }
}
