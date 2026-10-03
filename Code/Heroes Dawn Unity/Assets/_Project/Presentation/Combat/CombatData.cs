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
