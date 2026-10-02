using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class NodeDef
    {
        public CaseId Id { get; }
        public CaseType Type { get; }
        /// <summary>Case fixe : son contenu ne change jamais (objectifs, combats Élite, entrée).</summary>
        public bool IsFixed { get; }
        public CombatRank Rank { get; }
        /// <summary>Position sur la carte, en unités de grille.</summary>
        public int X { get; }
        public int Y { get; }

        public NodeDef(CaseId id, CaseType type, bool isFixed, CombatRank rank, int x, int y)
        {
            Id = id; Type = type; IsFixed = isFixed; Rank = rank; X = x; Y = y;
        }
    }
}
