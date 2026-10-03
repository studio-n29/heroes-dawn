#nullable enable
using System;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Hub
{
    public enum HubTab { Team, Recruit }

    /// <summary>En-tête commun des cartes de membre (effectif et recrutement).</summary>
    public struct MemberCardData
    {
        public MemberId Id;
        public string Name;
        public Family Family;
        public string RarityText;
        public Rarity Rarity;
        public string RoleLine;
        public string LevelText;
        public string Stats;
        public string Skills;
        public PortraitData Portrait;
    }

    /// <summary>Carte d'un membre de l'effectif.</summary>
    public struct TeamCardData
    {
        public MemberCardData Card;
        public float Fatigue01;
        public int Fatigue;
        public string FatigueText;
        public bool InSquad;
        public string SquadLabel;
        public string TrainLabel;
        public bool TrainEnabled;
        public bool Exhausted;
    }

    /// <summary>Carte d'une recrue disponible.</summary>
    public struct RecruitCardData
    {
        public MemberCardData Card;
        public string RecruitLabel;
        /// <summary>Emplacement dans la boutique (0 à 2).</summary>
        public int Slot;
        public bool Recruited;
        public string StampText;
        public bool RecruitEnabled;
    }

    /// <summary>Membre de l'escouade (QG et plateau).</summary>
    public struct SquadSlotData
    {
        public string Name;
        public PortraitData Portrait;
        public float Fatigue01;
        public int Fatigue;
        public bool Exhausted;
    }

    /// <summary>Tout ce qu'affiche le QG.</summary>
    public struct HubData
    {
        public string PlayerLine;
        public HubTab Tab;
        public string Hint;
        public TeamCardData[] Team;
        public RecruitCardData[] Recruits;
        public string EmptyRecruitText;
        public string ShopCountdown;
        public string RerollLabel;
        public bool RerollEnabled;
        public string[] ZoneObjectives;
        public string ZoneStatus;
        public bool ZoneDone;
        public string SquadTitle;
        public SquadSlotData[] Squad;
        public string HealLabel;
        public bool HealEnabled;
        public bool DeployEnabled;
        public string DeployHint;

        public static HubData Empty => new HubData
        {
            PlayerLine = "", Hint = "", Team = Array.Empty<TeamCardData>(), Recruits = Array.Empty<RecruitCardData>(),
            EmptyRecruitText = "", ShopCountdown = "", RerollLabel = "", ZoneObjectives = Array.Empty<string>(), ZoneStatus = "", SquadTitle = "",
            Squad = Array.Empty<SquadSlotData>(), HealLabel = "", DeployHint = ""
        };
    }
}
