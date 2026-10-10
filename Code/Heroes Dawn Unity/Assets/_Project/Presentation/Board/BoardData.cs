#nullable enable
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

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
        /// <summary>Fond de carte généré (ressource Resources/Zones/clé) et rectangle de grille qu'il couvre ; clé vide : pas de fond.</summary>
        public string BackgroundKey;
        public int BgMinX;
        public int BgMinY;
        public int BgMaxX;
        public int BgMaxY;
        public CaseId PawnAt;
    }

    /// <summary>Surbrillance d'une case pendant le déplacement.</summary>
    public enum NodeMark
    {
        None,
        /// <summary>Destination atteignable.</summary>
        Destination,
        /// <summary>Case encore sous le brouillard mais à portée : elle mène à un arrêt forcé.</summary>
        FogDestination,
        /// <summary>Case traversée par le chemin prévisualisé.</summary>
        Path,
        /// <summary>Case visée par la prévisualisation (« S'y rendre »).</summary>
        Target,
    }

    /// <summary>Rôle d'un lien dans le chemin prévisualisé.</summary>
    public enum LinkMark
    {
        None,
        /// <summary>Chemin réellement parcouru.</summary>
        Path,
        /// <summary>Suite du chemin au-delà de l'arrêt forcé (sous le brouillard).</summary>
        Beyond,
    }

    /// <summary>État affiché d'une case (brouillard, type, coût si destination).</summary>
    public struct BoardNodeState
    {
        public NodeMark Mark;
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
        public LinkMark Mark;
    }

    /// <summary>Carte d'information de la case visée (pop-up « S'y rendre ») pendant le choix de destination.</summary>
    public struct MovePreviewData
    {
        public string Title;
        public string Body;
        /// <summary>Coût, arrêt forcé, PM restants…</summary>
        public string Detail;
        public Tone Tone;
        public string ConfirmLabel;
        public string CancelLabel;
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
