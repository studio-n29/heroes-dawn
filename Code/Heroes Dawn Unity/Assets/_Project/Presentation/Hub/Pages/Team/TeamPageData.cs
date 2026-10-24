#nullable enable
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte de la grille Équipe (M20 ③) : membre, place dans l'escouade, fatigue et statut écrit.</summary>
    public struct MemberTileData
    {
        public MemberId Id;
        public MemberCardData Card;
        public bool InSquad;
        public int Fatigue;
        public float Fatigue01;
        public string FatigueText;
        public bool Exhausted;
        /// <summary>« DANS L'ESCOUADE », « ÉPUISÉ » ou vide : le sens n'est jamais porté par la couleur seule.</summary>
        public string Status;
        public string SquadLabel;
    }

    /// <summary>Tout ce qu'affiche la page Équipe (M20).</summary>
    public struct TeamPageData
    {
        public MemberTileData[] Members;
        /// <summary>0 = toutes les familles, 1 + (int)Family sinon.</summary>
        public int FilterIndex;
        public MemberSort Sort;
        public string SquadTitle;
        public SquadSlotData[] Squad;
        public string[] Synergies;
        public bool MissionEnabled;
        public string MissionHint;
        /// <summary>Libellé de chaque emplacement de composition (« 1 · 3 agents », « 1 · vide »).</summary>
        public string[] PresetLabels;
        public bool[] PresetSaved;
        public bool SaveMode;
        public string SaveLabel;
    }
}
