#nullable enable
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Case de la carte : position dans la grille de la zone, marque d'objectif.</summary>
    public struct BoardMapNode
    {
        public CaseId Id;
        public int X;
        public int Y;
        public bool IsObjective;
    }

    public struct BoardMapLink
    {
        public CaseId A;
        public CaseId B;
    }

    /// <summary>Ce qu'il faut pour construire la carte d'une partie (une fois par partie).</summary>
    public struct BoardMapData
    {
        public BoardMapNode[] Nodes;
        public BoardMapLink[] Links;
        public PortraitData Pawn;
        public CaseId PawnAt;
    }

    /// <summary>État affiché d'une case (brouillard, type, coût si destination).</summary>
    public struct BoardNodeState
    {
        public CaseId Id;
        public bool Revealed;
        public string Glyph;
        public string Sub;
        public bool SubElite;
        public CaseType Type;
        public CombatRank Rank;
        public bool Spent;
        public bool ObjectiveDone;
        /// <summary>Coût en pas si la case est une destination atteignable, sinon -1.</summary>
        public int DestinationCost;
    }

    /// <summary>État d'un lien (même ordre que <see cref="BoardMapData.Links"/>).</summary>
    public struct BoardLinkState
    {
        public bool Visible;
        public bool Dim;
    }

    /// <summary>HUD du bas : consigne, PM, réserve du dé bonus, boutons.</summary>
    public struct BoardHudData
    {
        public string Hint;
        public string PmText;
        public float Reserve01;
        public string ReserveLabel;
        public bool RollEnabled;
        public string BonusLabel;
        public bool BonusEnabled;
        public bool EndEnabled;
    }

    public struct ObjectiveLineData
    {
        public string Text;
        public bool Done;
    }
}
