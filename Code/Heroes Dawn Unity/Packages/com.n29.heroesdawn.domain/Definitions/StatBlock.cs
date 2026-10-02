namespace N29.HeroesDawn.Definitions
{
    public sealed class StatBlock
    {
        public int Hp { get; }
        public int Atk { get; }
        public int Def { get; }
        public int Spd { get; }
        public StatBlock(int hp, int atk, int def, int spd) { Hp = hp; Atk = atk; Def = def; Spd = spd; }
    }
}
