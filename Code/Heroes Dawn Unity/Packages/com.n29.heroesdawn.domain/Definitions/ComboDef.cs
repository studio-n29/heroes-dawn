namespace N29.HeroesDawn.Definitions
{
    public sealed class ComboDef
    {
        public string Id { get; }
        public string NameKey { get; }
        public Family A { get; }
        public Family B { get; }
        public SkillDef Effect { get; }

        public ComboDef(string id, string nameKey, Family a, Family b, SkillDef effect)
        {
            Id = id; NameKey = nameKey; A = a; B = b; Effect = effect;
        }

        public bool Matches(Family x, Family y) => (x == A && y == B) || (x == B && y == A);
    }
}
