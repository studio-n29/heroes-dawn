#nullable enable
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
        /// <summary>Activité lancée par une case Mini-jeu ; vide : l'activité par défaut (<see cref="ActivityIds.Reflex"/>).</summary>
        public ActivityId? Activity { get; }
        /// <summary>Combat d'une case Combat : table d'ennemis et manches ; vide : rencontres de la zone par rang.</summary>
        public CombatSpec? Combat { get; }
        /// <summary>Dialogues joués avant et/ou après l'action de la case ; vide : aucun.</summary>
        public NarrationDef? Narration { get; }

        public NodeDef(CaseId id, CaseType type, bool isFixed, CombatRank rank, int x, int y, ActivityId? activity = null, CombatSpec? combat = null, NarrationDef? narration = null)
        {
            Id = id; Type = type; IsFixed = isFixed; Rank = rank; X = x; Y = y; Activity = activity; Combat = combat; Narration = narration;
        }
    }
}
