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
        /// <summary>Icônes des 4 compétences (alliés seulement) ; null pour les ennemis.</summary>
        public SkillSlotData[]? Skills;
    }

    /// <summary>Bouton de compétence (emplacement 0 à 2).</summary>
    public struct SkillSlotData
    {
        public bool Visible;
        public string Label;
        public string Description;
        public bool Pending;
        public bool Enabled;
        /// <summary>Emplacement réservé à une carte du grimoire (4e compétence), pas encore équipé.</summary>
        public bool Locked;
        /// <summary>Tours de recharge restants (0 : disponible).</summary>
        public int Cooldown;
        /// <summary>La compétence se pose sur une cible (glisser sur une carte) ; sinon on la lâche dans le vide.</summary>
        public bool NeedsTarget;
        /// <summary>Initiale affichée sur l'icône.</summary>
        public string Glyph;
    }

    public struct SkillPanelData
    {
        public bool Visible;
        /// <summary>Agent dont les icônes sont lançables ; -1 : aucun.</summary>
        public int UnitId;
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
        /// <summary>En canalisation (position négative).</summary>
        public bool Channelling;
        public bool Ready;
        public bool InPrep;
        /// <summary>Compétence annoncée (0 à 2) ; -1 : aucune.</summary>
        public int PlannedSlot;
        /// <summary>Point de la frise (negatif en canalisation).</summary>
        public int Point;
        public bool Boss;
    }

    public struct TimelineData
    {
        public TimelineMarkerData[] Markers;
        public float PrepStart01;
        /// <summary>Nombre de points entre le depart et le point d action.</summary>
        public int Points;
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
