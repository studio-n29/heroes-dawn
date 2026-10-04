#nullable enable
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>En-tête commun des cartes de membre (Équipe, fiche, boutique, pièces).</summary>
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

    /// <summary>Membre de l'escouade (Équipe et plateau).</summary>
    public struct SquadSlotData
    {
        public string Name;
        public PortraitData Portrait;
        public float Fatigue01;
        public int Fatigue;
        public bool Exhausted;
    }
}
