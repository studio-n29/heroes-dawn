namespace N29.HeroesDawn.Domain.Board
{
    public enum BoardActionType { Roll, BonusDie, Move, EndMove, CombatResult, CaseResolved }

    /// <summary>Entrée du journal de zone (rejoué par le serveur au M2).</summary>
    public readonly struct BoardAction
    {
        public BoardActionType Type { get; }
        public string Arg { get; }
        public int Value { get; }
        public BoardAction(BoardActionType type, string arg, int value) { Type = type; Arg = arg; Value = value; }
    }
}
