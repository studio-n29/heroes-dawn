#nullable enable
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Combat
{
    /// <summary>Carte d'unité à construire (une fois par combat).</summary>
    public struct UnitCardData
    {
        public int Id;
        public bool Ally;
        public string Name;
        public bool Boss;
        public Family Family;
        public PortraitData Portrait;
        /// <summary>Ligne fixe sous les barres (alliés : fatigue).</summary>
        public string Sub;
    }

    /// <summary>État affiché d'une unité (rafraîchi à chaque image).</summary>
    public struct UnitStateData
    {
        public int Id;
        public bool Ally;
        public float Hp01;
        public string HpText;
        public float Gauge01;
        public bool GaugeFull;
        public string Statuses;
        public bool Alive;
        public bool Highlight;
        public bool Selected;
        public bool Targetable;
        /// <summary>Premier affichage : barres sans interpolation.</summary>
        public bool Instant;
    }

    /// <summary>Bouton de compétence (emplacement 0 à 2).</summary>
    public struct SkillSlotData
    {
        public bool Visible;
        public string Label;
        public string Description;
        public bool Pending;
        public bool Enabled;
    }

    public struct SkillPanelData
    {
        public bool Visible;
        public string Title;
        public SkillSlotData[] Slots;
    }

    public struct CombatTopBarData
    {
        public string Title;
        public bool AutoOn;
        public string SpeedText;
        public bool RetreatEnabled;
        public bool DebugVisible;
    }

    /// <summary>Une unité sur la frise chronologique : position 0..1, annonce de l action, état.</summary>
    public struct TimelineMarkerData
    {
        public int Id;
        public bool Ally;
        public bool Alive;
        public string Initial;
        public Family Family;
        public float Position01;
        /// <summary>En canalisation (position négative après un combo).</summary>
        public bool Channelling;
        public bool Ready;
        public bool InPrep;
        /// <summary>Compétence annoncée (0 à 2) ; -1 : aucune.</summary>
        public int PlannedSlot;
        public bool Boss;
    }

    public struct TimelineData
    {
        public TimelineMarkerData[] Markers;
        public float PrepStart01;
        public int Bank;
        public int BankMax;
        public string BankText;
        public bool SkipVisible;
        public bool SkipEnabled;
        public bool SkipTargeting;
        public string SkipLabel;
        public string SkipReason;
    }

    /// <summary>Ligne de fatigue des résultats (avant → après).</summary>
    public struct FatigueRowData
    {
        public string Name;
        public float Before01;
        public float After01;
        public int After;
        public string Label;
        public string Delta;
    }

    public struct CombatResultsData
    {
        public string Title;
        public bool Victory;
        public string Reward;
        public string FatigueTitle;
        public FatigueRowData[] Fatigue;
        public string Verify;
        public string ToHq;
        public string Continue;
    }
}
